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
        public LogWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 変換ステータス変更イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ConvertStateChanged(object sender, ConvertStateChangedEventArgs e)
        {
            if (Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    ConvertStateChanged(sender,e);
                }));
                return;
            }
            LogListBox.Items.Add(e.LogData);
        }
    }
}
