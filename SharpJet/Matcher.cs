// <copyright file="Matcher.cs" company="Hottinger Baldwin Messtechnik GmbH">
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

using System;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("SharpJetTests")]

namespace Hbm.Devices.Jet
{
    /// <summary>
    /// This class holds the properties required to fill a path matcher.
    /// </summary>
    public class Matcher
    {
        public string Contains { get; set; }

        public string[] ContainsAllOf { get; set; }

        public string StartsWith { get; set; }

        public string EndsWith { get; set; }

        public string EqualsTo { get; set; }

        public string EqualsNotTo { get; set; }

        public bool CaseInsensitive { get; set; }

        internal bool Match(string path)
        {
            StringComparison comparison = CaseInsensitive
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (string.IsNullOrEmpty(Contains) == false && path.IndexOf(Contains, comparison) >= 0)
                return true;

            if (ContainsAllOf != null)
            {
                foreach (string contains in ContainsAllOf)
                {
                    if (path.IndexOf(contains, comparison) >= 0)
                        return true;
                }
            }

            if (string.IsNullOrEmpty(StartsWith) == false && path.StartsWith(StartsWith, comparison))
                return true;

            if (string.IsNullOrEmpty(EndsWith) == false && path.EndsWith(EndsWith, comparison))
                return true;

            if (string.IsNullOrEmpty(EqualsTo) == false && EqualsTo.Equals(path, comparison))
                return true;

            if (string.IsNullOrEmpty(EqualsNotTo) == false && EqualsNotTo.Equals(path, comparison) == false)
                return true;

            return false;
        }
    }
}
