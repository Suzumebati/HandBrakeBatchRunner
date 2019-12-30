// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using HandBrakeBatchRunner.Setting;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace HandBrakeBatchRunner
{

    /// <summary>
    /// SettingWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow : Window
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingWindow()
        {
            InitializeComponent();

            // リストボックスにバインド
            this.ConvertSettingListBox.ItemsSource = ConvertSettingManager.Current.ConvertSettingList;
        }

        /// <summary>
        /// ファイル選択ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
             using (var dlg = new CommonOpenFileDialog("HandBrakeCLIを選択してください。"))
            {
                dlg.Filters.Add(new CommonFileDialogFilter("実行ファイル(*.exe)", "*.exe"));
                if (!string.IsNullOrWhiteSpace(this.HandBrakeCLIFilePath.Text))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(HandBrakeCLIFilePath.Text);
                }
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    HandBrakeCLIFilePath.Text = dlg.FileName;
                }
            }
        }

        /// <summary>
        /// 設定追加ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewSettingTextBox.Text) == false && ConvertSettingManager.Current.GetSetting(NewSettingTextBox.Text) == null)
            {
                ConvertSettingManager.Current.SetSetting(NewSettingTextBox.Text, string.Empty);
            }
            NewSettingTextBox.Text = string.Empty;
        }

        /// <summary>
        /// 設定削除ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConvertSettingListBox.SelectedIndex != -1)
            {
                var selectedSetting = this.ConvertSettingListBox.SelectedItem as ConvertSettingItem;
                ConvertSettingManager.Current.DeleteSetting(selectedSetting.ConvertSettingName);
            }
        }

        /// <summary>
        /// 選択変更時のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertSettingListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ConvertSettingListBox.SelectedIndex != -1)
            {
                var selectedSetting = this.ConvertSettingListBox.SelectedItem as ConvertSettingItem;
                this.CommandTemplateText.Text = selectedSetting.CommandLineTemplate;
                this.DestinationFileTemplateTextBox.Text = selectedSetting.DestinationFileNameTemplate;
            }
            else
            {
                this.CommandTemplateText.Text = string.Empty;
                this.DestinationFileTemplateTextBox.Text = string.Empty;
            }
        }

        /// <summary>
        /// 設定更新ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertSettingModify_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConvertSettingListBox.SelectedIndex != -1)
            {
                var selectedSetting = this.ConvertSettingListBox.SelectedItem as ConvertSettingItem;
                selectedSetting.CommandLineTemplate = this.CommandTemplateText.Text;
                selectedSetting.DestinationFileNameTemplate = this.DestinationFileTemplateTextBox.Text;
            }
        }
    }
}
