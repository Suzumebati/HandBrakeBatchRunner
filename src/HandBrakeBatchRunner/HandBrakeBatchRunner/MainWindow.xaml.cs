// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;
using System.IO;
using System.Linq;
using System.Windows;
using HandBrakeBatchRunner.Convert;
using HandBrakeBatchRunner.Setting;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Taskbar;

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
            // 設定読込
            ConvertSettingManager.Current.LoadSettings();
            // コンポーネント初期化
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
            dropFileArray.ToList<string>().ForEach(item => SourceFileListBox.Items.Add(item));
        }
               
        /// <summary>
        /// 変換開始ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertStart_Click(object sender, RoutedEventArgs e)
        {
            // 変換ファイルがない場合は終了
            var setting = SettingCombo.SelectedItem as ConvertSettingItem;
            if (setting == null || SourceFileListBox.Items.Count == 0)
            {
                return;
            }

            // 状態の変更
            SetButtonStatus(true);
            SetStatus(0, string.Empty, 0, string.Empty,null);
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);

            // 変換クラス作成
            runner = new ConvertBatchRunner(SourceFileListBox.Items.OfType<string>().ToList(),
                                            DestinationFolderTextBox.Text,
                                            CompleteFolderTextBox.Text,
                                            setting.ConvertSettingName,
                                            ConvertSettingManager.Current.ConvertSettingBody.HandBrakeCLIFilePath);
            runner.ConvertStateChangedEvent += new ConvertStateChangedHandler(ConvertStateChanged);
            
            // 一括変換開始
            var convTask = runner.BatchConvert();

            // タスク終了後に画面状態を変更する
            convTask.ContinueWith(task =>
            {
                SetButtonStatus(false);
                if (runner !=null && !runner.IsCancellationRequested && !runner.IsCancellationNextRequested)
                {
                    SetStatus(100, $"{SourceFileListBox.Items.Count}/{SourceFileListBox.Items.Count}", 100, "完了", null);
                }
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                runner = null;
            });
        }

        /// <summary>
        /// 次でキャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertCancelNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (runner != null && runner.IsCancellationNextRequested == false)
            {
                runner.CancelNextConvert();
                ConvertCancelNextButton.IsEnabled = false;
                ConvertCancelButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// キャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertCancelButton_Click(object sender, RoutedEventArgs e)
        {
            if(runner != null && runner.IsCancellationRequested == false)
            {
                runner.CancelConvert();
                ConvertCancelButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// 変換ステータス変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConvertStateChanged(object sender, ConvertStateChangedEventArgs e)
        {
            SetStatus(e);
        }

        /// <summary>
        /// クリアボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearFileList_Click(object sender, RoutedEventArgs e)
        {
            SourceFileListBox.Items.Clear();
        }

        /// <summary>
        /// 削除ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            var selectItem = SourceFileListBox.SelectedItem;
            if (selectItem != null)
            {
                SourceFileListBox.Items.Remove(selectItem);
            }
        }

        /// <summary>
        /// ファイルリストの保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveFileList_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonSaveFileDialog("ファイルリストの保存先を選んでください。"))
            {
                dlg.Filters.Add(new CommonFileDialogFilter("HandBrakeBatchRunner File List", "hfl"));
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    using (var sr= new StreamWriter(dlg.FileName))
                    {
                        foreach(string item in SourceFileListBox.Items)
                        {
                            sr.WriteLine(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// ファイルリストの読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadFileList_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new CommonOpenFileDialog("ファイルリストの読込先を選んでください。"))
            {
                dlg.Filters.Add(new CommonFileDialogFilter("HandBrakeBatchRunner File List", "hfl"));
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    using (var sr = new StreamReader(dlg.FileName))
                    {
                        while (sr.Peek() > -1)
                        {
                            var line = sr.ReadLine();
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            SourceFileListBox.Items.Add(line);
                        }
                    }
                }
            }
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
                if (!string.IsNullOrEmpty(DestinationFolderTextBox.Text)) dlg.InitialDirectory = DestinationFolderTextBox.Text;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    DestinationFolderTextBox.Text = dlg.FileName;
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
            using (var dlg = new CommonOpenFileDialog("変換完了後格納フォルダを選んでください。"))
            {
                dlg.IsFolderPicker = true;
                if(!string.IsNullOrEmpty(CompleteFolderTextBox.Text)) dlg.InitialDirectory = CompleteFolderTextBox.Text;
                if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    CompleteFolderTextBox.Text = dlg.FileName;
                }
            }
        }

        #endregion

        #region "Method"

        /// <summary>
        /// ボタン状態を変更する
        /// </summary>
        /// <param name="isStart"></param>
        private void SetButtonStatus(bool isStart)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    SetButtonStatus(isStart);
                }));
                return;
            }

            if(isStart)
            {
                ConvertStartButton.IsEnabled = false;
                ConvertCancelNextButton.IsEnabled = true;
                ConvertCancelButton.IsEnabled = true;
            }
            else
            {
                ConvertStartButton.IsEnabled = true;
                ConvertCancelNextButton.IsEnabled = false;
                ConvertCancelButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// イベント引数をもとに状態を変更する
        /// </summary>
        /// <param name="e"></param>
        private void SetStatus(ConvertStateChangedEventArgs e)
        {
            SetStatus(e.AllProgress, e.AllStatus, e.FileProgress, e.FileStatus,e.SourceFilePath);
        }

        /// <summary>
        /// 状態を変更する
        /// </summary>
        /// <param name="allProgress"></param>
        /// <param name="allStatus"></param>
        /// <param name="fileProgress"></param>
        /// <param name="fileStatus"></param>
        private void SetStatus(int allProgress,string allStatus,int fileProgress,string fileStatus,string sourceFilePath)
        {
            if(Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    SetStatus(allProgress, allStatus, fileProgress, fileStatus, sourceFilePath);
                }));
                return;
            }

            if (allProgress != 0)
            {
                AllProgress.Value = allProgress;
                TaskbarManager.Instance.SetProgressValue(allProgress, 100);
            }
            AllStatus.Content = allStatus;
            if(fileProgress != 0) FileProgress.Value = fileProgress;
            FileStatus.Content = fileStatus;
            
            var selectItem = SourceFileListBox.SelectedItem as string;
            if (sourceFilePath != null && (selectItem == null || selectItem != sourceFilePath))
            {
                SourceFileListBox.SelectedItem = sourceFilePath;
            }
        }

        #endregion

    }
}
