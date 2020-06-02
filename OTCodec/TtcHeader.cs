using System;
using System.IO;
using OTCodec;


namespace OTCodec
{
    public struct TtcHeader
    {
        // Defining as a struct as there will only ever be one instance that's held by an OTFile object.

        #region fields & accessors

        private UInt32 _fileOffset;
        private byte[] _tag;
        private UInt16 _majorVersion;
        private UInt16 _minorVersion;
        private UInt32 _numFonts;
        private UInt32[] _offsetTableOffsets; // offset for each font from beginning of file
        // version 2.0 fields:
        private byte[] _dsigTag;
        private UInt32 _dsigLength;
        private UInt32 _dsigOffset;
        private UInt64 _validationStatus;

        public uint FileOffset => _fileOffset;
        public OTTag TtcTag => _tag == null ? null : new OTTag(_tag);
        public UInt16 MajorVersion => _majorVersion;
        public UInt16 MinorVersion => _minorVersion;
        // In earlier OT spec versions, the TTC header version was documented as a ULONG (32-bit)
        public UInt32 VersionAsOTULONG => (UInt32)(_majorVersion << 16) + _minorVersion;
        public UInt32 NumFonts => _numFonts;
        public UInt32[] OffsetTableOffsets => _offsetTableOffsets;
        public OTTag DSIGTag => _dsigTag == null ? null : new OTTag(_dsigTag);
        public UInt32 DSIGLength => _dsigLength;
        public UInt32 DSIGOffset => _dsigOffset;
        public bool HasDSIG => _dsigLength > 0 && _dsigOffset > 0; // Allow for a collection file to have a DSIG table but an incorrect tag in the TTC header
        public UInt32 ExpectedLength
        {
            get
            {
                switch (_majorVersion)
                {
                    case 1:
                        return 12 + _numFonts * 4;
                    case 2:
                        return 24 + _numFonts * 4;
                    default:
                        return 0;
                }
            }
        }
        public UInt64 ValidationStatus => _validationStatus;

        #endregion


        #region public methods

        public void ReadTtcHeader(MemoryStream ms)
        {
            // Makes sure we're at the start of the file.
            ms.Seek(0, SeekOrigin.Begin);
            _fileOffset = 0;

            _validationStatus = OTFile.OTFileValidation_PartialValidation;

            if (ms.Length < (4 + 2 * sizeof(UInt16)))
            {
                _validationStatus |= OTFile.OTFileValidation_TtcHeaderLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }

            try
            {
                _tag = new byte[4];
                if (ms.Read(_tag) < 4)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                    throw new OTDataIncompleteReadException("OT parse error: unable to read sufficient bytes to TTC header tag");
                }

                _majorVersion = OTFile.ReadUInt16(ms);
                _minorVersion = OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read TTC header", e);
            }

            if (_majorVersion != 1 && _majorVersion != 2)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureVersionNotSupported;
                throw new OTUnknownVersionException("OT parse error: unrecognized TTC header version");
            }

            if (ms.Length < (4 + 2 * sizeof(UInt16) + sizeof(UInt32)))
            {
                _validationStatus |= OTFile.OTFileValidation_TtcHeaderLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }

            try
            {
                _numFonts = OTFile.ReadUInt32(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTFileParseException("OT parse error: unable to read TTC header", e);
            }

            if (ms.Length < (4 + 2 * sizeof(UInt16) + (_numFonts + 1) * sizeof(UInt32)))
            {
                _validationStatus |= OTFile.OTFileValidation_TtcHeaderLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }

            _offsetTableOffsets = new uint[_numFonts];
            for (UInt32 i = 0; i < _numFonts; i++)
            {
                try
                {
                    _offsetTableOffsets[i] = OTFile.ReadUInt32(ms);
                }
                catch (OTDataIncompleteReadException e)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                    throw new OTFileParseException("OT parse error: unable to read TTC header", e);
                }
            } // loop over offsets array

            if (_majorVersion == 2)
            {
                // may have DSIG table

                if (ms.Length < (4 + 2 * sizeof(UInt16) + (_numFonts + 4) * sizeof(UInt32)))
                {
                    _validationStatus |= OTFile.OTFileValidation_TtcHeaderLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
                }

                try
                {
                    _dsigTag = new byte[4];
                    if (ms.Read(_dsigTag) < 4)
                    {
                        throw new OTDataIncompleteReadException("OT parse error: unable to read sufficient bytes to DSIG tag");
                    }

                    _dsigLength = OTFile.ReadUInt32(ms);
                    _dsigOffset = OTFile.ReadUInt32(ms);
                }
                catch (OTDataIncompleteReadException e)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                    throw new OTFileParseException("OT parse error: unable to read TTC header", e);
                }
            } // version 2.x

        } // ReadTtcHeader


        public UInt64 Validate(long fileLength)
        {
            /* Assumes that the TtcHeader was already read from the file and
             * partially validated. Other things to validate (assuming a 
             * supported version):
             *   - Offset table offsets are in bounds of the file
             *   - If DSIG offset or length are > 0:
             *        - both DSIG offset and length are > 0 and in bounds
             *        - DSIG tag is valid
             */

            // check offset tables
            for (int i = 0; i < _numFonts; i++)
            {
                if (_offsetTableOffsets[i] > fileLength)
                {
                    _validationStatus |= OTFile.OTFileValidation_TtcOffsetTableOutOfRange;
                    break; // don't need to keep checking
                }
            }

            // check DSIG details

            if (_dsigLength > 0 || _dsigOffset > 0)
            {
                if (_dsigLength == 0 || _dsigOffset == 0)
                {
                    _validationStatus |= OTFile.OTFileValidation_TtcHeaderFieldsInvalid;
                }
                if (_dsigTag != new byte[4] { 0x44, 0x53, 0x49, 0x47 })
                {
                    _validationStatus |= OTFile.OTFileValidation_TtcHeaderFieldsInvalid;
                }
                if (_dsigOffset > fileLength)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReferencedStructureOffsetOutOfRange;
                }
                if ((_dsigOffset + _dsigLength) > fileLength)
                {
                    _validationStatus |= OTFile.OTFileValidation_ReferencedStructureLengthOutOfRange;
                }
            }

            // completed validateions; clear the partial validation flag and check for errors
            _validationStatus &= ~OTFile.OTFileValidation_PartialValidation;
            if ((_validationStatus & OTFile.OTFileValidation_ValidationIssueMask) == 0)
                _validationStatus = OTFile.OTFileValidation_Valid;

            return _validationStatus;
        } // Validate

        #endregion

    } // struct TTCHeader

} // namespace OTCodec
