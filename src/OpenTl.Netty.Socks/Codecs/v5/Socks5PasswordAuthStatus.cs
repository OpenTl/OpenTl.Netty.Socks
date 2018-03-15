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
 * The status of {@link Socks5PasswordAuthResponse}.
 */
    public sealed class Socks5PasswordAuthStatus : IComparable<Socks5PasswordAuthStatus>
    {
        public static readonly Socks5PasswordAuthStatus Success = new Socks5PasswordAuthStatus(0x00, "SUCCESS");

        public static readonly Socks5PasswordAuthStatus Failure = new Socks5PasswordAuthStatus(0xFF, "FAILURE");

        private readonly string _name;

        private string _text;

        public Socks5PasswordAuthStatus(int byteValue, string name = "UNKNOWN")
        {
            ByteValue = (byte) byteValue;
            _name = name;
        }

        public byte ByteValue { get; }

        public int CompareTo(Socks5PasswordAuthStatus other)
        {
            if (ReferenceEquals(this, other)) return 0;

            if (ReferenceEquals(null, other)) return 1;

            return ByteValue.CompareTo(other.ByteValue);
        }

        public override bool Equals(object obj)
        {
            return ByteValue == (obj as Socks5PasswordAuthStatus)?.ByteValue;
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

        public static Socks5PasswordAuthStatus ValueOf(byte b)
        {
            switch (b)
            {
                case 0x00:
                    return Success;
                case 0xFF:
                    return Failure;
            }

            return new Socks5PasswordAuthStatus(b);
        }
    }
}