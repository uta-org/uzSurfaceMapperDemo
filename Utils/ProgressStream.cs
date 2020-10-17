using System;
using System.ComponentModel;
using System.IO;

namespace APIScripts.Utils
{
    public class ProgressStream : MemoryStream, IProgress<float>
    {
        public ProgressStream(byte[] buffer) : base(buffer)
        {
        }

        public ProgressStream()
        {
        }

        //private int _lastProgress = 0;

        //public ProgressStream(Stream stream) : base(stream)
        //{
        //    if (stream.Length <= 0 || !stream.CanRead) throw new ArgumentException("stream");
        //}

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = base.Read(buffer, offset, count);
            if (ProgressChanged != null)
            {
                float newProgress = (float)Position / Length;
                if (newProgress > Progress)
                {
                    Report(newProgress);
                    ProgressChanged(new FloatProgressChangedEventArgs(Progress));
                }
            }
            return amountRead;
        }

        public event FloatProgressChangedEventHandler ProgressChanged = delegate { };

        public void Report(float value)
        {
            Progress = value;
        }

        public float Progress { get; set; }
    }

    public delegate void FloatProgressChangedEventHandler(FloatProgressChangedEventArgs args);
}