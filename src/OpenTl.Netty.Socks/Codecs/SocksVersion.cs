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

namespace OpenTl.Netty.Socks.Codecs.Socksx
{
    /**
    * The version of SOCKS protocol.
    */
    public enum SocksVersion : byte
    {
        /**
         * SOCKS protocol version 4a (or 4)
         */
        Socks4A = (byte) 0x04,

        /**
         * SOCKS protocol version 5
         */
        Socks5 = (byte) 0x05,

        /**
         * Unknown protocol version
         */
        Unknown = (byte) 0xff
    }

    public static class SocksVersionExtensions
    {
        public static SocksVersion ValueOf(byte b)
        {
            try
            {
                return (SocksVersion) b;
            }
            catch (Exception e)
            {
                return SocksVersion.Unknown;
            }
        }
    }
}