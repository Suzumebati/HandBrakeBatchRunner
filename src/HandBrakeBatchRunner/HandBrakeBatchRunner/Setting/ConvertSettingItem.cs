using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HandBrakeBatchRunner.Setting
{
    /// <summary>
    /// 変換設定アイテム
    /// </summary>
    [DataContract]
    public class ConvertSettingItem
    {
        /// <summary>
        /// 変換設定名
        /// </summary>
        [DataMember]
        public string ConvertSettingName { get; set; }

        /// <summary>
        /// コマンドラインテンプレート
        /// </summary>
        [DataMember]
        public string CommandLineTemplate { get; set; }

        /// <summary>
        /// コマンドラインパラメータを取得
        /// </summary>
        /// <param name="replaceWord"></param>
        /// <returns></returns>
        public string GetCommandLineParameter(Dictionary<string, string> replaceWord)
        {
            string ret = CommandLineTemplate;
            foreach (KeyValuePair<string, string> rep in replaceWord)
            {
                ret = ret.Replace(rep.Key, rep.Value);
            }
            return ret;
        }
    }
}