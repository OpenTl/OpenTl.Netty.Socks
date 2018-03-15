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

using System.Globalization;
using System.Text;
using BarsGroup.CodeGuard.Exceptions;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * The default {@link Socks4CommandRequest}.
 */
    public sealed class DefaultSocks4CommandRequest : AbstractSocks4Message,
        ISocks4CommandRequest
    {
        private static readonly IdnMapping Idn = new IdnMapping();

        /**
         * Creates a new instance.
         *
         * @param type the type of the request
         * @param dstAddr the {@code DSTIP} field of the request
         * @param dstPort the {@code DSTPORT} field of the request
         */

        /**
         * Creates a new instance.
         *
         * @param type the type of the request
         * @param dstAddr the {@code DSTIP} field of the request
         * @param dstPort the {@code DSTPORT} field of the request
         * @param userId the {@code USERID} field of the request
         */
        public DefaultSocks4CommandRequest(Socks4CommandType type, string dstAddr, int dstPort, string userId = "")
        {
            if (dstPort <= 0 || dstPort >= 65536)
                throw new ArgumentException("dstPort: " + dstPort + " (expected: 1~65535)");

            UserId = userId;
            Type = type;
            DstAddr = Idn.GetAscii(dstAddr);
            DstPort = dstPort;
        }

        public Socks4CommandType Type { get; }

        public string DstAddr { get; }

        public int DstPort { get; }

        public string UserId { get; }

        public override string ToString()
        {
            var buf = new StringBuilder(128);
            buf.Append(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", type: ");
            }
            else
            {
                buf.Append("(type: ");
            }

            buf.Append(Type);
            buf.Append(", dstAddr: ");
            buf.Append(DstAddr);
            buf.Append(", dstPort: ");
            buf.Append(DstPort);
            buf.Append(", userId: ");
            buf.Append(UserId);
            buf.Append(')');

            return buf.ToString();
        }
    }
}