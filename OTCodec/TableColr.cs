using System;
using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public class TableColr : OTTable
    {
        // https://docs.microsoft.com/en-us/typography/opentype/spec/colr

        #region Structs used in the context of COLR table

        public struct BaseGlyphRecord
        {
            #region Fields & accessors

            private UInt16 _gID;
            private UInt16 _firstLayerIndex;
            private UInt16 _numLayers;
            private byte _validationStatus;

            public UInt16 GID => _gID;
            public UInt16 FirstLayerIndex => _firstLayerIndex;
            public UInt16 NumLayers => _numLayers;

            #endregion

            #region Public methods

            public void ReadBaseGlyphRecord(MemoryStream ms)
            {
                try
                {
                    _gID = OTFile.ReadUInt16(ms);
                    _firstLayerIndex = OTFile.ReadUInt16(ms);
                    _numLayers = OTFile.ReadUInt16(ms);
                }
                catch (OTDataIncompleteReadException e)
                {
                    _validationStatus |= OTFile.OTRecordValidation_ReadTrunctated;
                    throw new OTDataIncompleteReadException("OT parse error: unable to read base glyph record", e);
                }
            }

            #endregion
        } // struct BaseGlyphRecord

        public struct LayerRecord
        {
            #region Fields & accessors

            private UInt16 _gID;
            private UInt16 _paletteIndex;
            private byte _validationStatus;

            public UInt16 GID => _gID;
            public UInt16 PaletteIndex => _paletteIndex;

            #endregion

            #region Public methods

            public void ReadLayerRecord(MemoryStream ms)
            {
                try
                {
                    _gID = OTFile.ReadUInt16(ms);
                    _paletteIndex = OTFile.ReadUInt16(ms);
                }
                catch (OTDataTypeReadException e)
                {
                    _validationStatus |= OTFile.OTRecordValidation_ReadTrunctated;
                    throw new OTDataIncompleteReadException("OT parse error: unable to read layer record", e);
                }
            }

            #endregion

        } // struct LayerRecord

        #endregion


        #region Constants, fields & accessors

        // private constants
        private const string _tableTag = "head";
        private const uint _latestKnownMinorVersion = 0;
        private const uint _colr_0_headerLength = 3 * sizeof(UInt16) + 2 * sizeof(UInt32);
        private const uint _baseGlyphRecordLength = sizeof(UInt16) * 3;
        private const uint _layerRecordLength = sizeof(UInt16) * 2;


        // private fields

        // base class holds _parentFont, _tableRecord, _calculatedChecksum, _validationStatus
        private UInt16 _version;
        private UInt16 _numBaseGlyphRecords;
        private UInt32 _baseGlyphRecordsOffset;
        private UInt32 _layerRecordsOffset;
        private UInt16 _numLayerRecords;
        private BaseGlyphRecord[] _baseGlyphRecords; // array dimensions: 0 to numBaseGlyphRecords - 1
        private LayerRecord[] _layerRecords; // array dimensions: 0 to numLayerRecords


        // accessors

        public UInt16 Version => _version;
        public UInt16 NumBaseGlyphRecords => _numBaseGlyphRecords;
        public UInt32 BaseGlyphRecordsOffset => _baseGlyphRecordsOffset;
        public UInt32 LayerRecordsOffset => _layerRecordsOffset;
        public UInt16 NumLayerRecords => _numLayerRecords;
        public BaseGlyphRecord[] BaseGlyphRecords => _baseGlyphRecords;
        public LayerRecord[] LayerRecords => _layerRecords;

        #endregion


        #region Constructors

        public TableColr(OTFont parentFont, TableRecord tableRecord)
            : base(parentFont, tableRecord, _tableTag, /* parseWhenConstructed */ true)
        {
            // Base class constructor validates table record values, and marks
            // validation completion status as partial validation
        }

        #endregion


        #region Public instance methods

        public override UInt64 Validate(bool simpleValidationOnly)
        {
            throw new NotImplementedException();
        }

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


            // Read header fields
            // Table has 16-bit (minor) version field, so any version has known fields

            if (_tableRecord.Length < _colr_0_headerLength)
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;

            try
            {
                ms.Seek(_tableRecord.Offset, SeekOrigin.Begin);
                _version = OTFile.ReadUInt16(ms);
                _numBaseGlyphRecords = OTFile.ReadUInt16(ms);
                _baseGlyphRecordsOffset = OTFile.ReadUInt32(ms);
                _layerRecordsOffset = OTFile.ReadUInt32(ms);
                _numLayerRecords = OTFile.ReadUInt16(ms);

            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTDataIncompleteReadException("OT parse error: unable to read " + _tableTag + " table", e);
            }

            if (_version > _latestKnownMinorVersion) 
                _validationStatus |= OTFile.OTFileValidation_StructureMinorVersionUnknown;


            // check length then read base glyph records
            uint requiredLength = _baseGlyphRecordsOffset + _numBaseGlyphRecords * _baseGlyphRecordLength;
            if (_tableRecord.Length < requiredLength) 
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;

            try
            {
                ms.Seek(_tableRecord.Offset + _baseGlyphRecordsOffset, SeekOrigin.Begin);
                _baseGlyphRecords = new BaseGlyphRecord[_numBaseGlyphRecords];
                for (uint i = 0; i < _numBaseGlyphRecords; i++)
                {
                    _baseGlyphRecords[i].ReadBaseGlyphRecord(ms);
                }
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTDataIncompleteReadException("OT parse error: unable to read " + _tableTag + " table", e);
            }


            // check length then read layer records
            requiredLength = _layerRecordsOffset + _numLayerRecords * _layerRecordLength;
            if (_tableRecord.Length < requiredLength)
                _validationStatus |= OTFile.OTFileValidation_TableLengthTooShort;

            try
            {
                ms.Seek(_tableRecord.Offset + _baseGlyphRecordsOffset, SeekOrigin.Begin);
                _layerRecords = new LayerRecord[_numLayerRecords];
                for (uint i = 0; i < _numLayerRecords; i++)
                {
                    _layerRecords[i].ReadLayerRecord(ms);
                }
            }
            catch (OTDataIncompleteReadException e)
            {
                _validationStatus |= OTFile.OTFileValidation_ReadTrunctated;
                throw new OTDataIncompleteReadException("OT parse error: unable to read " + _tableTag + " table", e);
            }

        } // ReadTable_Internal


        private void DoInterTableValidation()
        {
            // TO DO: implement
        }

        #endregion

    } // class TableColr
}
