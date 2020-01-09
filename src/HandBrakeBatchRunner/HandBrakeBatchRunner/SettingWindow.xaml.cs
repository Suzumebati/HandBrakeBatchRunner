// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using HandBrakeBatchRunner.Setting;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

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
            ConvertSettingListBox.ItemsSource = ConvertSettingManager.Current.ConvertSettingList;
        }


        /// <summary>
        /// フォームクローズ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ConvertSettingListBox.SelectedIndex != -1)
            {
                // 設定が選ばれている場合は設定を保存
                var selectedSetting = ConvertSettingListBox.SelectedItem as ConvertSettingItem;
                selectedSetting.CommandLineTemplate = CommandTemplateText.Text;
                selectedSetting.DestinationFileNameTemplate = DestinationFileTemplateTextBox.Text;
            }
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
                if (!string.IsNullOrWhiteSpace(HandBrakeCLIFilePath.Text))
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
        /// 監視フォルダ選択ボタンクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WatchFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog("HandBrakeCLIを選択してください。"))
            {
                dlg.IsFolderPicker = true;
                if (!string.IsNullOrWhiteSpace(WatchFolderTextBox.Text))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(WatchFolderTextBox.Text);
                }
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    WatchFolderTextBox.Text = dlg.FileName;
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
            if (ConvertSettingListBox.SelectedIndex != -1)
            {
                var selectedSetting = ConvertSettingListBox.SelectedItem as ConvertSettingItem;
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
            var beforeSelection = e.RemovedItems.OfType<ConvertSettingItem>();
            var afterSelection = e.AddedItems.OfType<ConvertSettingItem>();

            if (beforeSelection.Any())
            {
                if (afterSelection.Any())
                {
                    var beforeSetting = beforeSelection.First();
                    beforeSetting.CommandLineTemplate = CommandTemplateText.Text;
                    beforeSetting.DestinationFileNameTemplate = DestinationFileTemplateTextBox.Text;
                    var afterSetting = afterSelection.First();
                    CommandTemplateText.Text = afterSetting.CommandLineTemplate;
                    DestinationFileTemplateTextBox.Text = afterSetting.DestinationFileNameTemplate;
                }
                else
                {
                    var beforeSetting = beforeSelection.First();
                    beforeSetting.CommandLineTemplate = CommandTemplateText.Text;
                    beforeSetting.DestinationFileNameTemplate = DestinationFileTemplateTextBox.Text;
                    CommandTemplateText.Text = string.Empty;
                    DestinationFileTemplateTextBox.Text = string.Empty;
                }
            }
            else
            {
                if (afterSelection.Any())
                {
                    var afterSetting = afterSelection.First();
                    CommandTemplateText.Text = afterSetting.CommandLineTemplate;
                    DestinationFileTemplateTextBox.Text = afterSetting.DestinationFileNameTemplate;
                }
                else
                {
                    CommandTemplateText.Text = string.Empty;
                    DestinationFileTemplateTextBox.Text = string.Empty;
                }

            }
        }

        /// <summary>
        /// コマンドテンプレートのフォーカスイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandTemplateText_GotFocus(object sender, RoutedEventArgs e)
        {
            CommandTemplateSupplementPopUp.IsOpen = true;
        }

        /// <summary>
        /// コマンドテンプレートのフォーカスアウトイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandTemplateText_LostFocus(object sender, RoutedEventArgs e)
        {
            CommandTemplateSupplementPopUp.IsOpen = false;
        }

        /// <summary>
        /// ポップアップへのフォーカスイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandTemplateSupplementPopUp_GotFocus(object sender, RoutedEventArgs e)
        {
            CommandTemplateSupplementPopUp.IsOpen = true;
        }

        /// <summary>
        /// ポップアップのフォーカスアウトイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CommandTemplateSupplementPopUp_LostFocus(object sender, RoutedEventArgs e)
        {
            CommandTemplateSupplementPopUp.IsOpen = false;
        }

        /// <summary>
        /// ウインドウの非アクティブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Deactivated(object sender, System.EventArgs e)
        {
            CommandTemplateSupplementPopUp.IsOpen = false;
        }
    }
}
