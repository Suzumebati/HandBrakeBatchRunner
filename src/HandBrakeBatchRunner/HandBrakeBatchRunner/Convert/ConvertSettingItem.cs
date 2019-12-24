namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 変換設定アイテム
    /// </summary>
    public class ConvertSettingItem
    {
        /// <summary>
        /// 変換設定名
        /// </summary>
        public string ConvertSettingName { get; set; }

        /// <summary>
        /// コマンドラインテンプレート
        /// </summary>
        public string CommandLineTemplate { get; set; }
    }
}
