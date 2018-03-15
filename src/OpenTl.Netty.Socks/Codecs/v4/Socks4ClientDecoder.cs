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
using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.DecoderResults;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * Decodes a single {@link Socks4CommandResponse} from the inbound {@link ByteBuf}s.
 * On successful decode, this decoder will forward the received data to the next handler, so that
 * other handler can remove this decoder later.  On failed decode, this decoder will discard the
 * received data, so that other handler closes the connection later.
 */
    public class Socks4ClientDecoder : ReplayingDecoder<Socks4ClientDecoder.Socks4ClientDecoderState>
    {
        public enum Socks4ClientDecoderState
        {
            Start,

            Success,

            Failure
        }

        public Socks4ClientDecoder() : base(Socks4ClientDecoderState.Start)
        {
            SingleDecode = true;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (State)
                {
                    case Socks4ClientDecoderState.Start:
                        int version = input.ReadByte();
                        if (version != 0)
                            throw new DecoderException("unsupported reply version: " + version + " (expected: 0)");

                        var status = Socks4CommandStatus.ValueOf(input.ReadByte());
                        int dstPort = input.ReadUnsignedShort();
                        var dstAddr = input.ReadInt().ToString();

                        output.Add(new DefaultSocks4CommandResponse(status, dstAddr, dstPort));
                        Checkpoint(Socks4ClientDecoderState.Success);
                        break;
                    case Socks4ClientDecoderState.Success:
                    {
                        if (ActualReadableBytes > 0) output.Add(input.ReadRetainedSlice(ActualReadableBytes));

                        break;
                    }
                    case Socks4ClientDecoderState.Failure:
                    {
                        input.SkipBytes(ActualReadableBytes);
                        break;
                    }
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

            var m = new DefaultSocks4CommandResponse(Socks4CommandStatus.RejectedOrFailed);
            m.SetDecoderResult(DecoderResult.Failure(cause));
            output.Add(m);

            Checkpoint(Socks4ClientDecoderState.Failure);
        }
    }
}