using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Zamasoft.CTI.Impl
{
    internal class RequestConsumerStream : Stream
    {
        private readonly RequestConsumer request;

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
                return true;
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
                return false;
            }
        }

        public RequestConsumerStream(RequestConsumer request)
        {
            this.request = request;
        }

        public override int Read(byte[] buffer, int off, int len)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            this.request.eof();
        }

        public override void Write(byte[] buffer, int off, int len)
        {
            this.request.data(buffer, off, len);
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
            // ignore
        }
    }
}
