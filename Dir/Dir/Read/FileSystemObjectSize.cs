namespace Dir.Read
{
    public class FileSystemObjectSize
    {
        private readonly long _sizeInBytes;

        public static readonly FileSystemObjectSize Undefined = new FileSystemObjectSize(0);

        public FileSystemObjectSize(long sizeInBytes)
        {
            _sizeInBytes = sizeInBytes;
        }

        public override string ToString()
        {
            if (!IsDefined)
            {
                return "";
            }

            if (_sizeInBytes > 1 << 30)
            {
                return $"{_sizeInBytes/(1 << 30)} GB";
            }

            if (_sizeInBytes > 1 << 20)
            {
                return $"{_sizeInBytes/(1 << 20)} MB";
            }

            if (_sizeInBytes > 1 << 10)
            {
                return $"{_sizeInBytes/(1 << 10)} KB";
            }

            return _sizeInBytes.ToString();
        }

        public bool IsDefined => _sizeInBytes > 0;

        public static implicit operator FileSystemObjectSize(long value)
        {
            return new FileSystemObjectSize(value);
        }
    }
}