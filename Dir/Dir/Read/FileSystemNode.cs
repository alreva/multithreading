namespace Dir.Read
{
    public class FileSystemNode
    {
        public FileSystemNode(string path, FileSystemObjectSize size)
        {
            Path = path;
            Size = size;
        }

        public string Path { get; private set; }
        public FileSystemObjectSize Size { get; private set; }
    }
}