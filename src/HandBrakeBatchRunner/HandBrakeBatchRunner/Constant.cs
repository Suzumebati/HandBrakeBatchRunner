using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
        /// 変換ステータス
        /// </summary>
        public enum ConvertFileStatus
        {
            NotRuning,
            Running,
            Canceled,
            TimeOut,
            Completed
        }
    }
}