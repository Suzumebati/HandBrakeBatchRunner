using System.Collections.Generic;
using System.IO;
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
        /// 変換設定名
        /// </summary>
        public string ConvertSettingName { get; set; }

        /// <summary>
        /// HandBrakeCLIのファイルパス
        /// </summary>
        public string HandBrakeCLIFilePath { get; set; }

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
        /// <param name="convertSettingName"></param>
        /// <param name="handBrakeCLIFilePath"></param>
        public ConvertBatchRunner(List<string> sourceFileList,
                                  string destinationFolder,
                                  string convertSettingName,
                                  string handBrakeCLIFilePath)
        {
            SourceFileList = sourceFileList;
            DestinationFolder = destinationFolder;
            ConvertSettingName = convertSettingName;
            HandBrakeCLIFilePath = handBrakeCLIFilePath;
        }

        #endregion

        #region "method"

        /// <summary>
        /// 複数のファイルをバッチで変換実行する
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="cliPath"></param>
        /// <returns></returns>
        public async Task BatchConvert()
        {
            contoller = new ConvertProcessController(HandBrakeCLIFilePath);
            contoller.ConvertStateChangedEvent += new ConvertStateChangedHandler(ConvertStateChanged);

            for (currentFileIndex = 0; currentFileIndex < SourceFileList.Count; currentFileIndex++)
            {
                string currentSourceFilePath = SourceFileList[currentFileIndex];
                await contoller.ExecuteConvert(ConvertSettingName,
                                               currentSourceFilePath,
                                               CreateReplaceData(ConvertSettingName,
                                                                 currentSourceFilePath,
                                                                 DestinationFolder));
                if (IsCancellationRequested) break;
            }

            // イベントを発行
            if(IsCancellationRequested == false)
            {
                var e = new ConvertStateChangedEventArgs();
                e.AllProgress = 100;
                e.AllStatus = $"{SourceFileList.Count}/{SourceFileList.Count}";
                e.FileProgress = 100;
                e.FileStatus = string.Empty;
                OnConvertStateChanged(e);
            }
        }

        /// <summary>
        /// 変換をキャンセルする
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="cliPath"></param>
        /// <returns></returns>
        public void CancelConvert()
        {
            if (contoller != null && IsCancellationRequested == false)
            {
                contoller.CancelConvert();
            }
            IsCancellationRequested = true;
        }
        
        /// <summary>
        /// パラメータ置換用の辞書を作成する
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="sourceFilePath"></param>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public Dictionary<string, string> CreateReplaceData(string convertSettingName,
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

        #endregion

        #region "event"

        /// <summary>
        /// コントローラからのイベント受け取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConvertStateChanged(object sender, ConvertStateChangedEventArgs e)
        {
            // イベントを発行
            e.AllProgress = (int)(((double)currentFileIndex / SourceFileList.Count) * 100);
            e.AllStatus = $"{currentFileIndex}/{SourceFileList.Count}";
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
