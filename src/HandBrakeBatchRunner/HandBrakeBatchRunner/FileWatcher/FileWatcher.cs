using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly List<string> checkFilePathList = new List<string>();

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
            // ファイルチェック開始
            Task.Factory.StartNew(() =>
            {
                FileCheckThread();
            }, TaskCreationOptions.LongRunning);

            // ファイルの変更イベント開始
            watcher = new FileSystemWatcher(path, string.IsNullOrWhiteSpace(filter) ? "*.*" : filter);
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;

            IsWatch = true;
            stopping = false;
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
                watcher = null;
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
        public async void FileCheckThread()
        {
            while (true)
            {
                // 1秒待ち合わせ(キャンセル受付)
                if (stopping) return;
                await Task.Delay(1000);
                if (stopping) return;

                // 新規ファイルが来ていない場合は何もしない
                if (checkFilePathList.Count == 0) continue;

                var addFilePathList = new List<string>();
                var delCheckFilePathList = new List<string>();
                lock (lockObj)
                {
                    foreach (string filePath in checkFilePathList)
                    {
                        // 書き込みオープンで開けるか(ほかソフトの書き込みがおわったかチェック)
                        if (CanWrite(filePath))
                        {
                            addFilePathList.Add(filePath);
                            delCheckFilePathList.Add(filePath);
                        }
                    }
                    // 追加可能となったものはチェック対象から削除
                    delCheckFilePathList.ForEach(item => checkFilePathList.Remove(item));
                }

                // ファイル追加イベント発行
                if (addFilePathList.Count > 0)
                {
                    var e = new FileAddedEventArgs
                    {
                        FileList = addFilePathList
                    };
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
            catch (IOException)
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
                if (checkFilePathList.IndexOf(e.FullPath) == -1)
                {
                    checkFilePathList.Add(e.FullPath);
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
                // アンマネージ開放
                Stop();

                disposedValue = true;
            }
        }

        ~ConvertFileWatcher()
        {
            // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
            Dispose(false);
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
