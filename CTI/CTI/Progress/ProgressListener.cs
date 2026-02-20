using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zamasoft.CTI.Progress
{
    /// <summary>
    /// サーバ側でのメインドキュメントの処理状況を受け取ります。
    /// </summary>
    public interface ProgressListener
    {
        /// <summary>
        /// サーバ側で見積もられたメインドキュメントの大きさが渡されます。
        /// <para>このメソッドは呼ばれないことがあり、不正確な値が渡される可能性もあります。</para>
        /// </summary>
        /// <param name="sourceLength">メインドキュメントのバイト数</param>
        void SourceLength(long sourceLength);

        /// <summary>
        /// 処理されたメインドキュメントのバイト数が渡されます。
        /// <para>このメソッドは呼ばれないことがあり、不正確な値が渡される可能性もあります。</para>
        /// </summary>
        /// <param name="serverRead">読み込み済みバイト数</param>
        void Progress(long serverRead);
    }
}
