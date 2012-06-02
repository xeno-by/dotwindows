namespace FarPod.Services
{
    using System.Collections.Generic;
    using System.IO;
    using FarPod.Helpers;

    /// <summary>
    /// Generic file system enumerator
    /// </summary>
    class FileSystemEnumerator
    {
        private readonly string _root;
        private readonly IEnumerable<string> _pathList;

        private Stack<string> _passedDirList;

        private readonly MaskMatcher _mm;

        public long TotalFileSize
        {
            get;
            private set;
        }

        public FileSystemEnumerator(string root, IEnumerable<string> pathList)
        {
            _root = root;
            _pathList = pathList;

            _mm = new MaskMatcher(FarPodSetting.Default.SupportFileMask);
        }

        public IEnumerable<string> FetchFiles()
        {
            var inputStack = new Stack<string>(_pathList);

            _passedDirList = new Stack<string>();

            while (inputStack.Count > 0)
            {
                string currentPath = Path.Combine(_root, inputStack.Pop());

                if (File.Exists(currentPath))
                {
                    if (_mm.Compare(currentPath)) yield return processFile(currentPath);
                }
                else
                {
                    _passedDirList.Push(currentPath);

                    foreach (string file in Directory.GetFiles(currentPath))
                    {
                        if (_mm.Compare(file)) yield return processFile(file);
                    }

                    foreach (string folder in Directory.GetDirectories(currentPath))
                    {
                        inputStack.Push(folder);
                    }
                }
            }
        }

        public IEnumerable<string> GetBypassDirectores()
        {
            while (_passedDirList.Count > 0) yield return _passedDirList.Pop();
        }

        private string processFile(string fn)
        {
            TotalFileSize += new FileInfo(fn).Length;

            return fn;
        }
    }
}
