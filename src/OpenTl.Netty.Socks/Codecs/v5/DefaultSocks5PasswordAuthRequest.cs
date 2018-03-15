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
using System.Text;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The default {@link Socks5PasswordAuthRequest}.
 */
    public sealed class DefaultSocks5PasswordAuthRequest : AbstractSocks5Message,
        ISocks5PasswordAuthRequest
    {
        public DefaultSocks5PasswordAuthRequest(string username, string password)
        {
            if (username.Length > 255) throw new ArgumentException("username: **** (expected: less than 256 chars)");

            if (password.Length > 255) throw new ArgumentException("password: **** (expected: less than 256 chars)");

            Username = username;
            Password = password;
        }

        public string Username { get; }

        public string Password { get; }

        public override string ToString()
        {
            var buf = new StringBuilder(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", username: ");
            }
            else
            {
                buf.Append("(username: ");
            }

            buf.Append(Username);
            buf.Append(", password: ****)");

            return buf.ToString();
        }
    }
}