// <copyright file="FullFetchHandlerTests.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using System;
    using FakeItEasy;
    using Hbm.Devices.Jet;
    using Hbm.Devices.Jet.Utils;
    using Newtonsoft.Json.Linq;
    using NUnit.Framework;

    [TestFixture]
    public class FullFetchHandlerTests
    {
        [Test]
        public void TestFetchIncreasesFetchId()
        {
            FullFetchHandler fetchHandler = new FullFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            FetchId id;
            int numOfFetches = 5;
            for (int i = 0; i < numOfFetches; i++)
            {
                fetchHandler.Fetch(out id, new Matcher(), A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(),
                    1000.0);
                Assert.AreEqual(i + 1, id.GetId());
            }
        }

        [Test]
        public void TestFetchFillsPath()
        {
            FullFetchHandler fetchHandler = new FullFetchHandler(A.Dummy<Func<JetMethod, JObject>>());
            fetchHandler.PathBuilder = A.Fake<IPathBuilder>();
            FetchId id;
            Matcher matcher = new Matcher();
            fetchHandler.Fetch(out id, matcher, A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(),
                1000.0);
            A.CallTo(() => fetchHandler.PathBuilder.Fill(matcher)).MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestFetchInvokesWithJsonMethodFetch()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            FetchId id;
            fetchHandler.Fetch(out id, new Matcher(), A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(),
                1000.0);

            A.CallTo(() => jetMethodExecution(A<JetMethod>.That.Matches
                    (m => m.GetJson()["method"].Equals(new JValue("fetch")))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestFetchInvokesWithJetMethod()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            FetchId id;
            fetchHandler.Fetch(out id, new Matcher(), A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(),
                1000.0);

            A.CallTo(() => jetMethodExecution(A<JetMethod>.That.Matches
                    (m => m.GetTimeoutMs() == 1000.0 && m.GetRequestId() > 0)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestUnfetchInvokesWithJsonMethodUnfetch()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            FetchId id;

            fetchHandler.Fetch(out id, new Matcher(), A.Dummy<Action<JToken>>(), A.Dummy<Action<bool, JToken>>(),
                1000.0);

            fetchHandler.Unfetch(id, A.Dummy<Action<bool, JToken>>(), 1000.0);

            A.CallTo(() => jetMethodExecution(A<JetMethod>.That.Matches
                    (m => m.GetJson()["method"].Equals(new JValue("unfetch")))))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestUnfetchInvokesWithJetMethod()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            FetchId id = new FetchId(1);

            fetchHandler.Unfetch(id, A.Dummy<Action<bool, JToken>>(), 1000.0);

            A.CallTo(() => jetMethodExecution(A<JetMethod>.That.Matches
                    (m => m.GetTimeoutMs() == 1000.0 && m.GetRequestId() > 0)))
                .MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestHandleFetchInvokesFetchCallbackIfFetchIdExists()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            Action<JToken> fetchCallback = A.Fake<Action<JToken>>();
            FetchId id;
            fetchHandler.Fetch(out id, new Matcher(), fetchCallback, A.Dummy<Action<bool, JToken>>(),
                1000.0);

            JObject json = new JObject();
            json["params"] = new JValue(42);
            fetchHandler.HandleFetch(1, json);

            A.CallTo(() => fetchCallback.Invoke(A<JToken>.That.Matches(t => t.Value<int>() == 42))).
                MustHaveHappened(Repeated.Exactly.Once);
        }

        [Test]
        public void TestHandleFetchDoesNotInvokeFetchCallbackIfFetchDoesNotExist()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);
            Action<JToken> fetchCallback = A.Fake<Action<JToken>>();
            FetchId id;
            fetchHandler.Fetch(out id, new Matcher(), fetchCallback, A.Dummy<Action<bool, JToken>>(),
                1000.0);

            JObject json = new JObject();
            json["params"] = new JValue(15);
            fetchHandler.HandleFetch(2, json);

            A.CallTo(() => fetchCallback.Invoke(A<JToken>._)).MustNotHaveHappened();
        }

        [Test]
        public void TestRemoveAllFetches()
        {
            Func<JetMethod, JObject> jetMethodExecution = A.Fake<Func<JetMethod, JObject>>();
            FullFetchHandler fetchHandler = new FullFetchHandler(jetMethodExecution);

            FetchId id;
            Action<JToken> fetchCallback1 = A.Fake<Action<JToken>>();
            fetchHandler.Fetch(out id, new Matcher(), fetchCallback1, A.Dummy<Action<bool, JToken>>(),
                1000.0);
            Action<JToken> fetchCallback2 = A.Fake<Action<JToken>>();
            fetchHandler.Fetch(out id, new Matcher(), fetchCallback2, A.Dummy<Action<bool, JToken>>(),
                1000.0);

            fetchHandler.RemoveAllFetches();

            A.CallTo(() => fetchCallback1(A<JToken>._)).MustNotHaveHappened();
            A.CallTo(() => fetchCallback2(A<JToken>._)).MustNotHaveHappened();
        }
    }
}
