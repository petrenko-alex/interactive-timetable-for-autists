using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InteractiveTimetable.Tests
{
    [TestFixture]
    public class TestClass
    {
        [Test]
        public void TestMethod()
        {
            // arrange
            int expected = 1;
            int actual = 1;

            // act
            // assert
            Assert.AreEqual(expected,actual);
        }
    }
}
