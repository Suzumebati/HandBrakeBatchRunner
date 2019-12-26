using System;
using System.Linq;
using System.Windows;
using HandBrakeBatchRunner.Convert;
using HandBrakeBatchRunner.Setting;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace HandBrakeBatchRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConvertBatchRunner runner;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        #region Event

        /// <summary>
        /// ロードイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 設定読込
            ConvertSettingManager.Current.LoadSettings();

            // バインド
            SettingCombo.ItemsSource = ConvertSettingManager.Current.ConvertSettingList;
        }

        /// <summary>
        /// クローズ中イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 設定保存
            ConvertSettingManager.Current.SaveSettings();
        }

        /// <summary>
        /// ファイルリストへのドラッグ時のイベントハンドリング
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            // ドラッグされたファイルによって判断
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // ファイルのドラッグの場合は許可する
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                // ファイル以外の場合は許可しない
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        /// <summary>
        /// ファイルリストへドロップされたファイルをリストに追加する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            // ドロップされたファイルパスを取得
            var dropFileArray = (string[])e.Data.GetData(DataFormats.FileDrop);
            // ファイルをすべてファイルリストに追加
            dropFileArray.ToList<string>().ForEach(item => fileListBox.Items.Add(item));
        }
               
        private void ConvertStart_Click(object sender, RoutedEventArgs e)
        {
            ConvertStartButton.IsEnabled = false;
            ConvertCancelButton.IsEnabled = true;
            var setting = SettingCombo.SelectedItem as ConvertSettingItem;
            if (setting == null)
            {
                return;
            }

            runner = new ConvertBatchRunner(fileListBox.Items.OfType<string>().ToList(),
                                            DstFolderTextBox.Text,
                                            setting.ConvertSettingName,
                                            @"C:\soft\HandBrakeCLI-1.2.2-win-x86_64\HandBrakeCLI.exe");

            runner.OutputDataReceivedEvent += new OutputDataReceivedHandler(OutputDataReceived);
            
            // 一括変換開始
            var convTask = runner.BatchConvert();

            // タスク終了後に画面状態を変更する
            convTask.ContinueWith(task =>
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    ConvertStartButton.IsEnabled = true;
                    ConvertCancelButton.IsEnabled = false;
                    AllProgress.Value = 100;
                    AllStatus.Content = $"{fileListBox.Items.Count}/{fileListBox.Items.Count}";
                    FileProgress.Value = 100;
                    FileStatus.Content = "完了";
                }));
            });
        }

        private void ConvertCancelButton_Click(object sender, RoutedEventArgs e)
        {
            //TODOキャンセルロジック
        }

        /// <summary>
        /// 変換ステータス変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OutputDataReceived(object sender, ConvertStateChangedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                AllProgress.Value = e.AllProgress;
                AllStatus.Content = e.AllStatus;
                FileProgress.Value = e.FileProgress;
                FileStatus.Content = e.FileStatus;
            }));
        }

        private void ClearFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LoadFileList_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 設定ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertSetting_Click(object sender, RoutedEventArgs e)
        {
            // 設定ウインドウをモーダル表示する
            var win = new SettingWindow();
            win.ShowDialog();
        }

        /// <summary>
        /// 変換後フォルダ選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDstFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog("変換ファイル格納フォルダを選んでください。"))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    DstFolderTextBox.Text = dlg.FileName;
                }
            }
        }

        /// <summary>
        /// 変換完了フォルダ選択ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectCompleteFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog("変換後格納フォルダを選んでください。"))
            {
                dlg.IsFolderPicker = true;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    CompleteFolderTextBox.Text = dlg.FileName;
                }
            }
        }

        #endregion

    }
}
