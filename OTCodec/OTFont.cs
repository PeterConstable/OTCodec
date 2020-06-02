using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace OTCodec
{
    public class OTFont
    {
        #region Fields & accessors

        // private fields

        private OTFile  _file = null;
        private uint    _offsetInFile = 0;
        private string  _defaultLabel = "";                      // a default label identifying the font using file name and TTC index
        private bool    _isInTtc = false;
        private uint    _ttcIndex = 0;
        private OffsetTable _offsetTable;
        private OTTable[] _tables = new OTTable[0];
        private UInt64 _validationStatus = OTFile.OTFileValidation_NotValidated;
        private UInt64[] _validationStatusOfTables;



        public OTFile File => _file;
        protected internal MemoryStream MemoryStream => _file.GetMemoryStream();
        public uint OffsetInFile => _offsetInFile;
        public bool IsWithinTtc => _isInTtc;
        public uint TtcIndex => _ttcIndex;  // struct: initialized, but some fields are objects that may be null
        public OTTag SfntVersionTag => _offsetTable.SfntVersion;  // object may be null
        public OffsetTable OffsetTable => _offsetTable;  // struct: initialized, but some fields are objects that may be null
        public string DefaultFontLabel => _defaultLabel;
        public bool IsVariableFont => this.ContainsTable((OTTag)"fvar") && (this.ContainsTable((OTTag)"gvar") || this.ContainsTable((OTTag)"CFF2"));
        public UInt64 ValidationStatus => _validationStatus;

        #endregion


        #region constructors

        public OTFont()
        {
        }

        // following constructor will result in Offset table for font being read
        // and certain simple tables but not other tables

        public OTFont(OTFile f)
        {
            // used only for single-font file; start of font assumed to be @ 0

            _file = f;
            _offsetInFile = 0;
            _defaultLabel = f.GetFileInfo().Name;

            MemoryStream ms = f.GetMemoryStream();

            ReadFont_Internal(ms);
        }

        public OTFont(OTFile f, uint fileOffset, uint ttcIndex)
        {
            // Used only for TTCs

            _file = f;
            _offsetInFile = fileOffset;
            _ttcIndex = ttcIndex;
            _isInTtc = true;
            _defaultLabel = f.GetFileInfo().Name + ":" + ttcIndex.ToString();

            if (ttcIndex >= f.NumFonts)
                throw new ArgumentOutOfRangeException("TTCIndex", "TTCIndex is greater than the last font index (number of fonts - 1)");

            MemoryStream ms = f.GetMemoryStream();

            if (fileOffset >= ms.Length)
            {
                _validationStatus |= OTFile.OTFileValidation_StructureOffsetOutOfRange;
                throw new ArgumentOutOfRangeException("fileOffset", "The offset in fileOffset is greater than the length of the file");
            }

            ReadFont_Internal(ms);
        }

        #endregion


        #region public instance methods

        public bool ContainsTable(OTTag tag)
        {
            uint? idx = _offsetTable.GetTableRecordIndex(tag);
            return idx.HasValue;
        }

        public bool TryGetTable(OTTag tag, out OTTable table)
        {
            uint? idx = _offsetTable.GetTableRecordIndex(tag);
            if (idx.HasValue)
            {
                table = _tables[idx.Value];
                return true;
            }
            else
            {
                table = null;
                return false;
            }
        }

        public UInt64 Validate(long lengthOfFile, bool simpleValidationOnly)
        {
            _validationStatus = (_offsetTable.Validate(lengthOfFile) & OTFile.OTFileValidation_ValidationIssueMask);

            // validate each table -- simple validation vs. full validation determined on a table-by-table basis
            for (int i = 0; i < _offsetTable.NumTables; i++)
            {
                if (_tables[i] != null)
                {
                    _validationStatusOfTables[i] = _tables[i].Validate(simpleValidationOnly);
                }
            }

            return _validationStatus;
        }

        #endregion


        #region public static methods

        public static bool IsSupportedSfntVersion(OTTag tag)
        {
            if (tag is OTTag)
            {
                if ((tag == new byte[4] { 0, 1, 0, 0 }) || (tag == (OTTag)"OTTO") || (tag == (OTTag)"true"))
                    return true;
                else
                    return false;
            }
            else
                return false;
        }

        public static bool IsKnownTableType(OTTag tag)
        {
            switch (tag.ToString())
            {
                // OT tables
                case "avar":
                case "BASE":
                case "CBDT":
                case "CBLC":
                case "CFF ":
                case "CFF2":
                case "cmap":
                case "COLR":
                case "CPAL":
                case "cvar":
                case "cvt ":
                case "DSIG":
                case "EBDT":
                case "EBLC":
                case "EBSC":
                case "fpgm":
                case "fvar":
                case "gasp":
                case "GDEF":
                case "glyf":
                case "GPOS":
                case "GSUB":
                case "gvar":
                case "hdmx":
                case "head":
                case "hhea":
                case "hmtx":
                case "HVAR":
                case "JSTF":
                case "kern":
                case "loca":
                case "LTSH":
                case "MATH":
                case "maxp":
                case "MERG":
                case "meta":
                case "MVAR":
                case "name":
                case "OS/2":
                case "PCLT":
                case "post":
                case "prep":
                case "sbix":
                case "STAT":
                case "SVG ":
                case "VDMX":
                case "vhea":
                case "vmtx":
                case "VORG":
                case "VVAR":

                // Apple-specific tables
                case "acnt":
                case "ankr":
                case "bdat":
                case "bhed":
                case "bloc":
                case "bsln":
                case "fdsc":
                case "feat":
                case "fmtx":
                case "fond":
                case "gcid":
                case "hsty":
                case "just":
                case "lcar":
                case "ltag":
                case "mort":
                case "morx":
                case "opbd":
                case "prop":
                case "trak":
                case "xref":
                case "Zapf":

                // Graphite-specific tables
                case "Feat":
                case "Glat":
                case "Gloc":
                case "Sill":
                case "Silf":

                // VOLT source table
                case "TSIV":

                // VTT source tables
                case "TSI0":
                case "TSI1":
                case "TSI2":
                case "TSI3":
                case "TSI5":
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSupportedTableType(OTTag tag)
        {
            switch (tag.ToString())
            {
                case "fmtx":
                //case "fvar":
                //case "GPOS":
                //case "GSUB":
                case "head":
                case "hhea":
                case "maxp":
                //case "name":
                //case "OS/2":
                    return true;

                default:
                    return false;
            }
        }

        #endregion


        #region private and internal instance methods

        private void ReadFont_Internal(MemoryStream ms)
        {
            // results in Offset table for font being read, but no other tables

            // assumed: _offsetInFile has already been set and validated as in bounds
            try
            {
                _offsetTable.ReadOffsetTable(ms, _offsetInFile);
            }
            catch (OTTableParseException e)
            {
                throw new OTFontParseException("Unable to parse font data", e);
            }


            // Offset table for font has been read; we know how many and what kind
            // of tables.

            _tables = new OTTable[_offsetTable.NumTables]; // initialized with entries == null
            _validationStatusOfTables = new UInt64[_offsetTable.NumTables]; // initialized with entries == NotValidated

            // The following is something temporary: populate an array of OTTable objects
            // passing each its offset table record. The record is all that's needed to
            // parse each table; but once an object is created (OTTable), it can't change
            // itself into a different type; it can only be substituted with a different
            // type of object, and that substitution would have to be done by something 
            // else.

            for (int i = 0; i < _offsetTable.NumTables; i++)
            {
                if (OTFont.IsSupportedTableType(_offsetTable.TableRecords[i].Tag))
                {
                    switch (_offsetTable.TableRecords[i].Tag.ToString())
                    {
                        case "fmtx":
                            _tables[i] = new TableFmtx(this, _offsetTable.TableRecords[i]);
                            break;
                        case "head":
                            _tables[i] = new TableHead(this, _offsetTable.TableRecords[i]);
                            break;
                        case "hhea":
                            _tables[i] = new TableHhea(this, _offsetTable.TableRecords[i]);
                            break;
                        case "maxp":
                            _tables[i] = new TableMaxp(this, _offsetTable.TableRecords[i]);
                            break;
                    }
                }
                else
                {
                    _tables[i] = new OTTable(this, _offsetTable.TableRecords[i]);
                }
            }

        } // ReadFont_Internal


        #endregion

    } // class OTFont

} // namespace OTCodec
