// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace HandBrakeBatchRunner.Setting
{
    /// <summary>
    /// 変換設定アイテム
    /// </summary>
    [DataContract]
    public class ConvertSettingBody
    {
        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// HandBrakeCLIのファイルパス
        /// </summary>
        [DataMember]
        private string _HandBrakeCLIFilePath;
        public string HandBrakeCLIFilePath
        {
            get { return _HandBrakeCLIFilePath; }
            set { SetProperty(ref _HandBrakeCLIFilePath, value); }
        }

        /// <summary>
        /// 選んでいる変換設定
        /// </summary>
        [DataMember]
        private string _ChoiceConvertSettingName;
        public string ChoiceConvertSettingName
        {
            get { return _ChoiceConvertSettingName; }
            set { SetProperty(ref _ChoiceConvertSettingName, value); }
        }

        /// <summary>
        /// 選んでいる変換元フォルダ設定
        /// </summary>
        [DataMember]
        private string _ChoiceDestinationFolder;
        public string ChoiceDestinationFolder
        {
            get { return _ChoiceDestinationFolder; }
            set { SetProperty(ref _ChoiceDestinationFolder, value); }
        }

        /// <summary>
        /// 監視フォルダ設定
        /// </summary>
        [DataMember]
        private string _WatchFolder;
        public string WatchFolder
        {
            get { return _WatchFolder; }
            set { SetProperty(ref _WatchFolder, value); }
        }

        /// <summary>
        /// 監視ファイルパターン
        /// </summary>
        [DataMember]
        private string _WatchPattern;
        public string WatchPattern
        {
            get { return _WatchPattern; }
            set { SetProperty(ref _WatchPattern, value); }
        }

        /// <summary>
        /// 自動追加が有効かどうか
        /// </summary>
        [DataMember]
        private bool _EnableAutoAdd;
        public bool EnableAutoAdd
        {
            get { return _EnableAutoAdd; }
            set { SetProperty(ref _EnableAutoAdd, value); }
        }

        /// <summary>
        /// 自動変換が有効かどうか
        /// </summary>
        [DataMember]
        private bool _EnableAutoConvert;
        public bool EnableAutoConvert
        {
            get { return _EnableAutoConvert; }
            set { SetProperty(ref _EnableAutoConvert, value); }
        }

        /// <summary>
        /// 選んでいる変換後フォルダ設定
        /// </summary>
        [DataMember]
        private string _ChoiceCompleteFolder;
        public string ChoiceCompleteFolder
        {
            get { return _ChoiceCompleteFolder; }
            set { SetProperty(ref _ChoiceCompleteFolder, value); }
        }

        /// <summary>
        /// プロパティ変更イベント実行
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        private void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
