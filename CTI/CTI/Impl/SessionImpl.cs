using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Zamasoft.CTI.Result;
using Zamasoft.CTI.Message;
using Zamasoft.CTI.Progress;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI.Impl
{
    internal class SessionImpl : Session
    {
        private Uri uri;
        private Hashtable props;
        private int state = 0;
        private ContentProducer producer;
        private RequestConsumer request;

        private Results results;
        private Builder builder;

        private MessageHandler messageHandler;
        private ProgressListener progressListener;
        private SourceResolver resolver;

        private Thread buildThread = null;

	    public const int BUFFER_SIZE = 8192;

        private readonly byte[] readBuff = new byte[BUFFER_SIZE];
        private readonly byte[] writeBuff = new byte[BUFFER_SIZE];

        public SessionImpl(Uri uri, Hashtable props)
		{
		    this.uri = uri;
		    this.props = props;

            this.producer = new ContentProducer(uri);
            this.request = this.producer.connect();
            this.request.setSession(this);
            this.request.connect(this.props);
        }

	    protected void init() {
		    // 認証
		    if (this.producer == null) {
                this.producer = new ContentProducer(this.uri);
			    this.request = producer.connect();
			    this.request.connect(this.props);
		    }
	    }

        public Stream GetServerInfo(string uri)
        {
		    if (this.state >= 2) {
			    throw new InvalidOperationException ("既に本体が変換されています。");
		    }
		    this.init();
            this.request.serverInfo(uri);
            return new ContentProducerStream(producer);
	    }

        public Results Results
        {
            set
            {
                this.results = value;
            }
        }

        public MessageHandler MessageHandler
        {
            set
            {
                if (this.state >= 2)
                {
                    throw new InvalidOperationException("既に本体が変換されています。");
                }
                this.messageHandler = value;
            }
        }

        public ProgressListener ProgressListener
        {
            set
            {
                if (this.state >= 2)
                {
                    throw new InvalidOperationException("既に本体が変換されています。");
                }
                this.progressListener = value;
            }
        }

        public bool Continuous
        {
            set
            {
                this.init();
                this.request.continuous(value);
            }
        }

        public SourceResolver SourceResolver
        {
            set
            {
                this.resolver = value;
                this.init();
                this.request.clientResource(this.resolver != null);
            }
        }

	    public void Property(string key, string value) {
		    if (this.state >= 2) {
                throw new InvalidOperationException("既に本体が変換されています。");
		    }
		    this.init();
		    this.request.property(key, value);
	    }

        public Stream Resource(SourceInfo info) {
		    if (this.state >= 2) {
                throw new InvalidOperationException("既に本体が変換されています。");
		    }
		    this.init();
            this.request.startResource(info.Uri,
                    info.MimeType, info.Encoding,
                    info.Length);
		    return new RequestConsumerStream(this.request);
	    }

	    public void Resource(SourceInfo info, Stream input) {
		    if (this.state >= 2) {
			    throw new InvalidOperationException("既に本体が変換されています。");
		    }
		    this.init();
            this.request.startResource(info.Uri,
                    info.MimeType, info.Encoding,
                    info.Length);
		    try {
				for (int len = input
						.Read(this.writeBuff, 0, this.writeBuff.Length); len > 0; len = input
						.Read(this.writeBuff, 0, this.writeBuff.Length)) {
					this.request.data(this.writeBuff, 0, len);
				}
 		    } finally {
			    this.request.eof();
		    }
	    }

	    internal bool buildNext() {
		    if (this.state <= 1) {
			    return false;
		    }
		    this.producer.next();
		    bool serial = false;
		    switch (this.producer.Type) {
		    case ContentProducer.START_DATA: {
			    if (this.builder != null) {
				    this.builder.Finish();
				    this.builder.Dispose();
				    this.builder = null;
			    }
                SourceInfo info = new SourceInfo(this.producer.Uri, this.producer.MimeType, this.producer.Encoding, this.producer.Length);
			    this.builder = this.results.NextBuilder(info);
		    }
			    break;

		    case ContentProducer.BLOCK_DATA: {
			    // 結果データ
			    int blockId = this.producer.BlockId;
			    for (int len = this.producer.read(this.readBuff, 0,
					    this.readBuff.Length); len > 0; len = this.producer.read(
					    this.readBuff, 0, this.readBuff.Length)) {
				    this.builder.Write(blockId, this.readBuff, 0, len);
			    }
		    }
			    break;

		    case ContentProducer.ADD_BLOCK: {
			    this.builder.AddBlock();
		    }
			    break;

		    case ContentProducer.INSERT_BLOCK: {
			    int anchorId = this.producer.AnchorId;
			    this.builder.InsertBlockBefore(anchorId);
		    }
			    break;

		    case ContentProducer.CLOSE_BLOCK: {
			    int anchorId = this.producer.AnchorId;
			    this.builder.CloseBlock(anchorId);
		    }
			    break;

		    case ContentProducer.MESSAGE: {
			    if (this.messageHandler != null) {
				    short code = this.producer.Code;
				    String mes = this.producer.Message;
				    String[] args = this.producer.Args;
				    this.messageHandler.Message(code, args, mes);
			    }
		    }
			    break;

		    case ContentProducer.MAIN_LENGTH: {
			    if (this.progressListener != null) {
				    long sourceLength = this.producer.Length;
				    this.progressListener.SourceLength(sourceLength);
			    }
		    }
			    break;

		    case ContentProducer.MAIN_READ: {
			    if (this.progressListener != null) {
				    long serverRead = this.producer.Length;
				    this.progressListener.Progress(serverRead);
			    }
		    }
			    break;

		    case ContentProducer.DATA: {
			    // 結果データ
				for (int len = this.producer.read(this.readBuff, 0,
						this.readBuff.Length); len > 0; len = this.producer
						.read(this.readBuff, 0, this.readBuff.Length)) {
					this.builder.Write(this.readBuff, 0, len);
				}
			    if (!serial) {
				    serial = true;
			    }
		    }
			    break;

		    case ContentProducer.RESOURCE_REQUEST: {
			    // リソース要求
                string uri = this.producer.Uri;
			    if (this.resolver != null) {
                    SourceInfo info = null;
				    Stream input = this.resolver.Resolve(uri, ref info);
                    if (input == null)
                    {
                        this.request.missingResource(uri);
                    }
				    else {
					    using(input) {
                            this.request.startResource(info.Uri,
                                    info.MimeType, info.Encoding,
                                    info.Length);
							try {
								Stream output = new RequestConsumerStream(
										this.request);
								try {
                                    for (int len = input.Read(this.writeBuff, 0, this.writeBuff.Length); len > 0; len = input
                                            .Read(this.writeBuff, 0, this.writeBuff.Length))
                                    {
										output.Write(this.writeBuff, 0, len);
									}
								} finally {
                                    output.Close();
								}
							} finally {
								input.Close();
							}
					    }
				    }
			    } else {
				    this.request.missingResource(uri);
			    }
		    }
			    break;

		    case ContentProducer.EOF:
			    if (this.builder != null) {
				    this.builder.Finish();
				    this.builder.Dispose();
				    this.builder = null;
			    }
                this.state = 1;
                return false;
		    case ContentProducer.NEXT: {
			    this.state = 1;
		    }
			    return false;

		    case ContentProducer.ABORT: {
                StateCode state;
			    if (this.producer.Mode == 0) {
				    this.builder.Finish();
				    state = StateCode.READABLE;
			    } else {
                    state = StateCode.BROKEN;
			    }
			    if (this.builder != null) {
				    this.builder.Dispose();
				    this.builder = null;
			    }
			    this.state = 1;
			    throw new TranscoderException(state, this.producer.Code,
					    this.producer.Args, this.producer.Message);
		    }

		    default:
			    throw new IOException("不正なレスポンスです: "
					    + this.producer.Type.ToString("X"));
		    }
		    return true;
	    }

        class MyRequestConsumerStream : RequestConsumerStream
        {
            readonly SessionImpl session;
			bool closed = false;

            public MyRequestConsumerStream(SessionImpl session)
                : base(session.request)
            {
                this.session = session;
            }

			public override void Close() {
				if (this.closed) {
					return;
				}
				this.closed = true;
				try {
					base.Close();
				} finally {
					this.session.next();
				}
			}
        }

	    public Stream Transcode(SourceInfo info) {
		    if (this.results == null) {
                throw new InvalidOperationException("Resultsが設定されていません。");
		    }
		    if (this.state >= 2) {
                throw new InvalidOperationException("既に本体が変換されています。");
		    }
		    this.init();
            this.request.startMain(info.Uri,
                    info.MimeType, info.Encoding,
                    info.Length);
		    this.state = 2;
            return new MyRequestConsumerStream(this);
	    }

        public void Transcode(string uri)
        {
		    if (this.results == null) {
                throw new InvalidOperationException("Resultsが設定されていません。");
		    }
		    this.request.serverMain(uri);
		    this.state = 2;
		    this.next();
	    }

	    public void Transcode(SourceInfo info, Stream input) {
		    if (this.results == null) {
			    throw new InvalidOperationException("Resultsが設定されていません。");
		    }
		    this.init();
		    using(Stream output = this.Transcode(info))
		    {
                for (int len = input.Read(this.writeBuff, 0, this.writeBuff.Length); len > 0; len = input
                        .Read(this.writeBuff, 0, this.writeBuff.Length))
                {
					output.Write(this.writeBuff, 0, len);
				}
		    }
	    }

        internal void buildAsync()
        {
            if (this.buildThread != null)
            {
                return;
            }
            this.buildThread = new Thread(new ThreadStart(buildTask));
            this.buildThread.Start();
        }

        internal void buildTask()
        {
            try
            {
                while (this.buildNext())
                {
                    // do nothing
                }
            }
            finally
            {
                this.buildThread = null;

            }
        }

	    internal void next() {
            try
            {
                if (this.buildThread != null)
                {
                    this.buildThread.Join();
                }
                else
                {
                    this.buildTask();
                }
		    } finally {
 			    this.state = 1;
		    }
	    }

	    public void Join() {
		    this.request.join();
		    this.state = 2;
 		    this.next();
	    }

        public void Abort(AbortMode mode) {
            byte code;
            switch (mode)
            {
                case AbortMode.NORMAL:
                    code = RequestConsumer.ABORT_NORMAL;
                    break;
                case AbortMode.FORCE:
                    code = RequestConsumer.ABORT_FORCE;
                    break;
                default:
                    throw new InvalidOperationException();

            }
		    this.request.abort(code);
	    }

	    public void Reset() {
            if (this.request != null)
            {
			    this.request.reset();
		    }
		    this.resolver = null;
		    this.results = null;
		    this.state = 1;
		    this.builder = null;
            if (this.buildThread != null)
            {
                this.buildThread.Abort();
                this.buildThread = null;
            }
        }

	    public void Close() {
 		    if (this.state >= 3) {
			    return;
		    }
		    if (this.producer != null) {
			    try {
				    this.request.close();
			    } finally {
				    this.producer.close();
			    }
		    }
  		    this.state = 3;
            if (this.buildThread != null)
            {
                this.buildThread.Abort();
                this.buildThread = null;
            }
        }

        public void Dispose()
        {
             this.Close();
        }

    }
}
