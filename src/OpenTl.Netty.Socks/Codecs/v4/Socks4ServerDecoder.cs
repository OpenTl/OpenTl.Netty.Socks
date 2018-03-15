/*
 * Copyright 2012 The Netty Project
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
using System.Net;
using System.Text;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.DecoderResults;

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * Decodes a single {@link Socks4CommandRequest} from the inbound {@link ByteBuf}s.
 * On successful decode, this decoder will forward the received data to the next handler, so that
 * other handler can remove this decoder later.  On failed decode, this decoder will discard the
 * received data, so that other handler closes the connection later.
 */
    public class Socks4ServerDecoder : ReplayingDecoder<Socks4ServerDecoder.Socks4ServerDecoderState>
    {
        public enum Socks4ServerDecoderState
        {
            Start,

            ReadUserid,

            ReadDomain,

            Success,

            Failure
        }

        private static readonly int MaxFieldLength = 255;

        private string _dstAddr;

        private int _dstPort;

        private Socks4CommandType _type;

        private string _userId;

        public Socks4ServerDecoder() : base(Socks4ServerDecoderState.Start)
        {
            SingleDecode = true;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            try
            {
                switch (State)
                {
                    case Socks4ServerDecoderState.Start:
                        var version = input.ReadByte();
                        if (version != (byte) SocksVersion.Socks4A)
                            throw new DecoderException("unsupported protocol version: " + version);

                        _type = Socks4CommandType.ValueOf(input.ReadByte());
                        _dstPort = input.ReadUnsignedShort();
                        _dstAddr = new IPAddress(input.ReadInt()).ToString();
                        Checkpoint(Socks4ServerDecoderState.ReadUserid);
                        break;
                    case Socks4ServerDecoderState.ReadUserid:
                        _userId = ReadString("userid", input);
                        Checkpoint(Socks4ServerDecoderState.ReadDomain);

                        break;
                    case Socks4ServerDecoderState.ReadDomain:

                        // Check for Socks4a protocol marker 0.0.0.x
                        if (_dstAddr != "0.0.0.0" && _dstAddr.StartsWith("0.0.0."))
                            _dstAddr = ReadString("dstAddr", input);

                        output.Add(new DefaultSocks4CommandRequest(_type, _dstAddr, _dstPort, _userId));
                        Checkpoint(Socks4ServerDecoderState.Success);
                        break;
                    case Socks4ServerDecoderState.Success:
                        var readableBytes = ActualReadableBytes;
                        if (readableBytes > 0) output.Add(input.ReadRetainedSlice(readableBytes));

                        break;
                    case Socks4ServerDecoderState.Failure:
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

            ISocks4CommandRequest m = new DefaultSocks4CommandRequest(
                _type ?? Socks4CommandType.Connect,
                _dstAddr ?? "",
                _dstPort != 0
                    ? _dstPort
                    : 65535,
                _userId ?? "");

            m.SetDecoderResult(DecoderResult.Failure(cause));
            output.Add(m);

            Checkpoint(Socks4ServerDecoderState.Failure);
        }

        /**
         * Reads a variable-length NUL-terminated string as defined input SOCKS4.
         */
        private static string ReadString(string fieldName, IByteBuffer input)
        {
            var length = input.BytesBefore(MaxFieldLength + 1, 0);
            if (length < 0)
                throw new DecoderException("field '" + fieldName + "' longer than " + MaxFieldLength + " chars");

            var value = input.ReadSlice(length).ToString(Encoding.ASCII);
            input.SkipBytes(1); // Skip the NUL.

            return value;
        }
    }
}