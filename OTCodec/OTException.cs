using System;
using System.Collections.Generic;
using System.Text;

namespace OTCodec
{
    public class OTException : Exception
    {
        public OTException()
        {
        }

        public OTException(string message)
            : base(message)
        {
        }

        public OTException(string message, Exception inner)
            : base(message, inner)
        {
        }

    } // class OTException

    public class OTInvalidOperationException : OTException
    {
        public OTInvalidOperationException()
        {
        }

        public OTInvalidOperationException(string message)
            : base(message)
        {
        }

        public OTInvalidOperationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }



    public class OTDataTypeReadException : OTException
    {
        // use when data can be read but can't be given valid interpretation
        private const string dfltMsg = "OT parse error: unable to read OT data type from file";

        public OTDataTypeReadException()
            : base(dfltMsg)
        {
        }

        public OTDataTypeReadException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTDataTypeReadException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTDataTypeReadException


    public class OTDataIncompleteReadException : OTException
    {
        private const string dfltMsg = "OT parse error: read operation returned fewer bytes than expected";

        public OTDataIncompleteReadException()
            : base(dfltMsg)
        {
        }

        public OTDataIncompleteReadException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTDataIncompleteReadException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTDataIncompleteReadException


    public class OTRecordParseException : OTException
    {
        private const string dfltMsg = "OT parse error: error while parsing record";

        public OTRecordParseException()
            : base(dfltMsg)
        {
        }

        public OTRecordParseException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTRecordParseException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTRecordParseException

    public class OTTableParseException : OTException
    {
        private const string dfltMsg = "OT parse error: error while parsing table";

        public OTTableParseException()
            : base(dfltMsg)
        {
        }

        public OTTableParseException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTTableParseException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTTableParseException

    public class OTFontParseException : OTException
    {
        private const string dfltMsg = "OT parse error: error while parsing font";

        public OTFontParseException()
            : base(dfltMsg)
        {
        }

        public OTFontParseException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTFontParseException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTFontParseException

    public class OTFileParseException : OTException
    {
        private const string dfltMsg = "OT parse error: error while parsing file";

        public OTFileParseException()
            : base(dfltMsg)
        {
        }

        public OTFileParseException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTFileParseException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTFileParseException


    public class OTFileNotOpenedException : OTException
    {
        private const string dfltMsg = "OTFile operation cannot be performed: OTFile object was constructed but a physical file has not been opened";

        public OTFileNotOpenedException()
            : base(dfltMsg)
        {
        }

        public OTFileNotOpenedException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTFileNotOpenedException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTFileNotOpenedException


    public class OTUnknownVersionException : OTException
    {
        private const string dfltMsg = "Parse operation stopped: the version of the table or record is unknown";

        public OTUnknownVersionException()
            : base(dfltMsg)
        {
        }

        public OTUnknownVersionException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTUnknownVersionException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTUnknownVersionException

    public class OTOutOfBoundsException : OTException
    {
        private const string dfltMsg = "Arguments lead to an index outside the bounds of a data array";

        public OTOutOfBoundsException()
            : base(dfltMsg)
        {
        }

        public OTOutOfBoundsException(string message)
            : base(message == "" ? dfltMsg : message)
        {
        }

        public OTOutOfBoundsException(string message, Exception inner)
            : base(message == "" ? dfltMsg : message, inner)
        {
        }
    } // class OTOutOfBoundsException

} // namespace OTCodec
