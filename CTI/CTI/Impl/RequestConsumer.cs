using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.IO;

namespace Zamasoft.CTI.Impl
{
    internal class RequestConsumer
    {
	    /**
	     * プロパティパケットです。
	     */
	    public const byte PROPERTY = 0x01;

	    /**
	     * 内容開始パケットです。
	     */
	    public const byte START_MAIN = 0x02;

	    /**
	     * サーバー側でメインドキュメントの取得を要求するパケットです。
	     */
	    public const byte SERVER_MAIN = 0x03;

	    /**
	     * クライアント側でリソースを解決するモードに切り替えるパケットです。
	     */
	    public const byte CLIENT_RESOURCE = 0x04;

	    /**
	     * 複数の結果を結合するモードに切り替えるパケットです。
	     */
	    public const byte CONTINUOUS = 0x05;

	    /**
	     * データパケットです。
	     */
	    public const byte DATA = 0x11;

	    /**
	     * リソース開始パケットです。
	     */
	    public const byte START_RESOURCE = 0x21;

	    /**
	     * 存在しないリソースを示すパケットです。
	     */
	    public const byte MISSING_RESOURCE = 0x22;

	    /**
	     * データの終了を示すパケットです。
	     */
	    public const byte EOF = 0x31;

	    /**
	     * 処理中断パケットです。
	     */
	    public const byte ABORT = 0x32;

	    /**
	     * 結果の結合パケットです。
	     */
	    public const byte JOIN = 0x33;

	    /**
	     * 状態リセットパケットです。
	     */
	    public const byte RESET = 0x41;

	    /**
	     * 通信終了パケットです。
	     */
	    public const byte CLOSE = 0x42;

	    /**
	     * サーバー情報パケットです。
	     */
	    public const byte SERVER_INFO = 0x51;

        public const byte ABORT_NORMAL = 0;

        public const byte ABORT_FORCE = 1;

        private static readonly byte[] EMPTY_BYTES = new byte[0];

        private readonly ContentProducer producer;
        private readonly Stream stream;

        private readonly byte[] buffer = new byte[SessionImpl.BUFFER_SIZE + 5];
        private int pos = 0;

        private SessionImpl session;

        public RequestConsumer(ContentProducer producer)
        {
            this.producer = producer;
            this.stream = producer.Stream;
            byte[] header = Encoding.ASCII.GetBytes("CTIP/2.0 UTF-8\n");
            this.writeAll(header, header.Length);
        }

        internal void setSession(SessionImpl session)
        {
            this.session = session;
        }

	    internal void connect(Hashtable props) {
            string user = (string)props["user"];
            string password = (string)props["password"];
            if (user == null)
            {
                user = "";
            }
            if (password == null)
            {
                password = "";
            }

            string transcoder = (string)props["transcoder"];
            string message;
            if (transcoder == null)
            {
                message = "PLAIN: " + user + " " + password + "\n";
            }
            else
            {
                message = "OPTIONS: ";
                foreach (string key in props.Keys)
                {
                    if (message.Length > 9) 
                    {
                        message += "&";
                    }
                    message += key + "=" + props[key];
                }
                message += "\n";
            }


            byte[] data = Encoding.UTF8.GetBytes(message);
            this.writeAll(data, data.Length);
            this.producer.readAll(this.buffer, 4);
            string response = Encoding.UTF8.GetString(this.buffer, 0, 4);
            if (response.Equals("NG \n"))
            {
                throw new IOException("認証に失敗しました");
		    }
		    if (!response.Equals("OK \n")) {
			    throw new IOException("不正なレスポンスです:" + response);
		    }
	    }

        internal void property(string name, string value)
        {
		    this.flush();
            if (name == null)
            {
                name = "";
            }
            if (value == null)
            {
                value = "";
            }
            byte[] nameBytes = Encoding.UTF8.GetBytes(name);
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);

		    int payload = 1 + 2 + nameBytes.Length + 2 + valueBytes.Length;
		    this.writeInt(payload);
		    this.writeByte(PROPERTY);
            this.writeShort((short)nameBytes.Length);
            this.writeAll(nameBytes);
            this.writeShort((short)valueBytes.Length);
            this.writeAll(valueBytes);
	    }

        internal void clientResource(bool on)
        {
		    this.flush();

		    int payload = 2;
            this.writeInt(payload);
            this.writeByte(CLIENT_RESOURCE);
            this.writeByte((byte)(on ? 1 : 0));
	    }

        internal void startMain(string uri, String mimeType, String encoding, long length)
        {
		    this.flush();
            byte[] uriBytes = Encoding.UTF8.GetBytes(uri);
            byte[] mimeTypeBytes = mimeType == null ? EMPTY_BYTES : Encoding.UTF8.GetBytes(mimeType);
            byte[] encodingBytes = encoding == null ? EMPTY_BYTES : Encoding.UTF8.GetBytes(encoding);

            int payload = 1 + 2 + uriBytes.Length + 2 + mimeTypeBytes.Length + 2
                    + encodingBytes.Length + 8;
            this.writeInt(payload);
            this.writeByte(START_MAIN);
            this.writeShort((short)uriBytes.Length);
            this.writeAll(uriBytes);
            this.writeShort((short)mimeTypeBytes.Length);
            this.writeAll(mimeTypeBytes);
            this.writeShort((short)encodingBytes.Length);
            this.writeAll(encodingBytes);
            this.writeLong(length);
	    }

        internal void serverMain(string uri)
        {
		    this.flush();
            byte[] uriBytes = Encoding.UTF8.GetBytes(uri);

		    int payload = 1 + 2 + uriBytes.Length;
            this.writeInt(payload);
            this.writeByte(SERVER_MAIN);
            this.writeShort((short)uriBytes.Length);
            this.writeAll(uriBytes);
	    }

        internal void serverInfo(string uri)
        {
		    this.flush();
            byte[] uriBytes = Encoding.UTF8.GetBytes(uri);

		    int payload = 1 + 2 + uriBytes.Length;
		    this.writeInt(payload);
		    this.writeByte(SERVER_INFO);
		    this.writeShort((short) uriBytes.Length);
		    this.writeAll(uriBytes);
	    }

        internal void data(byte[] b, int off, int len)
        {
		    for (int i = 0; i < len; ++i) {
			    if (this.pos >= SessionImpl.BUFFER_SIZE) {
				    this.flush();
			    }
			    this.buffer[(this.pos++) + 4 + 1] = b[i + off];
		    }
	    }

	    private void flush() {
            if (this.pos <= 0)
            {
                return;
            }
			int payload = 1 + this.pos;
            this.buffer[0] = (byte)(payload >> 24 & 0xFF);
            this.buffer[1] = (byte)(payload >> 16 & 0xFF);
            this.buffer[2] = (byte)(payload >> 8 & 0xFF);
            this.buffer[3] = (byte)(payload & 0xFF);
            this.buffer[4] = DATA;
            int len = 4 + payload;

            this.session.buildAsync();
            this.stream.Write(this.buffer, 0, len);
  			this.pos = 0;
	    }

        internal void startResource(string uri, String mimeType, String encoding, long length)
        {
		    this.flush();
		    byte[] uriBytes = Encoding.UTF8.GetBytes(uri);
            byte[] mimeTypeBytes = mimeType == null ? EMPTY_BYTES : Encoding.UTF8.GetBytes(mimeType);
            byte[] encodingBytes = encoding == null ? EMPTY_BYTES : Encoding.UTF8.GetBytes(encoding);

		    int payload = 1 + 2 + uriBytes.Length + 2 + mimeTypeBytes.Length + 2
				    + encodingBytes.Length + 8;
		    this.writeInt(payload);
            this.writeByte(START_RESOURCE);
            this.writeShort((short)uriBytes.Length);
            this.writeAll(uriBytes);
            this.writeShort((short)mimeTypeBytes.Length);
            this.writeAll(mimeTypeBytes);
            this.writeShort((short)encodingBytes.Length);
            this.writeAll(encodingBytes);
            this.writeLong(length);
	    }

        internal void missingResource(string uri)
        {
		    this.flush();
		    byte[] uriBytes = Encoding.UTF8.GetBytes(uri);

		    int payload = 1 + 2 + uriBytes.Length;
            this.writeInt(payload);
            this.writeByte(MISSING_RESOURCE);
            this.writeShort((short)uriBytes.Length);
            this.writeAll(uriBytes);
	    }

        internal void eof()
        {
		    this.flush();

		    int payload = 1;
            this.writeInt(payload);
            this.writeByte(EOF);
	    }

        internal void continuous(bool continuous)
        {
		    this.flush();

		    int payload = 2;
            this.writeInt(payload);
            this.writeByte(CONTINUOUS);
            this.writeByte((byte)(continuous ? 1 : 0));
	    }

        internal void join()
        {
		    int payload = 1;
            this.writeInt(payload);
            this.writeByte(JOIN);
	    }

        internal void abort(byte mode)
        {
		    this.flush();

		    int payload = 2;
            this.writeInt(payload);
            this.writeByte(ABORT);
            this.writeByte(mode);
	    }

        internal void reset()
        {
		    this.flush();

		    int payload = 1;
            this.writeInt(payload);
            this.writeByte(RESET);
	    }

        internal void close()
        {
            this.flush();

		    int payload = 1;
            this.writeInt(payload);
            this.writeByte(CLOSE);
	    }

        private void writeAll(byte[] buffer, int len)
        {
           this.stream.Write(buffer, 0, len);
        }

        private void writeAll(byte[] buffer)
        {
            this.writeAll(buffer, buffer.Length);
        }

        private void writeAll(int len)
        {
            this.writeAll(this.buffer, len);
        }

        private void writeByte(byte a)
        {
            this.buffer[0] = a;
            this.writeAll(1);
        }

        private void writeShort(short a)
        {
            this.buffer[0] = (byte)(a >> 8 & 0xFF);
            this.buffer[1] = (byte)(a & 0xFF);
            this.writeAll(2);
        }

        private void writeInt(int a)
        {
            this.buffer[0] = (byte)(a >> 24 & 0xFF);
            this.buffer[1] = (byte)(a >> 16 & 0xFF);
            this.buffer[2] = (byte)(a >> 8 & 0xFF);
            this.buffer[3] = (byte)(a & 0xFF);
            this.writeAll(4);
        }

        private void writeLong(long a)
        {
            this.buffer[0] = (byte)(a >> 56 & 0xFF);
            this.buffer[1] = (byte)(a >> 48 & 0xFF);
            this.buffer[2] = (byte)(a >> 40 & 0xFF);
            this.buffer[3] = (byte)(a >> 32 & 0xFF);
            this.buffer[4] = (byte)(a >> 24 & 0xFF);
            this.buffer[5] = (byte)(a >> 16 & 0xFF);
            this.buffer[6] = (byte)(a >> 8 & 0xFF);
            this.buffer[7] = (byte)(a & 0xFF);
            this.writeAll(8);
        }
    }
}
