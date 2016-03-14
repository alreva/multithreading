using Dir.Display;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dir.UnitTests
{
    public static partial class PathHelperTests
    {
        [TestClass]
        public class GetShortDirectoryNameTests
        {
            [TestMethod]
            public void should_return_directory_name_on_path_without_trailing_slash()
            {
                var path = @"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug";
                Assert.AreEqual("Debug", path.GetShortDirectoryName());
            }

            [TestMethod]
            public void should_return_directory_name_on_path_with_trailing_slash()
            {
                var path = @"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug\";
                Assert.AreEqual("Debug", path.GetShortDirectoryName());
            }

            [TestMethod]
            public void should_return_directory_name_for_root()
            {
                var path = @"C:\";
                Assert.AreEqual(@"C:", path.GetShortDirectoryName());
            }

            [TestMethod]
            public void should_return_directory_name_for_network_path()
            {
                var path = @"\\my-network-path\Share";
                Assert.AreEqual(@"Share", path.GetShortDirectoryName());
            }

            [TestMethod]
            public void should_return_directory_name_for_network_root_path()
            {
                var path = @"\\my-network-path";
                Assert.AreEqual(@"my-network-path", path.GetShortDirectoryName());
            }
        }
    }
}
