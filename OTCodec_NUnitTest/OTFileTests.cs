using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    public class OTFileTests
    {
        #region Text config

        [SetUp]
        public void Setup()
        {
            // appears to be run before each test
            if (!System.IO.Directory.Exists("TestData"))
            {
                string threeUpPath = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "..";
                System.IO.Directory.SetCurrentDirectory(threeUpPath);
            }
            DirectoryAssert.Exists("TestData", "Error: Unable to locate TestData folder.");
        }

        #endregion


        #region tests for contstructor, field initialization, destructor

        [Test]
        public void OTFile_DestructEmpty()
        {
            OTFile target = new OTFile();
            try
            {
                target = null;
            }
            catch (Exception)
            {
                Assert.Fail("Error: Unexpected exception occured.");
            }
        }

        [Test]
        public void OTFileFieldInitializationHandlingTest()
        {
            bool caughtExpectedException = false; // will set to true if expected exception is caught

            OTFile target = new OTFile();
            Assert.IsNull(target.GetFileInfo(), "Error: unexpected FileInfo");
            Assert.IsNull(target.GetMemoryStream(), "Error: unexpected FileStream");
            try
            {
                OTFont f = target.GetFont(0);
            }
            catch (NullReferenceException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
            Assert.IsFalse(target.IsCollection, "Error: unexpected value in unopened OTFile");
            Assert.IsFalse(target.IsSupportedFileType, "Error: unexpected value in unopened OTFile");
            Assert.IsTrue(target.Length == 0, "Error: unexpected value in unopened OTFile");
            Assert.IsTrue(target.NumFonts == 0, "Error: unexpected value in unopened OTFile");
            Assert.IsNull(target.SfntVersionTag, "Error: unexpected value in unopened OTFile");
            TtcHeader t = target.TtcHeader;
            Assert.IsTrue(t.FileOffset == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsNull(t.TtcTag, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsTrue(t.MajorVersion == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsTrue(t.MinorVersion == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsTrue(t.NumFonts == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsNull(t.OffsetTableOffsets, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsNull(t.DSIGTag, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsTrue(t.DSIGLength == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsTrue(t.DSIGOffset == 0, "Error: unexpected value in unconfigured TtcHeader");
            Assert.IsFalse(t.HasDSIG, "Error: unexpected value in unconfigured TtcHeader");
        }

        #endregion


        #region tests for GetFileInfo, ReadFromFile, GetMemoryStream

        [Test]
        public void OTFileGetFileInfoTest()
        {
            OTFile target = new OTFile();

            string FilePath = "TestData" + Path.DirectorySeparatorChar + "selawk.ttf";
            try
            {
                target.ReadFromFile(FilePath);
            }
            catch (Exception)
            {
                // unexpected exception
            }

            FileInfo expected = new FileInfo(FilePath);
            FileInfo actual = target.GetFileInfo();
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.IsTrue(actual.Length == expected.Length);
            Assert.IsTrue(actual.Name == "selawk.ttf");
            Assert.IsTrue(actual.Extension == ".ttf");
            Assert.IsTrue(actual.DirectoryName == Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "TestData");
        }

        [Test]
        public void OTFile_ReadFromFileAndGetMemoryStream()
        {
            // construct OTFile and read from test font
            OTFile target = new OTFile();
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "selawk.ttf";
            target.ReadFromFile(FilePath);

            // Get memory stream and take sample
            System.IO.MemoryStream ms = target.GetMemoryStream();
            ms.Seek(0, System.IO.SeekOrigin.Begin);
            byte[] data = new byte[32];
            int cb = ms.Read(data, 0, 32);
            Assert.IsTrue((cb == 32), "Error: Read from memory stream returned less data than expected");

            // Create comparison data and compare
            byte[] expectedData = { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            bool match = true;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != expectedData[i])
                {
                    match = false;
                    break;
                }
            }
            Assert.IsTrue(match, "Error: Read from memory stream returned unexpected data");
        }

        #endregion


        #region tests for OffsetTable & TTC header parsing/properties, GetFont method

        // Exception due to truncated read of sfnt version tag
        [Test]
        public void OTFileOpenTest_InvalidFile_3Bytes()
        {
            bool caughtExpectedException = false; // will set to true if expected exception is caught

            OTFile target = new OTFile();
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "InvalidFile_3Bytes.ttf";
            try
            {
                target.ReadFromFile(FilePath);
            }
            catch (OTFileParseException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        // Exception due to unrecognized sfnt version tag
        [Test]
        public void OTFileOpenTest_UnsupportedSfntTag()
        {
            OTFile target = new OTFile();
            bool caughtExpectedException = false; // will set to true if expected exception is caught

            // test for unrecognized sfnt tag
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "InvalidFile_4Bytes.ttf";
            try
            {
                target.ReadFromFile(FilePath);
            }
            catch (OTFileParseException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileOpenTest_ReadSingleFontOffsetTable()
        {
            OTFile target = new OTFile();
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "selawk.ttf";
            try
            {
                target.ReadFromFile(FilePath);
            }
            catch (Exception)
            {
                // unexpected exception
            }

            Assert.IsTrue(target.SfntVersionTag == (OTTag)(0x00010000), "Error: unexpected sfnt tag");

            OTFont f = target.GetFont(0);
            Assert.IsTrue(f.OffsetInFile == 0, "Error: unexpected font offset");
            Assert.IsTrue(f.SfntVersionTag == (OTTag)(0x00010000), "Error: unexpected font value");
            Assert.IsFalse(f.IsWithinTtc, "Error: unexpected font value");
            Assert.IsTrue(f.OffsetTable.OffsetInFile == 0, "Error: unexpected offset table offset");
            Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)(0x00010000), "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.NumTables == 15, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.SearchRange == 0x0080, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0003, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.RangeShift == 0x0070, "Error: unexpected font offset table value");
        }

        [Test]
        public void OTFileOpenTest_ReadTtcHeaderAndOffsetTables()
        {

            //OTFile target = new OTFile();
            //string FilePath = "TestData" + Path.DirectorySeparatorChar + "CAMBRIA.TTC";
            //try
            //{
            //    target.ReadFromFile(FilePath);
            //}
            //catch (Exception)
            //{
            //    // unexpected exception
            //}

            //Assert.IsTrue(target.SfntVersionTag == (OTTag)("ttcf"), "Error: unexpected sfnt tag");
            //// check one detail of ttc header
            //Assert.IsTrue(target.TtcHeader.DSIGOffset == 0x0018AB54, "Error: unexpected TtcHeader value");
            //Assert.IsTrue(target.NumFonts == 2, "Error: unexpected OTFile value");
            //Assert.IsTrue(target.Length == 1622732, "Error: unexpected OTFile value");
            //Assert.IsTrue(target.IsSupportedFileType, "Error: unexpected OTFile value");
            //Assert.IsTrue(target.IsCollection, "Error: unexpected OTFile value");

            //// now check that offset tables of both fonts are read
            //OTFont f = target.GetFont(0);
            //Assert.IsTrue(f.OffsetInFile == 0x20, "Error: unexpected font offset");
            //Assert.IsTrue(f.SfntVersionTag == (OTTag)(0x00010000), "Error: unexpected font value");
            //Assert.IsTrue(f.IsWithinTtc, "Error: unexpected font value");
            //Assert.IsTrue(f.OffsetTable.OffsetInFile == 0x20, "Error: unexpected offset table offset");
            //Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)(0x00010000), "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.NumTables == 20, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.SearchRange == 0x0100, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0004, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.RangeShift == 64, "Error: unexpected font offset table value");

            //f = target.GetFont(1);
            //Assert.IsTrue(f.OffsetInFile == 0x16C, "Error: unexpected font offset");
            //Assert.IsTrue(f.SfntVersionTag == (OTTag)(0x00010000), "Error: unexpected font value");
            //Assert.IsTrue(f.IsWithinTtc, "Error: unexpected font value");
            //Assert.IsTrue(f.OffsetTable.OffsetInFile == 0x16C, "Error: unexpected offset table offset");
            //Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)(0x00010000), "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.NumTables == 21, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.SearchRange == 0x0100, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0004, "Error: unexpected font offset table value");
            //Assert.IsTrue(f.OffsetTable.RangeShift == 80, "Error: unexpected font offset table value");


            // Alternate test using open-source font, SourceHanSans-Regular.ttc
            OTFile target = new OTFile();
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "SourceHanSans-Regular.TTC";
            try
            {
                target.ReadFromFile(FilePath);
            }
            catch (Exception)
            {
                // unexpected exception
            }

            Assert.IsTrue(target.SfntVersionTag == (OTTag)("ttcf"), "Error: unexpected sfnt tag");
            // check one detail of ttc header
            // SourceHanSans has a v1.0 ttc header (no DSIG table)
            //Assert.IsTrue(target.TtcHeader.DSIGOffset == 0x0018AB54, "Error: unexpected TtcHeader value");
            Assert.IsTrue(target.TtcHeader.OffsetTableOffsets[0] == 0x00000034, "Error: unexpected table directory offset");
            Assert.IsTrue(target.NumFonts == 10, "Error: unexpected OTFile value");
            Assert.IsTrue(target.Length == 20331260, "Error: unexpected OTFile value");
            Assert.IsTrue(target.IsSupportedFileType, "Error: unexpected OTFile value");
            Assert.IsTrue(target.IsCollection, "Error: unexpected OTFile value");

            // now check that offset tables of some of the fonts are read
            OTFont f = target.GetFont(0);
            Assert.IsTrue(f.OffsetInFile == 0x34, "Error: unexpected font offset");
            Assert.IsTrue(f.SfntVersionTag == (OTTag)("OTTO"), "Error: unexpected font value");
            Assert.IsTrue(f.IsWithinTtc, "Error: unexpected font value");
            Assert.IsTrue(f.OffsetTable.OffsetInFile == 0x34, "Error: unexpected offset table offset");
            Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)("OTTO"), "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.NumTables == 16, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.SearchRange == 0x0100, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0004, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.RangeShift == 0, "Error: unexpected font offset table value");

            f = target.GetFont(2);
            Assert.IsTrue(f.OffsetInFile == 0x24C, "Error: unexpected font offset");
            Assert.IsTrue(f.SfntVersionTag == (OTTag)("OTTO"), "Error: unexpected font value");
            Assert.IsTrue(f.IsWithinTtc, "Error: unexpected font value");
            Assert.IsTrue(f.OffsetTable.OffsetInFile == 0x24C, "Error: unexpected offset table offset");
            Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)("OTTO"), "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.NumTables == 16, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.SearchRange == 0x0100, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0004, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.RangeShift == 0, "Error: unexpected font offset table value");

            f = target.GetFont(8);
            Assert.IsTrue(f.OffsetInFile == 0x894, "Error: unexpected font offset");
            Assert.IsTrue(f.SfntVersionTag == (OTTag)("OTTO"), "Error: unexpected font value");
            Assert.IsTrue(f.IsWithinTtc, "Error: unexpected font value");
            Assert.IsTrue(f.OffsetTable.OffsetInFile == 0x894, "Error: unexpected offset table offset");
            Assert.IsTrue(f.OffsetTable.SfntVersion == (OTTag)("OTTO"), "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.NumTables == 16, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.SearchRange == 0x0100, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.EntrySelector == 0x0004, "Error: unexpected font offset table value");
            Assert.IsTrue(f.OffsetTable.RangeShift == 0, "Error: unexpected font offset table value");
        }

        #endregion


        #region tests for OTFile static helpers

        [Test]
        public void OTFileIsKnownSfntVersionTest()
        {
            OTTag target = new OTTag(0x00010000);
            Assert.IsTrue(OTFile.IsKnownSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("OTTO");
            Assert.IsTrue(OTFile.IsKnownSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("ttcf");
            Assert.IsTrue(OTFile.IsKnownSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("true");
            Assert.IsTrue(OTFile.IsKnownSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("typ1");
            Assert.IsTrue(OTFile.IsKnownSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("xxxx");
            Assert.IsFalse(OTFile.IsKnownSfntVersion(target), "Error: invalid tag recognized");
        }

        [Test]
        public void OTFileIsSupportedSfntVersionTest()
        {
            OTTag target = new OTTag(0x00010000);
            Assert.IsTrue(OTFile.IsSupportedSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("OTTO");
            Assert.IsTrue(OTFile.IsSupportedSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("ttcf");
            Assert.IsTrue(OTFile.IsSupportedSfntVersion(target), "Error: valid tag not recognized");
            target = new OTTag("true");
            Assert.IsTrue(OTFile.IsSupportedSfntVersion(target), "Error: invalid tag recognized");
            target = new OTTag("typ1");
            Assert.IsFalse(OTFile.IsSupportedSfntVersion(target), "Error: invalid tag recognized");
            target = new OTTag("xxxx");
            Assert.IsFalse(OTFile.IsSupportedSfntVersion(target), "Error: invalid tag recognized");
        }

        [Test]
        public void OTFileCalcTableChecksumTest()
        {
            // construct OTFile, read from test font and get memory stream
            OTFile target = new OTFile();
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "selawk.ttf";
            target.ReadFromFile(FilePath);
            System.IO.MemoryStream ms = target.GetMemoryStream();

            // Selawik GPOS table has a checksum of 0x8969_4DB2
            UInt32 expected = 0x8969_4DB2;
            // GPOS offset: 0x7BC4; length: 0x114C;
            UInt32 actual = OTFile.CalcTableCheckSum(ms, 0x7BC4, 0x114C);
            Assert.AreEqual(actual, expected);
        }

        #endregion


        #region tests for OTFile datatype read static methods

        [Test]
        public void OTFileReadInt8Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F
            Assert.IsTrue(OTFile.ReadInt8(ms) == 0x0F, "Error: unexpected value read");
            ms.Position = 16; // 0xF0
            Assert.IsTrue(OTFile.ReadInt8(ms) == -16, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadInt8(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadInt8(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileReadUInt8Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F
            Assert.IsTrue(OTFile.ReadUInt8(ms) == 0x0F, "Error: unexpected value read");
            ms.Position = 16; // 0xF0
            Assert.IsTrue(OTFile.ReadUInt8(ms) == 0xF0, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadUInt8(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadUInt8(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileReadInt16Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F, 0x00
            Assert.IsTrue(OTFile.ReadInt16(ms) == 0x0F00, "Error: unexpected value read");
            ms.Position = 16; // 0xF0, 0x54
            Assert.IsTrue(OTFile.ReadInt16(ms) == -4012, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadInt16(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 1;
            caughtExpectedException = false;
            try
            {
                OTFile.ReadInt16(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 2; // 0x45, 0x46
            Assert.IsTrue(OTFile.ReadInt16(ms) == 0x4546, "Error: expected value read");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadInt16(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileReadUInt16Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F, 0x00
            Assert.IsTrue(OTFile.ReadUInt16(ms) == 0x0F00, "Error: unexpected value read");
            ms.Position = 16; // 0xF0, 0x54
            Assert.IsTrue(OTFile.ReadUInt16(ms) == 0xF054, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 1;
            caughtExpectedException = false;
            try
            {
                OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 2;
            Assert.IsTrue(OTFile.ReadUInt16(ms) == 0x4546, "Error: expected value read");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadUInt16(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileReadInt32Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F, 0x00, 0x80, 0x00
            Assert.IsTrue(OTFile.ReadInt32(ms) == 0x0F00_8000, "Error: unexpected value read");
            ms.Position = 16; // 0xF0, 0x54, 0x3E, 0x26
            Assert.IsTrue(OTFile.ReadInt32(ms) == -262_914_522, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadInt32(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 3;
            caughtExpectedException = false;
            try
            {
                OTFile.ReadInt32(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 4;
            Int32 expected = 0x4744_4546;
            Assert.IsTrue(OTFile.ReadInt32(ms) == expected, "Error: expected value read");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadInt32(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileReadUInt32Test()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F, 0x00, 0x80, 0x00
            Assert.IsTrue(OTFile.ReadUInt32(ms) == 0x0F00_8000, "Error: unexpected value read");
            ms.Position = 16; // 0xF0, 0x54, 0x3E, 0x26
            Assert.IsTrue(OTFile.ReadUInt32(ms) == 0xF054_3E26, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadUInt32(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 3;
            caughtExpectedException = false;
            try
            {
                OTFile.ReadUInt32(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 4;
            Assert.IsTrue(OTFile.ReadUInt32(ms) == 0x4744_4546, "Error: expected value read");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadUInt32(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        [Test]
        public void OTFileGetOTLongDateTimeAsInt64()
        {
            byte[] b = new byte[32] { 0, 1, 0, 0, 0, 0x0F, 0, 0x80, 0, 3, 0, 0x70, 0x44, 0x53, 0x49, 0x47, 0xF0, 0x54, 0x3E, 0x26, 0, 0, 0x91, 0xE4, 0, 0, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46 };
            MemoryStream ms = new MemoryStream(b);

            ms.Position = 5; // 0x0F, 0x00, 0x80, 0x00, 0x03, 0x00, 0x70, 0x44
            Assert.IsTrue(OTFile.ReadOTLongDateTimeAsInt64(ms) == 0x0F00_8000_0300_7044, "Error: unexpected value read");
            ms.Position = 16; //  0xF0, 0x54, 0x3E, 0x26, 0x00, 0x00, 0x91, 0xE4
            Assert.IsTrue(OTFile.ReadOTLongDateTimeAsInt64(ms) == -1_129_209_273_633_435_164, "Error: unexpected value read");

            bool caughtExpectedException = false;
            ms.Position = ms.Length;
            try
            {
                OTFile.ReadOTLongDateTimeAsInt64(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 7;
            caughtExpectedException = false;
            try
            {
                OTFile.ReadOTLongDateTimeAsInt64(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            ms.Position = ms.Length - 8; // 0x00, 0x00, 0x1A, 0xDC, 0x47, 0x44, 0x45, 0x46
            Assert.IsTrue(OTFile.ReadOTLongDateTimeAsInt64(ms) == 0x0000_1ADC_4744_4546, "Error: expected value read");

            ms.Close();
            caughtExpectedException = false;
            try
            {
                OTFile.ReadOTLongDateTimeAsInt64(ms);
            }
            catch (ObjectDisposedException)
            {
                caughtExpectedException = true;
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        #endregion


        #region tests of OTTypeConvert class

        [Test]
        public void OTConvertOTLongDateTimeToDateTimeTest()
        {
            /* OTCodec.Convert.OTLongDateTimeToDateTime takes a value representing
             * a number of seconds since 12:00:00 midnight, January 1, 1904 and
             * converts it into a System.DateTime value.
             */

            DateTime startOfEpoch = new DateTime(1904, 1, 1, 0, 0, 0);
            DateTime expected = new DateTime(2014, 4, 5, 22, 48, 32);
            TimeSpan delta = expected - startOfEpoch;
            Int64 deltaSeconds = delta.Seconds + delta.Minutes * 60 + delta.Hours * 3600 + (Int64)delta.Days * 86400;
            DateTime actual = OTTypeConvert.OTLongDateTimeToDateTime(deltaSeconds);
            Assert.IsTrue(actual == expected, "Error: unexpected DateTime value");
        }

        #endregion

    } // class OTFileTests

    internal class TestFonts
    {
        internal static OTFile GetOTFile_ADMSB()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "ADMSB___.TTF";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_AndikaRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "Andika-R.TTF";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_BungeeColorRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "BungeeColor-Regular_colr_Windows.TTF";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        //internal static OTFile GetOTFile_CambriaTtc()
        //{
        //    string FilePath = "TestData" + Path.DirectorySeparatorChar + "CAMBRIA.TTC";
        //    OTFile f = new OTFile();
        //    f.ReadFromFile(FilePath);
        //    return f;
        //}

        internal static OTFile GetOTFile_CharisSILRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "CharisSIL-R.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_CharisSILBold()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "CharisSIL-B.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_CharisSILItalic()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "CharisSIL-I.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_CharisSILBoldItalic()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "CharisSIL-BI.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_NotoNastaliqUrduRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "NotoNastaliqUrdu-Regular.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_NotoSansMalayalamRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "NotoSansMalayalam-Regular.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_ScheherazadeRegular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "Scheherazade-Regular.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_SelawikRegular()
        {
            string FilePath = "TestData" + Path.AltDirectorySeparatorChar + "selawk.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        // Skia.ttf is the only font I know of that has an 'fmtx' table
        internal static OTFile GetOTFile_Skia()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "Skia.ttf";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }

        internal static OTFile GetOTFile_SourceHanSans_Regular()
        {
            string FilePath = "TestData" + Path.DirectorySeparatorChar + "SourceHanSans-Regular.ttc";
            OTFile f = new OTFile();
            f.ReadFromFile(FilePath);
            return f;
        }
        
    } // class TestFonts

} // namespace OTCodec_NUnitTest