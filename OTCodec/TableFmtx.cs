using System;
using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public class TableFmtx : OTTable
    {
        // https://developer.apple.com/fonts/TrueType-Reference-Manual/RM06/Chap6fmtx.html

        #region Constants, fields & accessors

        // private constants
        private const string _tableTag = "fmtx";
        private const uint _fmtx_2_latestKnownMinorVersion = 0;
        private const uint _fmtx_2_0_headerLength = 8 * sizeof(byte) + 2 * sizeof(UInt32);

        // private fields
        // base class holds _parentFont, _tableRecord, _calculatedChecksum, _validationStatus
        private OTFixed _version;
        private UInt32 _glyphIndex;
        private byte _horizontalBefore;
        private byte _horizontalAfter;
        private byte _horizontalCaretHead;
        private byte _horizontalCaretBase;
        private byte _verticalBefore;
        private byte _verticalAfter;
        private byte _verticalCaretHead;
        private byte _verticalCaretBase;

        // accessors
        public OTFixed Version => _version;
        public UInt32 GlyphIndex => _glyphIndex;
        public byte HorizontalBefore => _horizontalBefore;
        public byte HorizontalAfter => _horizontalAfter;
        public byte HorizontalCaretHead => _horizontalCaretHead;
        public byte HorizontalCaretBase => _horizontalCaretBase;
        public byte VerticalBefore => _verticalBefore;
        public byte VerticalAfter => _verticalAfter;
        public byte VerticalCaretHead => _verticalCaretHead;
        public byte VerticalCaretBase => _verticalCaretBase;

        #endregion


        #region Constructors

        public TableFmtx(OTFont parentFont, TableRecord tableRecord)
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
             *   
             * For simple validation, there are no additional internal validations.
             * 
             * Simple validation does not validate various fields that require comparison
             * with other tables:
             *   - glyphIndex is valid (compare to maxp or loca)
             *   - point numbers in remaining fields are valid (compare to 'glyf')
             */

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
                _version = OTFixed.ReadFixed(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
            }

            if (_version.Mantissa != 2)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureVersionNotSupported;
                throw new OTUnknownVersionException("OT parse error: unrecognized " + _tableTag + " version");
            }

            if (_version.FixedTableMinorVersionToInteger() > _fmtx_2_latestKnownMinorVersion)
                _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;
            if (_tableRecord.Length < _fmtx_2_0_headerLength)
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;


            // Known version, OK to continue
            try
            {
                _glyphIndex = OTFile.ReadUInt32(ms);
                _horizontalBefore = OTFile.ReadUInt8(ms);
                _horizontalAfter = OTFile.ReadUInt8(ms);
                _horizontalCaretHead = OTFile.ReadUInt8(ms);
                _horizontalCaretBase = OTFile.ReadUInt8(ms);
                _verticalBefore = OTFile.ReadUInt8(ms);
                _verticalAfter = OTFile.ReadUInt8(ms);
                _verticalCaretHead = OTFile.ReadUInt8(ms);
                _verticalCaretBase = OTFile.ReadUInt8(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
            }

        } // ReadTable_Internal

        private void DoInterTableValidation()
        {
            // TO DO: implement
        }


        #endregion

    } // class TableFmtx

} // namespace OTCodec
