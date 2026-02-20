using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Zamasoft.CTI.Result;
using Zamasoft.CTI.Message;
using Zamasoft.CTI.Progress;
using Zamasoft.CTI.Source;

namespace Zamasoft.CTI
{
    /// <summary>
    /// 処理の中断方法です。
    /// </summary>
    public enum AbortMode
    {
        /// <summary>
        /// なるべくきりのよいところ（不完全でも利用可能なデータが得られる）まで処理して中断します。
        /// </summary>
        NORMAL,
        /// <summary>
        /// 即座に処理を中断します。
        /// </summary>
        FORCE
    }

    /// <summary>
    /// ドキュメント変換処理を実行するためのサーバーとの接続です。
    /// </summary>
    public interface Session : IDisposable
    {
        /// <summary>
        /// 出力先です。
        /// <para>各Transcodeメソッドを呼ぶ前に設定してください。</para>
        /// </summary>
        Results Results
        {
            set;
        }

        /// <summary>
        /// メッセージを受け取るためのオブジェクトです。
        /// <para>各Transcodeメソッドを呼ぶ前に設定してください。</para>
        /// </summary>
        MessageHandler MessageHandler
        {
            set;
        }

        /// <summary>
        /// 進行状況を監視するためのオブジェクトです。
        /// <para>各Transcodeメソッドを呼ぶ前に設定してください。</para>
        /// </summary>
        ProgressListener ProgressListener
        {
            set;
        }

        /// <summary>
        /// trueを設定すると、複数の結果を結合するモードに切り替えられます。
        /// </summary>
        bool Continuous
        {
            set;
        }

        /// <summary>
        /// リソースを読み込むためのオブジェクトです。
        /// </summary>
        SourceResolver SourceResolver
        {
            set;
        }

        /// <summary>
        /// サーバーの情報を取得します。
        /// 取得したストリームは必ず閉じてください。
        /// </summary>
        /// <param name="uri">サーバー情報を選択するためのURI</param>
        /// <returns>入力ストリーム</returns>
        Stream GetServerInfo(string uri);

        /// <summary>
        /// プロパティを設定します。
        /// <para>各Transcodeメソッドを呼ぶ前に設定してください。</para>
        /// </summary>
        /// <param name="key">プロパティ名</param>
        /// <param name="value">値</param>
        void Property(string key, string value);

        /// <summary>
        /// リソースを送信するための出力ストリームを返します。
        /// <para>リソースを送信した後、出力ストリームは必ずクローズしてください。</para>
        /// <para>このメソッドは各transcodeメソッドの前に呼ぶ必要があります。</para>
        /// </summary>
        /// <param name="info">リソースデータのメタ情報</param>
        /// <returns>サーバーへの出力ストリーム</returns>
        Stream Resource(SourceInfo info);

        /// <summary>
        /// リソースを送信します
        /// <para>このメソッドは各transcodeメソッドの前に呼ぶ必要があります。</para>
        /// </summary>
        /// <param name="info">リソースデータのメタ情報</param>
        /// <param name="input">入力ストリーム</param>
	    void Resource(SourceInfo info, Stream input);

        /// <summary>
        /// メインドキュメントを送信するための出力ストリームを返します。
        /// <para>本体を送信した後、出力ストリームは必ずクローズしてください。</para>
        /// </summary>
        /// <param name="info">リソースデータのメタ情報</param>
        /// <returns>サーバーへの出力ストリーム</returns>
        Stream Transcode(SourceInfo info);

        /// <summary>
        /// メインドキュメントをストリームから取得して変換します。
        /// </summary>
        /// <param name="info">メインドキュメントのメタ情報</param>
        /// <param name="input">入力ストリーム</param>
        void Transcode(SourceInfo info, Stream input);

        /// <summary>
        /// 指定されたアドレスへサーバー側からアクセスしてメインドキュメントを取得して変換します。
        /// Resourceメソッドで事前に送信したリソースに対しても有効です。
        /// </summary>
        /// <param name="uri">メインドキュメントのURI</param>
        void Transcode(string uri);

        /// <summary>
        /// Continousがtrueに設定された状態で、複数回のTranscodeにより生成された結果を結合して出力します。
        /// </summary>
        void Join();

        /// <summary>
        /// 変換を中断します。
        /// このメソッドは非同期的に（別スレッドから）呼び出す必要があります。
        /// 実際に処理が中断された場合は、変換処理を行なっている（Transcodeを呼び出した）スレッドで、TranscoderExceptionがスローされます。
        /// </summary>
        /// <param name="mode">中断方法</param>
        void Abort(AbortMode mode);

        /// <summary>
        /// 送られたリソースと、プロパティ、メッセージハンドラ等の全ての設定をクリアして、セッションが作られた時点と同じ初期状態に戻します。
        /// </summary>
        void Reset();

        /// <summary>
        /// セッションをクローズします。
        /// <para>このメソッドを呼び出した後は、セッションに対して何もできません。</para>
        /// </summary>
        void Close();
    }
}
