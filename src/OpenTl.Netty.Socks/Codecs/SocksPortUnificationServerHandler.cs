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

using System.Collections.Generic;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using log4net;
using OpenTl.Netty.Socks.Codecs.Socksx.v4;
using OpenTl.Netty.Socks.Codecs.Socksx.v5;

namespace OpenTl.Netty.Socks.Codecs.Socksx
{
    /**
 * Detects the version of the current SOCKS connection and initializes the pipeline with
 * {@link Socks4ServerDecoder} or {@link Socks5InitialRequestDecoder}.
 */
    public class SocksPortUnificationServerHandler : ByteToMessageDecoder
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SocksPortUnificationServerHandler));

        private readonly Socks5ServerEncoder _socks5Encoder;

        /**
         * Creates a new instance with the default configuration.
         */
        public SocksPortUnificationServerHandler() : this(Socks5ServerEncoder.Default)
        {
        }

        /**
         * Creates a new instance with the specified {@link Socks5ServerEncoder}.
         * This constructor is useful when a user wants to use an alternative {@link Socks5AddressEncoder}.
         */
        public SocksPortUnificationServerHandler(Socks5ServerEncoder socks5Encoder)
        {
            _socks5Encoder = socks5Encoder;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer input, List<object> output)
        {
            var readerIndex = input.ReaderIndex;
            if (input.WriterIndex == readerIndex) return;

            var pipeline = context.Channel.Pipeline;
            var versionVal = input.GetByte(readerIndex);
            var version = SocksVersionExtensions.ValueOf(versionVal);

            switch (version)
            {
                case SocksVersion.Socks4A:
                    LogKnownVersion(context, version);
                    pipeline.AddAfter(context.Name, null, Socks4ServerEncoder.Instance);
                    pipeline.AddAfter(context.Name, null, new Socks4ServerDecoder());
                    break;
                case SocksVersion.Socks5:
                    LogKnownVersion(context, version);
                    pipeline.AddAfter(context.Name, null, _socks5Encoder);
                    pipeline.AddAfter(context.Name, null, new Socks5InitialRequestDecoder());
                    break;
                default:
                    LogUnknownVersion(context, versionVal);
                    input.SkipBytes(input.ReadableBytes);
                    context.CloseAsync();
                    return;
            }

            pipeline.Remove(this);
        }

        private static void LogKnownVersion(IChannelHandlerContext context, SocksVersion version)
        {
            Log.Debug($"{context.Channel} Protocol version: ({version})");
        }

        private static void LogUnknownVersion(IChannelHandlerContext context, byte versionVal)
        {
            Log.Debug($"{context.Channel} Unknown protocol version: {versionVal & 0xFF}");
        }
    }
}