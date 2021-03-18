using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using uzSurfaceMapper.Core.Generators;
using uzSurfaceMapper.Core.Workers.Interfaces;
using uzSurfaceMapper.Extensions.Demo;
using Debug = UnityEngine.Debug;

namespace uzSurfaceMapper.Core.Workers
{
    public abstract partial class TextureWorkerBase : MonoBehaviour, ITextureWorker, IProgress<float>
    {
        internal static List<IWorkerShareable> ShareableWorkers { get; } = new List<IWorkerShareable>();

        internal static HashSet<ITextureWorker> WorkersCollection { get; } = new HashSet<ITextureWorker>();

        private static Queue<Tuple<string, ITextureWorker>> Workers { get; } = new Queue<Tuple<string, ITextureWorker>>();

        // Dispatcher
        private static ConcurrentQueue<Action> enqueuedActions { get; } = new ConcurrentQueue<Action>();

        /// <summary>
        /// Is this essential for the terrain generation?
        /// </summary>
        public bool isEssential;

        protected bool showProgress = true; // TODO: Set this to false and test what happens

        private bool isTextureSaved;

        protected bool isSavingTexture;

        protected static bool terminateThread;
        private static Thread TextureWorker;
        private ITextureWorker _textureWorkerImplementation;

        // BE CAREFUL THIS IS NOT THREAD SAFE.
        public string Status { get; protected set; }

        public string Name { get; protected set; }

        private bool isReady;

        public bool IsReady
        {
            get => isReady || !isEssential;
            set => isReady = value;
        }

        private static Color[] MapColors { get; set; }

        public virtual Model.Color[] CurrentColors { get; set; }

        public bool IsFinished { get; set; }

        public bool saveOnDestroy;

        public bool saveAtFinish = true;

        public virtual void OnFinish()
        {
            if (saveAtFinish) SaveTexture(CurrentColors, true);
        }

        public static void SetReference(Color[] colors)
        {
            MapColors = colors;

            if (TextureWorker != null) return;
            TextureWorker = new Thread(RunThread);
            TextureWorker.Start();
        }

        private static void RunThread()
        {
            Debug.Log("Started texture worker base thread!");
            while (!terminateThread)
            {
                if (Workers.Count > 0)
                {
                    var tuple = Workers.Dequeue();
                    var worker = tuple.Item2;

                    var watch = Stopwatch.StartNew();
                    Debug.Log($"Running worker with name: '{worker.Name}'");
                    {
                        var cloned = ((TextureWorkerBase)worker).GetClonedColors();
                        worker.CurrentColors = cloned;
                        worker.Run(cloned);
                        if (!((TextureWorkerBase)worker).isEssential)
                            enqueuedActions.Enqueue(() => worker.SaveTexture(cloned, true));
                        worker.IsReady = true;
                        ((TextureWorkerBase)worker).showProgress = false; // Hide bar when finished in order to show other bars
                    }
                    watch.Stop();
                    Debug.Log($"Finished worker with name: '{worker.Name}'! Elapsed {watch.ElapsedMilliseconds} ms.");
                    worker.IsFinished = true;

                    //FinishedWorks.Enqueue(new Tuple<string, Components.Color[]>(tuple.Item1, cloned));
                }
                // TODO
                Thread.Sleep(100);
            }
            Debug.Log("Terminating texture worker base thread!");
        }

        public Model.Color[] GetClonedColors()
        {
            lock (MapColors)
            {
                return MapColors.Select(x => (Model.Color)x).ToArray();
            }

            // TODO: Check if statement above is thread safe.
            //return (Components.Color[])MapColors.Clone();
        }

        public void RegisterWorker(string texturePath, ITextureWorker worker)
        {
            Name = texturePath;
            Debug.Log($"Registering worker with name: '{Name}'");

            Workers.Enqueue(new Tuple<string, ITextureWorker>(texturePath, worker));
            if (!WorkersCollection.Add(worker))
                throw new InvalidOperationException($"Worker already registered for '{texturePath}'!");
        }

        public virtual void Run(Model.Color[] colors)
        {
        }

        public void SaveTexture(Model.Color[] colors, bool force = false)
        {
            //Debug.Log("Saving texture");
            if (!isEssential && !isReady && !force) return; // If isn't essential this is not finished yet.

            isSavingTexture = true;
            var colorsArray = colors.CastBack().ToArray();
            SaveAsColor32(colorsArray);
        }

        public void SaveTexture(Color32[] colors)
        {
            isSavingTexture = true;
            SaveAsColor32(colors);
        }

        private void SaveAsColor32(Color32[] colorsArray)
        {
            var path = Path.Combine(Environment.CurrentDirectory, $"{Name}.png");
            Debug.Log($"Saving texture from worker {GetType().Name} at '{path}'...");
            F.SaveAndClear(path, colorsArray, MapGenerator.mapWidth, MapGenerator.mapHeight);
            Debug.Log($"Saved successfully texture in '{path}' for {Name} worker!");
            isTextureSaved = true;
            isSavingTexture = false;
        }

        public void Report(float value)
        {
            Progress = value;
        }

        private float Progress { get; set; }

        private void Start()
        {
            Debug.Log("Started main texture worker");
            StartCoroutine(CheckFinish());
        }

        private IEnumerator CheckFinish()
        {
            yield return new WaitUntil(() => IsFinished);
            OnFinish();
        }
    }
}