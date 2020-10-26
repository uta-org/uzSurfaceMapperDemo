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

    public class ProgressBaseStream : Stream, IProgress<float>
    {
        public ProgressBaseStream()
        {
        }

        public ProgressBaseStream(Stream input)
        {
            Input = input;
            StreamLength = input.Length;
        }

        public Stream Input { get; }

        public long StreamLength { get; }

        //private int _lastProgress = 0;

        //public ProgressStream(Stream stream) : base(stream)
        //{
        //    if (stream.Length <= 0 || !stream.CanRead) throw new ArgumentException("stream");
        //}

        public override int Read(byte[] buffer, int offset, int count)
        {
            int amountRead = Input.Read(buffer, offset, count);
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

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        //public override int Read(byte[] buffer, int offset, int count)
        //{
        //    int n = m_input.Read(buffer, offset, count);
        //    _position += n;
        //    UpdateProgress?.Invoke(this, new ProgressEventArgs((1.0f * _position) / m_length));
        //    return n;
        //}

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => StreamLength;

        private long _position = 0L;

        public override long Position
        {
            get => _position;
            set => throw new NotImplementedException();
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