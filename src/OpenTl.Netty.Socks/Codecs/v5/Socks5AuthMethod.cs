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
 * The authentication method of SOCKS5.
 */
    public sealed class Socks5AuthMethod : IComparable<Socks5AuthMethod>
    {
        public static readonly Socks5AuthMethod NoAuth = new Socks5AuthMethod(0x00, "NO_AUTH");

        public static readonly Socks5AuthMethod Gssapi = new Socks5AuthMethod(0x01, "GSSAPI");

        public static readonly Socks5AuthMethod Password = new Socks5AuthMethod(0x02, "PASSWORD");

        /**
         * Indicates that the server does not accept any authentication methods the client proposed.
         */
        public static readonly Socks5AuthMethod Unaccepted = new Socks5AuthMethod(0xff, "UNACCEPTED");

        private readonly string _name;

        private string _text;

        public Socks5AuthMethod(int byteValue, string name = "UNKNOWN")
        {
            ByteValue = (byte) byteValue;
            _name = name;
        }

        public byte ByteValue { get; }

        public int CompareTo(Socks5AuthMethod other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (ReferenceEquals(null, other)) return 1;

            return ByteValue.CompareTo(other.ByteValue);
        }

        public override bool Equals(object obj)
        {
            return ByteValue == (obj as Socks5AuthMethod)?.ByteValue;
        }

        public override int GetHashCode()
        {
            return ByteValue;
        }

        public override string ToString()
        {
            var text = _text;
            if (text == null) _text = text = _name + '(' + (ByteValue & 0xFF) + ')';

            return text;
        }

        public static Socks5AuthMethod ValueOf(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return NoAuth;
                case 0x01:
                    return Gssapi;
                case 0x02:
                    return Password;
                case 0xFF:
                    return Unaccepted;
            }

            return new Socks5AuthMethod(b);
        }
    }
}