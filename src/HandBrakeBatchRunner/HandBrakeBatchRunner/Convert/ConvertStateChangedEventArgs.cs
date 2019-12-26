using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// アウトプットデータ受信イベントハンドラー
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void OutputDataReceivedHandler(object sender, ConvertStateChangedEventArgs e);

    /// <summary>
    /// アウトプットデータ受信イベント引数クラス
    /// </summary>
    public class ConvertStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// ファイルの進捗パーセント
        /// </summary>
        public int Progress { get; set; } = 0;

        /// <summary>
        /// ステータス表示
        /// </summary>
        public string ConvertStatus { get; set; } = string.Empty;

        /// <summary>
        /// ログのデータ
        /// </summary>
        public string LogData { get; set; } = string.Empty;
    }
}
