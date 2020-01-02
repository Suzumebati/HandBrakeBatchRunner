// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

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
        /// 変換後ファイル名テンプレート
        /// </summary>
        [DataMember]
        public string DestinationFileNameTemplate { get; set; }

        /// <summary>
        /// HandBrake用のコマンド引数を取得する
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

        /// <summary>
        /// 変換後ファイル名を取得する
        /// </summary>
        /// <param name="replaceWord"></param>
        /// <returns></returns>
        public string GetDestinationFileName(Dictionary<string, string> replaceWord)
        {
            string ret = DestinationFileNameTemplate;
            if (string.IsNullOrWhiteSpace(ret))
            {
                return null;
            }

            foreach (KeyValuePair<string, string> rep in replaceWord)
            {
                ret = ret.Replace(rep.Key, rep.Value);
            }
            return ret;
        }
    }
}