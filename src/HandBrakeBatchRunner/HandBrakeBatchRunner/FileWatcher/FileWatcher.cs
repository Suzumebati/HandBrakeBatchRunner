using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;

namespace HandBrakeBatchRunner.FileWatcher
{
    /// <summary>
    /// 変換ファイル監視クラス
    /// </summary>
    class ConvertFileWatcher : IDisposable
    {
        /// <summary>
        /// 変換状態変更イベント
        /// </summary>
        public event FileAddedHandler FileAddedEvent;

        /// <summary>
        /// ロックオブジェクト
        /// </summary>
        private Object lockObj = new Object();

        /// <summary>
        /// ファイルウォッチャー
        /// </summary>
        private FileSystemWatcher watcher;

        /// <summary>
        /// ファイル監視スレッド
        /// </summary>
        private Thread checkThread; 
        
        /// <summary>
        /// 追加ファイルリスト
        /// </summary>
        private readonly List<string> checkFilePath = new List<string>();

        /// <summary>
        /// 停止フラグ
        /// </summary>
        private bool stopping = false;

        /// <summary>
        /// 監視中かどうか
        /// </summary>
        public bool IsWatch { get; set; } = false;

        /// <summary>
        /// ファイルのチェック開始
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filter"></param>
        public void Start(string path,string filter)
        {
            // ファイルチェックスレッド開始
            checkThread = new Thread(new ThreadStart(FileCheckThread));
            checkThread.Start();

            // ファイルの変更イベント開始
            watcher = new FileSystemWatcher(path, string.IsNullOrWhiteSpace(filter) ? "*.*" : filter);
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;

            IsWatch = true;
        }

        /// <summary>
        /// 監視を終了する
        /// </summary>
        public void Stop()
        {
            stopping = true;
            if (watcher != null)
            {
                watcher.Dispose();
            }
            if(checkThread != null)
            {
                checkThread = null;
            }
            IsWatch = false;
        }

        /// <summary>
        /// ファイルのチェックスレッド
        /// </summary>
        public void FileCheckThread()
        {
            while (true)
            {
                if (stopping) return;
                Thread.Sleep(1000);
                if (stopping) return;
                if (checkFilePath.Count == 0) continue;

                var createFilePath = new List<string>();
                var delList = new List<string>();
                lock (lockObj)
                {
                    foreach (string filePath in checkFilePath)
                    {
                        // 書き込みオープンで開けるか(ほかソフトの書き込みがおわったかチェック)
                        if (CanWrite(filePath))
                        {
                            createFilePath.Add(filePath);
                            delList.Add(filePath);
                        }
                    }
                    delList.ForEach(item => checkFilePath.Remove(item));
                }

                // ファイル追加イベント発行
                if (createFilePath.Count > 0)
                {
                    var e = new FileAddedEventArgs();
                    e.FileList = createFilePath;
                    OnFileAdded(e);
                }
            }
        }

        /// <summary>
        /// 書き込みできる状態か判断
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool CanWrite(string filePath)
        {
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return true;
                }
            }
            catch (IOException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// ファイルの作成イベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileCreated(object sender, FileSystemEventArgs e)
        {
            lock(lockObj)
            {
                if (checkFilePath.IndexOf(e.FullPath) == -1)
                {
                    checkFilePath.Add(e.FullPath);
                }
            }
        }

        /// <summary>
        /// イベントを発生させる
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileAdded(FileAddedEventArgs e)
        {
            FileAddedEvent?.Invoke(this, e);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                disposedValue = true;
            }
        }
        
        // このコードは、破棄可能なパターンを正しく実装できるように追加されました。
        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
