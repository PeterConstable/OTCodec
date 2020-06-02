using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OTCodec_NUnitTest")]

namespace OTCodec
{
    public class OTFile
    {
        /* OTFile is a representation of an OpenType font file. It holds a memory buffer
         * with binary font data, and provides properties and methods to access that data.
         * Methods are provided to read the data from a file on disk or from a memory
         * buffer.
         */

        #region OTFile validation constants

        // file-wide validation constants

        // when used at OTFile level, validation errors may apply to one or more of the contained offset tables / fonts
        public const uint OTFileValidation_NotValidated                     = 0x00; // struct fields get initialized to 0
        // following two flags are mutually exclusive: one or the other can be set, but never both
        public const uint OTFileValidation_PartialValidation                = 0x01; // some but not all internal validations are done
        public const uint OTFileValidation_InternalValidationOnly           = 0x02; // all internal validations are done, but there are inter-table validations that are not done

        public const uint OTFileValidation_ReadTrunctated                   = 0x04; // read operations returned incomplete data -- use with any structure

        public const uint OTFileValidation_SfntVersionNotSupported          = 0x08;
        public const uint OTFileValidation_TtcHeaderLengthOutOfRange        = 0x10; 
        public const uint OTFileValidation_TtcHeaderFieldsInvalid           = 0x20; // use for validations other than offsets or lengths being in bounds
        public const uint OTFileValidation_TtcOffsetTableOutOfRange         = 0x40; // in collection, one or more offsets to offset tables
        public const uint OTFileValidation_OffsetTableLengthOutOfRange      = 0x80; // use for validations other than offsets or lengths being in bounds
        public const uint OTFileValidation_OffsetTableFieldsInvalid         = 0x100; // use for validations other than offsets or lengths being in bounds
        public const uint OTFileValidation_TableLengthTooShort              = 0x200; // table length is too short to fit header or indicated number of child structures

        public const uint OTFileValidation_StructureOffsetOutOfRange            = 0x400; // use for any structures (offset tables, ttc header or other tables
        public const uint OTFileValidation_StructureLengthOutOfRange            = 0x800; // reported table length or structure size extends beyond the end of the file
        public const uint OTFileValidation_StructureVersionNotSupported         = 0x1000;  // use for any structures (offset tables, ttc header or other tables
        public const uint OTFileValidation_StructureMinorVersionUnknown         = 0x2000; // table/structure has a later minor than known versions
        public const uint OTFileValidation_StructureFieldWarnings               = 0x4000;
        public const uint OTFileValidation_StructureFieldsInternallyInvalid     = 0x8000;
        public const uint OTFileValidation_StructureFieldsExternallyInvalid     = 0x0001_0000;
        public const uint OTFileValidation_StructureHasDuplicateEntries         = 0x0002_0000;
        public const uint OTFileValidation_ReferencedStructureOffsetOutOfRange  = 0x0004_0000;
        public const uint OTFileValidation_ReferencedStructureLengthOutOfRange  = 0x0008_0000;

        public const uint OTFileValidation_UnknownTableTag                      = 0x0010_0000;
        public const uint OTFileValidation_UnsupportedTableTag                  = 0x0020_0000;

        public const uint OTFileValidation_ValidationIssueInChildStructure      = 0x0040_0000;
        public const uint OTFileValidation_InvalidChecksum                      = 0x0080_0000;

        public const uint OTFileValidation_Valid                                = 0x8000_0000;

        public const uint OTFileValidation_ValidationIssueMask                  = 0x7FFF_FFFD;
        public const uint OTFileValidation_IncompleteValidationMask             = 0x0000_0003;
        public const uint OTFileValidation_FullValidationMask                   = 0xFFFF_FFFD;


        // record-level valication contants: single-byte for status of high-occurrence records
        public const byte OTRecordValidation_NotValidated               = 0x00;
        public const byte OTRecordValidation_PartialValidation          = 0x01;
        public const byte OTRecordValidation_InternalValidationOnly     = 0x02; // there are validations against other structures that have not yet been performed
        public const byte OTRecordValidation_ReadTrunctated             = 0x04;
        public const byte OTRecordValidation_InternalFieldWarning       = 0x08; // record field has a non-recommended or deprecated value
        public const byte OTRecordValidation_StructureOffsetOutOfRange  = 0x10; // record field has an offset to a structure that is out of range
        public const byte OTRecordValidation_InternalValidationError    = 0x20; // record field has invalid value per spec for record (no reference to other structures)
        public const byte OTRecordValidation_ExternalValidationError    = 0x40; // record field is not consistent with data elsewhere
        public const byte OTRecordValidation_Valid                      = 0x80;

        /// <summary>
        ///  Maps a single-byte record validation status to longer file-wide validation status flags
        /// </summary>
        /// <param name="recordValidationStatus"></param>
        /// <returns></returns>
        static UInt32 RecordValidationToFileLevelValidation(byte recordValidationStatus)
        {
            UInt32 fileStatus = 0;
            fileStatus |= (UInt32)(recordValidationStatus & OTRecordValidation_PartialValidation);
            fileStatus |= (UInt32)(recordValidationStatus & OTRecordValidation_InternalValidationOnly);
            fileStatus |= (UInt32)(recordValidationStatus & OTRecordValidation_ReadTrunctated);
            fileStatus |= (UInt32)((recordValidationStatus & OTRecordValidation_InternalFieldWarning) << 11);
            fileStatus |= (UInt32)((recordValidationStatus & OTRecordValidation_StructureOffsetOutOfRange) << 14);
            fileStatus |= (UInt32)((recordValidationStatus & OTRecordValidation_InternalValidationError) << 10);
            fileStatus |= (UInt32)((recordValidationStatus & OTRecordValidation_ExternalValidationError) << 10);
            fileStatus |= (UInt32)((recordValidationStatus & OTRecordValidation_Valid) << 24);
            return fileStatus;
        }

        #endregion


        #region Fields & accessors

        private FileInfo        _fi = null;
        private MemoryStream    _ms = null;
        private OTTag           _sfntVersionTag = null;
        private TtcHeader       _ttcHeader = new TtcHeader(); // struct: initialized with ref type members (e.g., _OffsetTableOffsets[]) = null
        private UInt32          _numFonts = 0;
        private OTFont[]        _fonts = null;
        private UInt64          _validationStatus = OTFileValidation_NotValidated;
        private UInt64[]        _validationStatusOfFonts;

        public OTTag SfntVersionTag => _sfntVersionTag;  // null if file not yet opened
        public TtcHeader TtcHeader => _ttcHeader;  // struct, so initialized, but ref type members will be null if not yet read from file
        public uint NumFonts => _numFonts;
        public long Length => _fi == null ? 0 : _fi.Length;
        public bool IsSupportedFileType => IsSupportedSfntVersion(_sfntVersionTag);
        public bool IsCollection => _sfntVersionTag == (OTTag)"ttcf";

        #endregion


        #region Constructors & destructors

        public OTFile()
        {
        }

        ~OTFile()
        {
            if (_ms != null)
            {
                _ms.Close();
                _ms.Dispose();
            }
        }

        #endregion


        #region public instance methods

        public void ReadFromFile(string filePath)
        {
            // construct FileInfo and save it for later reference
            // then call method overload using it

            // FileInfo constructor will throw exceptions if filePath is null,
            // not a valid file path, or if access is denied. It doesn't test
            // if a file exists at that path.
            _fi = new FileInfo(filePath);
            ReadFromFile(_fi);
        }

        public void ReadFromFile(FileInfo fileInfo)
        {
            
            // save fileinfo for later reference
            _fi = fileInfo;

            // Check that file exists. (Handle FileStream exception in advance.)
            if (!_fi.Exists)
            {
                throw new FileNotFoundException("File " + _fi.FullName + " was not found.", _fi.FullName);
            }

            // read file as a byte array, then construct memory stream from byte array
            {
                byte[] bytes;
                try
                {
                    // File.ReadAllBytes opens a filestream and then ensures it is closed
                    bytes = File.ReadAllBytes(_fi.FullName); 
                    _ms = new MemoryStream(bytes, 0, bytes.Length, false, true);
                }
                catch (IOException e)
                {
                    throw e;
                }
            }

            // Read sfntVersion tag
            try
            {
                _sfntVersionTag = OTTag.ReadTag(_ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                throw new OTFileParseException("OT parse error: unable to read sfnt tag", e);
            }

            // supported format?
            if (!IsSupportedSfntVersion(_sfntVersionTag))
            {
                _validationStatus = OTFileValidation_SfntVersionNotSupported;
                throw new OTFileParseException("OT parse error: not a known and supported sfnt type (sfnt tag = " + _sfntVersionTag.ToString());
            }

            _validationStatus = OTFile.OTFileValidation_PartialValidation;

            // TTC? get TTC header
            if (_sfntVersionTag == (OTTag)"ttcf")
            {
                try
                {
                    _ttcHeader.ReadTtcHeader(_ms); // will set read position to start of file
                }
                catch (OTException e)
                {
                    _validationStatus = _ttcHeader.ValidationStatus;
                    throw new OTFileParseException("OT parse error: unable to read TTC header", e);
                }

                _numFonts = _ttcHeader.NumFonts;
            }
            else
            {
                _numFonts = 1;
            }

            // initialize new font object(s); this will get the font headers for each font resource
            _fonts = new OTFont[_numFonts];
            if (_sfntVersionTag == (OTTag)"ttcf")
            {
                for (uint i = 0; i < _numFonts; i++)
                {
                    _fonts[i] = new OTFont(this, _ttcHeader.OffsetTableOffsets[i], i);
                }
            }
            else // single font
            {
                _fonts[0] = new OTFont(this);
            }

            // got TTC header; created font objects and got all the header info for each
            // caller can now poke individual fonts to get additional info as needed

            // finally, get validation status on ttc header, offset tables
            _validationStatusOfFonts = new UInt64[_numFonts];
            for (int i = 0; i < _numFonts; i++)
            {
                // simple validations -- full validation on a table-by-table basis
                _fonts[i].Validate(_ms.Length, true);
            }

            if (_sfntVersionTag == (OTTag)"ttcf")
            {
                // treat ttc header validation as part of the file validation
                _validationStatus |= _ttcHeader.Validate(_ms.Length);

                // check for validation issues in any of the font resources
                for (int i = 0; i < _numFonts; i++)
                {
                    if((_fonts[i].ValidationStatus & OTFile.OTFileValidation_ValidationIssueMask) != 0)
                    {
                        _validationStatus |= OTFile.OTFileValidation_ValidationIssueInChildStructure;
                        break;
                    }
                }
            }
            else
            {
                // treat font resource validation as part of the file validation
                _validationStatus |= (_fonts[0].ValidationStatus & OTFile.OTFileValidation_ValidationIssueMask);
            }

        } // ReadFromFile


        public FileInfo GetFileInfo()
        {
            return _fi;
        }

        public MemoryStream GetMemoryStream()
        {
            return _ms;
        }

        public OTFont GetFont(uint fontIndex) // if called before file has been read, will throw NullReferenceException
        {
            return _fonts[fontIndex];
        }

        #endregion


        #region public static methods

        public static bool IsKnownSfntVersion(OTTag tag)
        {
            if (tag is OTTag)
            {
                if ((tag == new byte[4] { 0, 1, 0, 0 }) || (tag == (OTTag)"OTTO") || (tag == (OTTag)"ttcf") || (tag == (OTTag)"true") || (tag == (OTTag)"typ1"))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static bool IsSupportedSfntVersion(OTTag tag)
        {
            if (tag is OTTag)
            {
                if ((tag == new byte[4] { 0, 1, 0, 0 }) || (tag == (OTTag)"OTTO") || (tag == (OTTag)"ttcf") || (tag == (OTTag)"true"))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static UInt32 CalcTableCheckSum(MemoryStream ms, UInt32 offset, UInt32 length)
        {
            // Call overload with 0 as the left-operand prior sum.
            return CalcTableCheckSum(ms, offset, length, 0);
        }

        public static UInt32 CalcTableCheckSum(MemoryStream ms, UInt32 offset, UInt32 length, UInt32 leftPriorSum)
        {
            // C# addition is left associative, not left-and-right. To combine checksums
            // computed on separate adjacent portions of memory, following UInt32s must
            // be added to the prior sum as the right operand: priorSum + nextVal, not
            // nextVal + priorSum. The leftPriorSum parameter is a starting left-operand
            // value.

            ms.Seek(offset, SeekOrigin.Begin);

            UInt32 sum = leftPriorSum;
            // greedy: length is expected to be multiple of 4 with 0 padding
            UInt32 count = ((length + 3U) & ~3U) / sizeof(UInt32);
            for (int i = 0; i < count; i++)
            {
                unchecked // allow overflow
                {
                    sum += OTFile.ReadUInt32(ms);
                }
            }
            return sum;
        }


        // methods to read data types from stream

        public static sbyte ReadChar(MemoryStream ms)
        {
            return ReadInt8(ms);
        }

        public static sbyte ReadInt8(MemoryStream ms)
        {
            int val;
            val = ms.ReadByte();
            if (val < 0) throw new OTDataIncompleteReadException("OT parse error: unable to read CHAR value");
            if (val > 127) val -= 256;
            return (sbyte)val;
        }

        public static byte ReadByte(MemoryStream ms)
        {
            return ReadUInt8(ms);
        }

        public static byte ReadUInt8(MemoryStream ms)
        {
            int val;
            val = ms.ReadByte();
            if (val < 0) throw new OTDataIncompleteReadException("OT parse error: unable to read BYTE value");
            return (byte)val;
        }

        public static Int16 ReadOTShort(MemoryStream ms)
        {
            return ReadInt16(ms);
        }

        public static Int16 ReadInt16(MemoryStream ms)
        {
            byte[] ab16 = new byte[2];
            if (ms.Read(ab16) < 2)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read int16 value");
            }
            //ab16 = ab16.Reverse().ToArray();
            Array.Reverse(ab16);
            return BitConverter.ToInt16(ab16, 0);
        }

        public static UInt16 ReadOTUshort(MemoryStream ms)
        {
            return ReadUInt16(ms);
        }

        public static UInt16 ReadUInt16(MemoryStream ms)
        {
            byte[] ab16 = new byte[2];
            if (ms.Read(ab16) < 2)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read uint16 value");
            }
            //ab16 = ab16.Reverse().ToArray();
            Array.Reverse(ab16);
            return BitConverter.ToUInt16(ab16, 0);
        }

        public static Int16 ReadOTFword(MemoryStream ms)
        {
            return ReadInt16(ms);
        }

        public static UInt16 ReadOTUfword(MemoryStream ms)
        {
            return ReadUInt16(ms);
        }

        public static Int32 ReadOTLong(MemoryStream ms)
        {
            return ReadInt32(ms);
        }

        public static Int32 ReadInt32(MemoryStream ms)
        {
            byte[] ab32 = new byte[4];
            if (ms.Read(ab32) < 4)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read int32 value");
            }
            //ab32 = ab32.Reverse().ToArray();
            Array.Reverse(ab32);
            return BitConverter.ToInt32(ab32, 0);
        }

        public static UInt32 ReadOTUlong(MemoryStream ms)
        {
            return ReadUInt32(ms);
        }

        public static UInt32 ReadUInt32(MemoryStream ms)
        {
            byte[] ab32 = new byte[4];
            if (ms.Read(ab32) < 4)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read uint32 value");
            }
            Array.Reverse(ab32);
            return BitConverter.ToUInt32(ab32, 0);
        }

        public static Int64 ReadOTLongDateTimeAsInt64(MemoryStream ms)
        {
            byte[] ab64 = new byte[8];
            if (ms.Read(ab64) < 8)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read LONGDATETIME value");
            }
            Array.Reverse(ab64);
            return BitConverter.ToInt64(ab64, 0);
        }


        // methods to access spans of data from file

        public static int GetSpanFromFontFile(MemoryStream ms, UInt32 offset, UInt32 length, out Span<byte> span)
        {
            if (ms == null) throw new ArgumentNullException("ms");
            if (offset >= ms.Length) throw new ArgumentOutOfRangeException("offset", offset, "Error: offset is past the end of the font file");
            if (offset + length > ms.Length) throw new ArgumentOutOfRangeException("length", length, "Error: length extends past the end of the font file");

            span = new byte[length];
            ms.Seek(offset, SeekOrigin.Begin);

            return ms.Read(span);
        }

        public static UInt32 ReadUInt32(Span<byte> span, UInt32 offset)
        {
            if (span == null) throw new ArgumentNullException("span");
            if (offset > span.Length) throw new ArgumentOutOfRangeException("offset", offset, "Error: offset is past the end of the span");
            if (offset + 4 > span.Length) throw new OTOutOfBoundsException("Error: a uint32 at the specified offset would extend beyond the end of the span");

            Span<byte> val = span.Slice((int)(offset), 4);
            byte[] ab32 = val.ToArray();
            Array.Reverse(ab32);
            return BitConverter.ToUInt32(ab32,0);
        }

        #endregion

    } // class OTFile


    #region static helper class OTTypeConvert

    public static class OTTypeConvert
    {
        public static DateTime OTLongDateTimeToDateTime(Int64 longDateTimeValue)
        {
            if (longDateTimeValue > double.MaxValue)
                throw new ArgumentOutOfRangeException("longDateTimeValue", "The LongDateTime value passed in is larger than OTLongDateTimeToDateTime can support");

            DateTime startOfEpoch = new DateTime(1904, 1, 1, 0, 0, 0);
            DateTime result;

            try
            {
                result = startOfEpoch.AddSeconds((double)longDateTimeValue);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException("longDateTimeValue", "The LongDateTime value passed in exceeds the maximum value supported by the DateTime date type");
            }
            return result;
        }

    } // end of class OTTypeConvert

    #endregion

} // namespace OTCodec
