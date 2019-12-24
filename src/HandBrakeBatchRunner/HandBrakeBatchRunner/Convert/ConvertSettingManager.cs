using System.Collections.ObjectModel;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 変換設定のマネージャクラス
    /// </summary>
    internal class ConvertSettingManager
    {
        /// <summary>
        /// セッティングリスト(Comboboxへバインド用)
        /// </summary>
        public ObservableCollection<string> ConvertSettingList { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// 変換設定値のファイルロード
        /// </summary>
        public void LoadSettings()
        {
            //TODO
        }

        /// <summary>
        /// 変換設定値のファイルセーブ
        /// </summary>
        public void SaveSettings()
        {
            //TODO
        }

        /// <summary>
        /// 変換設定情報の取得
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <returns></returns>
        public ConvertSettingItem GetSetting(string convertSettingName)
        {
            //TODO
            return null;
        }

        /// <summary>
        /// 変換設定情報の取得
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <param name="commandTemplate">コマンドラインのテンプレート</param>
        /// <returns></returns>
        public void AddSetting(string convertSettingName, string commandTemplate)
        {
            //TODO
        }

        /// <summary>
        /// 変換設定情報の変更
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <param name="commandTemplate">コマンドラインのテンプレート</param>
        /// <returns></returns>
        public void ChangeSetting(string convertSettingName, string commandTemplate)
        {
            //TODO
        }

        /// <summary>
        /// 変換設定情報の削除
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <returns></returns>
        public void DeleteSetting(string convertSettingName)
        {
            //TODO
        }
    }
}
