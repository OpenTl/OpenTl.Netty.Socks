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
using System.Net;
using System.Text;
using BarsGroup.CodeGuard.Exceptions;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The default {@link Socks5CommandResponse}.
 */
    public sealed class DefaultSocks5CommandResponse : AbstractSocks5Message,
        ISocks5CommandResponse
    {
        private static readonly IdnMapping Idn = new IdnMapping();

        public DefaultSocks5CommandResponse(
            Socks5CommandStatus status,
            Socks5AddressType bndAddrType,
            string bndAddr = null,
            int bndPort = 0)
        {
            if (bndAddr != null)
                if (bndAddrType == Socks5AddressType.Pv4)
                {
                    if (!IPAddress.TryParse(bndAddr, out _))
                        throw new ArgumentException("bndAddr: " + bndAddr + " (expected: a valid IPv4 address)");
                }
                else if (bndAddrType == Socks5AddressType.Domain)
                {
                    bndAddr = Idn.GetAscii(bndAddr);
                    if (bndAddr.Length > 255)
                        throw new ArgumentException("bndAddr: " + bndAddr + " (expected: less than 256 chars)");
                }
                else if (bndAddrType == Socks5AddressType.Pv6)
                {
                    if (!IPAddress.TryParse(bndAddr, out _))
                        throw new ArgumentException("bndAddr: " + bndAddr + " (expected: a valid IPv6 address)");
                }

            if (bndPort < 0 || bndPort > 65535)
                throw new ArgumentException("bndPort: " + bndPort + " (expected: 0~65535)");

            Status = status;
            BndAddrType = bndAddrType;
            BndAddr = bndAddr;
            BndPort = bndPort;
        }

        public Socks5CommandStatus Status { get; }

        public Socks5AddressType BndAddrType { get; }

        public string BndAddr { get; }

        public int BndPort { get; }

        public override string ToString()
        {
            var buf = new StringBuilder(128);
            buf.Append(StringUtil.SimpleClassName(this));

            if (!DecoderResult.IsSuccess())
            {
                buf.Append("(decoderResult: ");
                buf.Append(DecoderResult);
                buf.Append(", status: ");
            }
            else
            {
                buf.Append("(status: ");
            }

            buf.Append(Status);
            buf.Append(", bndAddrType: ");
            buf.Append(BndAddrType);
            buf.Append(", bndAddr: ");
            buf.Append(BndAddr);
            buf.Append(", bndPort: ");
            buf.Append(BndPort);
            buf.Append(')');

            return buf.ToString();
        }
    }
}