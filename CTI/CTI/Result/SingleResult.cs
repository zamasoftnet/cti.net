using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI.Result
{
    /// <summary>
    /// 単一の結果を生成します。
    /// 画像などを生成する場合２枚目以降は生成されません。
    /// </summary>
    public class SingleResult : Results
    {
        protected Builder builder = null;
        protected string file = null;

        /// <summary>
        /// Builderを指定してSingleResultを構築します。
        /// </summary>
        /// <param name="builder">出力先のBuilder</param>
        public SingleResult(Builder builder)
        {
            this.builder = builder;
        }

        /// <summary>
        /// ストリームを指定してSingleResultを構築します。
        /// </summary>
        /// <param name="output">出力ストリーム</param>
        public SingleResult(Stream output) : this(new Builder(output)) { }

        /// <summary>
        /// ファイルを指定してSingleResultを構築します。
        /// </summary>
        /// <param name="file">出力先ファイル</param>
        public SingleResult(string file)
        {
            this.file = file;
        }

        public bool HasNext()
        {
            return this.file == null && this.builder != null;
        }

        public Builder NextBuilder(SourceInfo info)
        {
            if (this.file != null)
            {
                this.builder = new Builder(new FileStream(this.file, FileMode.Create, FileAccess.Write));
                this.file = null;
            }
            try
            {
                this.builder.Info = info;
                return this.builder;
            }
            finally
            {
                this.builder = null;
            }
        }

        public void End()
        {
            // ignore
        }
    }
}
