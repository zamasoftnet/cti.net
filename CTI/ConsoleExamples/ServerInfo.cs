using System;
using System.IO;
using Zamasoft.CTI;

namespace examples
{
    /// <summary>
    /// Copper PDF の情報を取得して表示します。
    /// </summary>
    class ServerInfo
    {
        static void Main(string[] args)
        {
            using (Session session = DriverManager.getSession(new Uri("ctip://localhost:8099/"), "user", "kappa"))
            {
                // バージョン情報
                using (StreamReader info = new StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/version")))
                {
                    while (info.Peek() >= 0)
                    {
                        Console.WriteLine(info.ReadLine());
                    }
                }
                // サポートされる出力形式
                using (StreamReader info = new StreamReader(session.GetServerInfo("http://www.cssj.jp/ns/ctip/output-types")))
                {
                    while (info.Peek() >= 0)
                    {
                        Console.WriteLine(info.ReadLine());
                    }
                }
            }
        }
    }
}
