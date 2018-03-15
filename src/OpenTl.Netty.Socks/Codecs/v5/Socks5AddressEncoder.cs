/*
 * Copyright 2015 The Netty Project
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
using DotNetty.Buffers;
using DotNetty.Codecs;
using OpenTl.Netty.Socks.Utils;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    public sealed class DefaultSocks5AddressEncoder : ISocks5AddressEncoder
    {
        public static readonly ISocks5AddressEncoder Default = new DefaultSocks5AddressEncoder();

        public void EncodeAddress(Socks5AddressType addrType, string addrValue, IByteBuffer output)
        {
            var typeVal = addrType.ByteValue;
            if (typeVal == Socks5AddressType.Pv4.ByteValue)
                if (addrValue != null)
                    output.WriteBytes(NetUtil.CreateByteArrayFromIpAddressString(addrValue));
                else
                    output.WriteInt(0);
            else if (typeVal == Socks5AddressType.Domain.ByteValue)
                if (addrValue != null)
                {
                    output.WriteByte(addrValue.Length);
                    output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, addrValue,
                        Encoding.ASCII));
                }
                else
                {
                    output.WriteByte(1);
                    output.WriteByte(0);
                }
            else if (typeVal == Socks5AddressType.Pv6.ByteValue)
                if (addrValue != null)
                {
                    output.WriteBytes(NetUtil.CreateByteArrayFromIpAddressString(addrValue));
                }
                else
                {
                    output.WriteLong(0);
                    output.WriteLong(0);
                }
            else
                throw new EncoderException("unsupported addrType: " + (addrType.ByteValue & 0xFF));
        }
    }

    /**
 * Encodes a SOCKS5 address into binary representation.
 *
 * @see Socks5ClientEncoder
 * @see Socks5ServerEncoder
 */
    public interface ISocks5AddressEncoder
    {
        /**
         * Encodes a SOCKS5 address.
         *
         * @param addrType the type of the address
         * @param addrValue the string representation of the address
         * @param output the output buffer where the encoded SOCKS5 address field will be written to
         */
        void EncodeAddress(Socks5AddressType addrType, string addrValue, IByteBuffer output);
    }
}