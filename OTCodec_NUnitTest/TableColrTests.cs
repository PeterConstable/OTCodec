using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    class TableColrTests
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


        // COLR table tests

        [Test]
        public void ColrTableTest()
        {
            // Table constructor for 'COLR' will get the offset table record, calculate the table checksum
            // and then read the remainder of the table.

            OTFile f = TestFonts.GetOTFile_BungeeColorRegular();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("COLR"));
            Assert.IsTrue(idx.HasValue);

            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("COLR"), out table);
            Assert.IsTrue(result);
            VerifyBungeeColor_COLR((TableColr)(table));
        }


        #region Private helper methods

        private void VerifyBungeeColor_COLR(TableColr target)
        {
            Assert.IsTrue(target.Version == 0, "Error: unexpected version");
            Assert.IsTrue(target.NumBaseGlyphRecords == 0x120, "Error: unexpected numBaseGlyphRecords");
            Assert.IsTrue(target.BaseGlyphRecordsOffset == 0x0e, "Error: unexpected baseGlyphRecordsOffset");
            Assert.IsTrue(target.LayerRecordsOffset == 0x06ce, "Error: unexpected layerRecordsOffset");
            Assert.IsTrue(target.NumLayerRecords == 0x0240, "Error: unexpected numLayerRecords");

            TableColr.BaseGlyphRecord bgr = target.BaseGlyphRecords[0];
            Assert.IsTrue(bgr.GID == 0, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.FirstLayerIndex == 0, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.NumLayers == 2, "Error: unexpected BaseGlyphRecord value");

            bgr = target.BaseGlyphRecords[25];
            Assert.IsTrue(bgr.GID == 25, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.FirstLayerIndex == 50, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.NumLayers == 2, "Error: unexpected BaseGlyphRecord value");

            bgr = target.BaseGlyphRecords[184];
            Assert.IsTrue(bgr.GID == 184, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.FirstLayerIndex == 368, "Error: unexpected BaseGlyphRecord value");
            Assert.IsTrue(bgr.NumLayers == 2, "Error: unexpected BaseGlyphRecord value");

            TableColr.LayerRecord lr = target.LayerRecords[0];
            Assert.IsTrue(lr.GID == 288, "Error: unexpected LayerRecord value");
            Assert.IsTrue(lr.PaletteIndex == 0, "Error: unexpected LayerRecord value");

            lr = target.LayerRecords[25];
            Assert.IsTrue(lr.GID == 707, "Error: unexpected LayerRecord value");
            Assert.IsTrue(lr.PaletteIndex == 1, "Error: unexpected LayerRecord value");

            lr = target.LayerRecords[338];
            Assert.IsTrue(lr.GID == 340, "Error: unexpected LayerRecord value");
            Assert.IsTrue(lr.PaletteIndex == 0, "Error: unexpected LayerRecord value");
        }

        #endregion

    } // class TableColrTests

} // namespace OTCodec_NUnitTest
