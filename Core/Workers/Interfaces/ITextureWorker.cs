using UnityEngine;
using Color = uzSurfaceMapper.Model.Color;

namespace uzSurfaceMapper.Core.Workers.Interfaces
{
    public interface ITextureWorker
    {
        string Name { get; }
        bool IsReady { get; set; }
        bool IsFinished { get; set; }

        string Status { get; }
        Color[] CurrentColors { get; set; }

        //void RegisterWorker(string texturePath, ITextureWorker worker, bool isEssential); // TODO?

        void RegisterWorker(string texturePath, ITextureWorker worker);

        void Run(Color[] colors);

        void SaveTexture(Color[] colors, bool force);

        void SaveTexture(Color32[] colors);
    }
}