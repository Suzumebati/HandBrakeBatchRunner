using HandBrakeBatchRunner.Convert;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HandBrakeBatchRunner
{
    /// <summary>
    /// LogWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class LogWindow : Window
    {
        /// <summary>
        /// ログウィンドウのインスタンス
        /// </summary>
        private static LogWindow instance = null;

        /// <summary>
        /// ログ表示のスクロールビュー
        /// </summary>
        private ScrollViewer logScroll = null;

        /// <summary>
        /// ログ表示のスクロールビュー
        /// </summary>
        private ScrollViewer appLogScroll = null;

        /// <summary>
        /// メッセージ種別
        /// </summary>
        public enum MessageType
        {
            /// <summary>
            /// 情報
            /// </summary>
            Information,
            /// <summary>
            /// 警告
            /// </summary>
            Warning,
            /// <summary>
            /// エラー
            /// </summary>
            Error
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// ウインドウ表示イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            instance = this;

            // ScrollViewerを取得
            if (VisualTreeHelper.GetChild(LogListBox, 0) is Border border)
            {
                logScroll = border.Child as ScrollViewer;
            }
            if (VisualTreeHelper.GetChild(AppLogListBox, 0) is Border appBorder)
            {
                appLogScroll = appBorder.Child as ScrollViewer;
            }
        }

        /// <summary>
        /// ウインドウクローズ中イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            instance = null;
        }

        /// <summary>
        /// 変換状態変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConvertStateChanged(object sender, ConvertStateChangedEventArgs e)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    ConvertStateChanged(sender, e);
                }));
                return;
            }
            // 進捗率以外のログ内容をウインドウに表示する
            if (e.FileProgress == -1)
            {
                LogListBox.Items.Add(e.LogData);
                if (logScroll != null) logScroll.ScrollToEnd();
            }
        }

        /// <summary>
        /// メッセージ追加
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddMessage(string message, MessageType messageType)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    AddMessage(message, messageType);
                }));
                return;
            }

            var item = new ListBoxItem
            {
                Content = $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {message}"
            };
            switch (messageType)
            {
                case MessageType.Information:
                    item.Foreground = Brushes.Blue;
                    break;
                case MessageType.Warning:
                    item.Foreground = Brushes.Goldenrod;
                    break;
                case MessageType.Error:
                    item.Foreground = Brushes.Red;
                    break;
            }
            AppLogListBox.Items.Add(item);
            if (appLogScroll != null) appLogScroll.ScrollToEnd();
        }

        /// <summary>
        /// メッセージ追加(静的メソッド)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LogMessage(string message, MessageType messageType)
        {
            if (instance != null)
            {
                instance.AddMessage(message, messageType);
            }
        }

    }
}
