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

using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * Encodes a client-side {@link Socks5Message} into a {@link ByteBuf}.
 */
    public class Socks5ClientEncoder : MessageToByteEncoder<ISocks5Message>
    {
        public static readonly Socks5ClientEncoder Default = new Socks5ClientEncoder();

        /**
         * Creates a new instance with the default {@link Socks5AddressEncoder}.
         */
        protected Socks5ClientEncoder() : this(DefaultSocks5AddressEncoder.Default)
        {
        }

        /**
         * Creates a new instance with the specified {@link Socks5AddressEncoder}.
         */
        public Socks5ClientEncoder(ISocks5AddressEncoder addressEncoder)
        {
            AddressEncoder = addressEncoder;
        }

        public override bool IsSharable { get; } = true;

        /**
         * Returns the {@link Socks5AddressEncoder} of this encoder.
         */
        protected ISocks5AddressEncoder AddressEncoder { get; }

        protected override void Encode(IChannelHandlerContext context, ISocks5Message message, IByteBuffer output)
        {
            if (message is ISocks5InitialRequest)
                EncodeAuthMethodRequest((ISocks5InitialRequest) message, output);
            else if (message is ISocks5PasswordAuthRequest)
                EncodePasswordAuthRequest((ISocks5PasswordAuthRequest) message, output);
            else if (message is ISocks5CommandRequest)
                EncodeCommandRequest((ISocks5CommandRequest) message, output);
            else
                throw new EncoderException("unsupported message type: " + StringUtil.SimpleClassName(message));
        }

        private static void EncodeAuthMethodRequest(ISocks5InitialRequest message, IByteBuffer output)
        {
            output.WriteByte((int) message.Version());

            var authMethods = message.AuthMethods;
            output.WriteByte(authMethods.Count);

            foreach (var authMethod in authMethods) output.WriteByte(authMethod.ByteValue);
        }

        private void EncodeCommandRequest(ISocks5CommandRequest message, IByteBuffer output)
        {
            output.WriteByte((int) message.Version());
            output.WriteByte(message.Type.ByteValue);
            output.WriteByte(0x00);

            var dstAddrType = message.DstAddrType;
            output.WriteByte(dstAddrType.ByteValue);
            AddressEncoder.EncodeAddress(dstAddrType, message.DstAddr, output);
            output.WriteShort(message.DstPort);
        }

        private static void EncodePasswordAuthRequest(ISocks5PasswordAuthRequest message, IByteBuffer output)
        {
            output.WriteByte(0x01);

            output.WriteByte(message.Username.Length);
            output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, message.Username,
                Encoding.ASCII));

            output.WriteByte(message.Password.Length);
            output.WriteBytes(ByteBufferUtil.EncodeString(ByteBufferUtil.DefaultAllocator, message.Password,
                Encoding.ASCII));
        }
    }
}