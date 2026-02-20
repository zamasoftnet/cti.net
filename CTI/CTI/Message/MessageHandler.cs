using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zamasoft.CTI.Message
{
    /// <summary>
    /// メッセージを受け取るインターフェースです。
    /// </summary>
    public interface MessageHandler
    {
        /// <summary>
        /// メッセージ受け取ります。
        /// </summary>
        /// <param name="code">メッセージコード</param>
        /// <param name="args">メッセージに付随する値</param>
        /// <param name="mes">人間が読める形式のメッセージ</param>
        void Message(short code, string[] args, string mes);
    }
}
