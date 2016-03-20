using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Dir.Read;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dir.UnitTests
{
    public static class EnumFormatterTests
    {
        [TestClass]
        public class ToString2Tests
        {
            [TestMethod]
            [ExpectedException(typeof(ArgumentException))]
            public void should_throw_if_not_an_enum()
            {
                const decimal notAnEnum = 100M;
                notAnEnum.ToString2();
            }

            [TestMethod]
            public void should_return_as_is_if_contains_only_values_from_enum_definition()
            {
                const FileSystemRights sut = FileSystemRights.AppendData | FileSystemRights.ChangePermissions;
                Assert.AreEqual("AppendData, ChangePermissions", sut.ToString2());
            }

            [TestMethod]
            public void should_return_custom_value_if_values_not_only_from_enum_definition()
            {
                const FileSystemRights sut = (FileSystemRights) 270467583;
                Assert.AreEqual("custom (270467583): AppendData, ChangePermissions, CreateFiles, Delete, DeleteSubdirectoriesAndFiles, ExecuteFile, FullControl, Modify, Read, ReadAndExecute, ReadAttributes, ReadData, ReadExtendedAttributes, ReadPermissions, Synchronize, TakeOwnership, Write, WriteAttributes, WriteExtendedAttributes + possibly something else", sut.ToString2());
            }
        }
    }
}
