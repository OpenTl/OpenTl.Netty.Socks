/*
 * Copyright 2014 The Netty Project
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
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.Utils;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * Encodes a {@link Socks4CommandRequest} into a {@link ByteBuf}.
 */
    public sealed class Socks4ClientEncoder : MessageToByteEncoder<ISocks4CommandRequest>
    {
        /**
     * The singleton instance of {@link Socks4ClientEncoder}
     */
        public static readonly Socks4ClientEncoder Instance = new Socks4ClientEncoder();

        private static readonly byte[] Pv4DomainMarker = {0x00, 0x00, 0x00, 0x01};

        private Socks4ClientEncoder()
        {
        }

        public override bool IsSharable { get; } = true;

        protected override void Encode(IChannelHandlerContext context, ISocks4CommandRequest message,
            IByteBuffer output)
        {
            output.WriteByte((int) message.Version());
            output.WriteByte(message.Type.ByteValue);
            output.WriteShort(message.DstPort);
            if (IPAddress.TryParse(message.DstAddr, out _))
            {
                output.WriteBytes(NetUtil.CreateByteArrayFromIpAddressString(message.DstAddr));
                output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, message.UserId,
                    Encoding.ASCII));
                output.WriteByte(0);
            }
            else
            {
                output.WriteBytes(Pv4DomainMarker);
                output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, message.UserId,
                    Encoding.ASCII));
                output.WriteByte(0);
                output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, message.DstAddr,
                    Encoding.ASCII));
                output.WriteByte(0);
            }
        }
    }
}