// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;
using System.Collections.Generic;

namespace HandBrakeBatchRunner.FileWatcher
{
    /// <summary>
    /// ファイル追加イベントハンドラー
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void FileAddedHandler(object sender, FileAddedEventArgs e);

    /// <summary>
    /// ファイル追加イベント引数クラス
    /// </summary>
    public class FileAddedEventArgs : EventArgs
    {
        /// <summary>
        /// ファイルパスリスト
        /// </summary>
        public List<string> FileList { get; set; } = new List<string>();
    }
}
