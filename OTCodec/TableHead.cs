using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace OTCodec
{
    public class TableHead : OTTable
    {
        #region structs used in the context of 'head' table

        public struct HeadFlags
        {
            public const UInt16 Mask_BaselineAt0                        = 0x01;
            public const UInt16 Mask_LeftSideBearingAt0                 = 0x02;
            public const UInt16 Mask_InstructionsDependOnPointSize      = 0x04;
            public const UInt16 Mask_ForceIntegerPPEmCalculations       = 0x08;
            public const UInt16 Mask_InstructionsMayAlterAdvance        = 0x10;
            public const UInt16 Mask_Apple_IsForVertical                = 0x20;
            // bit 6 reserved
            public const UInt16 Mask_Apple_RequiresComplexLayout        = 0x80;
            public const UInt16 Mask_Apple_HasGxDefaultMortEffects      = 0x0100;
            public const UInt16 Mask_Apple_HasStrongRtlGlyphs           = 0x0200;
            public const UInt16 Mask_Apple_HasIndicRearrangementEffects = 0x0400;
            public const UInt16 Mask_IsAgfaMTELossless                  = 0x0800;
            public const UInt16 Mask_IsConverted                        = 0x1000;
            public const UInt16 Mask_IsClearTypeOptimized               = 0x2000;
            public const UInt16 Mask_LastResortFont                     = 0x4000;
            public const UInt16 Mask_Reserved                           = 0x8040;

            public bool BaselineAt0;                        // bit 0
            public bool LeftSideBearingAt0;                 // bit 1
            public bool InstructionsDependOnPointSize;      // bit 2
            public bool ForceIntegerPPEmCalculations;       // bit 3
            public bool InstructionsMayAlterAdvance;        // bit 4
            // bits 5 to 10 are used by Apple but are not used in OpenType
            public bool Apple_IsForVertical;                // bit 5
            public bool Apple_Reserved6;                    // bit 6 -- 0 expected
            public bool Apple_RequiresComplexLayout;        // bit 7
            public bool Apple_HasGxDefaultMortEffects;      // bit 8
            public bool Apple_HasStrongRtlGlyphs;           // bit 9
            public bool Apple_HasIndicRearrangementEffects; // bit 10
            public bool IsAgfaMTELossless;                  // bit 11
            public bool IsConverted;                        // bit 12
            public bool IsClearTypeOptimized;               // bit 13
            public bool LastResortFont;                     // bit 14
            public bool Reserved15;                         // bit 15 -- 0 expected
        }

        public struct HeadMacStyle
        {
            public const UInt16 Mask_Bold = 0x01;
            public const UInt16 Mask_Italic = 0x02;
            public const UInt16 Mask_Underline = 0x04;
            public const UInt16 Mask_Outline = 0x08;
            public const UInt16 Mask_Shadow = 0x10;
            public const UInt16 Mask_Condensed = 0x20;
            public const UInt16 Mask_Extended = 0x40;
            public const UInt16 Mask_Reserved = 0xFF80;

            public bool Bold;       // bit 0
            public bool Italic;     // bit 1
            public bool Underline;  // bit 2
            public bool Outline;    // bit 3
            public bool Shadow;     // bit 4
            public bool Condensed;  // bit 5
            public bool Extended;   // bit 6
            public UInt16 Reserved; // bits 7 to 15 -- 0 expected
        }

        public enum FontDirectionHintVal : Int16
        {
            FullyMixedDirectionGlyphs = 0,
            StrongLtrOnly = 1,
            LTRWithNeutrals = 2,
            StrongRtlOnly = -1,
            RtlWithNeutrals = -2
        }

        #endregion


        #region Constants, fields & accessors

        // private constants
        private const string _tableTag = "head";
        private const uint _head_1_latestKnownMinorVersion = 0;
        private const uint _head_1_0_headerLength = 6 * sizeof(UInt16) + 7 * sizeof(Int16)
            + 3 * sizeof(UInt32) + 2 * sizeof(UInt64);


        // private fields
        // base class holds _parentFont, _tableRecord, _calculatedChecksum, _validationStatus
        private UInt16 _majorVersion;
        private UInt16 _minorVersion;
        private OTFixed _fontRevision;
        private UInt32 _checkSumAdjustment;
        private UInt32 _calculatedCheckSumAdjustment;
        private UInt32 _magicNumber; // Should be set to 0x5F0F3CF5
        private UInt16 _flagsRaw;
        private HeadFlags _flags;  // keep?
        private UInt16 _unitsPerEm;
        private Int64 _createdRaw;
        private Int64 _modifiedRaw;
        private Int16 _xMin;
        private Int16 _yMin;
        private Int16 _xMax;
        private Int16 _yMax;
        private UInt16 _macStyleRaw;
        private HeadMacStyle _macStyle; // keep?
        private UInt16 _lowestRecPPEm;
        private Int16 _fontDirectionHint;   // deprecated, should be set to 0;
        private Int16 _indexToLocFormat;    // must be 0 or 1
        private Int16 _glyphDataFormat;     // must be 0


        // accessors

        public UInt16 MajorVersion => _majorVersion;
        public UInt16 MinorVersion => _minorVersion;
        public OTFixed FontRevision => _fontRevision;
        public UInt32 CheckSumAdjustment => _checkSumAdjustment;
        public UInt32 CalculatedCheckSumAdjustment => _calculatedCheckSumAdjustment;
        public UInt32 MagicNumber => _magicNumber;
        public UInt16 FlagsRaw => _flagsRaw;
        public HeadFlags Flags => _flags;
        public UInt16 UnitsPerEm => _unitsPerEm;
        public Int64 CreatedRaw => _createdRaw;
        public DateTime Created => OTTypeConvert.OTLongDateTimeToDateTime(_createdRaw);
        public Int64 ModifiedRaw => _modifiedRaw;
        public DateTime Modified => OTTypeConvert.OTLongDateTimeToDateTime(_modifiedRaw);
        public Int16 XMin => _xMin;
        public Int16 YMin => _yMin;
        public Int16 XMax => _xMax;
        public Int16 YMax => _yMax;
        public UInt16 MacStyleRaw => _macStyleRaw;
        public HeadMacStyle MacStyle => _macStyle;
        public UInt16 LowestRecPPEm => _lowestRecPPEm;
        public Int16 FontDirectionHint => _fontDirectionHint;
        public Int16 IndexToLocFormat => _indexToLocFormat;
        public Int16 GlyphDataFormat => _glyphDataFormat;

        #endregion


        #region Constructors

        public TableHead(OTFont parentFont, TableRecord tableRecord)
            : base(parentFont, tableRecord, _tableTag, /* parseWhenConstructed */ true)
        {
            // Base class constructor validates table record values, and marks
            // validation completion status as partial validation
        }

        #endregion


        #region Public instance methods

        public override UInt64 Validate(bool simpleValidationOnly)
        {
            /* See comments in OTTable regarding validation completion states and flags.
             * Assumed: this method does not re-do basic validations, and the completion
             * state flags can be set the same as on any prior call to the method.
             */

            /* Base Base class constructor ran partial validations on the table record:
             *   - table record tag matches type for this derived class
             *   - table offset and length are within file bounds
             *   - table record checksum == actual table checksum
             * Additional internal validations for 'head' table:
             *   - magicNumber is set to 0x5F0F3CF5
             *   - if variable font:
             *        flags bit 1 must be set and bit 5 must be 0
             *   - if not variable font:
             *        flags bit 5 should be 0
             *   - flags reserved bits (6, 15) are 0
             *   - flags bits 7 - 10 should be 0 in OT fonts
             *   - unitsPerEm: power of 2 recommended for TrueType outlines
             *   - macStyle reserved bits are 0
             *   - fontDirectionHint: must be -2 to +2 (see enum); should be 2
             *   - indexToLocFormat: must be 0 or 1
             *   - glyphDataFormat: must be 0
             *   
             * Simple validation does not validate various fields that require comparison
             * with other tables:
             *   - hhea.minLeftSideBearing compared to flags bit 1
             *   - if flags bit 14 is set, cmap should have format 13 subtable 
             *   - xMin etc. compared to glyf entries
             *   - indexToLocFormat consistent with loca length and maxp.numGlyphs
             */

            // always do simple validation
            if (_magicNumber != 0x5F0F3CF5)                         _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if (_parentFont.IsVariableFont)
            {
                if ((_flagsRaw & /* bit 1 */ 0x01) != 1)            _validationStatus |= OTFile.OTFileValidation_StructureFieldsExternallyInvalid;
                if ((_flagsRaw & /* bit 5 */ 0x20) != 0)            _validationStatus |= OTFile.OTFileValidation_StructureFieldsExternallyInvalid;
            }
            else
            {
                if ((_flagsRaw & /* bit 5 */ 0x20) != 0)            _validationStatus |= OTFile.OTFileValidation_StructureFieldWarnings;
            }
            if ((_flagsRaw & /* bit 6 */ 0x40) != 0)                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if ((_flagsRaw & /* bit 7 */ 0x80) != 0)                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if ((_flagsRaw & /* bit 8 */ 0x100) != 0)               _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if ((_flagsRaw & /* bit 9 */ 0x200) != 0)               _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if ((_flagsRaw & /* bit 10 */ 0x400) != 0)              _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if ((_flagsRaw & /* bit 15 */ 0x8000) != 0)             _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            double logUnitsPerEm = Math.Log(_unitsPerEm, 2);
            if (Math.Truncate(logUnitsPerEm) - logUnitsPerEm != 0)  _validationStatus |= OTFile.OTFileValidation_StructureFieldWarnings;
            if (_macStyle.Reserved != 0)                            _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if (_fontDirectionHint < -2 || _fontDirectionHint > 2)  _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if (_indexToLocFormat != 0 && _indexToLocFormat != 1)   _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            if (_glyphDataFormat != 0)                              _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;


            // set completion state as after first-time simple validation
            _validationStatus &= ~OTFile.OTFileValidation_PartialValidation;
            _validationStatus |= OTFile.OTFileValidation_InternalValidationOnly;

            if (!simpleValidationOnly)
            {
                // commented out until inter-table validation is implemented

                //DoInterTableValidation(); // TO DO: implement this!

                //// can clear internal-only flag and test for OK status
                //_validationStatus &= ~OTFile.OTFileValidation_InternalValidationOnly;
                //if ((_validationStatus & OTFile.OTFileValidation_ValidationIssueMask) == 0)
                //    _validationStatus = OTFile.OTFileValidation_Valid;
            }

            return _validationStatus;
        } // Validate

        #endregion


        #region Private instance methods

        protected override void ReadTable_Internal()
        {
            // Base class constructor ran partial validations on the table record,
            // including checking that the offset and length are within bounds

            if ((_validationStatus & OTFile.OTFileValidation_StructureOffsetOutOfRange) != 0)
            {
                // out of range so can't read anything
                return;
            }

            MemoryStream ms = _parentFont.MemoryStream;


            // Get the version and check that before continuing
            try
            {
                ms.Seek(_tableRecord.Offset, SeekOrigin.Begin);
                _majorVersion = OTFile.ReadUInt16(ms);
                _minorVersion = OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
            }

            if (_majorVersion != 1)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureVersionNotSupported;
                throw new OTUnknownVersionException("OT parse error: unrecognized " + _tableTag + " version");
            }

            if (_minorVersion > _head_1_latestKnownMinorVersion)
                _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;
            if (_tableRecord.Length < _head_1_0_headerLength)
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;


            // Known version, OK to continue
            try
            {
                _fontRevision = OTFixed.ReadFixed(ms);
                _checkSumAdjustment = OTFile.ReadUInt32(ms);
                _magicNumber = OTFile.ReadUInt32(ms);
                _flagsRaw = OTFile.ReadUInt16(ms);
                _unitsPerEm = OTFile.ReadUInt16(ms);
                _createdRaw = OTFile.ReadOTLongDateTimeAsInt64(ms);
                _modifiedRaw = OTFile.ReadOTLongDateTimeAsInt64(ms);
                _xMin = OTFile.ReadInt16(ms);
                _yMin = OTFile.ReadInt16(ms);
                _xMax = OTFile.ReadInt16(ms);
                _yMax = OTFile.ReadInt16(ms);
                _macStyleRaw = OTFile.ReadUInt16(ms);
                _lowestRecPPEm = OTFile.ReadUInt16(ms);
                _fontDirectionHint = OTFile.ReadInt16(ms);
                _indexToLocFormat = OTFile.ReadInt16(ms);
                _glyphDataFormat = OTFile.ReadInt16(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
            }

            SetHeadFlagValues(); // set HeadFlags struct values
            SetHeadMacStyleValues(); // set HeadMacStyle struct values

        } // ReadTable_Internal

        private void SetHeadFlagValues()
        {
            // assumed that _flagsRaw has already been set

            _flags.BaselineAt0 = ((_flagsRaw & 0x1) != 0);                          // bit 0
            _flags.LeftSideBearingAt0 = ((_flagsRaw & 0x2) != 0);                   // bit 1
            _flags.InstructionsDependOnPointSize = ((_flagsRaw & 0x4) != 0);        // bit 2
            _flags.ForceIntegerPPEmCalculations = ((_flagsRaw & 0x8) != 0);         // bit 3
            _flags.InstructionsMayAlterAdvance = ((_flagsRaw & 0x10) != 0);         // bit 4
            _flags.Apple_IsForVertical = ((_flagsRaw & 0x20) != 0);                 // bit 5
            _flags.Apple_Reserved6 = ((_flagsRaw & 0x40) != 0);                     // bit 6
            _flags.Apple_RequiresComplexLayout = ((_flagsRaw & 0x80) != 0);         // bit 7
            _flags.Apple_HasGxDefaultMortEffects = ((_flagsRaw & 0x100) != 0);      // bit 8
            _flags.Apple_HasStrongRtlGlyphs = ((_flagsRaw & 0x200) != 0);           // bit 9
            _flags.Apple_HasIndicRearrangementEffects = ((_flagsRaw & 0x400) != 0); // bit 10
            _flags.IsAgfaMTELossless = ((_flagsRaw & 0x800) != 0);                  // bit 11
            _flags.IsConverted = ((_flagsRaw & 0x1000) != 0);                       // bit 12
            _flags.IsClearTypeOptimized = ((_flagsRaw & 0x2000) != 0);              // bit 13
            _flags.LastResortFont = ((_flagsRaw & 0x4000) != 0);                    // bit 14
            _flags.Reserved15 = ((_flagsRaw & 0x8000) != 0);                        // bit 15
        }

        private void SetHeadMacStyleValues()
        {
            // assumed that mMacStyleRaw has already been set

            _macStyle.Bold = ((_macStyleRaw & 0x1) != 0);       // bit 0
            _macStyle.Italic = ((_macStyleRaw & 0x2) != 0);     // bit 1
            _macStyle.Underline = ((_macStyleRaw & 0x4) != 0);  // bit 2
            _macStyle.Outline = ((_macStyleRaw & 0x8) != 0);    // bit 3
            _macStyle.Shadow = ((_macStyleRaw & 0x10) != 0);    // bit 4
            _macStyle.Condensed = ((_macStyleRaw & 0x20) != 0); // bit 5
            _macStyle.Extended = ((_macStyleRaw & 0x40) != 0);  // bit 6
            _macStyle.Reserved = (UInt16)(_macStyleRaw & HeadMacStyle.Mask_Reserved);
        }


        protected override void CalculateCheckSum()
        {
            /* The 'head' table requires special handling for calculating a checksum. The
             * process also involves the head.checksumAdjustment field. Both will be
             * calculated.
             * 
             * From OT spec (v1.8.3) font file regarding TableRecord.checkSum for 'head':
             *     To calculate the checkSum for the 'head' table which itself includes the 
             *     checkSumAdjustment entry for the entire font, do the following:
             *       1. Set the checkSumAdjustment to 0.
             *       2. Calculate the checksum for all the tables including the 'head' table
             *          and enter that value into the table directory.
             *
             * NOTE: This wording is unclear and can be misleading. The TableRecord.checkSum
             * for 'head' is calculated using the modified 'head' data only, not the rest of
             * the file.
             * 
             * From OT spec 'head' table regarding checkSumAdjustment:
             *     To compute it: set it to 0, sum the entire font as uint32, 
             *     then store 0xB1B0AFBA - sum.
             * 
             *     If the font is used as a component in a font collection file, the value
             *     of this field will be invalidated by changes to the file structure and
             *     font table directory, and must be ignored. 
             * 
             * If in a TTC, ignore all that and just set both to 0.
             */

            if (_parentFont.File.IsCollection)
            {
                _calculatedChecksum = 0;
                _calculatedCheckSumAdjustment = 0;
                return;
            }

            // get a copy of the byte array segment for the 'head' table
            byte[] fileData = _parentFont.MemoryStream.GetBuffer(); // ref, not copy!

            // make sure to get a four-byte-integral slice
            uint length = (_tableRecord.Length + 3U) & ~3U;
            byte[] headDataSegment = new byte[length];
            Array.ConstrainedCopy(fileData, (int)_tableRecord.Offset, headDataSegment, 0, (int)length);

            // In the copy, clear the checkSumAdjustment field, bytes at relative offset 8 to 11
            byte[] bytes = new byte[4] { 0, 0, 0, 0 };
            bytes.CopyTo(headDataSegment, 8);

            // calculate the checksum for this modified 'head' copy -- that should match what's in TableRecord
            MemoryStream ms = new MemoryStream(headDataSegment, 0, headDataSegment.Length, true, true);
            _calculatedChecksum = OTFile.CalcTableCheckSum(ms, 0, length);


            // Now to calculate checkSumAdjustment: need to checksum the entire file with
            // head.checkSumAdjustment set to 0. Instead of copying the entire file, we
            // can compute checksum in steps: start with the file segment before the
            // 'head' table; then add onto that the modified 'head' copy; then add to 
            // that the remainder of the file.
            //
            // NOTE: C# addition is left associative. We can't calculate three separate
            // checksums and then combine them. We need to start the calculation for the
            // second and third portions using the prior sum as the initial left operand.



            // get checksum for first file segment before the 'head' table
            // (note: head offset must be > 0)
            UInt32 sum = OTFile.CalcTableCheckSum(_parentFont.MemoryStream, 0, _tableRecord.Offset);

            // continue with the modified 'head' segment, passing sum as leftPriorSum
            sum = OTFile.CalcTableCheckSum(ms, 0, length, sum);

            // continue with the remainder of the file
            uint offsetAfterHead = _tableRecord.Offset + length;
            if (offsetAfterHead < _parentFont.File.Length)
            {
                sum = OTFile.CalcTableCheckSum(_parentFont.MemoryStream, offsetAfterHead, (uint)(_parentFont.File.Length - offsetAfterHead), sum);
            }

            // Now get 0xB1B0AFBA - sum
            unchecked
            {
                _calculatedCheckSumAdjustment = 0xB1B0AFBA - sum;
            }

        } // CalculateChecksum

        private void DoInterTableValidation()
        {
            // TO DO: implement
        }

#endregion


} // class TableHead

} // namespace OTCodec
