/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The default {@link Socks5InitialRequest}.
 */
    public sealed class DefaultSocks5InitialRequest : AbstractSocks5Message,
        ISocks5InitialRequest
    {
        public DefaultSocks5InitialRequest(params Socks5AuthMethod[] authMethods)
        {
            var list = new List<Socks5AuthMethod>(authMethods.Length);
            foreach (var m in authMethods)
            {
                if (m == null) break;

                list.Add(m);
            }

            if (list.Count == 0) throw new ArgumentException("authMethods is empty");

            AuthMethods = list.AsReadOnly();
        }

        public DefaultSocks5InitialRequest(IEnumerable<Socks5AuthMethod> authMethods) : this(authMethods.ToArray())
        {
        }

        public ICollection<Socks5AuthMethod> AuthMethods { get; }

        public override string ToString()
        {
            var buf = new StringBuilder(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", authMethods: ");
            }
            else
            {
                buf.Append("(authMethods: ");
            }

            buf.Append(AuthMethods);
            buf.Append(')');

            return buf.ToString();
        }
    }
}