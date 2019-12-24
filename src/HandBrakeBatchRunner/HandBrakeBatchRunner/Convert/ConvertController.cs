using System;
using System.Diagnostics;
using System.Text;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// アウトプットデータ受信イベント引数クラス
    /// </summary>
    public class OutputDataReceivedEventArgs : EventArgs
    {
        public int Progress;
        public string ConvertStatus;
        public string LogData;
    }

    /// <summary>
    /// 変換コントローラ
    /// </summary>
    public class ConvertController : IDisposable
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
        /// キャンセルフラグ
        /// </summary>
        public bool IsCancel { get; set; } = false;

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handBrakeCLIFilePath"></param>
        public ConvertController(string handBrakeCLIFilePath)
        {
            this.HandBrakeCLIFilePath = handBrakeCLIFilePath;
        }

        /// <summary>
        /// 変換実行
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="srcFilePath"></param>
        /// <param name="dstFilePath"></param>
        public void ExecuteConvert(string convertSettingName, string srcFilePath, string dstFilePath)
        {
            //Processオブジェクトを作成
            using(var p = new Process()){
                //出力をストリームに書き込むようにする
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                p.OutputDataReceived += new DataReceivedEventHandler(this.OutputDataReceived);
                p.ErrorDataReceived += new DataReceivedEventHandler(this.OutputDataReceived);

                p.StartInfo.FileName = this.HandBrakeCLIFilePath;
                p.StartInfo.Arguments = "";
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.StandardErrorEncoding = Encoding.UTF8;
                p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                p.StartInfo.CreateNoWindow = true;

                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();

                // 1秒単位で待ち続ける(キャンセル待受用)
                while (p.WaitForExit(1000) == false)
                {
                    if (this.IsCancel)
                    {
                        p.Kill();
                        var args = new OutputDataReceivedEventArgs();
                        args.LogData = "強制終了しました。";
                        this.OnOutputDataReceived(args);
                        return;
                    }
                }
                this.IsComplete = true;
            }
        }

        /// <summary>
        /// プロセスからの標準出力・エラー出力を受け取る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OutputDataReceived(Object sender, DataReceivedEventArgs e)
        {
            // データが空の場合は終了
            if (e == null || string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            var args = new OutputDataReceivedEventArgs();
            args.LogData = e.Data;

            // パーセンテージ＋各種情報
            if (Constant.LOG_PROGRESS_AND_TIME_REGEX.IsMatch(e.Data))
            {
                var groups = Constant.LOG_PROGRESS_AND_TIME_REGEX.Match(e.Data).Groups;
                args.Progress = Decimal.ToInt32(Decimal.Round(Decimal.Parse(groups[1].Value)));
                args.ConvertStatus = groups[2].Value;
            }
            // パーセンテージのみ
            if (Constant.LOG_PROGRESS_REGEX.IsMatch(e.Data))
            {
                var groups = Constant.LOG_PROGRESS_REGEX.Match(e.Data).Groups;
                args.Progress = Decimal.ToInt32(Decimal.Round(Decimal.Parse(groups[1].Value)));
            }
            // イベントを発行
            this.OnOutputDataReceived(args);
        }

        /// <summary>
        /// イベントを発生させる
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOutputDataReceived(OutputDataReceivedEventArgs e)
        {
            OutputDataReceivedEvent?.Invoke(this, e);
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
