using System;
using System.Collections;
using APIScripts.Utils;
using UnityEngine;
using UnityEngine.Extensions;
using UnityEngine.UI;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Workers;
using uzSurfaceMapper.Core.Workers.Interfaces;
using uzSurfaceMapper.Extensions;
using uzSurfaceMapper.Model;
using Color = uzSurfaceMapper.Model.Color;
using F = uzSurfaceMapper.Extensions.F;
using UColor = UnityEngine.Color;

#if UNITY_WEBGL

using File = uzSurfaceMapperDemo.Utils.File;

#else

using System.IO;

#endif

namespace uzSurfaceMapper.Core.Generators
{
    public sealed partial class RoadGenerator : MapGenerator, IWorkerShareable
    {
        public static RoadModel RoadModel { get; private set; }

        public RoadModel Model { get; private set; }

        public Color[] Source { get; set; }
        public Color32[] Target { get; set; }
        public bool IsGeneratorFinished { get; set; }

        private float DeserializeProgress { get; set; }

        [InvokeAtAwake]
        public override void InvokeAtAwake()
        {
            base.InvokeAtAwake();

            // TODO
            //            if (!IS_DEMO)
            //#pragma warning disable 162
            //                return;
            //#pragma warning restore 162

            string path;
#if !UNITY_WEBGL
            path = RoadJSONPath;
#else
            path = RoadBINPath;
#endif

            bool exists = File.Exists(path);

            Debug.Log(exists
                ? $"'{path}' exists. Deserializing!"
                : $"'{path}' doesn't exists. Instantiating!");

            //Model = exists
            //    ? JsonConvert.DeserializeObject<RoadModel>(File.ReadAllText(RoadJSONPath))
            //    : new RoadModel();

            if (!exists)
            {
                Model = new RoadModel();
                RoadModel = Model;
            }
            else
            {
#if !UNITY_WEBGL
                StartCoroutine(F.AsyncReadFileWithWWW<string>(RoadJSONPath, result =>
                {
                    Model = result.Deserialize<RoadModel>();
                    RoadModel = Model;

                    Debug.Log($"Deserialized roads with {RoadModel.SimplifiedRoadNodes.Count} nodes!");

                    isRoadReady = true;
                }));
#else
                var url = WebRequestUtils.MakeInitialUrl(path);
                Debug.Log($"Road: '{path}' -> '{url}'");
                url.ReadDataFromWebAsync(result =>
                {
                    Func<RoadModel> roadAsync = () => F.Deserialize<RoadModel>(result, evnt =>
                    {
                        DeserializeProgress = evnt.Progress;
                    });
                    AsyncHelper.RunAsync(roadAsync, roadResult =>
                    {
                        Model = roadResult;
                        RoadModel = roadResult;

                        lock (RoadModel.SimplifiedRoadNodes)
                            Debug.Log($"Deserialized road with {RoadModel.SimplifiedRoadNodes.Count} nodes!");
                        isRoadReady = true;
                    });
                });

                //StartCoroutine(F.AsyncReadFileWithWWW<byte[]>(RoadBINPath, result =>
                //{
                //    Model = F.Deserialize<RoadModel>(result);
                //    RoadModel = Model;

                //    Debug.Log($"Deserialized roads with {RoadModel.SimplifiedRoadNodes.Count} nodes!");

                //    isRoadReady = true;
                //}));

#endif
            }

#if UNITY_WEBGL
            //Debug.Log(RoadBINPath);
            if (!File.Exists(RoadBINPath) && exists)
            {
                Debug.Log("Started coroutine!");
                StartCoroutine(SerializeBin());
            }
#endif
        }

#if UNITY_WEBGL

        private void OnGUI()
        {
            if (isRoadReady)
                return;

            UIUtils.DrawBar(RoadProgressRect, DeserializeProgress, UColor.white, UColor.gray, 1);
            GUI.Label(RoadProgressRect, $"Road progress: {DeserializeProgress * 100:F2} %", LabelStyle);
        }

#endif

        public void RegisterSharedWorker(IWorkerShareable worker)
        {
            TextureWorkerBase.ShareableWorkers.Add(worker);
        }

        protected override IEnumerator SerializeBin()
        {
            Debug.Log($"Waiting city to be deserialized in order to serialize to '{RoadBINPath}'...");
            yield return new WaitUntil(() => isRoadReady);
            Debug.Log($"Serializing '{RoadBINPath}'!");

            // ReSharper disable once InvokeAsExtensionMethod
            File.WriteAllBytes(RoadBINPath, F.Serialize(RoadModel, null));
        }
    }
}