using System;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// アウトプットデータ受信イベント引数クラス
    /// </summary>
    public class OutputDataReceivedEventArgs : EventArgs
    {
        public string OutputData;
    }

    /// <summary>
    /// 変換コントローラ
    /// </summary>
    internal class ConvertController : IDisposable
    {
        /// <summary>
        /// アウトプットデータ受信イベントハンドラー
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public delegate void OutputDataReceivedHandler(object sender, OutputDataReceivedEventArgs e);

        /// <summary>
        /// アウトプットデータイベント
        /// </summary>
        public event OutputDataReceivedHandler OutputDataReceivedEvent;

        /// <summary>
        /// HandBrakeCLIのファイルパス
        /// </summary>
        public string HandBrakeCLIFilePath { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handBrakeCLIFilePath"></param>
        public ConvertController(string handBrakeCLIFilePath)
        {

        }

        /// <summary>
        /// 変換実行
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="srcFilePath"></param>
        /// <param name="dstFilePath"></param>
        public void ExecuteConvert(string convertSettingName, string srcFilePath, string dstFilePath)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージド状態を破棄します (マネージド オブジェクト)。
                }

                // TODO: アンマネージド リソース (アンマネージド オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        // TODO: 上の Dispose(bool disposing) にアンマネージド リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
        // ~Class1() {
        //   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
        //   Dispose(false);
        // }

        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            // TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
