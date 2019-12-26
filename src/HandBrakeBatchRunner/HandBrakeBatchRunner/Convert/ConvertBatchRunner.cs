using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 複数のファイルをバッチで変換実行する
    /// </summary>
    internal class ConvertBatchRunner
    {
        /// <summary>
        /// 複数のファイルをバッチで変換実行する
        /// </summary>
        /// <param name="sourceFileList"></param>
        /// <param name="convertSettingName"></param>
        /// <param name="cliPath"></param>
        /// <returns></returns>
        public async Task BatchConvert(List<string> sourceFileList, string convertSettingName, string cliPath)
        {
            ConvertController contoller = new ConvertController(cliPath);
            foreach (string filePath in sourceFileList)
            {
                await contoller.ExecuteConvert(convertSettingName, filePath, CreateReplaceData(convertSettingName, filePath));
            }
        }

        /// <summary>
        /// パラメータ置換用の辞書を作成する
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public Dictionary<string, string> CreateReplaceData(string convertSettingName, string filePath)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>
            {
                ["{CONVERT_SETTING_NAME}"] = convertSettingName,
                ["{SOURCE_FILE_PATH}"] = filePath,
                ["{SOURCE_FILE_NAME}"] = Path.GetFileName(filePath),
                ["{SOURCE_FILE_NAME_WITHOUT_EXTENSION}"] = Path.GetFileNameWithoutExtension(filePath)
            };
            return ret;
        }
    }
}
