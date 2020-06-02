using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    class TableHheaTests
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


        [Test]
        public void HheaTableTest1()
        {
            // Table constructor for 'hhea' will get the offset table record, calculate the table checksum
            // and then read the remainder of the table.

            OTFile f = TestFonts.GetOTFile_SelawikRegular();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("hhea"));
            Assert.IsTrue(idx.HasValue);

            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("hhea"), out table);
            Assert.IsTrue(result);
            VerifySelawikRegularHheaTable((TableHhea)(table));
        }


        [Test]
        public void HheaTableTest2()
        {
            // Table constructor for 'hhea' will get the offset table record, calculate the table checksum
            // and then read the remainder of the table.

            OTFile f = TestFonts.GetOTFile_SourceHanSans_Regular();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("hhea"));
            Assert.IsTrue(idx.HasValue);

            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("hhea"), out table);
            Assert.IsTrue(result);
            VerifySourceHanSansHheaTable((TableHhea)(table));
        }


        #region Private helper methods

        private void VerifySelawikRegularHheaTable(TableHhea hhea)
        {
            // verify table record values
            Assert.IsTrue(hhea.Tag.Equals("hhea"), "Error: unexpected hhea table tag");
            Assert.IsTrue(hhea.TableRecordChecksum == 0x0e3003ce, "Error: unexpected hhea table checksum");
            Assert.IsTrue(hhea.CalculatedChecksum == 0x0e3003ce, "Error: unexpected fmtx table checksum");
            Assert.IsTrue(hhea.Offset == 0x0134, "Error: unexpected hhea table offset");
            Assert.IsTrue(hhea.Length == 0x24, "Error: unexpected hhea table length");

            // verify table values
            Assert.IsTrue(hhea.MajorVersion == 1, "Error: unexpected hhea majorVersion");
            Assert.IsTrue(hhea.MinorVersion == 0, "Error: unexpected hhea minorVersion");
            Assert.IsTrue(hhea.Ascender == 2027, "Error: unexpected ascender");
            Assert.IsTrue(hhea.Descender == -431, "Error: unexpected descender");
            Assert.IsTrue(hhea.LineGap == 0, "Error: unexpected lineGap");
            Assert.IsTrue(hhea.AdvanceWidthMax == 2478, "Error: unexpected advanceWidthMax");
            Assert.IsTrue(hhea.MinLeftSideBearing == -800, "Error: unexpected minLeftSideBearing");
            Assert.IsTrue(hhea.MinRightSideBearing == -1426, "Error: unexpected minRightSideBearing");
            Assert.IsTrue(hhea.XMaxExtent == 2402, "Error: unexpected xMaxExtent");
            Assert.IsTrue(hhea.CaretSlopeRise == 1, "Error: unexpected caretSlopeRise");
            Assert.IsTrue(hhea.CaretSlopeRun == 0, "Error: unexpected caretSlopeRun");
            Assert.IsTrue(hhea.CaretOffset == 0, "Error: unexpected caretOffset");
            Assert.IsTrue(hhea.Reserved1 == 0, "Error: unexpected reserved1");
            Assert.IsTrue(hhea.Reserved2 == 0, "Error: unexpected reserved2");
            Assert.IsTrue(hhea.Reserved3 == 0, "Error: unexpected reserved3");
            Assert.IsTrue(hhea.Reserved4 == 0, "Error: unexpected reserved4");
            Assert.IsTrue(hhea.MetricDataFormat == 0, "Error: unexpected metricDataFormat");
            Assert.IsTrue(hhea.NumberOfHMetrics == 352, "Error: unexpected numberOfHMetrics");
        }

        private void VerifySourceHanSansHheaTable(TableHhea hhea)
        {
            // verify table record values
            Assert.IsTrue(hhea.Tag.Equals("hhea"), "Error: unexpected hhea table tag");
            Assert.IsTrue(hhea.TableRecordChecksum == 0x0c12086d, "Error: unexpected hhea table checksum");
            Assert.IsTrue(hhea.CalculatedChecksum == 0x0c12086d, "Error: unexpected fmtx table checksum");
            Assert.IsTrue(hhea.Offset == 0x012de598, "Error: unexpected hhea table offset");
            Assert.IsTrue(hhea.Length == 0x24, "Error: unexpected hhea table length");

            // verify table values
            Assert.IsTrue(hhea.MajorVersion == 1, "Error: unexpected hhea majorVersion");
            Assert.IsTrue(hhea.MinorVersion == 0, "Error: unexpected hhea minorVersion");
            Assert.IsTrue(hhea.Ascender == 0x0488, "Error: unexpected ascender");
            Assert.IsTrue(hhea.Descender == -288, "Error: unexpected descender");
            Assert.IsTrue(hhea.LineGap == 0, "Error: unexpected lineGap");
            Assert.IsTrue(hhea.AdvanceWidthMax == 3000, "Error: unexpected advanceWidthMax");
            Assert.IsTrue(hhea.MinLeftSideBearing == -1002, "Error: unexpected minLeftSideBearing");
            Assert.IsTrue(hhea.MinRightSideBearing == -551, "Error: unexpected minRightSideBearing");
            Assert.IsTrue(hhea.XMaxExtent == 2928, "Error: unexpected xMaxExtent");
            Assert.IsTrue(hhea.CaretSlopeRise == 1, "Error: unexpected caretSlopeRise");
            Assert.IsTrue(hhea.CaretSlopeRun == 0, "Error: unexpected caretSlopeRun");
            Assert.IsTrue(hhea.CaretOffset == 0, "Error: unexpected caretOffset");
            Assert.IsTrue(hhea.Reserved1 == 0, "Error: unexpected reserved1");
            Assert.IsTrue(hhea.Reserved2 == 0, "Error: unexpected reserved2");
            Assert.IsTrue(hhea.Reserved3 == 0, "Error: unexpected reserved3");
            Assert.IsTrue(hhea.Reserved4 == 0, "Error: unexpected reserved4");
            Assert.IsTrue(hhea.MetricDataFormat == 0, "Error: unexpected metricDataFormat");
            Assert.IsTrue(hhea.NumberOfHMetrics == 0xFFFB, "Error: unexpected numberOfHMetrics");
        }

        #endregion


    } // class TableHheaTests

} // namespace OTCodec_NUnitTest
