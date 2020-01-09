// GNU LESSER GENERAL PUBLIC LICENSE
//    Version 3, 29 June 2007
// copyright twitter suzumebati(@suzumebati5)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HandBrakeBatchRunner.Convert
{
    /// <summary>
    /// MutexのDisposeサポートラッパークラス
    /// </summary>
    class MutexWrapper : IDisposable
    {
        /// <summary>
        /// 実インスタンス
        /// </summary>
        private Mutex instance;

        /// <summary>
        /// 取得開始イベント
        /// </summary>
        private CountdownEvent waitStartEvent = new CountdownEvent(1);

        /// <summary>
        /// 取得完了イベント
        /// </summary>
        private CountdownEvent waitEndEvent = new CountdownEvent(1);

        /// <summary>
        /// 開放イベント
        /// </summary>
        private CountdownEvent releaseEvent = new CountdownEvent(1);

        /// <summary>
        /// Mutexの管理タスク
        /// </summary>
        private Task mutexControll;

        /// <summary>
        /// 取得結果
        /// </summary>
        private bool waitResult;

        /// <summary>
        /// WaitOne引数(待ち時間)
        /// </summary>
        private int millisecondsTimeoutParam;

        /// <summary>
        /// WaitOne引数(exitContext)
        /// </summary>
        private bool exitContextParam;

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
        /// <remarks>
        /// Mutexは同一スレッドで取得、開放をしないといけない仕様のため
        /// awaitするためにタスクをロングラン(内部はスレッド実行)しシグナル処理で対応している。
        /// 無駄が多いのであまり良い実装ではないがこのツールでは頻繁に実行しないのでさっぱりかけるのを優先する。
        /// </remarks>
        public virtual Task<bool> WaitOne(int millisecondsTimeout, bool exitContext)
        {
            // 引数保存
            this.millisecondsTimeoutParam = millisecondsTimeout;
            this.exitContextParam = exitContext;

            if (mutexControll == null)
            {
                mutexControll = Task.Factory.StartNew(() =>
                {
                    MutexControlTask();
                }, TaskCreationOptions.LongRunning);
            }

            return Task.Run(() =>
            {
                waitStartEvent.Signal();
                waitEndEvent.Wait();
                waitEndEvent.Reset();
                return waitResult;
            });
        }

        /// <summary>
        /// Mutexのコントロールタスク
        /// </summary>
        private void MutexControlTask()
        {
            while (true)
            {
                // 開放が始まっていたら終了
                if (releaseEvent.IsSet) break;

                // Mutex取得開始まで待機(待ち続けると開放処理ができないので1secごとに待つ)
                if (waitStartEvent.Wait(1000) == false) continue;
                waitStartEvent.Reset();

                // Mutex取得を行う
                waitResult = instance.WaitOne(millisecondsTimeoutParam, exitContextParam);
                waitEndEvent.Signal();

                // 取得できた時点で処理終了
                if (waitResult) break;
            }

            // 開放まで待機
            releaseEvent.Wait();
            releaseEvent.Dispose();

            // Mutex開放処理
            instance.ReleaseMutex();
            instance.Dispose();
            instance = null;
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (mutexControll != null)
                {
                    releaseEvent.Signal();
                }

                waitStartEvent.Dispose();
                waitEndEvent.Dispose();

                disposedValue = true;
            }
        }

        ~MutexWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
