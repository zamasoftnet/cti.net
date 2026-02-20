using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace Zamasoft.CTI.Impl
{
    internal class ContentProducer
    {
        /**
         * データを開始します。
         */
        public const byte START_DATA = 0x01;

        /**
         * データパケットです。
         */
        public const byte BLOCK_DATA = 0x11;

        /**
         * 断片追加パケットです。
         */
        public const byte ADD_BLOCK = 0x12;

        /**
         * 断片挿入パケットです。
         */
        public const byte INSERT_BLOCK = 0x13;

        /**
         * エラーメッセージパケットです。
         */
        public const byte MESSAGE = 0x14;

        /**
         * メインドキュメントの長さを通知するパケットです。
         */
        public const byte MAIN_LENGTH = 0x15;

        /**
         * メインドキュメントの読み込みバイト数を通知するパケットです。
         */
        public const byte MAIN_READ = 0x16;

        /**
         * 断片化とは無関係なデータパケットです。
         */
        public const byte DATA = 0x17;

        /**
         * 断片のクローズを通知するパケットです。
         */
        public const byte CLOSE_BLOCK = 0x18;

        /**
         * リソース要求パケットです。
         */
        public const byte RESOURCE_REQUEST = 0x21;

        /**
         * データ終了パケットです。
         */
        public const byte EOF = 0x31;

        /**
         * データ中断パケットです。
         */
        public const byte ABORT = 0x32;

        /**
         * データ継続パケットです。
         */
        public const byte NEXT = 0x33;

        private readonly Random RND = new Random();

        private readonly Uri serverUri;

        private TcpClient tcp;

        private Stream stream;
        internal Stream Stream
        {
            get
            {
                return stream;
            }
        }

        private byte type;
        internal byte Type
        {
            get
            {
                return type;
            }
        }

        private byte mode;
        internal byte Mode
        {
            get
            {
                return mode;
            }
        }

        private int blockId;
        internal int BlockId
        {
            get
            {
                return blockId;
            }
        }

        private int anchorId;
        internal int AnchorId
        {
            get
            {
                return anchorId;
            }
        }

        private long length;
        internal long Length
        {
            get
            {
                return length;
            }
        }

        private short code;
        internal short Code
        {
            get
            {
                return code;
            }
        }

        private string uri;
        internal string Uri
        {
            get
            {
                return uri;
            }
        }

        private string mimeType;
        internal string MimeType
        {
            get
            {
                return mimeType;
            }
        }

        private string message;
        internal string Message
        {
            get
            {
                return message;
            }
        }

        private string encoding;
        internal string Encoding
        {
            get
            {
                return encoding;
            }
        }

        private List<string> args = new List<string>();
        public string[] Args
        {
            get
            {
                return this.args.ToArray();
            }
        }

        private int dataLength = 0, remaining = 0;

        private readonly byte[] buffer = new byte[SessionImpl.BUFFER_SIZE];

        public ContentProducer(Uri serverUri)
        {
            this.serverUri = serverUri;
        }

        internal RequestConsumer connect()
        {
            bool ssl;
            if (this.serverUri.Scheme == "ctip") {
                ssl = false;
            }
            else if (this.serverUri.Scheme == "ctips") {
                ssl = true;
            }
            else
            {
                throw new IOException("Unsupported scheme: "+this.serverUri.Scheme);
            }

            string host = this.serverUri.Host;
            int port = this.serverUri.Port;
            if (port == -1)
            {
                port = 8099;
            }
            this.tcp = new TcpClient(host, port);
 
            if (this.serverUri.Query != null && this.serverUri.Query.Length >= 1)
            {
                string[] pairs = this.serverUri.Query.Substring(1).Split('&');
                foreach(string s in pairs) {
                    string[] pair = s.Split('=');
                    if (pair[0] == "timeout")
                    {
                        this.tcp.ReceiveTimeout = this.tcp.SendTimeout = int.Parse(pair[1]);
                    }
                }
            }

            Stream tcpStream = this.tcp.GetStream();
            if (ssl)
            {
                SslStream sslStream = new SslStream(tcpStream, false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate));
                sslStream.AuthenticateAsClient(host);
                this.stream = sslStream;
            }
            else
            {
                this.stream = tcpStream;
            }

            if (this.tcp.ReceiveTimeout > 0)
            {
                this.stream.ReadTimeout = this.stream.WriteTimeout = this.tcp.ReceiveTimeout;
            }

            return new RequestConsumer(this);
        }

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors
        )
        {
            return true;
        }

        internal int read(byte[] b, int off, int len)
        {
            if (this.remaining <= 0)
            {
                return 0;
            }
            if (len > this.remaining)
            {
                len = this.remaining;
            }
            Array.Copy(this.buffer, this.dataLength - this.remaining, b, off, len);
            this.remaining -= len;
            return len;
        }

        internal void close()
        {
            if (this.stream != null)
            {
                this.stream.Close();
                this.stream = null;
            }
            if (this.tcp != null)
            {
                this.tcp.Close();
                this.tcp = null;
            }
        }

        internal void next()
        {
            int payload = this.readInt();
            this.type = this.readByte();
            switch (this.type)
            {
                case START_DATA:
                    this.uri = this.readString();
                    this.mimeType = this.readString();
                    this.encoding = this.readString();
                    this.length = this.readLong();
                    break;

                case BLOCK_DATA:
                    this.blockId = this.readInt();
                    payload -= 1 + 4;
                    this.readAll(payload);
                    this.dataLength = this.remaining = payload;
                    break;

                case ADD_BLOCK:
                    break;

                case INSERT_BLOCK:
                case CLOSE_BLOCK:
                    this.anchorId = this.readInt();
                    break;

                case MESSAGE:
                    this.code = this.readShort();
                    payload -= 1 + 2;
                    {
                        short len = 0;
                        this.message = this.readString(ref len);
                        payload -= 2 + len;
                    }
                    this.args.Clear();
                    while (payload > 0)
                    {
                        short len = 0;
                        this.args.Add(this.readString(ref len));
                        payload -= 2 + len;
                    }

                    break;
                case ABORT:
                    this.mode = this.readByte();
                    this.code = this.readShort();
                    payload -= 1 + 3;
                    {
                        short len = 0;
                        this.message = this.readString(ref len);
                        payload -= 2 + len;
                    }
                    while (payload > 0)
                    {
                        short len = 0;
                        this.args.Add(this.readString(ref len));
                        payload -= 2 + len;
                    }

                    break;

                case MAIN_LENGTH:
                case MAIN_READ:
                    this.length = this.readLong();
                    break;

                case DATA:
                    payload -= 1;
                    this.readAll(this.buffer, payload);
                    this.dataLength = this.remaining = payload;
                    break;

                case RESOURCE_REQUEST:
                    this.uri = this.readString();
                    break;

                case EOF:
                case NEXT:
                    break;

                default:
                    throw new IOException("Bad response: type "
                            + this.type.ToString("X"));
            }
        }

        internal void readAll(byte[] buffer, int len)
        {
            for (int i = 0; i < len; i += this.stream.Read(buffer, i, len - i))
                ;
        }

        internal void readAll(int len)
        {
            this.readAll(this.buffer, len);
        }

        internal byte readByte()
        {
            this.readAll(1);
            return buffer[0];
        }

        internal short readShort()
        {
            this.readAll(2);
            return (short)((((uint)buffer[0]) << 8)
            | ((uint)buffer[1]));
        }

        internal int readInt()
        {
            this.readAll(4);
            return (int)((((uint)buffer[0]) << 24)
            | (((uint)buffer[1]) << 16)
            | (((uint)buffer[2]) << 8)
            | ((uint)buffer[3]));
        }

        internal long readLong()
        {
            this.readAll(8);
            return (long)((((ulong)buffer[0]) << 56)
            | (((ulong)buffer[1]) << 48)
            | (((ulong)buffer[2]) << 40)
            | (((ulong)buffer[3]) << 32)
            | (((ulong)buffer[4]) << 24)
            | (((ulong)buffer[5]) << 16)
            | (((ulong)buffer[6]) << 8)
            | ((ulong)buffer[7]));
        }

        internal string readString(ref short len)
        {
            len = this.readShort();
            if (len == 0)
            {
                return "";
            }
            byte[] buffer = new byte[len];
            this.readAll(buffer, len);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        internal string readString()
        {
            short len = 0;
            return this.readString(ref len);
        }
    }
}
