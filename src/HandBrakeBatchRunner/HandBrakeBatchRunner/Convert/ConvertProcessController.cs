﻿using HandBrakeBatchRunner.Setting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 変換コントローラ
    /// </summary>
    public class ConvertProcessController
    {
        #region "event"

        /// <summary>
        /// アウトプットデータイベント
        /// </summary>
        public event OutputDataReceivedHandler OutputDataReceivedEvent;

        #endregion

        #region "variable"

        /// <summary>
        /// キャンセルトークンソース
        /// </summary>
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// 標準出力のエンドイベント
        /// </summary>
        private TaskCompletionSource<bool> outputEndEvent = new TaskCompletionSource<bool>();

        #endregion
               
        #region "property"

        /// <summary>
        /// HandBrakeCLIのファイルパス
        /// </summary>
        public string HandBrakeCLIFilePath { get; set; }

        /// <summary>
        /// キャンセルフラグ
        /// </summary>
        public bool IsCancel
        {
            set
            {
                if (value)
                {
                    tokenSource.Cancel();
                }
            }
            get => tokenSource.IsCancellationRequested;
        }

        /// <summary>
        /// 完了フラグ
        /// </summary>
        public bool IsComplete { get; set; } = false;

        #endregion

        #region "constructor"

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="handBrakeCLIFilePath"></param>
        public ConvertProcessController(string handBrakeCLIFilePath)
        {
            HandBrakeCLIFilePath = handBrakeCLIFilePath;
        }

        #endregion

        #region "method"

        /// <summary>
        /// 変換実行
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="srcFilePath"></param>
        /// <param name="dstFilePath"></param>
        public async Task ExecuteConvert(string convertSettingName, string srcFilePath, Dictionary<string, string> replaceParam)
        {
            // 変換設定を取得
            ConvertSettingItem setting = ConvertSettingManager.Current.GetSetting(convertSettingName);

            //Processオブジェクトを作成
            using (Process proc = new Process())
            {
                //出力をストリームに書き込むようにする
                ProcessStartInfo startInfo = proc.StartInfo;
                startInfo.FileName = HandBrakeCLIFilePath;
                startInfo.Arguments = setting.GetCommandLineParameter(replaceParam);
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                //startInfo.StandardErrorEncoding = Encoding.UTF8;
                //startInfo.StandardOutputEncoding = Encoding.UTF8;

                proc.OutputDataReceived += new DataReceivedEventHandler(OutputDataReceived);
                proc.ErrorDataReceived += new DataReceivedEventHandler(OutputDataReceived);

                // 非同期実行開始
                ProcessResult result = await ExecuteConvertCommand(proc, 24 * 60 * 1000);
                if (result.Status == ProcessResult.CONVERT_STATUS.Completed)
                {
                    IsComplete = true;
                }
            }
        }

        /// <summary>
        /// プロセスからの標準出力・エラー出力を受け取る
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // データがnullの場合はプロセスが終了した
            if (e == null)
            {
                outputEndEvent.SetResult(true);
                return;
            }

            // データが空の場合は無視
            if (string.IsNullOrWhiteSpace(e.Data))
            {
                return;
            }

            ConvertStateChangedEventArgs args = new ConvertStateChangedEventArgs
            {
                LogData = e.Data
            };

            // パーセンテージ＋各種情報
            if (Constant.LOG_PROGRESS_AND_TIME_REGEX.IsMatch(e.Data))
            {
                GroupCollection groups = Constant.LOG_PROGRESS_AND_TIME_REGEX.Match(e.Data).Groups;
                args.FileProgress = decimal.ToInt32(decimal.Round(decimal.Parse(groups[1].Value)));
                args.FileStatus = groups[2].Value;
            }
            // パーセンテージのみ
            if (Constant.LOG_PROGRESS_REGEX.IsMatch(e.Data))
            {
                GroupCollection groups = Constant.LOG_PROGRESS_REGEX.Match(e.Data).Groups;
                args.FileProgress = decimal.ToInt32(decimal.Round(decimal.Parse(groups[1].Value)));
            }
            // イベントを発行
            OnOutputDataReceived(args);
        }

        /// <summary>
        /// イベントを発生させる
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOutputDataReceived(ConvertStateChangedEventArgs e)
        {
            OutputDataReceivedEvent?.Invoke(this, e);
        }

        /// <summary>
        /// プロセスを非同期に実行する
        /// </summary>
        /// <param name="proc"></param>
        /// <param name="timeout"></param>
        /// <param name="token"></param>
        /// <param name="outputCloseEvent"></param>
        /// <param name="errorCloseEvent"></param>
        /// <returns></returns>
        protected virtual async Task<ProcessResult> ExecuteConvertCommand(Process proc, int timeout)
        {
            ProcessResult result = new ProcessResult();
            bool isStarted;

            try
            {
                // プロセス実行開始
                isStarted = proc.Start();
            }
            catch (Exception error)
            {
                result.Status = ProcessResult.CONVERT_STATUS.NotRuning;
                result.ExitCode = -1;
                result.ErrorMessage = error.Message;

                isStarted = false;
            }

            if (isStarted)
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();

                int runTime = 0;
                const int waitTime = 1000;

                while (true)
                {
                    // 一定時間待つ
                    bool isExit = await WaitForExitAsync(proc, waitTime);
                    runTime += waitTime;

                    if (isExit || outputEndEvent.Task.IsCompleted)
                    {
                        // プロセスが完了した場合
                        result.Status = ProcessResult.CONVERT_STATUS.Completed;
                        result.ExitCode = proc.ExitCode;
                        break;
                    }
                    else if (tokenSource.Token.IsCancellationRequested)
                    {
                        // キャンセルがされた場合
                        try
                        {
                            proc.Kill();
                        }
                        catch { }

                        result.Status = ProcessResult.CONVERT_STATUS.Canceled;
                        break;
                        //token.ThrowIfCancellationRequested();
                    }
                    else if (runTime > timeout)
                    {
                        // タイムアウトをオーバーした場合
                        try
                        {
                            proc.Kill();
                        }
                        catch { }

                        result.Status = ProcessResult.CONVERT_STATUS.TimeOut;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// プロセス完了を非同期に待つ
        /// </summary>
        /// <param name="process"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return Task.Run(() => process.WaitForExit(timeout));
        }

        #endregion

        #region "internal class"

        /// <summary>
        /// プロセス実行結果
        /// </summary>
        protected class ProcessResult
        {
            /// <summary>
            /// 変換ステータス
            /// </summary>
            public enum CONVERT_STATUS
            {
                NotRuning,
                Canceled,
                TimeOut,
                Completed
            }

            /// <summary>
            /// 変換ステータス
            /// </summary>
            public CONVERT_STATUS Status { get; set; } = CONVERT_STATUS.NotRuning;

            /// <summary>
            /// 終了コード
            /// </summary>
            public int? ExitCode { get; set; } = null;

            /// <summary>
            /// エラーメッセージ
            /// </summary>
            public string ErrorMessage { get; set; } = string.Empty;
        }

        #endregion

    }
}