using System;
using System.Collections.Generic;
using System.IO;
using OTCodec;

namespace OTCodec
{
    public struct OTFixed
    {
        /* Fixed is described in the OT spec as "32-bit signed fixed-point number (16.16)". 
         * 
         * In practice, prior to OT version 1.8.x, Fixed was used inconsistently as the specified type
         * for fields in several tables. In particular, the fractional portion of OT Fixed values
         * had been treated in two different ways. 
         * 
         * As defined, the fractional portion should represent fractional units of 1/65536. Fixed is
         * used in this way in several tables; for example, in VariationAxisRecords in the 'fvar'
         * table, and the italicAngle field in the 'post' table.
         * 
         * But Fixed has also been used for the version field of several tables with the fractional
         * portion interpreted as as though the hex value is read as a literal with an implicit ".".
         * This has occurred for the 'maxp', 'post' and 'vhea' tables, all of which include a non-
         * integer version. For instance, there is a version "0.5" 'maxp' table, and the "Fixed" data
         * representation for that is 0x00005000.
         * 
         * (Some Apple-specific tables have only integer versions but are documented as using Fixed
         * as the type for the version field.)
         * 
         * (In this alternate usage, Only the first (high-order) nibble of the fractional portion is 
         * ever used. Thus, the fractional portion could be shifted right by 12 bits and interpreted 
         * as 10ths. When a minor version of the GDEF table -- version 1.2 -- was added in OT 1.6, the 
         * value 0x00010002 was used and the type was changed from FIXED to ULONG, apparently in
         * recognition of the confusion from prior use of "Fixed" in relation to minor versions.)
         * 
         * Note: This usage in version fields with minor versions has led to some incorrect usage in 
         * other non-version fields. For instance, in the fontRevision field of the 'head' table, a 
         * value of "5.01" would normally be stored as 0x0005028F (0x28F = 655 decimal, = 65536 / 100).
         * However, in some fonts such a fontRevision value would be represented as 0x00050100.
         */

        #region Fields & accessors

        // private fields
        private byte[] _rawBytes;
        private Int16 _mantissa;
        private UInt16 _fraction;

        // accessors
        public Int16 Mantissa { get { return this._mantissa; } }
        public UInt16 Fraction { get { return this._fraction; } }

        #endregion


        #region constructors

        public OTFixed(Int16 mantissa, UInt16 fraction)
        {
            _mantissa = mantissa;
            _fraction = fraction;

            _rawBytes = new byte[4];
            byte[] m = BitConverter.GetBytes(_mantissa);
            byte[] f = BitConverter.GetBytes(_fraction);
            Array.Copy(m, 0, _rawBytes, 2, 2);
            Array.Copy(f, 0, _rawBytes, 0, 2);
            Array.Reverse(_rawBytes);
        }

        public OTFixed(byte[] buffer)
        {
            // bytes are ordered big-endian as they would occur in an OT file

            if (buffer.Length != 4) throw new ArgumentException("Byte array in buffer must have four elements", "buffer");

            _rawBytes = buffer;
            Array.Reverse(buffer);
            _mantissa = BitConverter.ToInt16(buffer, 2);
            _fraction = BitConverter.ToUInt16(buffer, 0);
        }

        /// <summary>
        /// Constructs a Fixed from a UInt32 by re-interpreting the big-endian byte
        /// sequence of the UInt32 as a Fixed byte sequence.
        /// </summary>
        /// <param name="val"></param>
        public OTFixed(UInt32 val)
        {
            // high-order word is mantissa; low-order word is fraction
            _mantissa = (Int16)((val & 0xffff0000) >> 16);
            _fraction = (UInt16)(val & 0xffff);

            _rawBytes = new byte[4];
            _rawBytes[0] = (byte)((val & 0xFF000000) >> 24);
            _rawBytes[1] = (byte)((val & 0xFF0000) >> 16);
            _rawBytes[2] = (byte)((val & 0xFF00) >> 8);
            _rawBytes[3] = (byte)(val & 0xFF);
        }

        #endregion


        #region overrides of ValueType base methods

        public override string ToString()
        {
            return this.ToDouble().ToString();
        }

        public override int GetHashCode()
        {
            return (_mantissa << 16) + _fraction;
        }

        public override bool Equals(object obj)
        {
            if (obj is OTFixed) // non-null
                return this.Equals((OTFixed)obj);
            else
                return false;
        }

        public bool Equals(OTFixed fixedVal)
        {
            // OTFixed is a value type, cannot be null
            return ((this.Mantissa == fixedVal.Mantissa) && (this.Fraction == fixedVal.Fraction));
        }

        public bool Equals(byte[] buffer)
        {
            if (buffer is byte[]) // not null
            {
                if (buffer.Length != 4) return false;

                return this.Equals(new OTFixed(buffer));
            }
            else
            {
                return false;
            }
        }

        public bool Equals(UInt32 val)
        {
            return val == ((UInt32)_rawBytes[0] << 24) + ((UInt32)_rawBytes[1] << 16) + ((UInt32)_rawBytes[2] << 8) + (UInt32)_rawBytes[3];
        }

        #endregion


        #region operator overrides

        // following: overloads for operator overrides come in == / != pairs
        public static bool operator ==(OTFixed lhs, OTFixed rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(OTFixed lhs, OTFixed rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator ==(OTFixed lhs, byte[] rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(OTFixed lhs, byte[] rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator ==(byte[] lhs, OTFixed rhs)
        {
            return rhs.Equals(lhs);
        }

        public static bool operator !=(byte[] lhs, OTFixed rhs)
        {
            return !rhs.Equals(lhs);
        }

        #endregion


        #region conversion operators

        public static implicit operator byte[](OTFixed fixedStruct)
        {
            return fixedStruct.GetBytes();
        }

        // can't implicitly convert byte[] to OTFixed: won't work if the number
        // of elements isn't 4
        public static explicit operator OTFixed(byte[] fixedBuffer)
        {
            if (fixedBuffer == null) throw new ArgumentNullException("fixedBuffer", "Cast to OTFixed requires a non-null byte array");
            if (fixedBuffer.Length != 4) throw new InvalidCastException("Cannot cast byte array to OTFixed if array does not have exactly four elements");

            return new OTFixed(fixedBuffer);
        }

        public static explicit operator OTFixed(UInt32 val)
        {
            return new OTFixed(val);
        }

        #endregion


        #region other public methods

        /// <summary>
        /// Returns FIXED as a byte array. Bytes are in big-endian order as would be read 
        /// from an OT file.
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return _rawBytes;
        }

        /// <summary>
        /// Returns a UInt32 re-interpretation of the Fixed value byte sequence. This is
        /// useful for comparing Fixed table version fields.
        /// </summary>
        /// <returns>UInt32</returns>
        public UInt32 GetFixedAsUInt32()
        {
            // Return Fixed bytes interpreted as UInt32
            byte[] bm = BitConverter.GetBytes(_mantissa);
            return ((uint)BitConverter.ToUInt16(bm, 0) << 16) + _fraction;
        }

        /// <summary>
        /// Converts the FIXED value to double using the strict interpretation of the 16.16 
        /// format with the low-order word treated as a fractional portion in units of 1/65536.
        /// </summary>
        /// <returns></returns>
        public double ToDouble()
        {
            return _mantissa + (double)_fraction / 65536;
        }

        /// <summary>
        /// Converts the FIXED value to double using the variant interpretation of the 16.16 
        /// format that is associated with table version numbers: with the high-order
        /// nibble of the low-order word representing minor versions 1 to 9. For
        /// example, 0x00021000 to represent 2.1.
        /// </summary>
        /// <returns></returns>
        public double FixedTableVersionToDouble()
        {
            return _mantissa + (double)(_fraction >> 12) / 10;
        }

        /// <summary
        /// Returns the fractional portion of a FIXED value as an integer, using
        /// a variant interpretation of the 16.16 format used for table version
        /// numbers with the high-order nibble of the low-order word repesenting
        /// minor versions 1 to 9.
        /// </summary>
        public UInt16 FixedTableMinorVersionToInteger()
        {
            return (UInt16)(_fraction >> 12);
        }

        /// <summary>
        /// Returns FIXED value as a numeric string, using a variant interpretation
        /// of the 16.16 format that is sometimes used for table version numbers,
        /// with the high-order nibble of the low-order word representing minor
        /// versions 1 to 9. For example, 0x00021000 to represent 2.1.
        /// </summary>
        /// <returns></returns>
        public string FixedTableVersionToString()
        {
            return this.FixedTableVersionToDouble().ToString();
        }


        // static helper method to read Fixed value from a file

        public static OTFixed ReadFixed(MemoryStream ms)
        {
            // reads data from memory stream at current position and returns OTFixed
            // if unable to read enough data, throws exception; does not catch IO exceptions

            byte[] rawBytes = new byte[4];
            if (ms.Read(rawBytes, 0, 4) < 4)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read fixed value");
            }
            return new OTFixed(rawBytes);
        }

        #endregion

    } // struct OTFixed
}
