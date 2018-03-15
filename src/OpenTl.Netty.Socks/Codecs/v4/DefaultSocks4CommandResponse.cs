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

using System.Net;
using System.Text;
using BarsGroup.CodeGuard.Exceptions;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * The default {@link Socks4CommandResponse}.
 */
    public class DefaultSocks4CommandResponse : AbstractSocks4Message,
        ISocks4CommandResponse
    {
        /**
         * Creates a new instance.
         *
         * @param status the status of the response
         */

        /**
         * Creates a new instance.
         *
         * @param status the status of the response
         * @param dstAddr the {@code DSTIP} field of the response
         * @param dstPort the {@code DSTPORT} field of the response
         */
        public DefaultSocks4CommandResponse(Socks4CommandStatus status, string dstAddr = null, int dstPort = 0)
        {
            if (dstAddr != null)
                if (!IPAddress.TryParse(dstAddr, out _))
                    throw new ArgumentException(
                        "dstAddr: " + dstAddr + " (expected: a valid IPv4 address)");

            if (dstPort < 0 || dstPort > 65535)
                throw new ArgumentException("dstPort: " + dstPort + " (expected: 0~65535)");

            Status = status;
            DstAddr = dstAddr;
            DstPort = dstPort;
        }

        public Socks4CommandStatus Status { get; }

        public string DstAddr { get; }

        public int DstPort { get; }

        public override string ToString()
        {
            var buf = new StringBuilder(96);
            buf.Append(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", dstAddr: ");
            }
            else
            {
                buf.Append("(dstAddr: ");
            }

            buf.Append(DstAddr);
            buf.Append(", dstPort: ");
            buf.Append(DstPort);
            buf.Append(')');

            return buf.ToString();
        }
    }
}