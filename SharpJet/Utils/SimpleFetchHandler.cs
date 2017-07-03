// <copyright file="SimpleFetchHandler.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System.Threading;

namespace Hbm.Devices.Jet.Utils
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;

    internal class SimpleFetchHandler : IFetchHandler
    {
        private readonly object lockObject = new object();
        private bool isFetching;
        private int fetchIdCounter;
        private readonly Func<JetMethod, JObject> jetMethodExecution;
        private readonly Dictionary<string, HashSet<Matcher>> cachedPaths;
        private readonly Dictionary<Matcher, JetFetcher> matcherFetcherPairs;
        private readonly Dictionary<int, Matcher> registeredMatchers;

        internal SimpleFetchHandler(Func<JetMethod, JObject> jetMethodExecution)
        {
            this.cachedPaths = new Dictionary<string, HashSet<Matcher>>();
            this.matcherFetcherPairs = new Dictionary<Matcher, JetFetcher>();
            this.registeredMatchers = new Dictionary<int, Matcher>();
            this.jetMethodExecution = jetMethodExecution;
        }

        public JObject Fetch(out FetchId id, Matcher matcher, Action<JToken> fetchCallback, Action<bool, JToken> responseCallback,
            double responseTimeoutMs)
        {
            if (matcher == null)
                throw new ArgumentNullException(nameof(matcher));

            int fetchId = Interlocked.Increment(ref this.fetchIdCounter);
            id = new FetchId(fetchId);
            RegisterMatcher(fetchId, matcher, fetchCallback);

            if (this.isFetching == false)
            {
                JObject parameters = new JObject();
                parameters["id"] = id.GetId();
                JetMethod fetch = new JetMethod(JetMethod.Fetch, parameters, responseCallback, responseTimeoutMs);
                this.isFetching = true;
                return jetMethodExecution.Invoke(fetch);
            }
            else
            {
                JObject json = new JObject();
                json["result"] = true;
                json["id"] = id.GetId();
                return json;
            }
        }

        public JObject Unfetch(FetchId fetchId, Action<bool, JToken> responseCallback, double responseTimeoutMs)
        {
            lock (this.lockObject)
            {
                int id = fetchId.GetId();
                if (this.registeredMatchers.ContainsKey(id))
                {
                    Matcher matcher = this.registeredMatchers[id];
                    this.matcherFetcherPairs.Remove(matcher);

                    foreach (string path in this.cachedPaths.Keys)
                    {
                        HashSet<Matcher> matchers = this.cachedPaths[path];
                        matchers.Remove(matcher);
                    }
                    this.registeredMatchers.Remove(id);
                }
            }
            return new JObject();
        }

        public void HandleFetch(int fetchId, JObject json)
        {
            lock (this.lockObject)
            {
                JToken parameter = json["params"];

                if (parameter == null)
                {
                    //TODO: handle
                    throw new Exception();
                }

                string jetEvent = parameter["event"]?.ToString();
                if (jetEvent == "add")
                {
                    HandleFetchAddEvent(parameter);
                }
                else if (jetEvent == "change")
                {
                    HandleFetchChangeEvent(parameter);
                }
                else if (jetEvent == "remove")
                {
                    HandleFetchRemoveEvent(parameter);
                }
            }
        }

        public void RemoveAllFetches()
        {
            lock (this.lockObject)
            {
                this.registeredMatchers.Clear();
                this.cachedPaths.Clear();
                this.matcherFetcherPairs.Clear();
            }
        }

        public JToken GetFetchId(JObject json)
        {
            JToken methodToken = json["method"];
            if ((methodToken != null) && (methodToken.Type == JTokenType.String) && (methodToken.ToString() == "fetch_all"))
            {
                return new JValue(-1);
            }

            return null;
        }

        private void ProcessPath(string path, JToken parameter)
        {
            HashSet<Matcher> matchers = this.cachedPaths[path];
            foreach (Matcher matcher in matchers)
            {
                JetFetcher fetcher = this.matcherFetcherPairs[matcher];
                fetcher.CallFetchCallback(parameter);
            }
        }

        private void HandleFetchRemoveEvent(JToken parameter)
        {
            string path = parameter["path"].ToString();
            if (this.cachedPaths.ContainsKey(path))
            {
                this.cachedPaths.Remove(path);
            }
            else
            {
                //TODO: Fehler remove ohne vorher add!
            }
        }

        private void HandleFetchChangeEvent(JToken parameter)
        {
            string path = parameter["path"].ToString();
            if (this.cachedPaths.ContainsKey(path) == false)
            {
                //TODO: Fehlerfall. Change darf nicht ohne Add aufgerufen
            }
            else
            {
                ProcessPath(path, parameter);
            }
        }

        private void HandleFetchAddEvent(JToken parameter)
        {
            string path = parameter["path"].ToString();
            if (this.cachedPaths.ContainsKey(path) == false)
            {
                HashSet<Matcher> matchersForPath = new HashSet<Matcher>();
                foreach (Matcher matcher in this.matcherFetcherPairs.Keys)
                {
                    if (matcher.Match(path))
                    {
                        matchersForPath.Add(matcher);
                    }
                }
                this.cachedPaths.Add(path, matchersForPath);
            }
            else
            {
                //TODO: Fehlerfall! Add darf nicht zweimal aufgerufen werden
            }
            ProcessPath(path, parameter);
        }

        private void RegisterMatcher(int fetchId, Matcher matcher, Action<JToken> fetchCallback)
        {
            lock (lockObject)
            {
                if (this.registeredMatchers.ContainsKey(fetchId) == false)
                {
                    this.registeredMatchers.Add(fetchId, matcher);
                    this.matcherFetcherPairs.Add(matcher, new JetFetcher(fetchCallback));

                    foreach (string path in this.cachedPaths.Keys)
                    {
                        if (matcher.Match(path))
                            this.cachedPaths[path].Add(matcher);
                    }
                }
            }
        }
    }
}
