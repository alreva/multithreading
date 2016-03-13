using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Dir
{
    public class FileSystemObjectCollection : ObservableCollection<FileSystemNode>
    {
        private readonly Dictionary<string, DirNode> _repo = new Dictionary<string, DirNode>();
        
        public void AddDir(string path)
        {
            if (_repo.ContainsKey(path))
            {
                return;
            }

            var nodeToAdd = new DirNode(path.GetShortDirectoryName(), FileSystemObjectSize.Undefined);
            _repo.Add(path, nodeToAdd);
            AddToHierarchy(path, nodeToAdd);
        }

        public void AddFile(string path, FileSystemObjectSize size)
        {
            var nodeToAdd = new FileSystemNode(Path.GetFileName(path), size);
            AddToHierarchy(path, nodeToAdd);
        }

        private void AddToHierarchy(string path, FileSystemNode nodeToAdd)
        {
            DirNode dir;
            if (TryGetParentFor(path, out dir))
            {
                dir.Children.Add(nodeToAdd);
            }
            else
            {
                Add(nodeToAdd);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _repo.Clear();
        }

        private bool TryGetParentFor(string path, out DirNode dir)
        {
            string parentPath;
            if (!PathHelper.TryGetParentPath(path, out parentPath))
            {
                dir = null;
                return false;
            }

            return _repo.TryGetValue(parentPath, out dir);
        }

        public void UpdateSize(string path, FileSystemObjectSize size)
        {
            DirNode dirNode;
            if (!_repo.TryGetValue(path, out dirNode))
            {
                return;
            }

            dirNode.Size = size;
        }
    }
}