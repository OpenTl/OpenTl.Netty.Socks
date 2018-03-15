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

using System;
using System.Net;
using DotNetty.Transport.Channels;
using OpenTl.Netty.Socks.Codecs.Socksx.v4;

namespace OpenTl.Netty.Socks.Handlers
{
    public sealed class Socks4ProxyHandler : ProxyHandler
    {
        private static readonly string AuthUsername = "username";

        private string _decoderName;
        private string _encoderName;

        public Socks4ProxyHandler(EndPoint proxyAddress, string username = null) : base(proxyAddress)
        {
            if (username != null && string.IsNullOrWhiteSpace(username)) username = null;
            Username = username;
        }

        public override string Protocol { get; } = "socks4";
        public override string AuthScheme => Username != null ? AuthUsername : AuthNone;

        public string Username { get; }

        protected override void AddCodec(IChannelHandlerContext ctx)
        {
            var p = ctx.Channel.Pipeline;
            var name = ctx.Name;

            var decoder = new Socks4ClientDecoder();
            p.AddBefore(name, null, decoder);

            _decoderName = p.Context(decoder).Name;
            _encoderName = _decoderName + ".encoder";

            p.AddBefore(name, _encoderName, Socks4ClientEncoder.Instance);
        }

        protected override void RemoveEncoder(IChannelHandlerContext ctx)
        {
            var p = ctx.Channel.Pipeline;
            p.Remove(_encoderName);
        }

        protected override void RemoveDecoder(IChannelHandlerContext ctx)
        {
            var p = ctx.Channel.Pipeline;
            p.Remove(_decoderName);
        }

        protected override object NewInitialMessage(IChannelHandlerContext ctx)
        {
            string rhost;
            int rport;

            switch (DestinationAddress)
            {
                case DnsEndPoint dnsEndPoint:
                    rhost = dnsEndPoint.Host;
                    rport = dnsEndPoint.Port;
                    break;
                case IPEndPoint ipEndPoint:
                    rhost = ipEndPoint.Address.ToString();
                    rport = ipEndPoint.Port;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new DefaultSocks4CommandRequest(Socks4CommandType.Connect, rhost, rport, Username ?? "");
        }

        protected override bool HandleResponse(IChannelHandlerContext ctx, object response)
        {
            var res = (ISocks4CommandResponse) response;
            var status = res.Status;
            if (Equals(status, Socks4CommandStatus.Success)) return true;

            throw new ProxyConnectException(ExceptionMessage("status: " + status), new InvalidOperationException());
        }
    }
}