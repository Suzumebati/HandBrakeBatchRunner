using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

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
        public ObservableCollection<ConvertSettingItem> ConvertSettingList { get; set; } = new ObservableCollection<ConvertSettingItem>();

        /// <summary>
        /// 変換設定値のファイルロード
        /// </summary>
        public void LoadSettings()
        {
            var json = new DataContractJsonSerializer(this.ConvertSettingList.GetType());
            try
            {
                using (var fs = new FileStream(Constant.CONVERT_SETTING_FILE_NAME,
                                               FileMode.Open, 
                                               FileAccess.Read, 
                                               FileShare.ReadWrite,
                                               64 * 1024))
                {
                    this.ConvertSettingList = (ObservableCollection<ConvertSettingItem>)json.ReadObject(fs);
                }
            }
            catch(Exception ex)
            {
                // 例外が起こったらロードしない
            }
        }

        /// <summary>
        /// 変換設定値のファイルセーブ
        /// </summary>
        public void SaveSettings()
        {
            var json = new DataContractJsonSerializer(this.ConvertSettingList.GetType());
            try
            {
                using (var fs = new FileStream(Constant.CONVERT_SETTING_FILE_NAME,
                                               FileMode.OpenOrCreate,
                                               FileAccess.Write,
                                               FileShare.Read,
                                               64 * 1024))
                {
                    json.WriteObject(fs,this.ConvertSettingList);
                }
            }
            catch (Exception ex)
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
            return this.ConvertSettingList.FirstOrDefault(item => item.ConvertSettingName == convertSettingName);
        }

        /// <summary>
        /// 変換設定情報の設定
        /// </summary>
        /// <param name="convertSettingName">変換設定名</param>
        /// <param name="commandTemplate">コマンドラインのテンプレート</param>
        /// <returns></returns>
        public void SetSetting(string convertSettingName, string commandTemplate)
        {
            var item = this.GetSetting(convertSettingName);
            if(item == null)
            {
                item = new ConvertSettingItem();
                this.ConvertSettingList.Add(item);
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
            var item = this.GetSetting(convertSettingName);
            if (item != null)
            {
                this.ConvertSettingList.Remove(item);
            }
        }
    }
}
