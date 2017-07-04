// <copyright file="SimpleFetchHandlerTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System.Collections.Generic;
using System.Linq;

namespace SharpJetTests
{
    using NUnit.Framework;
    using Hbm.Devices.Jet.Utils;
    using System;
    using FakeItEasy;
    using Hbm.Devices.Jet;
    using Newtonsoft.Json.Linq;

    [TestFixture]
    public class SimpleFetchHandlerTests
    {
        [Test]
        public void TestConstructorInitializesCaches()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            Assert.AreEqual(0, handler.CachedPaths.Count);
            Assert.AreEqual(0, handler.MatcherFetcherPairs.Count);
            Assert.AreEqual(0, handler.RegisteredMatchers.Count);
        }

        [Test]
        public void TestRemoveAllFetches()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());

            //Add some random stuff to make sure that caches get cleared.
            handler.CachedPaths.Add("key1", new HashSet<Matcher> { new Matcher { EqualsTo = "Hello" }, new Matcher() { EqualsTo = "world" } });
            handler.MatcherFetcherPairs.Add(new Matcher(), new JetFetcher(A.Dummy<Action<JToken>>()));
            handler.RegisteredMatchers.Add(1, new Matcher());

            handler.RemoveAllFetches();
            Assert.AreEqual(0, handler.CachedPaths.Count);
            Assert.AreEqual(0, handler.MatcherFetcherPairs.Count);
            Assert.AreEqual(0, handler.RegisteredMatchers.Count);
        }

        [Test, Parallelizable(ParallelScope.None)]
        public void TestFetchReturnsJObject()
        {
            Func<JetMethod, JObject> jetMethodExec = A.Fake<Func<JetMethod, JObject>>();
            JObject json = new JObject();
            json["id"] = 1;
            json["result"] = true;
            A.CallTo(() => jetMethodExec(A<JetMethod>._)).Returns(json);
            SimpleFetchHandler handler = new SimpleFetchHandler(jetMethodExec);

            FetchId fetchId;
            JObject json1 = handler.Fetch(out fetchId, new Matcher { EqualsNotTo = "charlie/the/unicorn" }, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json2 = handler.Fetch(out fetchId, new Matcher { EqualsNotTo = "foo/bar" }, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);
            Assert.AreEqual(json, json1);
            Assert.AreEqual(2, json2["id"].ToObject<int>());
            Assert.AreEqual(true, json2["result"].ToObject<bool>());
        }

        [Test]
        public void TestFetchExecutesMethodOnlyOnFirstCall()
        {
            Func<JetMethod, JObject> jetMethodExec = A.Fake<Func<JetMethod, JObject>>();
            SimpleFetchHandler handler = new SimpleFetchHandler(jetMethodExec);

            FetchId fetchId;
            //First call.
            handler.Fetch(out fetchId, new Matcher { EqualsTo = "hello/world" }, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);

            //Second call.
            handler.Fetch(out fetchId, new Matcher { EqualsTo = "foo/bar" }, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);
            A.CallTo(() => jetMethodExec(A<JetMethod>._)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestFetchRegistersMatcher()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());

            FetchId fetchId;
            //First call.
            Matcher matcher = new Matcher { EqualsTo = "hello/world" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);

            Assert.IsTrue(handler.RegisteredMatchers.Values.Any(m => m.Equals(matcher)));
            Assert.IsTrue(handler.MatcherFetcherPairs.ContainsKey(matcher));
        }

        [Test, Parallelizable(ParallelScope.None)]
        public void TestFetchIncreasesFetchId()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());

            FetchId fetchId;
            //First call.
            handler.Fetch(out fetchId, new Matcher { EqualsTo = "hello/world" }, A.Dummy<Action<JToken>>(),
                A.Dummy<Action<bool, JToken>>(), 1000.0);

            int firstId = fetchId.GetId();

            for (int i = 1; i <= 10; i++)
            {
                handler.Fetch(out fetchId, new Matcher { EqualsTo = "hello/world" }, A.Dummy<Action<JToken>>(),
                    A.Dummy<Action<bool, JToken>>(), 1000.0);
                Assert.AreEqual(i, fetchId.GetId() - firstId);
            }
        }

        [Test]
        public void TestFetchMatcherIsNull()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            Assert.Throws<ArgumentNullException>(() =>
            {
                FetchId fetchId;
                handler.Fetch(out fetchId, null, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);
            });
        }

        [Test]
        public void TestUnfetchFetchIdIsNull()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            Assert.Throws<ArgumentNullException>(() =>
            {
                handler.Unfetch(null, A.Dummy<Action<bool, JToken>>(), 1000.0);
            });
        }

        [Test]
        public void TestUnfetchReturnsJObject()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "foo/bar" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = handler.Unfetch(new FetchId(1), A.Dummy<Action<bool, JToken>>(), 1000.0);
            Assert.AreEqual(1, json["id"].ToObject<int>());
            Assert.IsTrue(json["result"].ToObject<bool>());
        }

        [Test, Parallelizable(ParallelScope.None)]
        public void TestUnfetchUpdatesCaches()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher m1 = new Matcher { EqualsTo = "foo/bar" };
            Matcher m2 = new Matcher { EqualsTo = "hello/world" };
            Matcher m3 = new Matcher { EqualsTo = "john/doe" };
            handler.Fetch(out fetchId, m1, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);
            handler.Fetch(out fetchId, m2, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);
            handler.Fetch(out fetchId, m3, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            handler.Unfetch(new FetchId(2), A.Dummy<Action<bool, JToken>>(), 1000.0);
            handler.Unfetch(new FetchId(3), A.Dummy<Action<bool, JToken>>(), 1000.0);
            Assert.IsTrue(handler.MatcherFetcherPairs.ContainsKey(m1));
            Assert.IsFalse(handler.MatcherFetcherPairs.ContainsKey(m2));
            Assert.IsFalse(handler.MatcherFetcherPairs.ContainsKey(m2));

            Assert.IsTrue(handler.RegisteredMatchers.ContainsKey(1));
            Assert.IsFalse(handler.RegisteredMatchers.ContainsKey(2));
            Assert.IsFalse(handler.RegisteredMatchers.ContainsKey(3));
        }

        [Test]
        public void TestGetFetchId()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            JObject json = new JObject();
            json["method"] = "fetch_all";
            JToken id = handler.GetFetchId(json);
            Assert.AreEqual(-1, id.ToObject<int>());
        }

        [Test]
        public void TestGetFetchIdInvalidJson()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            JObject json = new JObject();
            json["method"] = 15.32;
            JToken id = handler.GetFetchId(json);
            Assert.IsNull(id, $"Expected {nameof(id)} to be null but received {id}.");
        }

        [Test]
        public void TestHandleFetchRemoveWithoutAdd()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "remove";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode = handler.HandleFetch(1, json);

            Assert.AreEqual(StatusCode.RemoveWithoutAdd, statusCode);
        }

        [Test]
        public void TestHandleFetchOnRemove()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "add";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode1 = handler.HandleFetch(1, json);
            json["params"]["event"] = "remove";
            StatusCode statusCode2 = handler.HandleFetch(1, json);

            Assert.AreEqual(StatusCode.Success, statusCode1);
            Assert.AreEqual(StatusCode.Success, statusCode2);
            Assert.AreEqual(0, handler.CachedPaths.Count);
        }

        [Test]
        public void TestHandleFetchChangeWithoutAdd()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "change";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode = handler.HandleFetch(1, json);
            Assert.AreEqual(StatusCode.ChangeWithoutAdd, statusCode);
            Assert.AreEqual(0, handler.CachedPaths.Count);
        }

        [Test]
        public void TestHandleFetchOnAddAndRemoveInvokesCallback()
        {
            {
                SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
                FetchId fetchId;
                Matcher matcher = new Matcher {EqualsTo = "abc/def"};
                Action<JToken> fetchCallback = A.Fake<Action<JToken>>();
                handler.Fetch(out fetchId, matcher, fetchCallback, A.Dummy<Action<bool, JToken>>(), 1000.0);

                JObject json = new JObject();
                JObject p = new JObject();
                p["event"] = "add";
                p["path"] = "abc/def";
                json["params"] = p;
                handler.HandleFetch(1, json);
                json["params"]["event"] = "remove";
                handler.HandleFetch(1, json);

                A.CallTo(() => fetchCallback(p)).MustHaveHappened(Repeated.Exactly.Twice);
            }
        }

        [Test]
        public void TestHandleFetchOnAddAndChangeInvokesCallback()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            Action<JToken> fetchCallback = A.Fake<Action<JToken>>();
            handler.Fetch(out fetchId, matcher, fetchCallback, A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "add";
            p["path"] = "abc/def";
            json["params"] = p;
            handler.HandleFetch(1, json);
            json["params"]["event"] = "change";
            handler.HandleFetch(1, json);

            A.CallTo(() => fetchCallback(p)).MustHaveHappened(Repeated.Exactly.Twice);
        }

        [Test]
        public void TestHandleFetchOnChange()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "add";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode1 = handler.HandleFetch(1, json);
            json["params"]["event"] = "change";
            StatusCode statusCode2 = handler.HandleFetch(1, json);

            Assert.AreEqual(StatusCode.Success, statusCode1);
            Assert.AreEqual(StatusCode.Success, statusCode2);
            Assert.AreEqual(1, handler.CachedPaths.Count);
        }

        [Test]
        public void TestHandleFetchPathAddedTwice()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "add";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode1 = handler.HandleFetch(-1, json);
            StatusCode statusCode2 = handler.HandleFetch(-1, json);
            Assert.AreEqual(StatusCode.Success, statusCode1);
            Assert.AreEqual(StatusCode.MultipleAdd, statusCode2);
        }

        [Test]
        public void TestHandleFetchOnAdd()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId fetchId;
            Matcher matcher = new Matcher { EqualsTo = "abc/def" };
            handler.Fetch(out fetchId, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(), 1000.0);

            JObject json = new JObject();
            JObject p = new JObject();
            p["event"] = "add";
            p["path"] = "abc/def";
            json["params"] = p;
            StatusCode statusCode = handler.HandleFetch(1, json);
            Assert.AreEqual(StatusCode.Success, statusCode);
            Assert.IsTrue(handler.CachedPaths.ContainsKey("abc/def"));
        }

        [Test]
        public void TestHandleFetchEventIsNull()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());

            JObject json = new JObject();
            json["params"] = new JObject();
            StatusCode statusCode = handler.HandleFetch(1, json);
            Assert.AreEqual(StatusCode.FetchEventNotSpecified, statusCode);
        }

        [Test]
        public void TestHandleFetchParamsIsNull()
        {
            SimpleFetchHandler handler = new SimpleFetchHandler(A.Dummy<Func<JetMethod, JObject>>());

            JObject json = new JObject();
            StatusCode statusCode = handler.HandleFetch(1, json);
            Assert.AreEqual(StatusCode.ParamsNotSpecified, statusCode);
        }
    }
}
