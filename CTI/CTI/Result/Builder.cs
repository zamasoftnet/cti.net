using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI.Result
{
    /// <summary>
    /// ストリームに対してデータを生成します。
    /// </summary>
    public class Builder
    {
        /**
         * セグメントのサイズです。
         */
        const int SEGMENT_SIZE = 8192;


        /**
         * フラグメントバッファの最大サイズ、その合計の最大サイズ、クローズ時にディスクに落とすときの敷居値
         */
        private int blockBufferSize, totalBufferSize, threshold;

        protected Stream raf = null;

        protected string file = null;

        protected List<Block> frgs = null;

        protected Block first = null, last = null;

        protected long length = 0, onMemory = 0;

        protected int segment = 0;

        protected SourceInfo info;

        public SourceInfo Info
        {
            set
            {
                this.info = value;
            }
            get
            {
                return this.info;
            }
        }

        private byte[] buff = null;

        protected readonly Stream output;


        protected class Block
        {
            public Block prev = null, next = null;

            private int id;

            private int len = 0;

            private byte[] buffer = null;

            private List<int> segments;

            private int segLen = 0;

            private readonly Builder builder;

            public Block(Builder builder, int id)
            {
                this.builder = builder;
                this.id = id;
            }

            public int getId()
            {
                return this.id;
            }

            public int getLength()
            {
                return this.len;
            }

            public void write(byte[] buff, int pos, int len)
            {
                if (this.segments == null
                        && (this.len + len) < this.builder.blockBufferSize
                        && (this.builder.onMemory + this.builder.blockBufferSize) <= this.builder.totalBufferSize)
                {
                    if (this.buffer == null)
                    {
                        this.buffer = new byte[this.builder.blockBufferSize];
                        this.builder.onMemory += this.builder.blockBufferSize;
                    }
                    Array.Copy(buff, pos, this.buffer, this.len, len);
                }
                else
                {
                    if (this.buffer != null)
                    {
                        this.rafWrite(this.buffer, 0, this.len);
                        this.builder.onMemory -= this.builder.blockBufferSize;
                        this.buffer = null;
                    }
                    this.rafWrite(buff, pos, len);
                }
                this.len += len;
            }

            private void rafWrite(byte[] buff, int off, int len)
            {
                if (this.segments == null)
                {
                    this.segments = new List<int>(10);
                    this.segments
                            .Add(this.builder.segment++);
                }
                while (len > 0)
                {
                    if (this.segLen == Builder.SEGMENT_SIZE)
                    {
                        this.segments
                                .Add(this.builder.segment++);
                        this.segLen = 0;
                    }
                    int seg = this.segments[this.segments.Count - 1];
                    int wlen = Math.Min(len,
                            Builder.SEGMENT_SIZE
                                    - this.segLen);
                    long wpos = (long)seg
                            * (long)Builder.SEGMENT_SIZE
                            + (long)this.segLen;
                    this.builder.raf.Seek(wpos, SeekOrigin.Begin);
                    this.builder.raf.Write(buff, off, wlen);
                    this.segLen += wlen;
                    off += wlen;
                    len -= wlen;
                }
            }

            public void close()
            {
                if (this.buffer != null)
                {
                    if (this.len >= this.builder.threshold)
                    {
                        this.rafWrite(this.buffer, 0, this.len);
                        this.builder.onMemory -= this.builder.blockBufferSize;
                        this.buffer = null;
                    }
                    else if (this.len < this.buffer.Length)
                    {
                        byte[] temp = new byte[this.len];
                        Array.Copy(this.buffer, 0, temp, 0, temp.Length);
                        this.builder.onMemory -= (this.buffer.Length - this.len);
                        this.buffer = temp;
                    }
                }
            }

            public void writeTo(Stream output)
            {
                int seg;
                long rpos;
                if (this.segments == null)
                {
                    if (this.buffer != null)
                    {
                        output.Write(this.buffer, 0, this.len);
                    }
                }
                else
                {
                    if (this.builder.buff == null)
                    {
                        this.builder.buff = new byte[Builder.SEGMENT_SIZE];
                    }
                    byte[] buff = this.builder.buff;
                    for (int i = 0; i < this.segments.Count - 1; ++i)
                    {
                        seg = this.segments[i];
                        rpos = (long)seg
                                * (long)Builder.SEGMENT_SIZE;
                        this.builder.raf.Seek(rpos, SeekOrigin.Begin);
                        this.rafReadFully(buff, 0, buff.Length);
                        output.Write(buff, 0, buff.Length);
                    }
                    seg = this.segments[this.segments.Count - 1];
                    rpos = (long)seg
                            * (long)Builder.SEGMENT_SIZE;
                    this.builder.raf.Seek(rpos, SeekOrigin.Begin);
                    this.rafReadFully(buff, 0,
                            this.segLen);
                    output.Write(buff, 0, this.segLen);
                }
            }

            private void rafReadFully(byte[] buff, int off, int len)
            {
                for (int read = this.builder.raf.Read(buff, off, len); read > 0; read = this.builder.raf.Read(buff, off, len))
                {
                    off += read;
                    len -= read;
                }
            }

            public void dispose()
            {
                this.buffer = null;
            }
        }
        
        /// <summary>
        /// データの生成先ストリームを設定してBuilderを構築します。
        /// </summary>
        /// <param name="output">データの生成先出力ストリーム</param>
        public Builder(Stream output) : this(output, 8192, 1024 * 1024 * 2, 1024) { }

        public Builder(Stream output, int fragmentBufferSize,
                int totalBufferSize, int threshold)
        {
            this.output = output;
            this.blockBufferSize = fragmentBufferSize;
            this.totalBufferSize = totalBufferSize;
            this.threshold = threshold;
        }

        protected int nextId()
        {
            if (this.frgs == null)
            {
                this.frgs = new List<Block>();
                this.file = Path.GetTempFileName();
                this.raf = new FileStream(this.file, FileMode.Create, FileAccess.ReadWrite);
            }
            return this.frgs.Count;
        }

        protected Block getBlock(int id)
        {
            return (Block)this.frgs[id];
        }

        protected void putBlock(int id, Block frg)
        {
            this.frgs.Add(frg);
        }

        public void AddBlock()
        {
            int id = this.nextId();
            Block bk = new Block(this, id);
            if (this.first == null)
            {
                this.first = bk;
            }
            else
            {
                this.last.next = bk;
                bk.prev = this.last;
            }
            this.putBlock(id, bk);
            this.last = bk;
        }

        public void InsertBlockBefore(int anchorId)
        {
            int id = this.nextId();
            Block anchor = this.getBlock(anchorId);
            Block bk = new Block(this, id);
            this.putBlock(id, bk);
            bk.prev = anchor.prev;
            bk.next = anchor;
            anchor.prev.next = bk;
            anchor.prev = bk;
            if (this.first == anchor)
            {
                this.first = bk;
            }
        }

        public void Write(int id, byte[] b, int off, int len)
        {
            Block frg = this.getBlock(id);
            frg.write(b, off, len);
            this.length += len;
        }

        public void Write(byte[] b, int off, int len)
        {
            this.output.Write(b, off, len);
        }

        public void CloseBlock(int id)
        {
            Block frg = this.getBlock(id);
            frg.close();
        }

        public virtual void Finish()
        {
            if (this.first == null)
            {
                // 空
                this.clean();
                return;
            }

            Block bk = this.first;
            while (bk != null)
            {
                bk.writeTo(this.output);
                bk.dispose();
                bk = bk.next;
            }
            this.clean();
        }

        private void clean()
        {
            if (this.raf != null)
            {
                this.raf.Close();
                this.raf = null;
            }
            if (this.file != null)
            {
                File.Delete(this.file);
                this.file = null;
            }
            this.first = null;
            this.last = null;
            this.frgs = null;
            this.length = 0;
            this.onMemory = 0;
            this.segment = 0;
        }

        public void Dispose()
        {
            this.clean();
            this.output.Close();
        }
    }
}
