using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI.Result
{
    /// <summary>
    /// １つまたは複数の結果を生成するためのインターフェースです。
    /// </summary>
    public interface Results
    {
        /// <summary>
        /// 次の結果を生成可能であればtrueを返します。
        /// falseの場合、次の結果の生成は行われません。
        /// </summary>
        /// <returns>次の結果を生成可能であればtrue</returns>
        bool HasNext();

        /// <summary>
        /// 次の結果を生成するためのBuilderを返します。
        /// </summary>
        /// <param name="info">結果のメタ情報</param>
        /// <returns>出力先Builder</returns>
        Builder NextBuilder(SourceInfo info);

        /// <summary>
        /// 一連の結果の生成を終了します。
        /// </summary>
        void End();
    }
}
