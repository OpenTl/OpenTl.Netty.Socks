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

using System.Text;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The default {@link Socks5InitialResponse}.
 */
    public sealed class DefaultSocks5InitialResponse : AbstractSocks5Message,
        ISocks5InitialResponse
    {
        public DefaultSocks5InitialResponse(Socks5AuthMethod authMethod)
        {
            AuthMethod = authMethod;
        }

        public Socks5AuthMethod AuthMethod { get; }

        public string ToString()
        {
            var buf = new StringBuilder(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", authMethod: ");
            }
            else
            {
                buf.Append("(authMethod: ");
            }

            buf.Append(AuthMethod);
            buf.Append(')');

            return buf.ToString();
        }
    }
}