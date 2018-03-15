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
using NullGuard;
using OpenTl.Netty.Socks.Codecs.Socksx.v5;

namespace OpenTl.Netty.Socks.Handlers
{
    public sealed class Socks5ProxyHandler : ProxyHandler
    {
        private static readonly string AuthPassword = "password";

        private static readonly ISocks5InitialRequest InitRequestNoAuth =
            new DefaultSocks5InitialRequest(Socks5AuthMethod.NoAuth);

        private static readonly ISocks5InitialRequest InitRequestPassword =
            new DefaultSocks5InitialRequest(Socks5AuthMethod.NoAuth, Socks5AuthMethod.Password);

        private string _decoderName;

        private string _encoderName;

        public Socks5ProxyHandler(EndPoint proxyAddress, string username = null, string password = null) : base(
            proxyAddress)
        {
            if (username != null && string.IsNullOrWhiteSpace(username)) username = null;

            if (password != null && string.IsNullOrWhiteSpace(password)) password = null;

            Username = username;
            Password = password;
        }

        public override string Protocol { get; } = "socks5";

        public override string AuthScheme => SocksAuthMethod() == Socks5AuthMethod.Password
            ? AuthPassword
            : AuthNone;

        [AllowNull] public string Username { get; }

        [AllowNull] public string Password { get; }

        protected override void AddCodec(IChannelHandlerContext ctx)
        {
            var p = ctx.Channel.Pipeline;
            var name = ctx.Name;

            var decoder = new Socks5InitialResponseDecoder();
            p.AddBefore(name, null, decoder);

            _decoderName = p.Context(decoder).Name;
            _encoderName = _decoderName + ".encoder";

            p.AddBefore(name, _encoderName, Socks5ClientEncoder.Default);
        }

        protected override bool HandleResponse(IChannelHandlerContext ctx, object response)
        {
            if (response is ISocks5InitialResponse)
            {
                var res = (ISocks5InitialResponse) response;
                var authMethod = SocksAuthMethod();

                if (res.AuthMethod != Socks5AuthMethod.NoAuth && res.AuthMethod != authMethod)
                    throw new ProxyConnectException(ExceptionMessage("unexpected authMethod: " + res.AuthMethod),
                        new InvalidOperationException());

                if (authMethod == Socks5AuthMethod.NoAuth)
                {
                    SendConnectCommand(ctx);
                }
                else if (authMethod == Socks5AuthMethod.Password)
                {
                    // In case of password authentication, send an authentication request.
                    ctx.Channel.Pipeline.Replace(_decoderName, _decoderName, new Socks5PasswordAuthResponseDecoder());
                    SendToProxyServer(
                        new DefaultSocks5PasswordAuthRequest(
                            Username ?? "",
                            Password ?? ""));
                }
                else
                {
                    // Should never reach here.
                    throw new Exception();
                }

                return false;
            }

            if (response is ISocks5PasswordAuthResponse)
            {
                // Received an authentication response from the server.
                var res = (ISocks5PasswordAuthResponse) response;
                if (res.Status != Socks5PasswordAuthStatus.Success)
                    throw new ProxyConnectException(ExceptionMessage("authStatus: " + res.Status),
                        new InvalidOperationException());

                SendConnectCommand(ctx);
                return false;
            }
            else
            {
                // This should be the last message from the server.
                var res = (ISocks5CommandResponse) response;
                if (res.Status != Socks5CommandStatus.Success)
                    throw new ProxyConnectException(ExceptionMessage("status: " + res.Status),
                        new InvalidOperationException());
            }

            return true;
        }

        protected override object NewInitialMessage(IChannelHandlerContext ctx)
        {
            return SocksAuthMethod() == Socks5AuthMethod.Password
                ? InitRequestPassword
                : InitRequestNoAuth;
        }

        protected override void RemoveDecoder(IChannelHandlerContext ctx)
        {
            var p = ctx.Channel.Pipeline;
            if (p.Context(_decoderName) != null) p.Remove(_decoderName);
        }

        protected override void RemoveEncoder(IChannelHandlerContext ctx)
        {
            ctx.Channel.Pipeline.Remove(_encoderName);
        }

        private void SendConnectCommand(IChannelHandlerContext ctx)
        {
            string rhost;
            int rport;
            Socks5AddressType addrType;

            switch (DestinationAddress)
            {
                case DnsEndPoint dnsEndPoint:
                    rhost = dnsEndPoint.Host;
                    rport = dnsEndPoint.Port;
                    addrType = Socks5AddressType.Domain;
                    break;
                case IPEndPoint ipEndPoint:
                    rhost = ipEndPoint.Address.ToString();
                    rport = ipEndPoint.Port;

                    addrType = ipEndPoint.Address.IsIPv6Teredo
                        ? Socks5AddressType.Pv6
                        : Socks5AddressType.Pv4;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            ctx.Channel.Pipeline.Replace(_decoderName, _decoderName, new Socks5CommandResponseDecoder());
            SendToProxyServer(new DefaultSocks5CommandRequest(Socks5CommandType.Connect, addrType, rhost, rport));
        }

        private Socks5AuthMethod SocksAuthMethod()
        {
            Socks5AuthMethod authMethod;
            if (Username == null && Password == null)
                authMethod = Socks5AuthMethod.NoAuth;
            else
                authMethod = Socks5AuthMethod.Password;

            return authMethod;
        }
    }
}