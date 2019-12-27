using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace HandBrakeBatchRunner.Setting
{
    /// <summary>
    /// 変換設定のマネージャクラス
    /// </summary>
    public class ConvertSettingManager
    {
        /// <summary>
        /// インスタンス取得(シングルトン)
        /// </summary>
        public static ConvertSettingManager Current { get; } = new ConvertSettingManager();

        /// <summary>
        /// セッティングリスト(Comboboxへバインド用)
        /// </summary>
        public ObservableCollection<ConvertSettingItem> ConvertSettingList { get; set; } = new ObservableCollection<ConvertSettingItem>();

        /// <summary>
        /// 変換設定値のファイルロード
        /// </summary>
        public void LoadSettings()
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(ConvertSettingList.GetType());
            try
            {
                using (FileStream fs = new FileStream(Constant.ConvertSettingFileName,
                                               FileMode.Open,
                                               FileAccess.Read,
                                               FileShare.ReadWrite,
                                               64 * 1024))
                {
                    ConvertSettingList = (ObservableCollection<ConvertSettingItem>)json.ReadObject(fs);
                }
            }
            catch
            {
                // 例外が起こったらロードしない
            }
        }

        /// <summary>
        /// 変換設定値のファイルセーブ
        /// </summary>
        public void SaveSettings()
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(ConvertSettingList.GetType());
            try
            {
                using (FileStream fs = new FileStream(Constant.ConvertSettingFileName,
                                               FileMode.Create,
                                               FileAccess.Write,
                                               FileShare.Read,
                                               64 * 1024))
                {
                    json.WriteObject(fs, ConvertSettingList);
                }
            }
            catch
            {
                // 例外が起こったらセーブしない
            }
        }

        /// <summary>
        /// 変換設定情報の取得
        /// </summary>
        /// <param name="convertSettingName"></param>
        /// <returns></returns>
        public ConvertSettingItem GetSetting(string convertSettingName)
        {
            return ConvertSettingList.FirstOrDefault(item => item.ConvertSettingName == convertSettingName);
        }

        /// <summary>
        /// 変換設定情報の設定
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <param name="commandTemplate">コマンドラインのテンプレート</param>
        /// <returns></returns>
        public void SetSetting(string convertSettingName, string commandTemplate)
        {
            ConvertSettingItem item = GetSetting(convertSettingName);
            if (item == null)
            {
                item = new ConvertSettingItem();
                ConvertSettingList.Add(item);
            }
            item.ConvertSettingName = convertSettingName;
            item.CommandLineTemplate = commandTemplate;
        }

        /// <summary>
        /// 変換設定情報の削除
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <returns></returns>
        public void DeleteSetting(string convertSettingName)
        {
            ConvertSettingItem item = GetSetting(convertSettingName);
            if (item != null)
            {
                ConvertSettingList.Remove(item);
            }
        }
    }
}