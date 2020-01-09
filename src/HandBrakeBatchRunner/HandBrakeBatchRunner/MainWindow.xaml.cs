// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using HandBrakeBatchRunner.Convert;
using HandBrakeBatchRunner.FileWatcher;
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
        /// <summary>
        /// バッチ変換クラス
        /// </summary>
        private ConvertBatchRunner runner;

        /// <summary>
        /// 設定マネージャ
        /// </summary>
        private ConvertSettingManager settingManager = ConvertSettingManager.Current;

        /// <summary>
        /// 監視クラス
        /// </summary>
        private ConvertFileWatcher watcher = new ConvertFileWatcher();

        /// <summary>
        /// ログウィンドウ
        /// </summary>
        private LogWindow logWin = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            // 設定読込
            settingManager.LoadSettings();
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
            SettingCombo.ItemsSource = settingManager.ConvertSettingList;

            // 監視フォルダ機能の有効化
            watcher.FileAddedEvent += new FileAddedHandler(FileAdded);
            if (settingManager.ConvertSettingBody.EnableAutoAdd &&
                string.IsNullOrWhiteSpace(settingManager.ConvertSettingBody.WatchFolder) == false)
            {
                watcher.Start(settingManager.ConvertSettingBody.WatchFolder, settingManager.ConvertSettingBody.WatchPattern);
            }
        }

        /// <summary>
        /// クローズ中イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // 変換途中の場合は強制終了する
            if (runner != null) runner.CancelConvert();

            // 設定保存
            settingManager.SaveSettings();

            // フォルダ監視の終了
            watcher.Dispose();
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
            SourceFileListBox.ScrollIntoView(dropFileArray.Last<string>());

            if (runner != null)
            {
                runner.ChangeSorceFileList(SourceFileListBox.Items.OfType<string>().ToList());
            }
        }

        /// <summary>
        /// キーダウンイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SourceFileListBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Deleteキーの場合は選択状態のファイルを削除
                DeleteFile_Click(sender, null);
            }
            else if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                // Ctrl+Aキーの場合は全選択と全解除をトグル動作
                if (SourceFileListBox.SelectedItems.Count > 0)
                {
                    SourceFileListBox.SelectedItems.Clear();
                }
                else
                {
                    SourceFileListBox.SelectAll();
                }
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                // Ctrl+Cキーの場合は選択したファイルパスをクリップボードにコピー
                var stb = new StringBuilder();
                foreach (string item in SourceFileListBox.SelectedItems)
                {
                    stb.AppendLine(item);
                }
                Clipboard.SetText(stb.ToString());
            }
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                // Ctrl+Vキーの場合は選択したファイルパスをファイルリストに追加
                if (Clipboard.ContainsText())
                {
                    using (var sr = new StringReader(Clipboard.GetText()))
                    {
                        while (sr.Peek() > -1)
                        {
                            SourceFileListBox.Items.Add(sr.ReadLine());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 変換開始ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConvertStart_Click(object sender, RoutedEventArgs e)
        {
            // 入力チェック
            if (InputCheck() == false) return;

            var setting = SettingCombo.SelectedItem as ConvertSettingItem;

            // 状態の変更
            SetButtonStatus(true);
            SetStatus(0, string.Empty, 0, string.Empty, string.Empty);
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);

            // 変換クラス作成
            runner = new ConvertBatchRunner(SourceFileListBox.Items.OfType<string>().ToList(),
                                            DestinationFolderTextBox.Text,
                                            CompleteFolderTextBox.Text,
                                            setting.ConvertSettingName,
                                            settingManager.ConvertSettingBody.HandBrakeCLIFilePath);
            runner.ConvertStateChangedEvent += new ConvertStateChangedHandler(ConvertStateChanged);
            if (logWin != null) runner.ConvertStateChangedEvent += new ConvertStateChangedHandler(logWin.ConvertStateChanged);


            // 一括変換開始
            await runner.BatchConvert();

            // タスク終了後に画面状態を変更する
            SetButtonStatus(false);
            if (runner?.IsCancellationRequested == false && runner?.IsCancellationNextRequested == false)
            {
                SetStatus(100, $"{SourceFileListBox.Items.Count}/{SourceFileListBox.Items.Count}", 100, "完了", string.Empty);
            }
            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            runner = null;
        }

        /// <summary>
        /// 次でキャンセルボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConvertCancelNextButton_Click(object sender, RoutedEventArgs e)
        {
            if (runner?.IsCancellationNextRequested == false)
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
            if (runner?.IsCancellationRequested == false)
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
        /// ファイル追加イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void FileAdded(object sender, FileAddedEventArgs e)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    FileAdded(sender, e);
                }));
                return;
            }

            if (settingManager.ConvertSettingBody.EnableAutoAdd)
            {
                // 自動追加が有効な場合はファイルリストに追加
                e.FileList.ForEach(item => SourceFileListBox.Items.Add(item));
                SourceFileListBox.ScrollIntoView(e.FileList.Last());
            }

            if (runner == null)
            {
                if (settingManager.ConvertSettingBody.EnableAutoConvert)
                {
                    // 変換中でないかつ、自動実行が有効な場合は変換スタート
                    ConvertStart_Click(this, null);
                }
            }
            else
            {
                // 既に変換中の場合はリストを更新
                runner.ChangeSorceFileList(SourceFileListBox.Items.OfType<string>().ToList());
            }
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
            var selectItem = SourceFileListBox.SelectedItems.OfType<string>();
            if (selectItem.Any())
            {
                selectItem.ToList().ForEach((item) => SourceFileListBox.Items.Remove(item));

                if (runner != null)
                {
                    runner.ChangeSorceFileList(SourceFileListBox.Items.OfType<string>().ToList());
                }
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
                    using (var sr = new StreamWriter(dlg.FileName))
                    {
                        foreach (string item in SourceFileListBox.Items)
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
            if (runner != null)
            {
                runner.ChangeSorceFileList(SourceFileListBox.Items.OfType<string>().ToList());
            }
        }

        /// <summary>
        /// ログボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            if (logWin == null || !logWin.IsLoaded)
            {
                // ログウインドウをメインウインドウの下に表示する
                logWin = new LogWindow();
                if (runner != null)
                {
                    runner.ConvertStateChangedEvent += new ConvertStateChangedHandler(logWin.ConvertStateChanged);
                }
                logWin.Owner = this;
                logWin.Width = this.Width;
                logWin.Top = this.Top + this.Height - 8;
                logWin.Left = this.Left;
                logWin.Show();
            }
            else if (logWin != null)
            {
                // ウインドウがすでに開いている場合は閉じる
                logWin.Close();
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
            win.Owner = this;
            win.Top = this.Top;
            win.Left = this.Left;
            win.ShowDialog();

            // 監視設定画変更された場合に反映する
            if (watcher.IsWatch && !settingManager.ConvertSettingBody.EnableAutoAdd)
            {
                watcher.Stop();
            }
            else if (!watcher.IsWatch && settingManager.ConvertSettingBody.EnableAutoAdd)
            {
                watcher.Start(settingManager.ConvertSettingBody.WatchFolder, settingManager.ConvertSettingBody.WatchPattern);
            }
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
                if (!string.IsNullOrEmpty(CompleteFolderTextBox.Text)) dlg.InitialDirectory = CompleteFolderTextBox.Text;
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

            if (isStart)
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
            SetStatus(e.AllProgress, e.AllStatus, e.FileProgress, e.FileStatus, e.SourceFilePath);
        }

        /// <summary>
        /// 状態を変更する
        /// </summary>
        /// <param name="allProgress"></param>
        /// <param name="allStatus"></param>
        /// <param name="fileProgress"></param>
        /// <param name="fileStatus"></param>
        private void SetStatus(int allProgress, string allStatus, int fileProgress, string fileStatus, string sourceFilePath)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    SetStatus(allProgress, allStatus, fileProgress, fileStatus, sourceFilePath);
                }));
                return;
            }

            if (allProgress != -1)
            {
                AllProgress.Value = allProgress;
                TaskbarManager.Instance.SetProgressValue(allProgress, 100);
            }
            if (allStatus != null) AllStatus.Content = allStatus;
            if (fileProgress != -1) FileProgress.Value = fileProgress;
            if (fileStatus != null) FileStatus.Content = fileStatus;

            var selectItems = SourceFileListBox.SelectedItems.OfType<string>();
            if (!string.IsNullOrWhiteSpace(sourceFilePath) && (selectItems.Count() != 1 || selectItems.First() != sourceFilePath))
            {
                SourceFileListBox.SelectedItems.Clear();
                SourceFileListBox.SelectedItem = sourceFilePath;
                SourceFileListBox.ScrollIntoView(sourceFilePath);
            }
        }

        /// <summary>
        /// 変換に必要な入力項目のチェック
        /// </summary>
        /// <returns></returns>
        private bool InputCheck()
        {
            if (SourceFileListBox.Items.Count == 0)
            {
                MessageBox.Show("変換ファイルが指定されていません。", "HandBrakeBatchRunner", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (SettingCombo.SelectedItem == null)
            {
                MessageBox.Show("変換設定が選ばれていません。", "HandBrakeBatchRunner", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DestinationFolderTextBox.Text))
            {
                MessageBox.Show("変換先フォルダが選択されていません。", "HandBrakeBatchRunner", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (File.Exists(settingManager.ConvertSettingBody.HandBrakeCLIFilePath) == false)
            {
                MessageBox.Show("HandbrakeCLIファイルが見つかりません。\r\n別途ダウンロードして任意の場所に格納します。格納した変換設定でHandbrakeCLIを指定してください。", "HandBrakeBatchRunner", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        #endregion

    }
}
