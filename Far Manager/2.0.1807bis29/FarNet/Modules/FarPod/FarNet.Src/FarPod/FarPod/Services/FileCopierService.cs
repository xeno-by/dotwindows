namespace FarPod.Services
{
    using System;
    using System.IO;

    /// <summary>
    /// Generic file copy service
    /// </summary>
    class FileCopierService : IDisposable
    {
        const int FileCopyBufferSize = 1048576 * 2; // 262144;

        private readonly string _sourcePath;
        private readonly string _destinationPath;

        private Stream _sourceFile;
        private Stream _destinationFile;

        private byte[] _copyBuffer;

        public string SourcePath
        {
            get { return _sourcePath; }
        }

        public string DestinationPath
        {
            get { return _destinationPath; }
        }

        public long TotalBytes
        {
            get;
            private set;
        }

        public long TotalTransferBytes
        {
            get;
            private set;
        }

        public long LastBlockTransferBytes
        {
            get;
            private set;
        }

        public FileCopierService(string sourcePath, string destinationPath)
        {
            _sourcePath = sourcePath;
            _destinationPath = destinationPath;
        }

        public bool CopyBlock()
        {
            if (_destinationFile == null) return false;

            int realCopyLength = _sourceFile.Read(_copyBuffer, 0, _copyBuffer.Length);

            if (realCopyLength > 0)
            {
                _destinationFile.Write(_copyBuffer, 0, realCopyLength);

                TotalTransferBytes += realCopyLength;
                LastBlockTransferBytes = realCopyLength;
            }

            return realCopyLength > 0;
        }

        public void Start()
        {
            _sourceFile = File.OpenRead(_sourcePath);
            _destinationFile = File.Create(_destinationPath);

            _copyBuffer = new byte[FileCopyBufferSize];

            TotalBytes = _sourceFile.Length;
            TotalTransferBytes = 0;
        }

        public void Finish(bool cancel = false)
        {
            if (_sourceFile != null)
            {
                _sourceFile.Close();
            }
            if (_destinationFile != null)
            {
                _destinationFile.Close();
            }

            if (cancel)
            {
                if (File.Exists(_destinationPath))
                {
                    File.Delete(_destinationPath);
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_sourceFile != null)
            {
                _sourceFile.Dispose();
                _sourceFile = null;
            }

            if (_destinationFile != null)
            {
                _destinationFile.Dispose();
                _destinationFile = null;
            }
        }

        #endregion
    }
}
