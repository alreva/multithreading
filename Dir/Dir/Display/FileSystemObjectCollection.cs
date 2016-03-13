using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Dir.Read;

namespace Dir.Display
{
    public class FileSystemObjectCollection : ObservableCollection<FileSystemNodeView>
    {
        private readonly Dictionary<string, FileSystemNodeView> _repo = new Dictionary<string, FileSystemNodeView>();
        
        public void AddDir(string path)
        {
            if (_repo.ContainsKey(path))
            {
                return;
            }

            var nodeToAdd = new FileSystemNodeView(path.GetShortDirectoryName(), FileSystemObjectSize.Undefined, FileSystemNodeViewType.Dir);
            _repo.Add(path, nodeToAdd);
            AddToHierarchy(path, nodeToAdd);
        }

        public void AddFile(string path, FileSystemObjectSize size)
        {
            var nodeToAdd = new FileSystemNodeView(Path.GetFileName(path), size);
            AddToHierarchy(path, nodeToAdd);
        }

        public void UpdateSize(string path, FileSystemObjectSize size)
        {
            FileSystemNodeView dirNode;
            if (!_repo.TryGetValue(path, out dirNode))
            {
                return;
            }

            dirNode.Size = size;
        }

        public void SetError(string path)
        {
            FileSystemNodeView dirNode;
            if (!_repo.TryGetValue(path, out dirNode))
            {
                return;
            }

            dirNode.SetNodeType(FileSystemNodeViewType.DirWithIssues);
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            _repo.Clear();
        }

        private void AddToHierarchy(string path, FileSystemNodeView nodeToAdd)
        {
            FileSystemNodeView dir;
            if (TryGetParentFor(path, out dir))
            {
                dir.Children.Add(nodeToAdd);
            }
            else
            {
                Add(nodeToAdd);
            }
        }

        private bool TryGetParentFor(string path, out FileSystemNodeView dir)
        {
            string parentPath;
            if (!PathHelper.TryGetParentPath(path, out parentPath))
            {
                dir = null;
                return false;
            }

            return _repo.TryGetValue(parentPath, out dir);
        }
    }
}