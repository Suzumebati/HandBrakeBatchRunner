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

                    // 変換元が存在しない場合はスキップ
                    if (File.Exists(currentSourceFilePath) == false) continue;

                    // 変換先にすでにファイルが有る場合はスキップ
                    if (IsAlreadyExistCompleteFolder(replaceParam)) continue;

                    // 一個のファイルを変換する
                    await contoller.ExecuteConvert(ConvertSetting, replaceParam);

                    // 完了フォルダに移動
                    if (contoller.Status == Constant.ConvertFileStatus.Completed)
                    {
                        MoveCompleteFolder(currentSourceFilePath);
                    }

                    // キャンセルされていたら中断
                    if (IsCancellationRequested || IsCancellationNextRequested) break;
                }
            }
            
            // イベントを発行
            if (!IsCancellationRequested  && !IsCancellationNextRequested)
            {
                var e = new ConvertStateChangedEventArgs
                {
                    AllProgress = 100,
                    AllStatus = $"{SourceFileList.Count}/{SourceFileList.Count}",
                    FileProgress = 100,
                    FileStatus = "完了"
                };
                OnConvertStateChanged(e);
            }

            contoller = null;
        }

        /// <summary>
        /// 次で変換をキャンセルする
        /// </summary>
        public void CancelNextConvert()
        {
            IsCancellationNextRequested = true;
        }

        /// <summary>
        /// 変換をキャンセルする
        /// </summary>
        public void CancelConvert()
        {
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
                return false;
            }
            else if (File.Exists(Path.Combine(DestinationFolder,dstFileName)))
            {
                return true;
            }
            else
            {
                return false;
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

            var compFilePath = Path.Combine(CompleteFolder,Path.GetFileName(currentSourceFilePath));
            if (File.Exists(compFilePath) == false)
            {
                // プロセス終了後に開放されるラグ時間があるため、一定時間待機後移動する
                Task.Run(async () => {
                    await Task.Delay(Constant.FileMoveDelayMiliSecond);
                    try
                    {
                        File.Move(currentSourceFilePath, compFilePath);
                    }
                    catch(IOException)
                    {

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

                if(createdNew)
                {
                    return true;
                }
                else if(waitCount * Constant.MutexWaitIntervalMiliSecond > Constant.MutexWaitMaxMiliSecond)
                {
                    return false;
                }
                else
                {
                    var e = new ConvertStateChangedEventArgs
                    {
                        AllProgress = 0,
                        AllStatus = $"0/{SourceFileList.Count}",
                        FileProgress = 0,
                        FileStatus = $"Wait {Math.Round((double)waitCount / 60, 0)}min"
                    };
                    OnConvertStateChanged(e);
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
        /// <param name="e"></param>
        protected virtual void OnConvertStateChanged(ConvertStateChangedEventArgs e)
        {
            ConvertStateChangedEvent?.Invoke(this, e);
        }

        #endregion

    }
}
