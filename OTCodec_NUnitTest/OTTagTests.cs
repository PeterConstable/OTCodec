using NUnit.Framework;
using System;
using System.IO;
using OTCodec;

namespace OTCodec_NUnitTest
{
    public class OTTagTests
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

        // private method to compare byte arrays
        private bool ArraysMatch(byte[] arr1, byte[] arr2)
        {
            if (arr1.Length != arr2.Length) return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i]) return false;
            }
            return true;
        }


        #region constructor tests

        // A test for OTTag Constructor OTTag()
        [Test]
        public void OTTagConstructorTest1()
        {
            OTTag target = new OTTag();
            Assert.IsTrue(ArraysMatch(new byte[4] { 0, 0, 0, 0 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }


        // A test for OTTag Constructor OTTag(byte[] tagBuffer) -- good buffer
        [Test]
        public void OTTagConstructorTest2a()
        {
            byte[] tagBuffer = new byte[4] { 1, 3, 5, 7 };
            OTTag target = new OTTag(tagBuffer);
            Assert.IsTrue(ArraysMatch(new byte[4] { 1, 3, 5, 7 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        // A test for OTTag Constructor OTTag(byte[] tagBuffer) -- buffer too long
        [Test]
        public void OTTagConstructorTest2b()
        {
            byte[] tagBuffer = new byte[5] { 1, 3, 5, 7, 9 };
            OTTag target = new OTTag(tagBuffer);
            Assert.IsTrue(ArraysMatch(new byte[4] { 1, 3, 5, 7 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        // A test for OTTag Constructor OTTag(byte[] tagBuffer) -- buffer too short
        [Test]
        public void OTTagConstructorTest2c()
        {
            byte[] tagBuffer = new byte[3] { 1, 3, 5 };
            OTTag target = new OTTag(tagBuffer);
            Assert.IsTrue(ArraysMatch(new byte[4] { 1, 3, 5, 0 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        // A test for OTTag Constructor OTTag(byte[] tagBuffer) -- buffer is null
        [Test]
        public void OTTagConstructorTest2d()
        {
            byte[] tagBuffer = null;
            bool caughtExpectedException = false;
            try
            {
                OTTag target = new OTTag(tagBuffer);
            }
            catch (ArgumentNullException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }


        ///A test for OTTag Constructor OTTag(string tag) -- 4 char string
        [Test]
        public void OTTagConstructorTest6a()
        {
            string tag = "abcd";
            OTTag target = new OTTag(tag);
            Assert.IsTrue(ArraysMatch(new byte[4] { 97, 98, 99, 100 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        ///A test for OTTag Constructor OTTag(string tag) -- string has extra chars
        [Test]
        public void OTTagConstructorTest6b()
        {
            string tag = "abcde";
            OTTag target = new OTTag(tag);
            Assert.IsTrue(ArraysMatch(new byte[4] { 97, 98, 99, 100 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        ///A test for OTTag Constructor OTTag(string tag) -- string is short
        [Test]
        public void OTTagConstructorTest6c()
        {
            string tag = "abc";
            OTTag target = new OTTag(tag);
            Assert.IsTrue(ArraysMatch(new byte[4] { 97, 98, 99, 32 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        ///A test for OTTag Constructor OTTag(string tag) -- string is empty
        [Test]
        public void OTTagConstructorTest6d()
        {
            string tag = "";
            OTTag target = new OTTag(tag);
            Assert.IsTrue(ArraysMatch(new byte[4] { 32, 32, 32, 32 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        ///A test for OTTag Constructor OTTag(string tag) -- string is null
        [Test]
        public void OTTagConstructorTest6e()
        {
            string tag = null;
            bool caughtExpectedException = false;
            try
            {
                OTTag target = new OTTag(tag);
            }
            catch (ArgumentNullException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }

        // test for OTTag Constructor OTTag(string tag) -- string has character > U+00FF
        [Test]
        public void OTTagConstructorTest6f()
        {
            //test with U+00FF -- no exception expected
            string tag = "abcÿ";
            OTTag target = new OTTag(tag);

            // now with U+0100
            tag = "abcĀ";
            bool caughtExpectedException = false;
            try
            {
                target = new OTTag(tag);
            }
            catch (ArgumentException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }

        ///A test for OTTag Constructor OTTag(uint tagValue)
        [Test]
        public void OTTagConstructorTest7()
        {
            uint tagValue = 0x11223344;
            OTTag target = new OTTag(tagValue);
            Assert.IsTrue(ArraysMatch(new byte[4] { 0x11, 0x22, 0x33, 0x44 }, target.GetBytes()), "OTTag not as expected: hash = " + target.GetHashCode());
        }

        #endregion


        #region tests for Equals overrides

        ///A test for Equals -- OTTag.Equals(object), object is OTTag (equal)
        [Test]
        public void OTTagEqualsTest1a()
        {
            byte[] b = new byte[4] { 1, 3, 5, 7 };
            OTTag target = new OTTag(b);
            object obj = new OTTag(b);
            bool expected = true;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(object); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(object), object is OTTag (unequal)
        [Test]
        public void OTTagEqualsTest1b()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            object obj = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = false;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(object); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(object), object is null (unequal)
        [Test]
        public void OTTagEqualsTest1c()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            object tag = null;
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(object); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(object), object is not OTTag
        [Test]
        public void OTTagEqualsTest1d()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            object obj = new object();
            bool expected = false;
            bool actual;
            actual = target.Equals(obj);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(object); hash = " + target.GetHashCode());
        }


        ///A test for Equals -- OTTag.Equals(array) (equal)
        [Test]
        public void OTTagEqualsTest2a()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] tagBuffer = new byte[4] { 1, 3, 5, 7 };
            bool expected = true;
            bool actual;
            actual = target.Equals(tagBuffer);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(byte[]); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(array) (unequal)
        [Test]
        public void OTTagEqualsTest2b()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] tagBuffer = new byte[4] { 1, 3, 5, 9 };
            bool expected = false;
            bool actual;
            actual = target.Equals(tagBuffer);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(byte[]); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(array), array is null (unequal)
        [Test]
        public void OTTagEqualsTest2c()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] tag = null;
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(byte[]); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(array), array is too short (unqual)
        [Test]
        public void OTTagEqualsTest2d()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] tagBuffer = new byte[3] { 1, 3, 5 };
            bool expected = false;
            bool actual;
            actual = target.Equals(tagBuffer);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(byte[]); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(array), array is too long (unqual)
        [Test]
        public void OTTagEqualsTest2e()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] tagBuffer = new byte[5] { 1, 3, 5, 7, 9 };
            bool expected = false;
            bool actual;
            actual = target.Equals(tagBuffer);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(byte[]); hash = " + target.GetHashCode());
        }


        ///A test for Equals -- OTTag.Equals(OTTag) (equal)
        [Test]
        public void OTTagEqualsTest3a()
        {
            byte[] b = new byte[4] { 1, 3, 5, 7 };

            OTTag target = new OTTag(b);
            OTTag tag = new OTTag(b);
            bool expected = true;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(OTTag) (unequal)
        [Test]
        public void OTTagEqualsTest3b()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag tag = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(OTTag), OTTag is null (unequal)
        [Test]
        public void OTTagEqualsTest3c()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag tag = null;
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }


        ///A test for Equals -- OTTag.Equals(string) (equal)
        [Test]
        public void OTTagEqualsTest4a()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string tag = "abcd";
            bool expected = true;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(string), string too short (unequal)
        [Test]
        public void OTTagEqualsTest4b()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string tag = "abc";
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(string), string too long (unequal)
        [Test]
        public void OTTagEqualsTest4c()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string tag = "abcde";
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(string), different string (unequal)
        [Test]
        public void OTTagEqualsTest4d()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string tag = "abce";
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(string), string is null (unequal)
        [Test]
        public void OTTagEqualsTest4e()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string tag = null;
            bool expected = false;
            bool actual;
            actual = target.Equals(tag);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag.Equals(OTTag); hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(uint) (equal)
        [Test]
        public void OTTagEqualsTest5a()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            uint tagValue = 0x01030507;
            bool expected = true;
            bool actual;
            actual = target.Equals(tagValue);
            Assert.AreEqual(expected, actual, "OTTag not as expected: hash = " + target.GetHashCode());
        }

        ///A test for Equals -- OTTag.Equals(uint) (unequal)
        [Test]
        public void OTTagEqualsTest5b()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            uint tagValue = 0x01030509;
            bool expected = false;
            bool actual;
            actual = target.Equals(tagValue);
            Assert.AreEqual(expected, actual, "OTTag not as expected: hash = " + target.GetHashCode());
        }

        #endregion


        #region tests for == operator overrides

        ///A test for op_Equality -- OTTag == OTTag (equal)
        [Test]
        public void OTTagOperator_EqualityTest1a()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == OTTag; hash = " + lhs.GetHashCode());
        }

        ///A test for op_Equality -- OTTag == OTTag (not equal)
        [Test]
        public void OTTagOperator_EqualityTest1b()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == OTTag; hash = " + lhs.GetHashCode());
        }

        ///A test for op_Equality -- OTTag == OTTag, rhs is null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest1c()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == OTTag; hash = " + lhs.GetHashCode());
        }

        ///A test for op_Equality -- OTTag == OTTag, lhs is null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest1d()
        {
            OTTag lhs = null;
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == OTTag");
        }

        ///A test for op_Equality -- OTTag == OTTag, lhs and rhs both null (equal)
        [Test]
        public void OTTagOperator_EqualityTest1e()
        {
            OTTag lhs = null;
            OTTag rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == OTTag");
        }


        ///A test for op_Equality -- OTTag == array (equal)
        [Test]
        public void OTTagOperator_EqualityTest2a()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[4] { 1, 3, 5, 7 };
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array (not equal)
        [Test]
        public void OTTagOperator_EqualityTest2b()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[4] { 1, 3, 5, 9 };
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array, array too long (not equal)
        [Test]
        public void OTTagOperator_EqualityTest2c()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[5] { 1, 3, 5, 7, 9 };
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array, array too short (not equal)
        [Test]
        public void OTTagOperator_EqualityTest2d()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[3] { 1, 3, 5 };
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array, lhs is null, rhs is non-null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest2e()
        {
            OTTag lhs = null;
            byte[] rhs = new byte[4] { 1, 3, 5, 9 };
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array, lhs is non-null, rhs is null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest2f()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }

        ///A test for op_Equality -- OTTag == array, lhs and rhs are both null (equal)
        [Test]
        public void OTTagOperator_EqualityTest2g()
        {
            OTTag lhs = null;
            byte[] rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag == byte[]");
        }


        ///A test for op_Equality -- array == OTTag (equal)
        [Test]
        public void OTTagOperator_EqualityTest3a()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag (not equal)
        [Test]
        public void OTTagOperator_EqualityTest3b()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag, array too long (not equal)
        [Test]
        public void OTTagOperator_EqualityTest3c()
        {
            byte[] lhs = new byte[5] { 1, 3, 5, 7, 9 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag, array too short (not equal)
        [Test]
        public void OTTagOperator_EqualityTest3d()
        {
            byte[] lhs = new byte[3] { 1, 3, 5 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag, lhs is null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest3e()
        {
            byte[] lhs = null;
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag, rhs is null (not equal)
        [Test]
        public void OTTagOperator_EqualityTest3f()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        ///A test for op_Equality -- array == OTTag, lhs and rhs are both null (equal)
        [Test]
        public void OTTagOperator_EqualityTest3g()
        {
            byte[] lhs = null;
            OTTag rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs == rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] == OTTag");
        }

        #endregion


        #region tests for != operator overrides

        ///A test for op_Inequality -- OTTag != OTTag (not equal)
        [Test]
        public void OTTagOperator_InequalityTest1a()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != OTTag");
        }

        ///A test for op_Inequality -- OTTag != OTTag (equal)
        [Test]
        public void OTTagOperator_InequalityTest1b()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != OTTag");
        }

        ///A test for op_Inequality -- OTTag != OTTag, lhs is null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest1c()
        {
            OTTag lhs = null;
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != OTTag");
        }

        ///A test for op_Inequality -- OTTag != OTTag, rhs is null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest1d()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != OTTag");
        }

        ///A test for op_Inequality -- OTTag != OTTag, lhs and rhs are both null (equal)
        [Test]
        public void OTTagOperator_InequalityTest1e()
        {
            OTTag lhs = null;
            OTTag rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != OTTag");
        }


        ///A test for op_Inequality -- OTTag == array (not equal)
        [Test]
        public void OTTagOperator_InequalityTest2a()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[4] { 1, 3, 5, 9 };
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array (equal)
        [Test]
        public void OTTagOperator_InequalityTest2b()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[4] { 1, 3, 5, 7 };
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array, array too long (not equal)
        [Test]
        public void OTTagOperator_InequalityTest2c()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[5] { 1, 3, 5, 7, 9 };
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array, array too short (not equal)
        [Test]
        public void OTTagOperator_InequalityTest2d()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = new byte[3] { 1, 3, 5 };
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array, lhs is null, rhs is non-null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest2e()
        {
            OTTag lhs = null;
            byte[] rhs = new byte[4] { 1, 3, 5, 9 };
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array, lhs is non-null, rhs is null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest2f()
        {
            OTTag lhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }

        ///A test for op_Inequality -- OTTag == array, lhs and rhs are both null (equal)
        [Test]
        public void OTTagOperator_InequalityTest2g()
        {
            OTTag lhs = null;
            byte[] rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from OTTag != byte[]");
        }


        ///A test for op_Inequality -- array == OTTag (not equal)
        [Test]
        public void OTTagOperator_InequalityTest3a()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag (equal)
        [Test]
        public void OTTagOperator_InequalityTest3b()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag, array too long (not equal)
        [Test]
        public void OTTagOperator_InequalityTest3c()
        {
            byte[] lhs = new byte[5] { 1, 3, 5, 7, 9 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag, array too short (not equal)
        [Test]
        public void OTTagOperator_InequalityTest3d()
        {
            byte[] lhs = new byte[3] { 1, 3, 5 };
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 7 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag, lhs is null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest3e()
        {
            byte[] lhs = null;
            OTTag rhs = new OTTag(new byte[4] { 1, 3, 5, 9 });
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag, rhs is null (not equal)
        [Test]
        public void OTTagOperator_InequalityTest3f()
        {
            byte[] lhs = new byte[4] { 1, 3, 5, 7 };
            OTTag rhs = null;
            bool expected = true;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        ///A test for op_Inequality -- array == OTTag, lhs and rhs are both null (equal)
        [Test]
        public void OTTagOperator_InequalityTest3g()
        {
            byte[] lhs = null;
            OTTag rhs = null;
            bool expected = false;
            bool actual;
            actual = (lhs != rhs);
            Assert.AreEqual(expected, actual, "Unexpected result from byte[] != OTTag");
        }

        #endregion


        #region tests for explicit conversions

        ///A test for op_Explicit -- (OTTag)(array), good array
        [Test]
        public void OTTagOpConvExplicitTest1a()
        {
            byte[] tagBuffer = new byte[4] { 1, 3, 5, 7 };
            OTTag expected = new OTTag(new byte[4] { 1, 3, 5, 7 });
            OTTag actual;
            actual = ((OTTag)(tagBuffer));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        ///A test for op_Explicit -- (OTTag)(array), array wrong size
        [Test]
        public void OTTagOpConvExplicitTest1b()
        {
            byte[] tagBuffer = new byte[3] { 1, 3, 5 };
            bool caughtExpectedException = false;
            try
            {
                OTTag actual;
                actual = ((OTTag)(tagBuffer));
            }
            catch (InvalidCastException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException, "Error: explicit cast to OTTag exception from under-long array not caught");

            tagBuffer = new byte[5] { 1, 2, 3, 4, 5 };
            caughtExpectedException = false;
            try
            {
                OTTag actual = ((OTTag)(tagBuffer));
            }
            catch (InvalidCastException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException, "Error: explicit cast to OTTag exception from over-long array not caught");
        }

        ///A test for op_Explicit -- (OTTag)(array), array is null
        [Test]
        public void OTTagOpConvExplicitTest1c()
        {
            byte[] tagBuffer = null;
            bool caughtExpectedException = false;
            try
            {
                OTTag actual;
                actual = ((OTTag)(tagBuffer));
            }
            catch (ArgumentNullException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException, "Error: explicit cast to OTTag exception from null array not caught");
        }


        /// A test of explicit casting -- (OTTag)(string), good 4 char string
        [Test]
        public void OTTagOpConvExplicitTest2a()
        {
            string tag = "abcd";
            OTTag expected = new OTTag("abcd");
            OTTag actual;
            actual = ((OTTag)(tag));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        /// A test of explicit casting -- (OTTag)(string), short string
        [Test]
        public void OTTagOpConvExplicitTest2b()
        {
            string tag = "abc";
            OTTag expected = new OTTag("abc");
            OTTag actual;
            actual = ((OTTag)(tag));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        /// A test of explicit casting -- (OTTag)(string), long string
        [Test]
        public void OTTagOpConvExplicitTest2c()
        {
            string tag = "abcde";
            bool caughtExpectedException = false;
            try
            {
                OTTag actual = ((OTTag)(tag));
            }
            catch (InvalidCastException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }

        /// A test of explicit casting -- (OTTag)(string), empty string
        [Test]
        public void OTTagOpConvExplicitTest2d()
        {
            string tag = "";
            OTTag expected = new OTTag("    ");
            OTTag actual;
            actual = ((OTTag)(tag));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        /// A test of explicit casting -- (OTTag)(string), string is null
        [Test]
        public void OTTagOpConvExplicitTest2e()
        {
            string tag = null;
            bool caughtExpectedException = false;
            try
            {
                OTTag actual;
                actual = ((OTTag)(tag));
            }
            catch (ArgumentNullException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }

        /// A test of explicit casting -- (OTTag)(string), string has invalid charactesr
        [Test]
        public void OTTagOpConvExplicitTest2f()
        {
            string tag = "abc\u0100";
            bool caughtExpectedException = false;
            try
            {
                OTTag actual;
                actual = ((OTTag)(tag));
            }
            catch (InvalidCastException)
            {
                caughtExpectedException = true;
            }
            Assert.IsTrue(caughtExpectedException);
        }

        /// A test of explicit casting -- (OTTag)(uint), , uint = 0
        [Test]
        public void OTTagOpConvExplicitTest3a()
        {
            uint tagValue = 0;
            OTTag expected = new OTTag(new byte[4] { 0, 0, 0, 0 });
            OTTag actual;
            actual = ((OTTag)(tagValue));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        /// A test of explicit casting -- (OTTag)(uint), uint > 0
        [Test]
        public void OTTagOpConvExplicitTest3b()
        {
            uint tagValue = 0x12345678;
            OTTag expected = new OTTag(new byte[4] { 0x12, 0x34, 0x56, 0x78 });
            OTTag actual;
            actual = ((OTTag)(tagValue));
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()));
        }

        #endregion


        #region tests for other methods

        ///A test for ReadTag
        [Test]
        public void OTTagReadTagTest1()
        {
            bool caughtExpectedException = false; // will set to true if expected exception is caught
            OTTag tag;

            byte[] b = new byte[3] { 0, 1, 0 };
            MemoryStream ms = new MemoryStream(b);
            try
            {
                tag = OTTag.ReadTag(ms);
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
        }

        ///A test for ReadTag
        [Test]
        public void OTTagReadTagTest2()
        {
            byte[] b = new byte[5] { 0, 1, 0, 0, 1 };
            MemoryStream ms = new MemoryStream(new byte[4] { 0, 1, 0, 0 });
            OTTag expected = new OTTag(b);
            OTTag actual = null;
            try
            {
                actual = OTTag.ReadTag(ms);
            }
            catch (Exception)
            {
                // unexpected exception
            }
            Assert.IsTrue(ArraysMatch(expected.GetBytes(), actual.GetBytes()), "Error: unexpected result from OTTag.ReadTag()");
        }

        ///A test for GetBytes()
        [Test]
        public void OTTagGetBytesTest()
        {
            OTTag target = new OTTag(new byte[4] { 1, 3, 5, 7 });
            byte[] expected = new byte[4] { 1, 3, 5, 7 };
            byte[] actual;
            actual = target.GetBytes();
            Assert.IsTrue(ArraysMatch(expected, actual), "Error: unexpected result from OTTag.GetBytes()");
        }

        ///A test for GetHashCode()
        [Test]
        public void OTTagGetHashCodeTest()
        {
            OTTag target = new OTTag(new byte[4] { 0xff, 0xfe, 0xfd, 0xfc });
            // Object.GetHashCode returns int (signed). Can't cast 0xFF00_0000 directly to int.
            int expected = 0xfefdfc + (int)(0xff << 24);
            int actual;
            actual = target.GetHashCode();
            Assert.AreEqual(expected, actual, "Error: unexpected result from OTTag.GetHashCode()");
        }

        ///A test for ToString()
        [Test]
        public void OTTagToStringTest()
        {
            OTTag target = new OTTag(new byte[4] { 97, 98, 99, 100 });
            string expected = "abcd";
            string actual;
            actual = target.ToString();
            Assert.AreEqual(expected, actual, "Error: unexpected value");

            target = new OTTag(new byte[4] { 97, 98, 99, 255 });
            expected = "abcÿ";
            actual = target.ToString();
            Assert.AreEqual(expected, actual, "Error: unexpected value");
        }

        #endregion

    } // class OTTagTests

} // namespace OTCodec_NUnitTest