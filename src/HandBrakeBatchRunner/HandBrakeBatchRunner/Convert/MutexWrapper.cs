// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// MutexのDisposeサポートラッパークラス
    /// </summary>
    class MutexWrapper: IDisposable
    {
        /// <summary>
        /// 実インスタンス
        /// </summary>
        private Mutex instance;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="initiallyOwned"></param>
        /// <param name="name"></param>
        public MutexWrapper(bool initiallyOwned, string name)
        {
            instance = new Mutex(initiallyOwned, name);
        }
       
        /// <summary>
        /// 指定時間Mutexが取得できるまで待つ
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="exitContext"></param>
        /// <returns></returns>
        public virtual bool WaitOne(int millisecondsTimeout, bool exitContext)
        {
            return instance.WaitOne(millisecondsTimeout, exitContext);
        }

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }

                if (instance != null)
                {
                    instance.ReleaseMutex();
                    instance.Dispose();
                    instance = null;
                }

                disposedValue = true;
            }
        }

        ~MutexWrapper()
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
