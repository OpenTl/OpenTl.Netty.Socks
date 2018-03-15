/*
 * Copyright 2015 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except input compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to input writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 */

using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
* Decodes a SOCKS5 address field into its string representation.
*
* @see Socks5CommandRequestDecoder
* @see Socks5CommandResponseDecoder
*/
    public interface ISocks5AddressDecoder
    {
        /**
           * Decodes a SOCKS5 address field into its string representation.
           *
           * @param addrType the type of the address
           * @param input the input buffer which contains the SOCKS5 address field at its reader index
           */
        string DecodeAddress(Socks5AddressType addrType, IByteBuffer input);
    }

    public sealed class DefaultSocks5AddressDecoder : ISocks5AddressDecoder
    {
        private const int Pv6Len = 16;

        public static readonly ISocks5AddressDecoder Default = new DefaultSocks5AddressDecoder();

        public string DecodeAddress(Socks5AddressType addrType, IByteBuffer input)
        {
            if (Equals(addrType, Socks5AddressType.Pv4))
            {
                var bytes = new byte[4];
                input.ReadBytes(bytes);
                return new IPAddress(bytes).ToString();
            }

            if (Equals(addrType, Socks5AddressType.Domain))
            {
                int length = input.ReadByte();
                var domain = input.ToString(input.ReaderIndex, length, Encoding.ASCII);
                input.SkipBytes(length);
                return domain;
            }

            if (Equals(addrType, Socks5AddressType.Pv6))
            {
                var ipAddressData = new byte[Pv6Len];
                input.ReadBytes(ipAddressData);
                return new IPAddress(ipAddressData).ToString();
            }

            throw new DecoderException("unsupported address type: " + (addrType.ByteValue & 0xFF));
        }
    }
}