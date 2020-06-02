using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;

namespace OTCodec
{
    public class TableHhea : OTTable
    {
        // https://docs.microsoft.com/en-us/typography/opentype/spec/hhea

        #region Constants, fields & accessors

        // private constants
        private const string _tableTag = "hhea";
        private const uint _hhea_1_latestKnownVersion = 0;
        private const uint _hhea_1_0_headerLength = 4 * sizeof(UInt16) + 14 * sizeof(Int16);

        // private fields
        // base class holds _parentFont, _tableRecord, _calculatedChecksum, _validationStatus
        private UInt16 _majorVersion;
        private UInt16 _minorVersion;
        private Int16 _ascender;
        private Int16 _descender;
        private Int16 _lineGap;
        private UInt16 _advanceWidthMax;
        private Int16 _minLeftSideBearing;
        private Int16 _minRightSideBearing;
        private Int16 _xMaxExtent;
        private Int16 _caretSlopeRise;
        private Int16 _caretSlopeRun;
        private Int16 _caretOffset;
        // four reserved shorts
        private Int16 _reserved1;
        private Int16 _reserved2;
        private Int16 _reserved3;
        private Int16 _reserved4;
        private Int16 _metricDataFormat;
        private UInt16 _numberOfHMetrics;

        // accessors

        public UInt16 MajorVersion => _majorVersion;
        public UInt16 MinorVersion => _minorVersion;
        public Int16 Ascender => _ascender;
        public Int16 Descender => _descender;
        public Int16 LineGap => _lineGap;
        public UInt16 AdvanceWidthMax => _advanceWidthMax;
        public Int16 MinLeftSideBearing => _minLeftSideBearing;
        public Int16 MinRightSideBearing => _minRightSideBearing;
        public Int16 XMaxExtent => _xMaxExtent;
        public Int16 CaretSlopeRise => _caretSlopeRise;
        public Int16 CaretSlopeRun => _caretSlopeRun;
        public Int16 CaretOffset => _caretOffset;
        public Int16 Reserved1 => _reserved1;
        public Int16 Reserved2 => _reserved2;
        public Int16 Reserved3 => _reserved3;
        public Int16 Reserved4 => _reserved4;
        public Int16 MetricDataFormat => _metricDataFormat;
        public UInt16 NumberOfHMetrics => _numberOfHMetrics;

        #endregion


        #region Constructors

        public TableHhea(OTFont parentFont, TableRecord tableRecord)
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
             * Additional internal validations for 'hhea table:
             *   - reserved fields are set to 0
             *   - metricDataFormat = 0
             *   
             * For simple validation, various fields that require comparison with other 
             * tables are not validated; e.g., advanceWidthMax (requires comparison with
             * 'hmtx')
             */

            // always do simple validation
            if (_reserved1 != 0 || _reserved2 != 0 || _reserved3 != 0 || _reserved4 != 0 || _metricDataFormat != 0)
                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;

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

            if (_minorVersion > _hhea_1_latestKnownVersion)
                _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;
            if (_tableRecord.Length < _hhea_1_0_headerLength)
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;


            // Known version, OK to continue
            try
            {
                _ascender = OTFile.ReadOTFword(ms);
                _descender = OTFile.ReadOTFword(ms);
                _lineGap = OTFile.ReadOTFword(ms);
                _advanceWidthMax = OTFile.ReadOTUfword(ms);
                _minLeftSideBearing = OTFile.ReadOTFword(ms);
                _minRightSideBearing = OTFile.ReadOTFword(ms);
                _xMaxExtent = OTFile.ReadOTFword(ms);
                _caretSlopeRise = OTFile.ReadInt16(ms);
                _caretSlopeRun = OTFile.ReadInt16(ms);
                _caretOffset = OTFile.ReadInt16(ms);
                _reserved1 = OTFile.ReadInt16(ms);
                _reserved2 = OTFile.ReadInt16(ms);
                _reserved3 = OTFile.ReadInt16(ms);
                _reserved4 = OTFile.ReadInt16(ms);
                _metricDataFormat = OTFile.ReadInt16(ms);
                _numberOfHMetrics = OTFile.ReadUInt16(ms);
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

    } // class TableHhea

} // namespace OTCodec
