using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Zamasoft.CTI.Source
{
    /// <summary>
    /// URIで示されたリソースを取得するためのインターフェースです。
    /// </summary>
    public interface SourceResolver
    {
        /// <summary>
        /// URIで示されたリソースのストリームを返します。
        /// </summary>
        /// <param name="uri">リソースのURI</param>
        /// <param name="info">リソースのメタ情報。リソースが存在する場合は必ず設定してください</param>
        /// <returns>入力ストリーム。リソースが存在しない場合はnull。</returns>
        Stream Resolve(string uri, ref SourceInfo info);
    }
}
