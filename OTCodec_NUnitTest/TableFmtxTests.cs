using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    public class TableFmtxTests
    {
        #region Test config

        [SetUp]
        public void Setup()
        {
            // appears to be run before each test
            if (!System.IO.Directory.Exists("TestData"))
            {
                System.IO.Directory.SetCurrentDirectory("..\\..\\..");
            }
            DirectoryAssert.Exists("TestData", "Error: Unable to locate TestData folder.");
        }

        #endregion


        // fmtx table tests
        [Test]
        public void FmtxTableTest()
        {
            // Table constructor for 'fmtx' will get the offset table record, calculate the table checksum
            // and then read the remainder of the table.

            OTFile f = TestFonts.GetOTFile_Skia();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("fmtx"));
            Assert.IsTrue(idx.HasValue);

            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("fmtx"), out table);
            Assert.IsTrue(result);
            VerifySkiaFmtxTable((TableFmtx)(table));
        }


        #region Private helper methods

        private void VerifySkiaFmtxTable(TableFmtx fmtx)
        {
            // verify table record values
            Assert.IsTrue(fmtx.Tag.Equals("fmtx"), "Error: unexpected fmtx table tag");
            Assert.IsTrue(fmtx.TableRecordChecksum == 0x400c0804, "Error: unexpected fmtx table checksum");
            // The Skia font has many incorrect checksum values in table records, so don't verify the 
            // calculated checksum against the table record checksum
            //Assert.IsTrue(fmtx.CalculatedChecksum == 0x400c0804, "Error: unexpected fmtx table checksum");
            Assert.IsTrue(fmtx.Offset == 0x01bc, "Error: unexpected fmtx table offset");
            Assert.IsTrue(fmtx.Length == 0x10, "Error: unexpected fmtx table length");

            // verify table values
            Assert.IsTrue(fmtx.Version == (OTFixed)(0x00020000), "Error: unexpected fmtx version");
            Assert.IsTrue(fmtx.GlyphIndex == 568, "Error: unexpected GlyphIndex");
            Assert.IsTrue(fmtx.HorizontalBefore == 0, "Error: unexpected HorizontalBefore");
            Assert.IsTrue(fmtx.HorizontalAfter == 1, "Error: unexpected HorizontalAfter");
            Assert.IsTrue(fmtx.HorizontalCaretHead == 3, "Error: unexpected HorizontalCaretHead");
            Assert.IsTrue(fmtx.HorizontalCaretBase == 2, "Error: unexpected HorizontalCaretBase");
            Assert.IsTrue(fmtx.VerticalBefore == 4, "Error: unexpected VerticalBefore");
            Assert.IsTrue(fmtx.VerticalAfter == 5, "Error: unexpected VerticalAfter");
            Assert.IsTrue(fmtx.VerticalCaretHead == 7, "Error: unexpected VerticalCaretHead");
            Assert.IsTrue(fmtx.VerticalCaretBase == 6, "Error: unexpected VerticalCaretBase");
        }

        #endregion

    } // class TableFmtxTests

} // namespace OTCodec_NUnitTest
