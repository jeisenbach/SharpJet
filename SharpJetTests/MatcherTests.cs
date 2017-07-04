// <copyright file="MatcherTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
//
// SharpJet, a library to communicate with Jet IPC.
//
// The MIT License (MIT)
//
// Copyright (C) Hottinger Baldwin Messtechnik GmbH
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN
// ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//
// </copyright>

namespace SharpJetTests
{
    using NUnit.Framework;
    using Hbm.Devices.Jet;

    [TestFixture]
    public class MatcherTests
    {
        [Test]
        public void TestMatchWithContains()
        {
            Matcher matcher = new Matcher();
            matcher.Contains = "hello/world";
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("hello/world/how/are/youe");
            bool isNotAMatch = matcher.Match("hello/World/how/are/you");
            matcher.CaseInsensitive = true;
            bool isMatch2 = matcher.Match("Hello/World/how/are/youe");


            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch, $"Expected {nameof(isNotAMatch)} to be false but received true.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
        }

        [Test]
        public void TestMatchWithContainsAllOf()
        {
            Matcher matcher = new Matcher();
            matcher.ContainsAllOf = new[] {"hello", "world"};
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("hello/world/how/are/youe");
            bool isNotAMatch1 = matcher.Match("hello/World/how/are/you");
            bool isNotAMatch2 = matcher.Match("foo/world");
            matcher.CaseInsensitive = true;
            bool isMatch2 = matcher.Match("Hello/World/how/are/youe");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch1, $"Expected {nameof(isNotAMatch1)} to be false but received true.");
            Assert.IsFalse(isNotAMatch2, $"Expected {nameof(isNotAMatch2)} to be false but received true.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
        }

        [Test]
        public void TestMatchWithStartsWith()
        {
            Matcher matcher = new Matcher();
            matcher.StartsWith = "hello";
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("hello/world/how/are/youe");
            bool isNotAMatch = matcher.Match("Hello/World/how/are/you");
            matcher.CaseInsensitive = true;
            bool isMatch2 = matcher.Match("Hello/World/how/are/youe");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch, $"Expected {nameof(isNotAMatch)} to be false but received true.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
        }

        [Test]
        public void TestMatchWithEndsWith()
        {
            Matcher matcher = new Matcher();
            matcher.EndsWith = "world";
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("hello/world");
            bool isNotAMatch = matcher.Match("Hello/World");
            matcher.CaseInsensitive = true;
            bool isMatch2 = matcher.Match("hello/World");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch, $"Expected {nameof(isNotAMatch)} to be false but received true.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
        }

        [Test]
        public void TestMatchWithEqualsTo()
        {
            Matcher matcher = new Matcher();
            matcher.EqualsTo = "hello/world";
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("hello/world");
            bool isNotAMatch = matcher.Match("Hello/World");
            matcher.CaseInsensitive = true;
            bool isMatch2 = matcher.Match("hello/World");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch, $"Expected {nameof(isNotAMatch)} to be false but received true.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
        }

        [Test]
        public void TestMatchWithEqualsNotTo()
        {
            Matcher matcher = new Matcher();
            matcher.EqualsNotTo = "hello/world";
            matcher.CaseInsensitive = false;
            bool isMatch1 = matcher.Match("Hello/World");
            bool isNotAMatch1 = matcher.Match("hello/world");
            matcher.CaseInsensitive = true;
            bool isNotAMatch2 = matcher.Match("hello/World");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsFalse(isNotAMatch1, $"Expected {nameof(isNotAMatch1)} to be false but received true.");
            Assert.IsFalse(isNotAMatch2, $"Expected {nameof(isNotAMatch2)} to be false but received true.");
        }

        [Test]
        public void TestMatchFetchAll()
        {
            Matcher matcher = new Matcher();
            bool isMatch1 = matcher.Match("Hello/World");
            matcher.ContainsAllOf = new string[0];
            bool isMatch2 = matcher.Match("hello/world");
            matcher.CaseInsensitive = true;
            bool isMatch3 = matcher.Match("hello/World");

            Assert.IsTrue(isMatch1, $"Expected {nameof(isMatch1)} to be true but received false.");
            Assert.IsTrue(isMatch2, $"Expected {nameof(isMatch2)} to be true but received false.");
            Assert.IsTrue(isMatch3, $"Expected {nameof(isMatch3)} to be true but received false.");
        }
    }
}
