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
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * Encodes a server-side {@link Socks5Message} into a {@link IByteBuffer}.
 */
    public sealed class Socks5ServerEncoder : MessageToByteEncoder<ISocks5Message>
    {
        public static readonly Socks5ServerEncoder Default = new Socks5ServerEncoder();

        /**
         * Creates a new instance with the default {@link Socks5AddressEncoder}.
         */
        private Socks5ServerEncoder() : this(DefaultSocks5AddressEncoder.Default)
        {
            ;
        }

        /**
         * Creates a new instance with the specified {@link Socks5AddressEncoder}.
         */
        public Socks5ServerEncoder(ISocks5AddressEncoder addressEncoder)
        {
            AddressEncoder = addressEncoder;
        }

        /**
         * Returns the {@link Socks5AddressEncoder} of this encoder.
         */
        private ISocks5AddressEncoder AddressEncoder { get; }

        protected override void Encode(IChannelHandlerContext context, ISocks5Message message, IByteBuffer output)
        {
            switch (message)
            {
                case ISocks5InitialResponse initialResponse:
                    EncodeAuthMethodResponse(initialResponse, output);
                    break;
                case ISocks5PasswordAuthResponse authResponse:
                    EncodePasswordAuthResponse(authResponse, output);
                    break;
                case ISocks5CommandResponse commandResponse:
                    EncodeCommandResponse(commandResponse, output);
                    break;
                default:
                    throw new EncoderException("unsupported message type: " + StringUtil.SimpleClassName(message));
            }
        }

        private static void EncodeAuthMethodResponse(ISocks5InitialResponse message, IByteBuffer output)
        {
            output.WriteByte((int) message.Version());
            output.WriteByte(message.AuthMethod.ByteValue);
        }

        private void EncodeCommandResponse(ISocks5CommandResponse message, IByteBuffer output)
        {
            output.WriteByte((int) message.Version());
            output.WriteByte(message.Status.ByteValue);
            output.WriteByte(0x00);

            var bndAddrType = message.BndAddrType;
            output.WriteByte(bndAddrType.ByteValue);
            AddressEncoder.EncodeAddress(bndAddrType, message.BndAddr, output);

            output.WriteShort(message.BndPort);
        }

        private static void EncodePasswordAuthResponse(ISocks5PasswordAuthResponse message, IByteBuffer output)
        {
            output.WriteByte(0x01);
            output.WriteByte(message.Status.ByteValue);
        }
    }
}