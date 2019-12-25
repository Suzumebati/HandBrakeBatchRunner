using HandBrakeBatchRunner.Convert;
using Microsoft.Win32;
using System;
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
            var dialog = new OpenFileDialog();
            dialog.Title = "HandBrakeCLIを選択してください。";
            dialog.Filter = "実行ファイル(*.exe)|*.exe";
            if (dialog.ShowDialog() == true)
            {
                this.HandBrakeCLIFilePath.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// 設定追加ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSettingButton_Click(object sender, RoutedEventArgs e)
        {
            ConvertSettingManager.Current.SetSetting(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
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
            }
            else
            {
                this.CommandTemplateText.Text = string.Empty;
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
            }
        }
    }
}
