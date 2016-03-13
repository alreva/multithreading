namespace Dir
{
    public class FileSystemNode
    {
        public FileSystemNode(string path, FileSystemObjectSize size)
        {
            Path = path;
            Size = size;
        }

        public string Path { get; set; }
        public FileSystemObjectSize Size { get; set; }

        public override string ToString()
        {
            return $"{Path} {Size}";
        }
    }
}