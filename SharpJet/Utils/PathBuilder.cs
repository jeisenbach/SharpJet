// <copyright file="PathBuilder.cs" company="Hottinger Baldwin Messtechnik GmbH">
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
    using Newtonsoft.Json.Linq;

    internal class PathBuilder : IPathBuilder
    {
        public JObject Fill(Matcher matcher)
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
