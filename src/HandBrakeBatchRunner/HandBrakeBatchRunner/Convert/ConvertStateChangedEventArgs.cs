// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// 変換状態変更イベントハンドラー
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ConvertStateChangedHandler(object sender, ConvertStateChangedEventArgs e);

    /// <summary>
    /// 変換状態変更イベント引数クラス
    /// </summary>
    public class ConvertStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 全体進捗の％
        /// </summary>
        public int AllProgress { get; set; } = -1;

        /// <summary>
        /// ファイル進捗の％
        /// </summary>
        public int FileProgress { get; set; } = -1;

        /// <summary>
        /// 全体のステータス
        /// </summary>
        public string AllStatus { get; set; } = null;

        /// <summary>
        /// ファイルのステータス
        /// </summary>
        public string FileStatus { get; set; } = null;

        /// <summary>
        /// 変換元ファイルパス
        /// </summary>
        public string SourceFilePath { get; set; } = null;

        /// <summary>
        /// ログのデータ
        /// </summary>
        public string LogData { get; set; } = null;
    }
}
