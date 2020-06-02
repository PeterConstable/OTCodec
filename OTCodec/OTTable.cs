using System;
using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public class OTTable // temporarily removing the abstract attribute
    {
        #region Fields & accessors

        protected OTFont _parentFont;
        protected TableRecord _tableRecord;
        protected UInt32 _calculatedChecksum;
        protected UInt64 _validationStatus;


        public OTTag Tag => _tableRecord.Tag;
        public uint TableRecordChecksum => _tableRecord.Checksum;
        public uint CalculatedChecksum => _calculatedChecksum;
        public uint Offset => _tableRecord.Offset;
        public uint Length => _tableRecord.Length;
        public TableRecord TableRecord => _tableRecord;
        public UInt64 ValidationStatus => _validationStatus;

        #endregion


        #region Constructors

        public OTTable(OTFont parentFont, TableRecord tableRecord)
        {
            // This is only used temporarily to create a placeholder table if the table type is not supported.
            _parentFont = parentFont;
            _tableRecord = tableRecord;
            CalculateCheckSum();
            _validationStatus = OTFile.OTFileValidation_PartialValidation | ValidateTableRecord();
        }

        public OTTable(OTFont parentFont, TableRecord tableRecord, string expectedTag, bool parseWhenConstructed)
        {
            // Used for derived classes for specific table types
            if (tableRecord.Tag.ToString() != expectedTag) throw new ArgumentException("TableRecord has the wrong tag for the '" + expectedTag + " table");

            _parentFont = parentFont;
            _tableRecord = tableRecord;
            CalculateCheckSum();
            _validationStatus = OTFile.OTFileValidation_PartialValidation | ValidateTableRecord();

            if (parseWhenConstructed) ReadTable_Internal();
        }

        #endregion


        #region Public instance methods

        public virtual UInt64 Validate(bool simpleValidationOnly)
        {
            /* There are four possible validation states:
             *   - NotValidated: never occurs in an OTTable or sub-class instance.
             *   - Basic validation: initial validations done in OTTable constructor or in
             *     ReadTable_Internal methods of specific-table subclasses.
             *   - Simple validation: after calls to specific-table overrides of this method
             *     with simpleValidationOnly == true.
             *   - Full validation: after calls to specific-table overrides of this method with
             *     simpleValidationOnly == false.
             *     
             * Note: the OTTable base class implementation of this method leaves the completion
             * state as in basic validation only. The specific-table overrides always leave the
             * completion state as simple or full.
             * 
             * After basic validation, the PartialValidation flag will be set, and the
             * InternalValidationOnly flag will be clear.
             * 
             * After simple validation, the partial flag will be clear; the internal-only flag
             * will be set.
             * 
             * After full validation, the partial and internal-only flags will be clear; some
             * flag in the OTFileValidation_FullValidationMask will be set.
             * 
             * Assumed: the specific-table overrides of this method do not re-do basic 
             * validations, and that completion-state flags can be set the same as on any prior 
             * call to the method. 
             */


            return _validationStatus;
        }

        #endregion


        #region Private methods

        // required private method to read table from font file
        protected virtual void ReadTable_Internal() { } // temporarily replace the abstract modifier with virtual

        protected virtual void CalculateCheckSum()
        {
            // This will need to be overridden for the 'head' table
            _calculatedChecksum = OTFile.CalcTableCheckSum(_parentFont.MemoryStream, _tableRecord.Offset, _tableRecord.Length);
        }

        protected UInt64 ValidateTableRecord()
        {
            // Returns validation flags

            UInt64 val = OTFile.OTFileValidation_NotValidated;
            if (_tableRecord.Offset >= _parentFont.MemoryStream.Length)                         val |= OTFile.OTFileValidation_StructureOffsetOutOfRange;
            if (_tableRecord.Offset + _tableRecord.Length > _parentFont.MemoryStream.Length)    val |= OTFile.OTFileValidation_StructureLengthOutOfRange;
            if (_tableRecord.Checksum != _calculatedChecksum)                                   val |= OTFile.OTFileValidation_InvalidChecksum;
            return val;
        }

        #endregion

    } // class OTTable

} // namespace OTCodec
