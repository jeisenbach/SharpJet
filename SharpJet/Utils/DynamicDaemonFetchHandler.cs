// <copyright file="DynamicDaemonFetchHandler.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("SharpJetTests")]

namespace Hbm.Devices.Jet.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using Newtonsoft.Json.Linq;

    internal class DynamicDaemonFetchHandler : IFetchHandler
    {
        private int fetchIdCounter;
        private readonly HashSet<FetchId> allFetches;
        private readonly Dictionary<int, JetFetcher> openFetches;
        private readonly Func<JetMethod, JObject> jetMethodExecution;

        internal DynamicDaemonFetchHandler(Func<JetMethod, JObject> jetMethodExecution)
        {
            allFetches = new HashSet<FetchId>();
            openFetches = new Dictionary<int, JetFetcher>();
            this.jetMethodExecution = jetMethodExecution;
        }

        public JObject Fetch(out FetchId id, Matcher matcher, Action<JToken> fetchCallback, 
            Action<bool, JToken> responseCallback, double responseTimeoutMs)
        {
            int fetchId = Interlocked.Increment(ref this.fetchIdCounter);
            JetFetcher fetcher = new JetFetcher(fetchCallback);
            this.RegisterFetcher(fetchId, fetcher);

            JObject parameters = new JObject();
            JObject path = this.FillPath(matcher);
            if (path != null)
            {
                parameters["path"] = path;
            }

            parameters["caseInsensitive"] = matcher.CaseInsensitive;
            parameters["id"] = fetchId;
            JetMethod fetch = new JetMethod(JetMethod.Fetch, parameters, responseCallback, responseTimeoutMs);
            id = new FetchId(fetchId);
            lock (allFetches)
            {
                allFetches.Add(id);
            }

            return jetMethodExecution.Invoke(fetch);
        }

        public JObject Unfetch(FetchId fetchId, Action<bool, JToken> responseCallback, double responseTimeoutMs)
        {
            this.UnregisterFetcher(fetchId.GetId());

            JObject parameters = new JObject();
            parameters["id"] = fetchId.GetId();
            JetMethod unfetch = new JetMethod(JetMethod.Unfetch, parameters, responseCallback, responseTimeoutMs);
            lock (allFetches)
            {
                allFetches.Remove(fetchId);
            }

            return jetMethodExecution.Invoke(unfetch);
        }

        public void HandleFetch(int fetchId, JObject json)
        {
            JetFetcher fetcher = null;
            lock (this.openFetches)
            {
                if (this.openFetches.ContainsKey(fetchId))
                {
                    fetcher = this.openFetches[fetchId];
                }
            }

            if (fetcher != null)
            {
                JToken parameters = json["params"];
                if ((parameters != null) && (parameters.Type != JTokenType.Null))
                {
                    fetcher.CallFetchCallback(parameters);
                }
                else
                {
                    // Todo: Log error
                }
            }
        }

        public void RemoveAllFetches()
        {
            var tempSet = new HashSet<FetchId>();
            lock (allFetches)
            {
                foreach (var fetchId in allFetches)
                {
                    tempSet.Add(fetchId);
                }
            }

            foreach (var fetchId in tempSet)
            {
                this.Unfetch(fetchId, null, 0);
            }
        }

        private void RegisterFetcher(int fetchId, JetFetcher fetcher)
        {
            lock (this.openFetches)
            {
                this.openFetches.Add(fetchId, fetcher);
            }
        }

        private void UnregisterFetcher(int fetchId)
        {
            lock (this.openFetches)
            {
                this.openFetches.Remove(fetchId);
            }
        }

        private JObject FillPath(Matcher matcher)
        {
            JObject path = new JObject();
            if (!string.IsNullOrEmpty(matcher.Contains))
            {
                path["contains"] = matcher.Contains;
            }

            if (!string.IsNullOrEmpty(matcher.StartsWith))
            {
                path["startsWith"] = matcher.StartsWith;
            }

            if (!string.IsNullOrEmpty(matcher.EndsWith))
            {
                path["endsWith"] = matcher.EndsWith;
            }

            if (!string.IsNullOrEmpty(matcher.EqualsTo))
            {
                path["equals"] = matcher.EqualsTo;
            }

            if (!string.IsNullOrEmpty(matcher.EqualsNotTo))
            {
                path["equalsNot"] = matcher.EqualsNotTo;
            }

            if ((matcher.ContainsAllOf != null) && matcher.ContainsAllOf.Length > 0)
            {
                path["containsAllOf"] = JToken.FromObject(matcher.ContainsAllOf);
            }

            if (path.Count == 0)
            {
                return null;
            }
            else
            {
                return path;
            }
        }
    }
}
