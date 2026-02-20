using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Zamasoft.CTI.Impl
{
    internal class ContentProducerStream : Stream
    {
        private readonly ContentProducer producer;

        public override long Position
        {
            set
            {
                throw new NotImplementedException();
            }
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public ContentProducerStream(ContentProducer producer)
        {
            this.producer = producer;
            this.next();
        }

        private void next()
        {
            this.producer.next();
            int type = this.producer.Type;
            if (type != ContentProducer.EOF && type != ContentProducer.DATA)
            {
                throw new IOException("不正なパケットタイプです: " + type);
            }
        }

        public override int Read(byte[] buffer, int off, int len)
        {
            for (; ; )
            {
                if (this.producer.Type != ContentProducer.DATA)
                {
                    return 0;
                }
                int read = this.producer.read(buffer, off, len);
                if (read == 0)
                {
                    this.next();
                    continue;
                }
                return read;
            }
        }

        public override void Close()
        {
            while (this.producer.Type != ContentProducer.EOF)
            {
                this.next();
            }
        }

        public override void Write(byte[] buffer, int off, int len)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long len)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long len, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }
    }
}
