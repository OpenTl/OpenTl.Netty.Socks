/*
 * Copyright 2014 The Netty Project
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

using System;
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.DecoderResults;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * Decodes a single {@link Socks5CommandResponse} from the inbound {@link ByteBuf}s.
 * On successful decode, this decoder will forward the received data to the next handler, so that
 * other handler can remove or replace this decoder later.  On failed decode, this decoder will
 * discard the received data, so that other handler closes the connection later.
 */
    public class
        Socks5CommandResponseDecoder : ReplayingDecoder<Socks5CommandResponseDecoder.Socks5CommandResponseDecoderState>
    {
        public enum Socks5CommandResponseDecoderState
        {
            Init,

            Success,

            Failure
        }

        private readonly ISocks5AddressDecoder _addressDecoder;

        public Socks5CommandResponseDecoder() : this(DefaultSocks5AddressDecoder.Default)
        {
        }

        public Socks5CommandResponseDecoder(ISocks5AddressDecoder addressDecoder) : base(
            Socks5CommandResponseDecoderState.Init)
        {
            _addressDecoder = addressDecoder;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (State)
                {
                    case Socks5CommandResponseDecoderState.Init:
                        var version = input.ReadByte();
                        if (version != (byte) SocksVersion.Socks5)
                            throw new DecoderException(
                                "unsupported version: " + version + " (expected: " + SocksVersion.Socks5 + ')');

                        var status = Socks5CommandStatus.ValueOf(input.ReadByte());
                        input.SkipBytes(1); // Reserved
                        var addrType = Socks5AddressType.ValueOf(input.ReadByte());
                        var addr = _addressDecoder.DecodeAddress(addrType, input);
                        int port = input.ReadUnsignedShort();

                        output.Add(new DefaultSocks5CommandResponse(status, addrType, addr, port));
                        Checkpoint(Socks5CommandResponseDecoderState.Success);
                        break;
                    case Socks5CommandResponseDecoderState.Success:
                        var readableBytes = ActualReadableBytes;
                        if (readableBytes > 0) output.Add(input.ReadRetainedSlice(readableBytes));

                        break;
                    case Socks5CommandResponseDecoderState.Failure:
                        input.SkipBytes(ActualReadableBytes);
                        break;
                }
            }
            catch (Exception e)
            {
                Fail(output, e);
            }
        }

        private void Fail(List<object> output, Exception cause)
        {
            if (!(cause is DecoderException)) cause = new DecoderException(cause);

            Checkpoint(Socks5CommandResponseDecoderState.Failure);

            ISocks5Message m = new DefaultSocks5CommandResponse(
                Socks5CommandStatus.Failure,
                Socks5AddressType.Pv4,
                null,
                0);
            m.SetDecoderResult(DecoderResult.Failure(cause));
            output.Add(m);
        }
    }
}