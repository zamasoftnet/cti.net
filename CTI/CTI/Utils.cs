using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zamasoft.CTI.Message;
using Zamasoft.CTI.Result;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI
{
    class ConsoleMessageHandler : MessageHandler
    {
        public void Message(short code, string[] args, string mes)
        {
            Console.WriteLine(mes);
        }
    }

    class ErrorMessageHandler : MessageHandler
    {
        public void Message(short code, string[] args, string mes)
        {
            Console.Error.WriteLine(mes);
        }
    }

    /// <summary>
    /// Zammasoft.CTI.Sessionを扱う上でのユーティリティです。
    /// </summary>
    public class Utils
    {
        static readonly ConsoleMessageHandler CMH = new ConsoleMessageHandler();
        static readonly ErrorMessageHandler EMH = new ErrorMessageHandler();

        /// <summary>
        /// メッセージの出力先を標準出力に設定します。
        /// </summary>
        /// <param name="session">セッション</param>
        public static void SetConsoleMessageHander(Session session)
        {
            session.MessageHandler = Utils.CMH;
        }

        /// <summary>
        /// メッセージの出力先を標準エラー出力に設定します。
        /// </summary>
        /// <param name="session">セッション</param>
        public static void SetErrorMessageHander(Session session)
        {
            session.MessageHandler = Utils.EMH;
        }

        /// <summary>
        /// 単一のファイルに出力するように設定します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="file">出力先ファイルパス</param>
        public static void SetResultFile(Session session, string file)
        {
            session.Results = new SingleResult(file);
        }

        /// <summary>
        /// 単一のストリームに出力するように設定します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="output">出力先ストリーム</param>
        public static void SetResultStream(Session session, Stream output)
        {
            session.Results = new SingleResult(output);
        }

        /// <summary>
        /// リソースとしてファイルを送信します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="file">送信するファイル</param>
        /// <param name="mimeType">MIME型(省略する場合はnull)</param>
        /// <param name="encoding">エンコーディング(省略する場合はnull)</param>
        public static void SendResourceFile(Session session, string file,
                String mimeType, String encoding)
        {
            Uri bu = new Uri(System.Environment.CurrentDirectory+"/");
            String uri = new Uri(bu, file).AbsoluteUri;
            session.Resource(new SourceInfo(uri, mimeType, encoding, new FileInfo(file).Length), new FileStream(file, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// リソースとしてストリームから取り出されるデータを送信します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="input">入力ストリーム</param>
        /// <param name="mimeType">MIME型(省略する場合はnull)</param>
        /// <param name="encoding">エンコーディング(省略する場合はnull)</param>
        public static void SendResourceStream(Session session, Stream input, string uri,
                String mimeType, String encoding)
        {
            session.Resource(new SourceInfo(uri, mimeType, encoding, -1), input);
        }

        /// <summary>
        /// ファイルの内容を変換します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="file">変換対象ファイル</param>
        /// <param name="mimeType">MIME型(省略する場合はnull)</param>
        /// <param name="encoding">エンコーディング(省略する場合はnull)</param>
        public static void TranscodeFile(Session session, string file,
                String mimeType, String encoding)
        {
            Uri bu = new Uri(System.Environment.CurrentDirectory + "/");
            String uri = new Uri(bu, file).AbsoluteUri;
            session.Transcode(new SourceInfo(uri, mimeType, encoding, new FileInfo(file).Length), new FileStream(file, FileMode.Open, FileAccess.Read));
        }

        /// <summary>
        /// ストリームから取り出される内容を変換します。
        /// </summary>
        /// <param name="session">セッション</param>
        /// <param name="input">入力ストリーム</param>
        /// <param name="mimeType">MIME型(省略する場合はnull)</param>
        /// <param name="encoding">エンコーディング(省略する場合はnull)</param>
        public static void TranscodeStream(Session session, Stream input, string uri,
                String mimeType, String encoding)
        {
            session.Transcode(new SourceInfo(uri, mimeType, encoding, -1), input);
        }
    }
}
