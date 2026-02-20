using System;
using System.Collections;
using System.Linq;
using System.Text;
using Zamasoft.CTI.Impl;

namespace Zamasoft.CTI
{
    /// <summary>
    /// ドライバの窓口クラスです。
    /// </summary>
    public class DriverManager
    {
        /// <summary>
        /// 指定されたURIへ接続するためのドライバを返します。
        /// </summary>
        /// <param name="uri">接続先URI</param>
        /// <returns>ドライバ</returns>
        public static Driver getDriver(Uri uri)
        {
            return new DriverImpl();
        }

        /// <summary>
        /// セッションを返します。
        /// <para>URIの形式は、ドライバの種類に依存します。 現在、実装が提供されているのはCTIP2プロトコルに対応したドライバです。</para>
        /// <para>"ctip://ホスト名:ポート番号/"という形式のURIを使用してください。 </para>
        /// </summary>
        /// <param name="uri">接続先のURI</param>
        /// <param name="props">接続プロパティのキーと値の組み合わせです。キー"user"はユーザー名、キー"password"はパスワードです。</param>
        /// <returns>セッション</returns>
        public static Session getSession(Uri uri, Hashtable props)
        {
            Driver driver = DriverManager.getDriver(uri);
            return driver.getSession(uri, props);
        }

        /// <summary>
        /// セッションを返します。
        /// <para>URIの形式は、ドライバの種類に依存します。 現在、実装が提供されているのはCTIP2プロトコルに対応したドライバです。</para>
        /// <para>"ctip://ホスト名:ポート番号/"という形式のURIを使用してください。 </para>
        /// </summary>
        /// <param name="uri">接続先のURI</param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>セッション</returns>
        /// <exception cref="System.IO.IOException">接続・認証に失敗した場合</exception>
        public static Session getSession(Uri uri, String user, String password)
        {
            Hashtable props = new Hashtable();
            props["user"] = user;
            props["password"] = password;
            return DriverManager.getSession(uri, props);
        }
    }
}
