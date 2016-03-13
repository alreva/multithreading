using System.Collections.ObjectModel;

namespace Dir
{
    public class DirNode : FileSystemNode
    {
        public DirNode(
            string path,
            FileSystemObjectSize size,
            params FileSystemNode[] children)
            : base(path, size)
        {
            Children = new ObservableCollection<FileSystemNode>(children);
        }

        public ObservableCollection<FileSystemNode> Children { get; set; }
    }
}