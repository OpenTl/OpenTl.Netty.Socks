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

using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.Utils;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    public sealed class Socks4ServerEncoder : MessageToByteEncoder<ISocks4CommandResponse>
    {
        public static readonly Socks4ServerEncoder Instance = new Socks4ServerEncoder();

        private static readonly byte[] Pv4HostnameZeroed = {0x00, 0x00, 0x00, 0x00};

        private Socks4ServerEncoder()
        {
        }

        public override bool IsSharable { get; } = true;

        protected override void Encode(IChannelHandlerContext context, ISocks4CommandResponse message,
            IByteBuffer output)
        {
            output.WriteByte(0);
            output.WriteByte(message.Status.ByteValue);
            output.WriteShort(message.DstPort);
            output.WriteBytes(
                message.DstAddr == null
                    ? Pv4HostnameZeroed
                    : NetUtil.CreateByteArrayFromIpAddressString(message.DstAddr));
        }
    }
}