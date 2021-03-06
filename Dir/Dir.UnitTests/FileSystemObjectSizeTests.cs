﻿using Dir.Read;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dir.UnitTests
{
    public static class FileSystemObjectSizeTests
    {
        [TestClass()]
        public class ToStringTests
        {
            [TestMethod()]
            public void greater_than_gigabyte_should_return_size_in_G()
            {
                var size = new FileSystemObjectSize(10000000000);

                string stringRepresentation = size.ToString();

                Assert.AreEqual("9 GB", stringRepresentation);
            }

            [TestMethod()]
            public void greater_than_megabyte_should_return_size_in_M()
            {
                var size = new FileSystemObjectSize(10000000);

                string stringRepresentation = size.ToString();

                Assert.AreEqual("9 MB", stringRepresentation);
            }

            [TestMethod()]
            public void greater_than_kilobyte_should_return_size_in_K()
            {
                var size = new FileSystemObjectSize(10000);

                string stringRepresentation = size.ToString();

                Assert.AreEqual("9 KB", stringRepresentation);
            }

            [TestMethod()]
            public void less_than_kilobyte_should_note_have_postfix()
            {
                var size = new FileSystemObjectSize(100);

                string stringRepresentation = size.ToString();

                Assert.AreEqual("100", stringRepresentation);
            }
        }
    }
}