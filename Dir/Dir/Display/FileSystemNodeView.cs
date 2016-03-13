using System.Collections.ObjectModel;
using Dir.Read;

namespace Dir.Display
{
    public class FileSystemNodeView
    {
        public FileSystemNodeView(
            string path,
            FileSystemObjectSize size,
            FileSystemNodeViewType type = default(FileSystemNodeViewType),
            params FileSystemNodeView[] children)
        {
            Path = path;
            Size = size;
            SetNodeType(type);
            Children = new ObservableCollection<FileSystemNodeView>(children);
        }

        public string Path { get; set; }
        public FileSystemObjectSize Size { get; set; }
        public string IconPath { get; set; }

        public void SetNodeType(FileSystemNodeViewType type)
        {
            switch (type)
            {
                case FileSystemNodeViewType.Dir:
                    IconPath = ".icons/appbar.folder.png";
                    break;
                case FileSystemNodeViewType.DirWithIssues:
                    IconPath = ".icons/appbar.warning.circle.png";
                    break;
                default:
                    IconPath = ".icons/appbar.page.text.png";
                    break;
            }
        }

        public override string ToString()
        {
            return $"{Path} {Size}";
        }

        public ObservableCollection<FileSystemNodeView> Children { get; set; }
    }
}