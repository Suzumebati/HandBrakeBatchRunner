using System.Linq;
using System.Windows;
using HandBrakeBatchRunner.Convert;

namespace HandBrakeBatchRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        /// <summary>
        /// 設定ボタンのクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // 設定ウインドウをモーダル表示する
            var win = new SettingWindow();
            win.ShowDialog();
        }

        #endregion

    }
}
