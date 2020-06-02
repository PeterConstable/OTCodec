using System;
//using System.Collections.Generic;
using System.IO;

namespace OTCodec
{
    public class OTTag
    {
        // used to hold four-byte / four-ASCII-char tags used in OT files
        // for sfnt version, tables, features, scripts, language systems
        //
        // Tags are stored in the array in the reading order; e.g., for
        // 'cmap' the first element in the array will be x63 ("c"). Treated
        // as an int32, the bytes are in big-endian order.

        // The tag data
        private byte[] _tag;


        #region constructors

        public OTTag()
        {
            //construct a tag of NULs
            _tag = new byte[4] { 0, 0, 0, 0 };
        }

        public OTTag(byte[] tagBuffer)
        {
            if (tagBuffer == null) throw new ArgumentNullException("tagBuffer", "Tag constructor requires a non-null byte array");

            // take the first four bytes in tagBuffer as the tag; if tagBuffer
            // has < 4 elements, the rest of the tag will be NULs
            _tag = new byte[4] { 0, 0, 0, 0 };
            int n; //number of elements to fetch
            n = tagBuffer.Length > 4 ? 4 : tagBuffer.Length;
            for (int i = 0; i < n; i++)
            {
                _tag[i] = tagBuffer[i];
            }
        }

        public OTTag(string tag)
        {
            // If string is less then four characters, pad tag with spaces (0x32)

            if (tag == null) throw new ArgumentNullException("tag", "OTTag constructor requires a non-null string");

            // use first four chars; always pad with space
            // chars must be <= 255

            _tag = new byte[4] { 32, 32, 32, 32 };
            int n; //number of chars to fetch
            n = tag.Length > 4 ? 4 : tag.Length;

            int cp; //code point of char
            for (int i = 0; i < n; i++)
            {
                cp = char.ConvertToUtf32(tag, i);
                if (cp > 255) throw new ArgumentException("Tag string has invalid characters", "tag");
                _tag[i] = (byte)cp;
            }
        }

        public OTTag(UInt32 tagValue)
        {
            _tag = new byte[4];
            _tag[0] = (byte)((tagValue & 0xFF000000) >> 24);
            _tag[1] = (byte)((tagValue & 0xFF0000) >> 16);
            _tag[2] = (byte)((tagValue & 0xFF00) >> 8);
            _tag[3] = (byte)(tagValue & 0xFF);
        }

        #endregion


        #region overrides of Object base methods

        public override bool Equals(object obj) // avoids warnings (expected when overriding ==, !=)
        {
            if (obj is OTTag)
                return this.Equals((OTTag)obj);
            else
                return false;
        }

        public bool Equals(OTTag tag)
        {
            if (tag is OTTag) // non-null
            {
                // get byte array for tag and compare with mTag
                byte[] tb = tag.GetBytes();
                return this.Equals(tb);
            }
            else
            {
                return false;
            }
        }

        public bool Equals(byte[] tagBuffer)
        {
            if (tagBuffer is byte[]) // not null
            {
                // compare tagBuffer with _tag
                if (tagBuffer.Length != 4) return false;
                for (int i = 0; i < 4; i++)
                {
                    if (tagBuffer[i] != _tag[i]) return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(string tag)
        {
            if (tag == null) return false;
            if (tag.Length != 4) return false;

            // convert tag string to byte array and compare with _tag
            int cp;
            for (int i = 0; i < 4; i++)
            {
                cp = char.ConvertToUtf32(tag, i);
                if (cp != _tag[i]) return false;
            }
            return true;
        }

        public bool Equals(uint tagValue)
        {
            return tagValue == ((uint)_tag[0] << 24) + ((uint)_tag[1] << 16) + ((uint)_tag[2] << 8) + (uint)_tag[3];
        }

        public override int GetHashCode() // gets used in unit tests
        {
            return ((int)_tag[0] << 24) + ((int)_tag[1] << 16) + ((int)_tag[2] << 8) + (int)_tag[3];
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < 4; i++)
            {
                s = s + char.ConvertFromUtf32(_tag[i]);
            }
            return s;
        }

        #endregion


        #region operator overrides

        // following: overloads for operator overrides come in == / != pairs

        public static bool operator ==(OTTag lhs, OTTag rhs)
        {
            if (lhs is OTTag) // lhs is non-null
            {
                return lhs.Equals(rhs);
            }
            else if (rhs is OTTag) // lhs is null, but rhs is non-null
            {
                return false;
            }
            else // lhs and rhs both null
            {
                return true;
            }
        }

        public static bool operator !=(OTTag lhs, OTTag rhs)
        {
            if (lhs is OTTag) // lhs is non-null
            {
                return !lhs.Equals(rhs);
            }
            else if (rhs is OTTag) // lhs is null, rhs is non-null
            {
                return true;
            }
            else // lhs and rhs are both null
            {
                return false;
            }
        }

        // For comparison with a byte array, special handling is
        // required (can't simply cast or construct a tag) since
        // the array length might not be four bytes.

        public static bool operator ==(OTTag lhs, byte[] rhs)
        {
            if (lhs is OTTag) // lhs is non-null
            {
                if (rhs is byte[])
                {
                    if (rhs.Length != 4) return false;

                    byte[] blhs = lhs.GetBytes();
                    for (int i = 0; i < 4; i++)
                    {
                        if (blhs[i] != rhs[i]) return false;
                    }
                    return true;
                }
                else // lhs is OTTag but rhs is null
                {
                    return false;
                }
            }
            else if (rhs is byte[]) // lhs is null
            {
                return false;
            }
            else // lhs and rhs are both null
            {
                return true;
            }
        }

        public static bool operator !=(OTTag lhs, byte[] rhs)
        {
            if (lhs is OTTag) // lhs is non-null
            {
                if (rhs is byte[])
                {
                    if (rhs.Length != 4) return true;

                    byte[] blhs = lhs.GetBytes();
                    for (int i = 0; i < 4; i++)
                    {
                        if (blhs[i] != rhs[i]) return true;
                    }
                    return false;
                }
                else // lhs is OTTag but rhs is null
                {
                    return true;
                }
            }
            else if (rhs is byte[]) // lhs is null
            {
                return true;
            }
            else // lhs and rhs are both null
            {
                return false;
            }
        }


        public static bool operator ==(byte[] lhs, OTTag rhs)
        {
            return (rhs == lhs);
        }

        public static bool operator !=(byte[] lhs, OTTag rhs)
        {
            return (rhs != lhs);
        }

        #endregion


        #region conversion operators

        public static explicit operator OTTag(byte[] tagBuffer)
        {
            if (tagBuffer == null) throw new ArgumentNullException("tagBuffer", "Cast to OTTag requires a non-null byte array");
            if (tagBuffer.Length != 4) throw new InvalidCastException("Cannot cast byte array to OTTag if array does not have exactly four elements");

            return new OTTag(tagBuffer);
        }

        public static explicit operator OTTag(string tag)
        {
            if (tag == null) throw new ArgumentNullException("tagBuffer", "Cast to OTTag requires a non-null string");
            if (tag.Length > 4) throw new InvalidCastException("Cannot cast byte array to OTTag if string is over four characters long");

            try
            {
                return new OTTag(tag);
            }
            catch (ArgumentException)
            {
                throw new InvalidCastException("Cannot cast string to OTTag if string has invalid characters");
            }
        }

        public static explicit operator OTTag(UInt32 tagValue)
        {
            return new OTTag(tagValue);
        }

        #endregion


        #region Other public methods

        public byte[] GetBytes()
        {
            return (byte[])_tag.Clone();
        }


        // static helper method to read Tag from a memory stream

        public static OTTag ReadTag(MemoryStream ms)
        {
            // reads data from file at current position and returns OTTag
            // if unable to read enough data, throws exception; does not catch IO exceptions

            byte[] ab32 = new byte[4];
            if (ms.Read(ab32, 0, 4) < 4)
            {
                throw new OTDataIncompleteReadException("OT parse error: unable to read sufficient bytes to recognize an OpenType tag");
            }
            return new OTTag(ab32);
        }

        #endregion

    } // class OTTag
}
