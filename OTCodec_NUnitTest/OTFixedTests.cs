using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    public class OTFixedTests
    {
        [SetUp]
        public void Setup()
        {
            // appears to be run before each test
            if (!System.IO.Directory.Exists("TestData"))
            {
                System.IO.Directory.SetCurrentDirectory("..\\..\\..");
            }
            DirectoryAssert.Exists("TestData", "Error: Unable to locate TestData folder.");
        }


        #region constructor tests

        ///A test for OTFixed Constructor
        [Test]
        public void OTFixedConstructorTest1()
        {
            short mantissa = 0x0001;
            ushort fraction = 0x5000;
            OTFixed target = new OTFixed(mantissa, fraction);
            Assert.IsTrue((mantissa == target.Mantissa) && (fraction == target.Fraction), "Error: unexpected values");
        }

        ///A test for OTFixed Constructor
        [Test]
        public void OTFixedConstructorTest2()
        {
            byte[] buffer = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed target = new OTFixed(buffer);
            Assert.IsTrue((target.Mantissa == 1) && (target.Fraction == 0x5000), "Error: unexpected values");
        }

        #endregion


        #region accessor tests

        ///A test for Mantissa
        [Test]
        public void OTFixedMantissaTest1()
        {
            OTFixed target = new OTFixed(-5, 0);
            short expected = -5;
            short actual = target.Mantissa;
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        [Test]
        public void OTFixedMantissaTest2()
        {
            OTFixed target = new OTFixed(0xF0008000);
            short expected = -4096;
            short actual = target.Mantissa;
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        ///A test for Fraction
        [Test]
        public void OTFixedFractionTest()
        {
            OTFixed target = new OTFixed(0, 0x5000);
            ushort expected = 0x5000;
            ushort actual = target.Fraction;
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        #endregion


        #region tests of ValueType base method overrides

        ///A test for ToString
        [Test]
        public void OTFixedToStringTest()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            string expected = "1.5";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        ///A test for GetHashCode
        [Test]
        public void OTFixedGetHashCodeTest()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            int expected = 0x00018000;
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        // tests for Equals(object)
        [Test]
        public void OTFixedEqualsTest1a()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            object obj = new OTFixed(1, 0x8000);
            Assert.IsTrue(target.Equals(obj), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest1b()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x7f, 0 });
            object obj = new OTFixed(1, 0x8000);
            Assert.IsFalse(target.Equals(obj), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest1c()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x7f, 0 });
            object obj = null;
            Assert.IsFalse(target.Equals(obj), "Error: unexpected comparison result");
        }


        // tests for Equals(OTFixed)
        [Test]
        public void OTFixedEqualsTest2a()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed fixedVal = new OTFixed(1, 0x8000);
            Assert.IsTrue(target.Equals(fixedVal), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest2b()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x7f, 0 });
            OTFixed fixedVal = new OTFixed(1, 0x8000);
            Assert.IsFalse(target.Equals(fixedVal), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest2c()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed fixedVal = new OTFixed(2, 0x8000);
            Assert.IsFalse(target.Equals(fixedVal), "Error: unexpected comparison result");
        }


        // tests for Equals(byte[])
        [Test]
        public void OTFixedEqualsTest3a()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            byte[] buffer = new byte[4] { 0, 1, 0x80, 0 };
            Assert.IsTrue(target.Equals(buffer), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest3b()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            byte[] buffer = new byte[4] { 0, 1, 0x7f, 0 };
            Assert.IsFalse(target.Equals(buffer), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest3c()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            byte[] buffer = new byte[5] { 0, 1, 0x7f, 0, 0 };
            Assert.IsFalse(target.Equals(buffer), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest3d()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            byte[] buffer = new byte[3] { 0, 1, 0x7f };
            Assert.IsFalse(target.Equals(buffer), "Error: unexpected comparison result");
        }

        [Test]
        public void OTFixedEqualsTest3e()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            byte[] buffer = null;
            Assert.IsFalse(target.Equals(buffer), "Error: unexpected comparison result");
        }

        #endregion


        #region tests for operator overrides

        // tests for == operator
        [Test]
        public void OTFixedOperator_EqualityTest1a()
        {
            OTFixed lhs = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed rhs = new OTFixed(1, 0x8000);
            Assert.IsTrue(lhs == rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_EqualityTest1b()
        {
            OTFixed lhs = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed rhs = new OTFixed(1, 0x7f00);
            Assert.IsFalse(lhs == rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_EqualityTest2a()
        {
            OTFixed lhs = new OTFixed(1, 0x5000);
            byte[] rhs = new byte[4] { 0, 1, 0x50, 0 };
            Assert.IsTrue(lhs == rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_EqualityTest2b()
        {
            OTFixed lhs = new OTFixed(1, 0x5000);
            byte[] rhs = new byte[4] { 0, 1, 0x4f, 0 };
            Assert.IsFalse(lhs == rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_EqualityTest3a()
        {
            byte[] lhs = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed rhs = new OTFixed(1, 0x5000);
            Assert.IsTrue(lhs == rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_EqualityTest3b()
        {
            byte[] lhs = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed rhs = new OTFixed(1, 0x4f00);
            Assert.IsFalse(lhs == rhs, "Error: unexpected operator result");
        }

        // tests for != operator
        [Test]
        public void OTFixedOperator_InequalityTest1a()
        {
            OTFixed lhs = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed rhs = new OTFixed(1, 0x7f00);
            Assert.IsTrue(lhs != rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_InequalityTest1b()
        {
            OTFixed lhs = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            OTFixed rhs = new OTFixed(1, 0x8000);
            Assert.IsFalse(lhs != rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_InequalityTest2a()
        {
            OTFixed lhs = new OTFixed(1, 0x5000);
            byte[] rhs = new byte[4] { 0, 1, 0x4f, 0 };
            Assert.IsTrue(lhs != rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_InequalityTest2b()
        {
            OTFixed lhs = new OTFixed(1, 0x5000);
            byte[] rhs = new byte[4] { 0, 1, 0x50, 0 };
            Assert.IsFalse(lhs != rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_InequalityTest3a()
        {
            byte[] lhs = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed rhs = new OTFixed(1, 0x4f00);
            Assert.IsTrue(lhs != rhs, "Error: unexpected operator result");
        }

        [Test]
        public void OTFixedOperator_InequalityTest3b()
        {
            byte[] lhs = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed rhs = new OTFixed(1, 0x5000);
            Assert.IsFalse(lhs != rhs, "Error: unexpected operator result");
        }


        #endregion


        #region tests for conversions

        // test for implicit OTFixed to byte[] cast
        [Test]
        public void OTFixedOpConvImplicitTest3()
        {
            OTFixed fixedStruct = new OTFixed(1, 0x5000);
            byte[] expected = new byte[4] { 0, 1, 0x50, 0 };
            byte[] actual;
            actual = fixedStruct;
            bool result = true;
            for (int i = 0; i < 4; i++)
            {
                if (expected[i] != actual[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsTrue(result, "Error: unexpected conversion result");
        }

        // test for explicit byte[] to OTFixed cast
        [Test]
        public void OTFixedOpConvExplicitTest()
        {
            byte[] fixedBuffer = new byte[4] { 0, 1, 0x50, 0 };
            OTFixed expected = new OTFixed(1, 0x5000);
            OTFixed actual;
            actual = ((OTFixed)(fixedBuffer));
            Assert.AreEqual(expected, actual, "Error: unexpected conversion result");
        }


        #endregion


        #region other public method tests

        ///A test for GetBytes
        [Test]
        public void OTFixedGetBytesTest()
        {
            OTFixed target = new OTFixed(1, 0x5000);
            byte[] expected = new byte[4] { 0, 1, 0x50, 0 };
            byte[] actual = target.GetBytes();
            bool result = true;
            for (int i = 0; i < 4; i++)
            {
                if (expected[i] != actual[i])
                {
                    result = false;
                    break;
                }
            }
            Assert.IsTrue(result, "Error: unexpected value");
        }

        ///A test for GetUInt
        [Test]
        public void OTFixedGetFixedAsUInt32Test()
        {
            OTFixed target = new OTFixed(1, 0x5000);
            uint expected = 0x00015000;
            uint actual;
            actual = target.GetFixedAsUInt32();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        ///A test for ToDouble
        [Test]
        public void OTFixedToDoubleTest()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x80, 0 });
            double expected = 1.5;
            double actual = target.ToDouble();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        ///A test for FixedTableVersionToDouble
        [Test]
        public void OTFixed_FixedTableVersionToDoubleTest()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x50, 0 });
            double expected = 1.5;
            double actual;
            actual = target.FixedTableVersionToDouble();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        ///A test for FixedTableVersionToString
        [Test]
        public void OTFixed_FixedTableVersionToStringTest()
        {
            OTFixed target = new OTFixed(new byte[4] { 0, 1, 0x50, 0 });
            string expected = "1.5";
            string actual;
            actual = target.FixedTableVersionToString();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }


        ///A test for ReadFixed
        [Test]
        public void OTFixedReadFixedTest1()
        {
            bool caughtExpectedException = false; // will set to true if expected exception is caught

            byte[] b = new byte[1] { 0 };
            MemoryStream ms = new MemoryStream(b);
            try
            {
                OTFixed f = OTFixed.ReadFixed(ms);
            }
            catch (OTDataIncompleteReadException)
            {
                caughtExpectedException = true; // caught expected argument: test passes
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");

            b = new byte[3] { 0, 1, 0 };
            ms = new MemoryStream(b);
            try
            {
                OTFixed f = OTFixed.ReadFixed(ms);
            }
            catch (OTDataTypeReadException)
            {
                caughtExpectedException = true; // caught expected argument: test passes
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(caughtExpectedException, "Error: expected exception not caught");
        }

        ///A test for GetFixed
        [Test]
        public void OTFixedReadFixedTest2()
        {
            byte[] b = new byte[5] { 0, 1, 0x50, 0, 1 };
            MemoryStream ms = new MemoryStream(b);
            OTFixed expected = new OTFixed(1, 0x5000);
            OTFixed actual = new OTFixed();
            try
            {
                actual = OTFixed.ReadFixed(ms);
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(expected == actual, "Error: unexpected value read");
        }

        #endregion

    } // class OTFixedTests

} // namespace OTCodec_NUnitTest