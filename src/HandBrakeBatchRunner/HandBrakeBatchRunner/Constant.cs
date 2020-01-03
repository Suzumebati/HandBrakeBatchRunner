// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System.Text.RegularExpressions;

namespace HandBrakeBatchRunner
{
    public static class Constant
    {
        /// <summary>
        /// 変換ファイル名
        /// </summary>
        public readonly static string ConvertSettingFileName = "ConvertSettings.data";

        /// <summary>
        /// HandBrakeの標準出力のパーセンテージ取得用の正規表現
        /// </summary>
        public readonly static Regex RegexLogOutputProgress = new Regex(@"Encoding\: task [0-9]+ of [0-9]+\, ([0-9]+\.[0-9]+) %");

        /// <summary>
        /// HandBrakeの標準出力のパーセンテージ取得用の正規表現
        /// </summary>
        public readonly static Regex RegexLogOutputProgressAndRemain = new Regex(@"Encoding\: task [0-9]+ of [0-9]+\, ([0-9]+\.[0-9]+) % \([0-9]+\.[0-9]+ fps, avg [0-9]+\.[0-9]+ fps, ETA ([0-9]+h[0-9]+m[0-9]+s)\)");

        /// <summary>
        /// 待ち間隔
        /// </summary>
        public readonly static int WaitIntervalMiliSecond = 1000;

        /// <summary>
        /// 最大待ち時間
        /// </summary>
        public readonly static int WaitMiliSecond = 24*60*60*1000;

        /// <summary>
        /// 変換ステータス
        /// </summary>
        public enum ConvertFileStatus
        {
            NotRuning,
            Running,
            Canceled,
            TimeOut,
            Error,
            Completed
        }
    }
}