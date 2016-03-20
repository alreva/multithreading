using System.Collections.Specialized;

namespace Dir.Read
{
    public class FileSystemNode
    {
        public FileSystemNode(string path, FileSystemObjectSize size, params NameValue[] properties)
        {
            Path = path;
            Size = size;
            Properties = properties;
        }

        public string Path { get; private set; }
        public FileSystemObjectSize Size { get; private set; }
        public NameValue[] Properties { get; set; }
    }

    public class FileAccessRule
    {
        public FileAccessRule(string rights, string accessorType)
        {
            Rights = rights;
            AccessorType = accessorType;
        }

        public string Rights { get; }

        public string AccessorType { get; }

        public override string ToString()
        {
            return $"{AccessorType} {Rights}";
        }
    }

    public class NameValue
    {
        public NameValue(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }
}