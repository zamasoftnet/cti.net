using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zamasoft.CTI
{
    /// <summary>
    /// 中断後のデータの状態です。
    /// </summary>
    public enum StateCode
    {
        /// <summary>
        /// 不完全な可能性があるものの、読み込み可能なデータが生成されていることを示す定数です。
        /// </summary>
        READABLE,
        /// <summary>
        /// 生成されたデータが破壊されていることを示す定数です。
        /// </summary>
        BROKEN
    }

    /// <summary>
    /// 変換処理中の中断に対して発生する例外です。
    /// </summary>
    public class TranscoderException : Exception
    {
	    private readonly short code;
        /// <summary>
        /// 中断の原因となったメッセージコードです。
        /// </summary>
        public short Code
        {
            get
            {
                return this.code;
            }
        }

	    private readonly string[] args;
        /// <summary>
        /// メッセージに付随する値です。
        /// </summary>
        public string[] Args
        {
            get
            {
                return this.args;
            }
        }

        /// <summary>
        /// 変換後の状態です。
        /// </summary>
        private readonly StateCode state;
        public StateCode State
        {
            get
            {
                return this.state;
            }
        }

        public TranscoderException(StateCode state, short code, string[] args, string message)
            : base(message)
        {
            this.code = code;
            this.args = args;
            this.state = state;
        }
    }
}
