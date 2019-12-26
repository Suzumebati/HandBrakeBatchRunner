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
        /// <summary>
        /// アウトプットデータイベント
        /// </summary>
        public event OutputDataReceivedHandler OutputDataReceivedEvent;

        public List<string> SourceFileList { get; set; }
        public string DstFolder { get; set; }
        public string ConvertSettingName { get; set; }
        public string CliPath { get; set; }

        private int currentFileIndex = 0;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="dstFolder"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="cliPath"></param>
        public ConvertBatchRunner(List<string> sourceFileList,
                                  string dstFolder,
                                  string convertSettingName,
                                  string cliPath)
        {
            SourceFileList = sourceFileList;
            DstFolder = dstFolder;
            ConvertSettingName = convertSettingName;
            CliPath = cliPath;
        }

        /// <summary>
        /// 複数のファイルをバッチで変換実行する
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="cliPath"></param>
        /// <returns></returns>
        public async Task BatchConvert()
        {
            ConvertProcessController contoller = new ConvertProcessController(CliPath);
            contoller.OutputDataReceivedEvent += new OutputDataReceivedHandler(OutputDataReceived);

            for (currentFileIndex = 0; currentFileIndex < SourceFileList.Count; currentFileIndex++)
            {
                string filePath = SourceFileList[currentFileIndex];
                await contoller.ExecuteConvert(ConvertSettingName,
                                               filePath,
                                               CreateReplaceData(ConvertSettingName,
                                                                 filePath,
                                                                 DstFolder));
            }

            // イベントを発行
            var e = new ConvertStateChangedEventArgs();
            e.AllProgress = 100;
            e.AllStatus = $"{SourceFileList.Count}/{SourceFileList.Count}";
            e.FileProgress = 100;
            e.FileStatus = string.Empty;
            OnOutputDataReceived(e);
        }

        /// <summary>
        /// コントローラからのイベント受け取り
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OutputDataReceived(object sender, ConvertStateChangedEventArgs e)
        {
            // イベントを発行
            e.AllProgress = (int)currentFileIndex/SourceFileList.Count*100;
            e.AllStatus = $"{currentFileIndex}/{SourceFileList.Count}";
            OnOutputDataReceived(e);
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
        /// パラメータ置換用の辞書を作成する
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Dictionary<string, string> CreateReplaceData(string convertSettingName,
                                                            string filePath,
                                                            string dstFolder)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>
            {
                ["{CONVERT_SETTING_NAME}"] = convertSettingName,
                ["{SOURCE_FILE_PATH}"] = filePath,
                ["{SOURCE_FILE_NAME}"] = Path.GetFileName(filePath),
                ["{SOURCE_FILE_NAME_WITHOUT_EXTENSION}"] = Path.GetFileNameWithoutExtension(filePath),
                ["{DST_FOLDER}"] = dstFolder
            };
            return ret;
        }
    }
}
