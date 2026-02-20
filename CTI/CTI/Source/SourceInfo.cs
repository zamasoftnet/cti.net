using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zamasoft.CTI.Source
{
    /// <summary>
    /// リソース、ドキュメントのメタ情報です。
    /// </summary>
    public class SourceInfo
    {
        private string uri;
        /// <summary>
        /// URIです。必ず設定されています。
        /// </summary>
        public string Uri
        {
            get
            {
                return this.uri;
            }
            set
            {
                this.uri = value;
            }
        }

        private string mimeType;
        /// <summary>
        /// MIME型です。不明な場合はnullです。
        /// </summary>
        public string MimeType
        {
            get
            {
                return this.mimeType;
            }
            set
            {
                this.mimeType = value;
            }
        }

        private string encoding;
        /// <summary>
        /// キャラクタエンコーディングです。不明な場合、あるいはバイナリではnullです。
        /// </summary>
        public string Encoding
        {
            get
            {
                return this.encoding;
            }
            set
            {
                this.encoding = value;
            }
        }

        private long length;
        /// <summary>
        /// データのバイト数です。不明な場合は-1です。
        /// </summary>
        public long Length
        {
            get
            {
                return this.length;
            }
            set
            {
                this.length = value;
            }
        }

        /// <summary>
        /// 全てのパラメータを設定してSourceInfoを構築します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="mimeType">MIME型</param>
        /// <param name="encoding">キャラクタエンコーディング</param>
        /// <param name="length">バイト数</param>
        public SourceInfo(string uri, string mimeType, string encoding, long length)
        {
            this.uri = uri;
            this.mimeType = mimeType;
            this.encoding = encoding;
            this.length = length;
        }

        /// <summary>
        /// Lengthに-1を設定したSourceInfoを構築します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="mimeType">MIME型</param>
        /// <param name="encoding">キャラクタエンコーディング</param>
        public SourceInfo(string uri, string mimeType, string encoding) : this(uri, mimeType, encoding, -1) { }

        /// <summary>
        /// Lengthに-1、Encodingにnullを設定したSourceInfoを構築します。
        /// </summary>
        /// <param name="uri">URI</param>
        /// <param name="mimeType">MIME型</param>
        public SourceInfo(string uri, string mimeType) : this(uri, mimeType, null) { }

        /// <summary>
        ///  Lengthに-1、Encodingにnull、MimeTypeにnullを設定したSourceInfoを構築します。
        /// </summary>
        /// <param name="uri">URI</param>
        public SourceInfo(string uri) : this(uri, null) { }
    }
}
