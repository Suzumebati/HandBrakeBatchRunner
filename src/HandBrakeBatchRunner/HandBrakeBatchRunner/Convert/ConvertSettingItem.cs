using System.Runtime.Serialization;

namespace HandBrakeBatchRunner.Convert
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
    }
}
