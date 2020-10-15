using UnityEngine;

namespace uzSurfaceMapper.Core.Workers.Interfaces
{
    public interface IWorkerShareable
    {
        // TODO: Implemented on RoadGenerator. Implement Color32[] array (global scope) in order to get the colors back from target (check where the target param (in MapIteration) is divided: MainGeneration)
        // then, register method (needs to be created here) also is needed to be shared with the TextureWorkerBase.
        // Also, a bool flag is needed to be checked on the while (todo) inside RoadTextureWorker (is documented already). So when the flag isReady is true then the Color32 array should be already set in order to be saved.

        Model.Color[] Source { get; set; }
        Color32[] Target { get; set; }
        bool IsGeneratorFinished { get; set; }

        void RegisterSharedWorker(IWorkerShareable worker);
    }
}