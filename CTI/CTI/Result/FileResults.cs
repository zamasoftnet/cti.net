using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI.Result
{
    /// <summary>
    /// 複数のファイルを複数の結果として生成します。
    /// </summary>
    public class FileResults : Results
    {
	    protected readonly string prefix, suffix;
	    protected int counter = 0;

        /// <summary>
        /// ファイル名の形式を指定してFileResultsを構築します。
        /// [prefix][0から始まる連番][suffix]という形式でファイルは生成されます。
        /// </summary>
        /// <param name="prefix">ファイル名の先頭のパス</param>
        /// <param name="suffix">ファイル名の末尾</param>
        public FileResults(string prefix, string suffix)
        {
            this.prefix = prefix;
            this.suffix = suffix;
        }

        public bool HasNext()
        {
            return true;
        }

        public Builder NextBuilder(SourceInfo info)
        {
            ++this.counter;
            Builder builder = new Builder(new FileStream(this.prefix + this.counter + this.suffix, FileMode.Create, FileAccess.Write));
            builder.Info = info;
            return builder;
        }

        public void End()
        {
            // ignore
        }
    }
}
