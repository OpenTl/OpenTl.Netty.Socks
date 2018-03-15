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

namespace OpenTl.Netty.Socks.Codecs.Socksx.v4
{
    /**
 * The status of {@link Socks4CommandResponse}.
 */
    public sealed class Socks4CommandStatus : IComparable<Socks4CommandStatus>
    {
        public static readonly Socks4CommandStatus Success = new Socks4CommandStatus(0x5a, "SUCCESS");

        public static readonly Socks4CommandStatus RejectedOrFailed =
            new Socks4CommandStatus(0x5b, "REJECTED_OR_FAILED");

        public static readonly Socks4CommandStatus IdentdUnreachable =
            new Socks4CommandStatus(0x5c, "IDENTD_UNREACHABLE");

        public static readonly Socks4CommandStatus IdentdAuthFailure =
            new Socks4CommandStatus(0x5d, "IDENTD_AUTH_FAILURE");

        private readonly string _name;

        private string _text;

        public Socks4CommandStatus(int byteValue, string name = "UNKNOWN")
        {
            ByteValue = (byte) byteValue;
            _name = name;
        }

        public byte ByteValue { get; }

        public int CompareTo(Socks4CommandStatus o)
        {
            return ByteValue - o.ByteValue;
        }

        public override bool Equals(object obj)
        {
            return ByteValue == (obj as Socks4CommandStatus)?.ByteValue;
        }

        public override int GetHashCode()
        {
            return ByteValue;
        }

        public bool IsSuccess()
        {
            return ByteValue == 0x5a;
        }

        public override string ToString()
        {
            var text = _text;
            if (text == null) _text = text = _name + '(' + (ByteValue & 0xFF) + ')';

            return text;
        }

        public static Socks4CommandStatus ValueOf(byte b)
        {
            switch (b)
            {
                case 0x5a:
                    return Success;
                case 0x5b:
                    return RejectedOrFailed;
                case 0x5c:
                    return IdentdUnreachable;
                case 0x5d:
                    return IdentdAuthFailure;
            }

            return new Socks4CommandStatus(b);
        }
    }
}