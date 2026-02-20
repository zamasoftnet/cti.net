using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Zamasoft.CTI
{
    /// <summary>
    /// ドライバーのインターフェースです。
    /// 通常はプログラマが直接使う必要がありません。
    /// Zamasoft.CTI.DriverManagerを使用してください。
    /// </summary>
    public interface Driver
    {
        /// <summary>
        /// セッションを作成します。
        /// <para>URIの形式は、ドライバの種類に依存します。 現在、実装が提供されているのはCTIP2プロトコルに対応したドライバです。</para>
        /// <para>"ctip://ホスト名:ポート番号/"という形式のURIを使用してください。 </para>
        /// </summary>
        /// <param name="uri">接続先のURI</param>
        /// <param name="props">接続プロパティのキーと値の組み合わせです。キー"user"はユーザー名、キー"password"はパスワードです。</param>
        /// <returns>セッション</returns>
        /// <exception cref="System.IO.IOException">接続・認証に失敗した場合</exception>
        Session getSession(Uri uri, Hashtable props);
    }
}
