using NUnit.Framework;
using System;
using System.IO;
using OTCodec;


namespace OTCodec_NUnitTest
{
    public class TableHeadTests
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


        #region Test cases

        [Test]
        public void HeadTableTest()
        {
            // Table constructor for 'head' will get the offset table record, calculate the table checksum
            // and then read the remainder of the table.

            OTFile f = TestFonts.GetOTFile_SelawikRegular();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            VerifySelawikRegularHead((TableHead)(table));

            f = TestFonts.GetOTFile_CharisSILBoldItalic();
            idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            table = null;
            result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            VerifyCharisSILBoldItalicHead((TableHead)(table));
        }


        [Test]
        public void HeadCalculatedChecksumsTest()
        {
            OTFile f = TestFonts.GetOTFile_AndikaRegular();
            uint? idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            OTTable table;
            bool result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            TableHead head = (TableHead)table;
            Assert.AreEqual(head.CalculatedChecksum, head.TableRecordChecksum);
            Assert.AreEqual(head.CalculatedCheckSumAdjustment, head.CheckSumAdjustment);

            f = TestFonts.GetOTFile_CharisSILBold();
            idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            table = null;
            result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            head = (TableHead)table;
            Assert.AreEqual(head.CalculatedChecksum, head.TableRecordChecksum);
            Assert.AreEqual(head.CalculatedCheckSumAdjustment, head.CheckSumAdjustment);

            f = TestFonts.GetOTFile_CharisSILBoldItalic();
            idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            table = null;
            result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            head = (TableHead)table;
            Assert.AreEqual(head.CalculatedChecksum, head.TableRecordChecksum);
            Assert.AreEqual(head.CalculatedCheckSumAdjustment, head.CheckSumAdjustment);

            f = TestFonts.GetOTFile_NotoNastaliqUrduRegular();
            idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            table = null;
            result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            head = (TableHead)table;
            Assert.AreEqual(head.CalculatedChecksum, head.TableRecordChecksum);
            Assert.AreEqual(head.CalculatedCheckSumAdjustment, head.CheckSumAdjustment);

            f = TestFonts.GetOTFile_SelawikRegular();
            idx = f.GetFont(0).OffsetTable.GetTableRecordIndex((OTTag)("head"));
            Assert.IsTrue(idx.HasValue);
            table = null;
            result = f.GetFont(0).TryGetTable((OTTag)("head"), out table);
            Assert.IsTrue(result);
            head = (TableHead)table;
            Assert.AreEqual(head.CalculatedChecksum, head.TableRecordChecksum);
            Assert.AreEqual(head.CalculatedCheckSumAdjustment, head.CheckSumAdjustment);
        }

        #endregion


        #region Private helper methods

        // internal static since this is called from OTFontTests
        internal static void VerifySelawikRegularHead(TableHead target)
        {
            Assert.IsTrue(target.MajorVersion == 1, "Error: unexpected Version");
            Assert.IsTrue(target.MinorVersion == 0, "Error: unexpected Version");
            Assert.IsTrue(target.FontRevision == (OTFixed)0x0001028f, "Error: unexpected FontRevision");
            Assert.IsTrue(target.CheckSumAdjustment == 0x03E17969, "Error: unexpected CheckSumAdjustment");
            Assert.IsTrue(target.MagicNumber == 0x5F0F3CF5, "Error: unexpected MagicNumber");
            Assert.IsTrue(target.FlagsRaw == 0x0001, "Error: unexpected FlagsRaw");
            Assert.IsTrue(target.UnitsPerEm == 2048, "Error: unexpected UnitsPerEm");
            Assert.IsTrue(target.CreatedRaw == 3_510_942_803, "Error: unexpected CreatedRaw");
            Assert.IsTrue(target.ModifiedRaw == 3_511_866_748, "Error: unexpected ModifiedRaw");
            Assert.IsTrue(target.XMin == -800, "Error: unexpected XMin");
            Assert.IsTrue(target.YMin == -617, "Error: unexpected YMin");
            Assert.IsTrue(target.XMax == 2402, "Error: unexpected XMax");
            Assert.IsTrue(target.YMax == 1976, "Error: unexpected YMax");
            Assert.IsTrue(target.MacStyleRaw == 0, "Error: unexpected MacStyleRaw");
            Assert.IsTrue(target.LowestRecPPEm == 7, "Error: unexpected LowestRecPPEm");
            Assert.IsTrue(target.FontDirectionHint == 2, "Error: unexpected FontDirectionHint");
            Assert.IsTrue(target.IndexToLocFormat == 0, "Error: unexpected IndexToLocFormat");
            Assert.IsTrue(target.GlyphDataFormat == 0, "Error: unexpected GlyphDataFormat");

            TableHead.HeadFlags flags = target.Flags;
            Assert.IsTrue(flags.BaselineAt0, "Error: unexpected Flags value");
            Assert.IsFalse(flags.LeftSideBearingAt0, "Error: unexpected Flags value");
            Assert.IsFalse(flags.InstructionsDependOnPointSize, "Error: unexpected Flags value");
            Assert.IsFalse(flags.ForceIntegerPPEmCalculations, "Error: unexpected Flags value");
            Assert.IsFalse(flags.InstructionsMayAlterAdvance, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_IsForVertical, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_Reserved6, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_RequiresComplexLayout, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasGxDefaultMortEffects, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasStrongRtlGlyphs, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasIndicRearrangementEffects, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsAgfaMTELossless, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsConverted, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsClearTypeOptimized, "Error: unexpected Flags value");
            Assert.IsFalse(flags.LastResortFont, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Reserved15, "Error: unexpected Flags value");

            DateTime c = new DateTime(2015, 4, 3, 21, 53, 23);
            Assert.AreEqual(c, target.Created, "Error: unexpected created date");

            DateTime m = new DateTime(2015, 4, 14, 14, 32, 28);
            Assert.AreEqual(m, target.Modified, "Error: unexpected modified date");

            TableHead.HeadMacStyle macStyle = target.MacStyle;
            Assert.IsFalse(macStyle.Bold, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Italic, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Underline, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Outline, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Shadow, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Condensed, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Extended, "Error: unexpected MacStyle value");
            Assert.IsTrue(macStyle.Reserved == 0, "Error: unexpected MacStyle value");
        }

        internal static void VerifyCharisSILBoldItalicHead(TableHead target)
        {
            Assert.IsTrue(target.MajorVersion == 1, "Error: unexpected Version");
            Assert.IsTrue(target.MinorVersion == 0, "Error: unexpected Version");
            Assert.IsTrue(target.FontRevision == (OTFixed)0x00050000, "Error: unexpected FontRevision");
            Assert.IsTrue(target.CheckSumAdjustment == 0xD3D50036, "Error: unexpected CheckSumAdjustment");
            Assert.IsTrue(target.MagicNumber == 0x5F0F3CF5, "Error: unexpected MagicNumber");
            Assert.IsTrue(target.FlagsRaw == 0x0019, "Error: unexpected FlagsRaw");
            Assert.IsTrue(target.UnitsPerEm == 2048, "Error: unexpected UnitsPerEm");
            Assert.IsTrue(target.CreatedRaw == 3496972867, "Error: unexpected CreatedRaw");
            Assert.IsTrue(target.ModifiedRaw == 3496972890, "Error: unexpected ModifiedRaw");
            Assert.IsTrue(target.XMin == -1517, "Error: unexpected XMin");
            Assert.IsTrue(target.YMin == -1092, "Error: unexpected YMin");
            Assert.IsTrue(target.XMax == 6244, "Error: unexpected XMax");
            Assert.IsTrue(target.YMax == 2600, "Error: unexpected YMax");
            Assert.IsTrue(target.MacStyleRaw == 0x0003, "Error: unexpected MacStyleRaw");
            Assert.IsTrue(target.LowestRecPPEm == 9, "Error: unexpected LowestRecPPEm");
            Assert.IsTrue(target.FontDirectionHint == 2, "Error: unexpected FontDirectionHint");
            Assert.IsTrue(target.IndexToLocFormat == 1, "Error: unexpected IndexToLocFormat");
            Assert.IsTrue(target.GlyphDataFormat == 0, "Error: unexpected GlyphDataFormat");

            TableHead.HeadFlags flags = target.Flags;
            Assert.IsTrue(flags.BaselineAt0, "Error: unexpected Flags value");
            Assert.IsFalse(flags.LeftSideBearingAt0, "Error: unexpected Flags value");
            Assert.IsFalse(flags.InstructionsDependOnPointSize, "Error: unexpected Flags value");
            Assert.IsTrue(flags.ForceIntegerPPEmCalculations, "Error: unexpected Flags value");
            Assert.IsTrue(flags.InstructionsMayAlterAdvance, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_IsForVertical, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_Reserved6, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_RequiresComplexLayout, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasGxDefaultMortEffects, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasStrongRtlGlyphs, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Apple_HasIndicRearrangementEffects, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsAgfaMTELossless, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsConverted, "Error: unexpected Flags value");
            Assert.IsFalse(flags.IsClearTypeOptimized, "Error: unexpected Flags value");
            Assert.IsFalse(flags.LastResortFont, "Error: unexpected Flags value");
            Assert.IsFalse(flags.Reserved15, "Error: unexpected Flags value");

            DateTime c = new DateTime(2014, 10, 24, 5, 21, 7);
            Assert.AreEqual(c, target.Created, "Error: unexpected created date");

            DateTime m = new DateTime(2014, 10, 24, 5, 21, 30);
            Assert.AreEqual(m, target.Modified, "Error: unexpected modified date");

            TableHead.HeadMacStyle macStyle = target.MacStyle;
            Assert.IsTrue(macStyle.Bold, "Error: unexpected MacStyle value");
            Assert.IsTrue(macStyle.Italic, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Underline, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Outline, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Shadow, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Condensed, "Error: unexpected MacStyle value");
            Assert.IsFalse(macStyle.Extended, "Error: unexpected MacStyle value");
            Assert.IsTrue(macStyle.Reserved == 0, "Error: unexpected MacStyle value");
        }

        #endregion

    } // class TableHeadTests

} // namespace OTCodec_NUnitTest
