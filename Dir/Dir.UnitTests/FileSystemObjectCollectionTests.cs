using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dir.Display;
using Dir.Read;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dir.UnitTests
{
    public static partial class FileSystemObjectCollectionTests
    {
        [TestClass]
        public class AddDirTests
        {
            private FileSystemObjectCollection _sut;

            [TestInitialize]
            public void Setup()
            {
                _sut = new FileSystemObjectCollection();
            }

            [TestMethod]
            public void should_add_directory()
            {
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
                Assert.AreEqual(1, _sut.Count);
            }

            [TestMethod]
            public void should_not_add_same_directory_twice()
            {
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
                Assert.AreEqual(1, _sut.Count);
            }

            [TestMethod]
            public void should_build_hierarchy()
            {
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin");
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
                Assert.AreEqual(1, _sut.Count);
                Assert.AreEqual(1, _sut.First().Children.Count);
            }
        }

        [TestClass]
        public class SetSizeTests
        {
            private FileSystemObjectCollection _sut;

            [TestInitialize]
            public void Setup()
            {
                _sut = new FileSystemObjectCollection();
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin");
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
            }

            [TestMethod]
            public void should_update_size_for_exact_item()
            {
                _sut.SetSize(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug", 100);

                Assert.AreEqual("Debug 100", _sut.First().Children.First().ToString());
                Assert.IsFalse(_sut.First().Size.IsDefined);
            }
        }

        [TestClass]
        public class SetErrorTests
        {
            private FileSystemObjectCollection _sut;

            [TestInitialize]
            public void Setup()
            {
                _sut = new FileSystemObjectCollection();
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin");
                _sut.AddDir(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");
            }

            [TestMethod]
            public void should_set_error_for_exact_item()
            {
                _sut.SetError(@"C:\Users\areva\Source\Repos\multithreading\Dir\Dir\bin\Debug");

                Assert.AreEqual(".icons/appbar.warning.circle.png", _sut.First().Children.First().IconPath);
                Assert.AreNotEqual(".icons/appbar.warning.circle.png", _sut.First().IconPath);
            }
        }
    }
}
