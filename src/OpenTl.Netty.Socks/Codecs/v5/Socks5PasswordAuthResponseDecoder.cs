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
 * Decodes a single {@link Socks5PasswordAuthResponse} from the inbound {@link ByteBuf}s.
 * On successful decode, this decoder will forward the received data to the next handler, so that
 * other handler can remove or replace this decoder later.  On failed decode, this decoder will
 * discard the received data, so that other handler closes the connection later.
 */
    public class Socks5PasswordAuthResponseDecoder : ReplayingDecoder<Socks5PasswordAuthResponseDecoder.EState>
    {
        public enum EState
        {
            Init,

            Success,

            Failure
        }

        public Socks5PasswordAuthResponseDecoder() : base(EState.Init)
        {
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (State)
                {
                    case EState.Init:
                        var version = input.ReadByte();
                        if (version != 1)
                            throw new DecoderException("unsupported subnegotiation version: " + version +
                                                       " (expected: 1)");

                        output.Add(new DefaultSocks5PasswordAuthResponse(
                            Socks5PasswordAuthStatus.ValueOf(input.ReadByte())));
                        Checkpoint(EState.Success);
                        break;
                    case EState.Success:
                        var readableBytes = ActualReadableBytes;
                        if (readableBytes > 0) output.Add(input.ReadRetainedSlice(readableBytes));

                        break;
                    case EState.Failure:
                        input.SkipBytes(ActualReadableBytes);
                        break;
                }
            }
            catch
                (Exception e)
            {
                Fail(output, e);
            }
        }

        private void Fail(List<object> output, Exception cause)
        {
            if (!(cause is DecoderException)) cause = new DecoderException(cause);

            Checkpoint(EState.Failure);

            ISocks5Message m = new DefaultSocks5PasswordAuthResponse(Socks5PasswordAuthStatus.Failure);
            m.SetDecoderResult(DecoderResult.Failure(cause));
            output.Add(m);
        }
    }
}