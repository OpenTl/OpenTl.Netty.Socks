/*
 * Copyright 2013 The Netty Project
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

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * The status of {@link Socks5CommandResponse}.
 */
    public sealed class Socks5CommandStatus : IComparable<Socks5CommandStatus>
    {
        public static readonly Socks5CommandStatus Success = new Socks5CommandStatus(0x00, "SUCCESS");

        public static readonly Socks5CommandStatus Failure = new Socks5CommandStatus(0x01, "FAILURE");

        public static readonly Socks5CommandStatus Forbidden = new Socks5CommandStatus(0x02, "FORBIDDEN");

        public static readonly Socks5CommandStatus NetworkUnreachable =
            new Socks5CommandStatus(0x03, "NETWORK_UNREACHABLE");

        public static readonly Socks5CommandStatus HostUnreachable = new Socks5CommandStatus(0x04, "HOST_UNREACHABLE");

        public static readonly Socks5CommandStatus ConnectionRefused =
            new Socks5CommandStatus(0x05, "CONNECTION_REFUSED");

        public static readonly Socks5CommandStatus TtlExpired = new Socks5CommandStatus(0x06, "TTL_EXPIRED");

        public static readonly Socks5CommandStatus CommandUnsupported =
            new Socks5CommandStatus(0x07, "COMMAND_UNSUPPORTED");

        public static readonly Socks5CommandStatus AddressUnsupported =
            new Socks5CommandStatus(0x08, "ADDRESS_UNSUPPORTED");

        private readonly string _name;

        private string _text;

        public Socks5CommandStatus(int byteValue, string name = "UNKNOWN")
        {
            ByteValue = (byte) byteValue;
            _name = name;
        }

        public byte ByteValue { get; }

        public int CompareTo(Socks5CommandStatus other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (ReferenceEquals(null, other)) return 1;

            return ByteValue.CompareTo(other.ByteValue);
        }

        public override bool Equals(object obj)
        {
            return ByteValue == (obj as Socks5CommandStatus)?.ByteValue;
        }

        public override int GetHashCode()
        {
            return ByteValue;
        }

        public bool IsSuccess()
        {
            return ByteValue == 0;
        }

        public override string ToString()
        {
            var text = _text;
            if (text == null) _text = text = _name + '(' + (ByteValue & 0xFF) + ')';

            return text;
        }

        public static Socks5CommandStatus ValueOf(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return Success;
                case 0x01:
                    return Failure;
                case 0x02:
                    return Forbidden;
                case 0x03:
                    return NetworkUnreachable;
                case 0x04:
                    return HostUnreachable;
                case 0x05:
                    return ConnectionRefused;
                case 0x06:
                    return TtlExpired;
                case 0x07:
                    return CommandUnsupported;
                case 0x08:
                    return AddressUnsupported;
            }

            return new Socks5CommandStatus(b);
        }
    }
}