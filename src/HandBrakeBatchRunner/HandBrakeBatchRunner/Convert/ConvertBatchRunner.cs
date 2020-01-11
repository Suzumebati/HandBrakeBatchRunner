// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using HandBrakeBatchRunner.Setting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 複数のファイルをバッチで変換実行する
    /// </summary>
    public class ConvertBatchRunner
    {
        #region "event"

        /// <summary>
        /// 変換状態変更イベント
        /// </summary>
        public event ConvertStateChangedHandler ConvertStateChangedEvent;

        #endregion

        #region "variable"

        /// <summary>
        /// 変換コントローラ
        /// </summary>
        private ConvertProcessController contoller = null;

        /// <summary>
        /// 現在変換中のファイルインデックス
        /// </summary>
        private int currentFileIndex = 0;

        /// <summary>
        /// 現在変換中のファイル名
        /// </summary>
        private string currentFileName = string.Empty;

        #endregion

        #region "property"

        /// <summary>
        /// 変換元ファイルリスト
        /// </summary>
        public List<string> SourceFileList { get; set; }

        /// <summary>
        /// 変換先フォルダ
        /// </summary>
        public string DestinationFolder { get; set; }

        /// <summary>
        /// 完了フォルダ
        /// </summary>
        public string CompleteFolder { get; set; }

        /// <summary>
        /// 変換設定名
        /// </summary>
        public string ConvertSettingName { get; set; }

        /// <summary>
        /// 変換設定
        /// </summary>
        public ConvertSettingItem ConvertSetting { get; set; }

        /// <summary>
        /// HandBrakeCLIのファイルパス
        /// </summary>
        public string HandBrakeCLIFilePath { get; set; }

        /// <summary>
        /// 次でキャンセルがリクエストされた
        /// </summary>
        public bool IsCancellationNextRequested { get; set; } = false;

        /// <summary>
        /// キャンセルがリクエストされた
        /// </summary>
        public bool IsCancellationRequested { get; set; } = false;

        #endregion

        #region "constructor"

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="destinationFolder"></param>
        /// <param name="completeFolder"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="handBrakeCLIFilePath"></param>
        public ConvertBatchRunner(List<string> sourceFileList,
                                  string destinationFolder,
                                  string completeFolder,
                                  string convertSettingName,
                                  string handBrakeCLIFilePath)
        {
            SourceFileList = sourceFileList;
            DestinationFolder = destinationFolder;
            CompleteFolder = completeFolder;
            ConvertSettingName = convertSettingName;
            ConvertSetting = ConvertSettingManager.Current.GetSetting(convertSettingName);
            HandBrakeCLIFilePath = handBrakeCLIFilePath;
        }

        #endregion

        #region "method"

        /// <summary>
        /// 複数のファイルをバッチで変換実行する
        /// </summary>
        /// <returns>非同期タスク</returns>
        public async Task BatchConvert()
        {
            using (var mutex = new MutexWrapper(false, "HandBrakeBatchRunner"))
            {
                // 既に実行中の場合は待機する
                if (await WaitingPreviosConvert(mutex) == false) return;

                contoller = new ConvertProcessController(HandBrakeCLIFilePath);
                contoller.ConvertStateChangedEvent += new ConvertStateChangedHandler(ConvertStateChanged);

                for (currentFileIndex = 0; currentFileIndex < SourceFileList.Count; currentFileIndex++)
                {
                    string currentSourceFilePath = SourceFileList[currentFileIndex];
                    var replaceParam = CreateReplaceParam(ConvertSettingName, currentSourceFilePath, DestinationFolder);

                    // 変換開始時にステータス更新
                    OnConvertStateChanged((int)(((double)currentFileIndex / SourceFileList.Count) * 100), 0,
                        $"{currentFileIndex}/{SourceFileList.Count}", "スキャン中");

                    // 変換元が存在しない場合はスキップ
                    if (File.Exists(currentSourceFilePath) == false)
                    {
                        LogWindow.LogMessage($"元ファイルが存在しないのでスキップします。 File={currentSourceFilePath}", LogWindow.MessageType.Warning);
                        continue;
                    }

                    // 変換先にすでにファイルが有る場合はスキップ
                    if (IsAlreadyExistCompleteFolder(replaceParam)) continue;

                    // 一個のファイルを変換する
                    LogWindow.LogMessage($"Handbrakeで変換処理を開始します。 File={currentSourceFilePath}", LogWindow.MessageType.Information);
                    await contoller.ExecuteConvert(ConvertSetting, replaceParam);
                    LogWindow.LogMessage($"Handbrakeで変換処理を終了しました。 Status={contoller.Status} File={currentSourceFilePath}", LogWindow.MessageType.Information);

                    if (contoller.Status == Constant.ConvertFileStatus.Completed)
                    {
                        // 正常完了した場合は完了フォルダに移動
                        MoveCompleteFolder(currentSourceFilePath);
                    }
                    else
                    {
                        // 正常に完了しなかった場合は途中のファイルを削除する
                        DeleteNotCompleteFile(replaceParam);
                    }

                    // キャンセルされていたら中断
                    if (IsCancellationRequested || IsCancellationNextRequested) break;
                }
            }

            // イベントを発行
            if (!IsCancellationRequested && !IsCancellationNextRequested)
            {
                OnConvertStateChanged(100, 100, $"{SourceFileList.Count}/{SourceFileList.Count}", "完了");
            }

            contoller = null;
        }

        /// <summary>
        /// 次で変換をキャンセルする
        /// </summary>
        public void CancelNextConvert()
        {
            LogWindow.LogMessage($"次のファイルでキャンセルを受け付けました。", LogWindow.MessageType.Information);
            IsCancellationNextRequested = true;
        }

        /// <summary>
        /// 変換をキャンセルする
        /// </summary>
        public void CancelConvert()
        {
            LogWindow.LogMessage($"キャンセルを受け付けました。", LogWindow.MessageType.Information);
            if (contoller != null && IsCancellationRequested == false)
            {
                contoller.CancelConvert();
            }
            IsCancellationRequested = true;
        }

        /// <summary>
        /// ファイルリストを変更する
        /// </summary>
        /// <param name="sourceFileList"></param>
        public void ChangeSorceFileList(List<string> sourceFileList)
        {
            LogWindow.LogMessage($"変換ファイルリストの変更を受け付けました。", LogWindow.MessageType.Information);
            var index = sourceFileList.IndexOf(currentFileName);
            if (index != -1 && index != currentFileIndex)
            {
                currentFileIndex = index;
            }
            this.SourceFileList = sourceFileList;
        }

        /// <summary>
        /// パラメータ置換用の辞書を作成する
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, string> CreateReplaceParam(string convertSettingName,
                                                            string sourceFilePath,
                                                            string destinationFolder)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>
            {
                ["{CONVERT_SETTING_NAME}"] = convertSettingName,
                ["{SOURCE_FILE_PATH}"] = sourceFilePath,
                ["{SOURCE_FILE_NAME}"] = Path.GetFileName(sourceFilePath),
                ["{SOURCE_FILE_NAME_WITHOUT_EXTENSION}"] = Path.GetFileNameWithoutExtension(sourceFilePath),
                ["{DST_FOLDER}"] = destinationFolder
            };
            return ret;
        }

        /// <summary>
        /// 既に変換済みフォルダが存在するか確認する
        /// </summary>
        /// <param name="filePath"></param>
        protected virtual bool IsAlreadyExistCompleteFolder(Dictionary<string, string> replaceParam)
        {
            var dstFileName = ConvertSetting.GetDestinationFileName(replaceParam);
            if (string.IsNullOrEmpty(dstFileName))
            {
                // 変換後ファイル名が指定されていない場合は判断できないので存在しない扱いとする
                LogWindow.LogMessage($"変換後ファイル名が指定されていないため変換済みチェックはしません。dstFileName={dstFileName}", LogWindow.MessageType.Warning);
                return false;
            }
            else if (File.Exists(Path.Combine(DestinationFolder, dstFileName)))
            {
                LogWindow.LogMessage($"変換後ファイルが既に存在するのでスキップします。File={Path.Combine(DestinationFolder, dstFileName)} ", LogWindow.MessageType.Warning);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 変換が完了しなかった場合に変換後ファイルを削除する
        /// </summary>
        /// <param name="replaceParam"></param>
        protected virtual void DeleteNotCompleteFile(Dictionary<string, string> replaceParam)
        {
            var dstFileName = ConvertSetting.GetDestinationFileName(replaceParam);
            if (string.IsNullOrEmpty(dstFileName))
            {
                // 変換後ファイル名が指定されていない場合は判断できないので存在しない扱いとする
                return;
            }
            var dstFilePath = Path.Combine(DestinationFolder, dstFileName);
            if (File.Exists(dstFilePath))
            {
                // プロセス終了後に開放されるラグ時間があるため、一定時間待機後に未完了ファイルを削除する
                Task.Run(async () =>
                {
                    await Task.Delay(Constant.FileMoveDelayMiliSecond);
                    try
                    {
                        File.Delete(dstFilePath);
                        LogWindow.LogMessage($"未完了ファイルを削除しました。 File={dstFilePath}", LogWindow.MessageType.Information);
                    }
                    catch (IOException ex)
                    {
                        LogWindow.LogMessage($"未完了ファイルの削除に失敗しました。 File={dstFilePath} ex={ex}", LogWindow.MessageType.Error);
                    }
                });
            }
        }

        /// <summary>
        /// 完了フォルダに移動する
        /// </summary>
        /// <param name="filePath"></param>
        protected virtual void MoveCompleteFolder(string currentSourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(CompleteFolder))
            {
                return;
            }

            var compFilePath = Path.Combine(CompleteFolder, Path.GetFileName(currentSourceFilePath));
            if (File.Exists(compFilePath) == false)
            {
                // プロセス終了後に開放されるラグ時間があるため、一定時間待機後移動する
                Task.Run(async () =>
                {
                    await Task.Delay(Constant.FileMoveDelayMiliSecond);
                    try
                    {
                        File.Move(currentSourceFilePath, compFilePath);
                        LogWindow.LogMessage($"完了済みフォルダにファイル移動しました。 {currentSourceFilePath}->{compFilePath}", LogWindow.MessageType.Information);
                    }
                    catch (IOException ex)
                    {
                        LogWindow.LogMessage($"完了済みフォルダへファイル移動に失敗しました。 {currentSourceFilePath}->{compFilePath} ex={ex}", LogWindow.MessageType.Error);
                    }
                });
            }
        }

        /// <summary>
        /// 既に開始している変換が終わるまで待機する
        /// </summary>
        /// <param name="mutex"></param>
        /// <returns></returns>
        private async Task<bool> WaitingPreviosConvert(MutexWrapper mutex)
        {
            int waitCount = 0;
            while (true)
            {
                if (IsCancellationRequested || IsCancellationNextRequested) return false;
                bool createdNew;
                try
                {
                    createdNew = await mutex.WaitOne(Constant.MutexWaitIntervalMiliSecond, false);
                }
                catch (AbandonedMutexException)
                {
                    createdNew = true;
                }
                waitCount++;

                if (createdNew)
                {
                    return true;
                }
                else if (waitCount * Constant.MutexWaitIntervalMiliSecond > Constant.MutexWaitMaxMiliSecond)
                {
                    LogWindow.LogMessage($"変換完了待受の最大実効時間をオーバーしました。中断します。 runTime={waitCount}", LogWindow.MessageType.Error);
                    return false;
                }
                else
                {
                    OnConvertStateChanged(0, 0, $"0/{SourceFileList.Count}", $"Wait {Math.Round((double)waitCount / 60, 0)}min");
                }
            }
        }

        #endregion

        #region "event"

        /// <summary>
        /// コントローラからのイベント受け取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void ConvertStateChanged(object sender, ConvertStateChangedEventArgs e)
        {
            // イベントを発行
            e.AllProgress = (int)(((double)currentFileIndex / SourceFileList.Count) * 100);
            e.AllStatus = $"{currentFileIndex}/{SourceFileList.Count}";
            e.SourceFilePath = SourceFileList[currentFileIndex];
            OnConvertStateChanged(e);
        }

        /// <summary>
        /// イベントを発生させる
        /// </summary>
        /// <param name="allProgress"></param>
        /// <param name="fileProgress"></param>
        /// <param name="allStatus"></param>
        /// <param name="fileStatus"></param>
        protected virtual void OnConvertStateChanged(int allProgress, int fileProgress, string allStatus, string fileStatus)
        {
            var e = new ConvertStateChangedEventArgs
            {
                AllProgress = allProgress,
                AllStatus = allStatus,
                FileProgress = fileProgress,
                FileStatus = fileStatus
            };
            OnConvertStateChanged(e);
        }

        /// <summary>
        /// イベントを発生させる
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnConvertStateChanged(ConvertStateChangedEventArgs e)
        {
            ConvertStateChangedEvent?.Invoke(this, e);
        }

        #endregion

    }
}
