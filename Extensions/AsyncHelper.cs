using System;
using System.Collections;
using System.ComponentModel;

namespace UnityEngine.Extensions
{
    /// <summary>
    ///     The Async Helper
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        ///     Run a function asynchronously.
        /// </summary>
        /// <typeparam name="TPre">The type of the pre.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="beforeExecute">The before execute.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     func
        ///     or
        ///     beforeExecute
        ///     or
        ///     result
        /// </exception>
        public static BackgroundWorker RunAsync<TPre, T>(this Func<TPre, T> func, Func<TPre> beforeExecute,
            Action<T> result)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            if (beforeExecute == null) throw new ArgumentNullException(nameof(beforeExecute));

            if (result == null) throw new ArgumentNullException(nameof(result));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func((TPre)e.Argument);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result((T)e.Result);
            };

            var argument = beforeExecute();
            worker.RunWorkerAsync(argument);

            return worker;
        }

        /// <summary>
        ///     Runs the asynchronous.
        /// </summary>
        /// <typeparam name="TPre">The type of the pre.</typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="beforeExecute">The before execute.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// func
        /// or
        /// beforeExecute
        /// or
        /// result
        /// </exception>
        public static BackgroundWorker RunAsync<TPre, T>(this Func<TPre, BackgroundWorker, T> func, Func<TPre> beforeExecute,
            Action<T> result = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            if (beforeExecute == null) throw new ArgumentNullException(nameof(beforeExecute));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func((TPre)e.Argument, s as BackgroundWorker);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke((T)e.Result);
            };

            var argument = beforeExecute();
            worker.RunWorkerAsync(argument);

            return worker;
        }

        /// <summary>
        ///     Run a function asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="beforeExecute">The before execute.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///     func
        ///     or
        ///     beforeExecute
        ///     or
        ///     result
        /// </exception>
        public static BackgroundWorker RunAsync<T>(this Func<T> func, Action beforeExecute, Action<T> result)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            if (beforeExecute == null) throw new ArgumentNullException(nameof(beforeExecute));

            if (result == null) throw new ArgumentNullException(nameof(result));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func();
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result((T)e.Result);
            };

            beforeExecute();
            worker.RunWorkerAsync();

            return worker;
        }

        /// <summary>
        ///     Run a function asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">func</exception>
        public static BackgroundWorker RunAsync<T>(this Func<T> func, Action<T> result = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func();
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke((T)e.Result);
            };

            worker.RunWorkerAsync();

            return worker;
        }

        public static BackgroundWorker RunAsync<T1, TR>(this Func<T1, TR> func, T1 arg1, Action<T1, TR> result = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func(arg1);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke(arg1, (TR)e.Result);
            };

            worker.RunWorkerAsync();

            return worker;
        }

        public static BackgroundWorker RunAsync<T1, T2, TR>(this Func<T1, T2, TR> func, T1 arg1, T2 arg2, Action<T1, T2, TR> result = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func(arg1, arg2);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke(arg1, arg2, (TR)e.Result);
            };

            worker.RunWorkerAsync();

            return worker;
        }

        public static BackgroundWorker RunAsync<T1, TR>(this Func<T1, TR> func, T1 arg1, Action beforeExecute, Action<T1, TR> result, bool beforeAsync)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            if (beforeExecute == null) throw new ArgumentNullException(nameof(beforeExecute));

            if (result == null) throw new ArgumentNullException(nameof(result));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                if (beforeAsync)
                    beforeExecute();

                //Some work...
                e.Result = func(arg1);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result(arg1, (TR)e.Result);
            };

            if (!beforeAsync)
                beforeExecute();
            worker.RunWorkerAsync();

            return worker;
        }

        public static BackgroundWorker RunAsync<T1, T2, TR>(this Func<T1, T2, TR> func, T1 arg1, T2 arg2, Action beforeExecute, Action<T1, T2, TR> result, bool beforeAsync)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            if (beforeExecute == null) throw new ArgumentNullException(nameof(beforeExecute));

            if (result == null) throw new ArgumentNullException(nameof(result));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                if (beforeAsync)
                    beforeExecute();

                //Some work...
                e.Result = func(arg1, arg2);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result(arg1, arg2, (TR)e.Result);
            };

            if (!beforeAsync)
                beforeExecute();
            worker.RunWorkerAsync();

            return worker;
        }

        /// <summary>
        ///     Run a function asynchronously.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func">The function.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">func</exception>
        public static BackgroundWorker RunAsync<T>(this Func<BackgroundWorker, T> func, Action<T> result = null)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                e.Result = func(s as BackgroundWorker);
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke((T)e.Result);
            };

            worker.RunWorkerAsync();

            return worker;
        }

        /// <summary>
        ///     Run an action asynchronously.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">callback</exception>
        public static BackgroundWorker RunAsync(this Action callback, Action result = null)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                callback();
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke();
            };

            worker.RunWorkerAsync();

            return worker;
        }

        public static BackgroundWorker RunAsync(this Action callback, Action preRun, Action result = null)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));
            if (preRun == null) throw new ArgumentNullException(nameof(preRun));

            var worker = new BackgroundWorker();

            worker.DoWork += (s, e) =>
            {
                //Some work...
                callback();
            };

            worker.RunWorkerCompleted += (s, e) =>
            {
                //e.Result "returned" from thread
                result?.Invoke();
            };

            preRun();
            worker.RunWorkerAsync();

            return worker;
        }

        /// <summary>
        ///     Runs the asynchronous.
        /// </summary>
        /// <param name="coroutine">The coroutine.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">coroutine</exception>
        public static BackgroundWorker RunAsync(this IEnumerator coroutine, Action result = null)
        {
            if (coroutine == null) throw new ArgumentNullException(nameof(coroutine));

            Action action = () =>
            {
                while (coroutine.MoveNext())
                {
                }
            };

#if !UZSURFACEMAPPER // TODO
            return action.RunAsync(result);
#else
            return null;
#endif
        }

        /// <summary>
        ///     Waits for worker.
        /// </summary>
        /// <param name="backgroundWorker">The background worker.</param>
        /// <param name="callback">The callback.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">callback</exception>
        public static IEnumerator WaitForWorker(this BackgroundWorker backgroundWorker, Action callback)
        {
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            yield return new WaitUntil(() => !backgroundWorker.IsBusy);
            callback();
        }
    }
}