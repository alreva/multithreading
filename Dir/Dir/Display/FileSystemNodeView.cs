using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Dir.Read;

namespace Dir.Display
{
    public class FileSystemNodeView : INotifyPropertyChanged
    {
        private string _path;
        private FileSystemObjectSize _size;
        private string _iconPath;

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

        public string Path
        {
            get { return _path; }
            set
            {
                if (Equals(_path, value))
                {
                    return;
                }

                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }

        public FileSystemObjectSize Size
        {
            get { return _size; }
            set
            {
                if (Equals(_size, value))
                {
                    return;
                }

                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                if (Equals(_iconPath, value))
                {
                    return;
                }

                _iconPath = value;
                OnPropertyChanged(nameof(IconPath));
            }
        }

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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}