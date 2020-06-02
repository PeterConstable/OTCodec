using System;
using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public struct OffsetTable
    {
        #region Fields & accessors

        // private fields

        private uint _offsetInFile;
        private OTTag _sfntTag;
        private UInt16 _numTables;
        private UInt16 _searchRange;
        private UInt16 _entrySelector;
        private UInt16 _rangeShift;
        private TableRecord[] _tableRecords;
        private Dictionary<string, uint> _tableMap; // map tag -> index (if duplicates, only first is mapped)
        private UInt64 _validationStatus;


        // accessors

        public uint OffsetInFile => _offsetInFile;
        public OTTag SfntVersion => _sfntTag;
        public UInt16 NumTables => _numTables;
        public UInt16 SearchRange => _searchRange;
        public UInt16 EntrySelector => _entrySelector;
        public UInt16 RangeShift => _rangeShift;
        public TableRecord[] TableRecords => _tableRecords;
        public UInt64 ValidationStatus => _validationStatus;

        #endregion


        #region public instance methods

        public void ReadOffsetTable(MemoryStream ms, uint fileOffset)
        {
            // offset was validated as in bounds by upstream caller; set read position
            _offsetInFile = fileOffset;
            ms.Seek((long)fileOffset, SeekOrigin.Begin);

            _validationStatus = OTFile.OTFileValidation_PartialValidation;

            // check length to read sfnt version tag and read
            if (_offsetInFile + 4 > ms.Length)
            {
                _validationStatus |= OTFile.OTFileValidation_OffsetTableLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }
            try
            {
                _sfntTag = OTTag.ReadTag(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTTableParseException("OT parse error: unable to read Offset Table", e);
            }


            // check that the sfnt version is supported before continuing
            if (!OTFont.IsSupportedSfntVersion(_sfntTag))
            {
                _validationStatus |= OTFile.OTFileValidation_SfntVersionNotSupported;
                throw new OTTableParseException("OT parse error: font resource has an unsupported sfnt version");
            }


            // check length to read remaining header fields, then read
            if (_offsetInFile + 12 > ms.Length)
            {
                _validationStatus |= OTFile.OTFileValidation_OffsetTableLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }
            try
            {
                _numTables = OTFile.ReadUInt16(ms);
                _searchRange = OTFile.ReadUInt16(ms);
                _entrySelector = OTFile.ReadUInt16(ms);
                _rangeShift = OTFile.ReadUInt16(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTTableParseException("OT parse error: unable to read Offset Table", e);
            }


            // check length to read encoding records array, then read the records
            if (_offsetInFile + 12 + _numTables * TableRecord.TableRecordSize > ms.Length)
            {
                _validationStatus |= OTFile.OTFileValidation_OffsetTableLengthOutOfRange | OTFile.OTFileValidation_StructureLengthOutOfRange;
            }
            _tableRecords = new TableRecord[_numTables]; // constructs struct records with default values
            _tableMap = new Dictionary<string, uint>();
            try
            { 
                for (uint i = 0; i < _numTables; i++)
                {
                    _tableRecords[i].ReadTableRecord(ms);
                    try
                    {
                        _tableMap.Add(_tableRecords[i].Tag.ToString(), i);
                    }
                    catch (ArgumentException)
                    {
                        // duplicate tag; first one wins
                        _validationStatus |= OTFile.OTFileValidation_StructureHasDuplicateEntries;
                    }
                }
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTTableParseException("OT parse error: unable to read Offset Table", e);
            }

        } // ReadOffsetTable


        public UInt64 Validate(long fileLength)
        {
            /* ReadOffsetTable method validated that offset and length of the offset table 
             * are within file bounds, that the sfnt version is supported, and that there
             * aren't duplicate records for any given table tag. Other things to validate:
             * 
             * - searchRange, entrySelector and rangeShift fields are valid
             * - offset table records
             */

            uint maxPowerOf2LessThanNumTables = (uint)Math.Pow(2, Math.Floor(Math.Log(_numTables, 2)));
            if (_searchRange != maxPowerOf2LessThanNumTables * 16)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            }
            if (_entrySelector != Math.Log(maxPowerOf2LessThanNumTables, 2))
            {
                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            }
            if (_rangeShift != (_numTables * 16) - _searchRange)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureFieldsInternallyInvalid;
            }

            for (int i = 0; i < _numTables; i++)
            {
                _tableRecords[i].Validate(fileLength);
                if (_tableRecords[i].ValidationStatus != OTFile.OTFileValidation_Valid)
                {
                    _validationStatus |= OTFile.OTFileValidation_ValidationIssueInChildStructure;
                }
            }

            // completed validateions; clear the partial validation flag and check for errors
            _validationStatus &= ~OTFile.OTFileValidation_PartialValidation;
            if ((_validationStatus & OTFile.OTFileValidation_ValidationIssueMask) == 0)
                _validationStatus = OTFile.OTFileValidation_Valid;

            return _validationStatus;
        } // Validate


        public uint? GetTableRecordIndex(OTTag tag)
        {
            if (_tableMap == null) throw new OTInvalidOperationException("GetTableRecordIndex method error: cannot retrieve table records when Offset table has not yet been read");

            uint index;
            if (_tableMap.TryGetValue(tag.ToString(), out index))
            {
                return index;
            }
            else
            {
                return null;
            }
        } // GetTableRecordIndex

        #endregion

    } // class OffsetTable



    public struct TableRecord
    {
        #region constants, fields & accessors

        public const byte TableRecordSize = 16;

        private OTTag   _tableTag;
        private UInt32  _checksum;
        private UInt32  _tableOffset; // offset from start of file -- same for TTC
        private UInt32  _tableLength;
        private UInt64  _validationStatus;


        // accessors

        public OTTag Tag => _tableTag;
        public uint Checksum => _checksum;
        public uint Offset => _tableOffset;
        public uint Length => _tableLength;
        public UInt64 ValidationStatus => _validationStatus;

        #endregion


        #region internal constructor (for testing)

        // internal constructor for test purposes
        // AssemblyInfo.cs includes InternalsVisibleTo("OTCodec_Tests")

        internal TableRecord(OTTag tag, UInt32 checksum, UInt32 offset, UInt32 length)
        {
            _tableTag = tag;
            _checksum = checksum;
            _tableOffset = offset;
            _tableLength = length;
            _validationStatus = OTFile.OTFileValidation_NotValidated;
        }

        #endregion


        #region public instance methods

        public void ReadTableRecord(MemoryStream ms)
        {
            _validationStatus = OTFile.OTFileValidation_PartialValidation;

            if (ms.Length - ms.Position < TableRecordSize)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureLengthOutOfRange;
            }

            try
            {
                _tableTag = OTTag.ReadTag(ms);
                _checksum = OTFile.ReadUInt32(ms);
                _tableOffset = OTFile.ReadUInt32(ms);
                _tableLength = OTFile.ReadUInt32(ms);
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTRecordParseException("OT parse error: unable to read table record", e);
            }
        } // ReadTableRecord


        public UInt64 Validate(long fileLength)
        {
            // Check that tag is defined and known, and that table
            // offset & length are in bounds.

            if (!OTFont.IsKnownTableType(_tableTag)) _validationStatus |= OTFile.OTFileValidation_UnknownTableTag;
            if (!OTFont.IsSupportedTableType(_tableTag)) _validationStatus |= OTFile.OTFileValidation_UnsupportedTableTag;
            if (_tableOffset > fileLength) _validationStatus |= OTFile.OTFileValidation_ReferencedStructureOffsetOutOfRange;
            if (_tableOffset + _tableLength > fileLength) _validationStatus |= OTFile.OTFileValidation_ReferencedStructureLengthOutOfRange;

            // completed validateions; clear the partial validation flag and check for errors
            _validationStatus &= ~OTFile.OTFileValidation_PartialValidation;
            if ((_validationStatus & OTFile.OTFileValidation_ValidationIssueMask) == 0)
                _validationStatus = OTFile.OTFileValidation_Valid;

            return _validationStatus;
        } // Validate

        #endregion

    } // struct TableRecord

} // namespace OTCoded
