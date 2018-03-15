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

namespace OpenTl.Netty.Socks.Codecs.Socksx.v5
{
    /**
 * A SOCKS5 request detail message, as defined in
 * <a href="http://tools.ietf.org/html/rfc1928#section-4">the section 4, RFC1928</a>.
 */
    public interface ISocks5CommandRequest : ISocks5Message
    {
        /**
        * Returns the type of this request.
        */
        Socks5CommandType Type { get; }

        /**
        * Returns the type of the {@code DST.ADDR} field of this request.
        */
        Socks5AddressType DstAddrType { get; }

        /**
        * Returns the {@code DST.ADDR} field of this request.
        */
        string DstAddr { get; }

        /**
        * Returns the {@code DST.PORT} field of this request.
        */
        int DstPort { get; }
    }
}