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
using System.Globalization;
using System.Net;
using System.Text;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The default {@link Socks5CommandRequest}.
 */
    public sealed class DefaultSocks5CommandRequest : AbstractSocks5Message,
        ISocks5CommandRequest
    {
        private static readonly IdnMapping Idn = new IdnMapping();

        public DefaultSocks5CommandRequest(
            Socks5CommandType type,
            Socks5AddressType dstAddrType,
            string dstAddr,
            int dstPort)
        {
            if (Equals(dstAddrType, Socks5AddressType.Pv4))
            {
                if (!IPAddress.TryParse(dstAddr, out _))
                    throw new ArgumentException("dstAddr: " + dstAddr + " (expected: a valid IPv4 address)");
            }
            else if (dstAddrType == Socks5AddressType.Domain)
            {
                dstAddr = Idn.GetAscii(dstAddr);
                if (dstAddr.Length > 255)
                    throw new ArgumentException("dstAddr: " + dstAddr + " (expected: less than 256 chars)");
            }
            else if (dstAddrType == Socks5AddressType.Pv6)
            {
                if (!IPAddress.TryParse(dstAddr, out _))
                    throw new ArgumentException("dstAddr: " + dstAddr + " (expected: a valid IPv6 address");
            }

            if (dstPort < 0 || dstPort > 65535)
                throw new ArgumentException("dstPort: " + dstPort + " (expected: 0~65535)");

            Type = type;
            DstAddrType = dstAddrType;
            DstAddr = dstAddr;
            DstPort = dstPort;
        }

        public Socks5CommandType Type { get; }

        public Socks5AddressType DstAddrType { get; }

        public string DstAddr { get; }

        public int DstPort { get; }

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
            buf.Append(", dstAddrType: ");
            buf.Append(DstAddrType);
            buf.Append(", dstAddr: ");
            buf.Append(DstAddr);
            buf.Append(", dstPort: ");
            buf.Append(DstPort);
            buf.Append(')');

            return buf.ToString();
        }
    }
}