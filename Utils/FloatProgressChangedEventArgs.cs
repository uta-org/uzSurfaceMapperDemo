using System;

namespace APIScripts.Utils
{
    public class FloatProgressChangedEventArgs : EventArgs
    {
        public FloatProgressChangedEventArgs(float _progress)
        {
            Progress = _progress;
        }

        public float Progress { get; }
    }
}