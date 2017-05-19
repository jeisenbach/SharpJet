// <copyright file="PathBuilderTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using Hbm.Devices.Jet;
using Hbm.Devices.Jet.Utils;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace SharpJetTests
{
    [TestFixture]
    public class PathBuilderTests
    {
        [Test]
        public void TestFillEmptyMatcher()
        {
            PathBuilder builder = new PathBuilder();
            JObject json = builder.Fill(new Matcher());
            Assert.IsNull(json, $"Expected {nameof(json)} to be null but received {json}.");
        }

        [Test]
        public void TestFill()
        {
            PathBuilder builder = new PathBuilder();
            Matcher matcher = new Matcher();
            matcher.Contains = "contains";
            matcher.ContainsAllOf = new[] { "c1", "c2" };
            matcher.EndsWith = "endsWith";
            matcher.EqualsNotTo = "equalsNotTo";
            matcher.EqualsTo = "equalsTo";
            matcher.StartsWith = "startsWith";
            JObject json = builder.Fill(matcher);

            Assert.AreEqual("contains", (string)json["contains"]);
            Assert.AreEqual("c1", (string)json["containsAllOf"][0]);
            Assert.AreEqual("c2", (string)json["containsAllOf"][1]);
            Assert.AreEqual("endsWith", (string)json["endsWith"]);
            Assert.AreEqual("equalsNotTo", (string)json["equalsNot"]);
            Assert.AreEqual("equalsTo", (string)json["equals"]);
            Assert.AreEqual("startsWith", (string)json["startsWith"]);
        }
    }
}
