using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using uzSurfaceMapper.Core.Attrs;
using uzSurfaceMapper.Core.Workers;
using uzSurfaceMapper.Core.Workers.Interfaces;
using uzSurfaceMapper.Model;
using Color = uzSurfaceMapper.Model.Color;
using UColor = UnityEngine.Color;

namespace uzSurfaceMapper.Core.Generators
{
    public sealed partial class RoadGenerator : MapGenerator, IWorkerShareable
    {
        public static RoadModel RoadModel { get; private set; }

        public RoadModel Model { get; private set; }

        public Color[] Source { get; set; }
        public Color32[] Target { get; set; }
        public bool IsGeneratorFinished { get; set; }

        [InvokeAtAwake]
        public override void InvokeAtAwake()
        {
            base.InvokeAtAwake();

            var exists = File.Exists(RoadJSONPath);
            Debug.Log(exists
                ? $"'{RoadJSONPath}' exists. Deserializing!"
                : $"'{RoadJSONPath}' doesn't exists. Instantiating!");

            Model = exists
                ? JsonConvert.DeserializeObject<RoadModel>(File.ReadAllText(RoadJSONPath))
                : new RoadModel();

            RoadModel = Model;
        }

        public void RegisterSharedWorker(IWorkerShareable worker)
        {
            TextureWorkerBase.ShareableWorkers.Add(worker);
        }
    }
}