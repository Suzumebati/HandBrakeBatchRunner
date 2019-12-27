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
    public delegate void ConvertStateChangedHandler(object sender, ConvertStateChangedEventArgs e);

    /// <summary>
    /// アウトプットデータ受信イベント引数クラス
    /// </summary>
    public class ConvertStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 全体進捗の％
        /// </summary>
        public int AllProgress { get; set; } = 0;

        /// <summary>
        /// ファイル進捗の％
        /// </summary>
        public int FileProgress { get; set; } = 0;

        /// <summary>
        /// 全体のステータス
        /// </summary>
        public string AllStatus { get; set; } = string.Empty;

        /// <summary>
        /// ファイルのステータス
        /// </summary>
        public string FileStatus { get; set; } = string.Empty;

        /// <summary>
        /// ログのデータ
        /// </summary>
        public string LogData { get; set; } = string.Empty;
    }
}
