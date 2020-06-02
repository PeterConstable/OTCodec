using System;
using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public class TableMaxp : OTTable
    {
        #region Constants, fields & accessors

        // private constants
        private const string _tableTag = "maxp";
        private const uint _maxp_0_latestKnownMinorVersion = 5;
        private const uint _maxp_1_latestKnownMinorVersion = 0;
        private const uint _maxp_0_5_headerLength = sizeof(UInt32) + sizeof(UInt16);
        private const uint _maxp_1_0_headerLength = sizeof(UInt32) + 14 * sizeof(UInt16);


        // private fields
        // base class holds _parentFont, _tableRecord, _calculatedChecksum, _validationStatus
        private OTFixed _version;
        private UInt16 _numGlyphs; // version 0.5 ends here
        private UInt16 _maxPoints;
        private UInt16 _maxContours;
        private UInt16 _maxCompositePoints;
        private UInt16 _maxCompositeContours;
        private UInt16 _maxZones;
        private UInt16 _maxTwilightPoints;
        private UInt16 _maxStorage;
        private UInt16 _maxFunctionDefs;
        private UInt16 _maxInstructionDefs;
        private UInt16 _maxStackElements;
        private UInt16 _maxSizeOfInstructions;
        private UInt16 _maxComponentElements;
        private UInt16 _maxComponentDepth; // version 1.0 ends here


        // accessors
        public OTFixed Version => _version;
        public UInt16 NumGlyphs => _numGlyphs;
        public UInt16 MaxPoints => _maxPoints;
        public UInt16 MaxContours => _maxContours;
        public UInt16 MaxCompositePoints => _maxCompositePoints;
        public UInt16 MaxCompositeContours => _maxCompositeContours;
        public UInt16 MaxZones => _maxZones;
        public UInt16 MaxTwilightPoints => _maxTwilightPoints;
        public UInt16 MaxStorage => _maxStorage;
        public UInt16 MaxFunctionDefs => _maxFunctionDefs;
        public UInt16 MaxInstructionDefs => _maxInstructionDefs;
        public UInt16 MaxStackElements => _maxStackElements;
        public UInt16 MaxSizeOfInstructions => _maxSizeOfInstructions;
        public UInt16 MaxComponentElements => _maxComponentElements;
        public UInt16 MaxComponentDepth => _maxComponentDepth;

        #endregion


        #region Constructors

        public TableMaxp(OTFont parentFont, TableRecord tableRecord)
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
             *   - numGlyphs matches entries in loca table
             *   - several comparisons in 'glyf' table
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

            if (_version.Mantissa != 0 && _version.Mantissa != 1)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureVersionNotSupported;
                throw new OTUnknownVersionException("OT parse error: unrecognized " + _tableTag + " version");
            }

            switch (_version.Mantissa)
            {
                case 0:
                    if (_version.FixedTableMinorVersionToInteger() > _maxp_0_latestKnownMinorVersion)
                        _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;
                    if (_tableRecord.Length < _maxp_0_5_headerLength)
                        _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;
                    break;

                case 1:
                    if (_version.FixedTableMinorVersionToInteger() > _maxp_1_latestKnownMinorVersion)
                        _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;
                    if (_tableRecord.Length < _maxp_1_0_headerLength)
                        _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;
                    break;
            }


            // Known version, OK to continue
            try
            {
                _numGlyphs = OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
            }

            if (_version.Mantissa == 1)
            {
                try
                {
                    _maxPoints = OTFile.ReadUInt16(ms);
                    _maxContours = OTFile.ReadUInt16(ms);
                    _maxCompositePoints = OTFile.ReadUInt16(ms);
                    _maxCompositeContours = OTFile.ReadUInt16(ms);
                    _maxZones = OTFile.ReadUInt16(ms);
                    _maxTwilightPoints = OTFile.ReadUInt16(ms);
                    _maxStorage = OTFile.ReadUInt16(ms);
                    _maxFunctionDefs = OTFile.ReadUInt16(ms);
                    _maxInstructionDefs = OTFile.ReadUInt16(ms);
                    _maxStackElements = OTFile.ReadUInt16(ms);
                    _maxSizeOfInstructions = OTFile.ReadUInt16(ms);
                    _maxComponentElements = OTFile.ReadUInt16(ms);
                    _maxComponentDepth = OTFile.ReadUInt16(ms);
                }
                catch (OTDataIncompleteReadException e)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                    throw new OTFileParseException("OT parse error: unable to read " + _tableTag + " table", e);
                }
            }

        } // ReadTable_Internal

        private void DoInterTableValidation()
        {
            // TO DO: implement
        }

        #endregion

    } // class TableMaxp

} // namespace OTCodec
