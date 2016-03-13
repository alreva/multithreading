using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dir.Display;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dir.UnitTests
{
    public static partial class PathHelperTests
    {
        [TestClass]
        public class TryGetParentPathTests
        {
            [TestMethod]
            public void should_return_parent_path_for_file_system_folder()
            {
                string path;
                Assert.IsTrue(PathHelper.TryGetParentPath(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug", out path));
                Assert.AreEqual(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin", path);
            }

            [TestMethod]
            public void should_return_parent_path_for_network_folder()
            {
                string path;
                Assert.IsTrue(PathHelper.TryGetParentPath(@"\\my-network-path\share", out path));
                Assert.AreEqual(@"\\my-network-path", path);
            }

            [TestMethod]
            public void should_return_false_on_null_invalid_parameter()
            {
                string _;
                Assert.IsFalse(PathHelper.TryGetParentPath(null, out _));
            }

            [TestMethod]
            public void should_return_false_on_file_system_root_with_trailing_slash()
            {
                string _;
                Assert.IsFalse(PathHelper.TryGetParentPath(@"C:\", out _));
            }

            [TestMethod]
            public void should_return_false_on_file_system_root_with_no_trailing_slash()
            {
                string _;
                Assert.IsFalse(PathHelper.TryGetParentPath(@"C:", out _));
            }

            [TestMethod]
            public void should_return_false_on_network_root_with_trailing_slash()
            {
                string _;
                Assert.IsFalse(PathHelper.TryGetParentPath(@"\\my-network-path\", out _));
            }

            [TestMethod]
            public void should_return_false_on_network_root_with_no_trailing_slash()
            {
                string _;
                Assert.IsFalse(PathHelper.TryGetParentPath(@"\\my-network-path", out _));
            }
        }
    }
}
