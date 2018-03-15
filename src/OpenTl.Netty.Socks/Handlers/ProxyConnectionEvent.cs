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

using System.Net;
using System.Text;
using DotNetty.Common.Utilities;

namespace OpenTl.Netty.Socks.Handlers
{
    public sealed class ProxyConnectionEvent
    {
        private readonly EndPoint _destinationAddress;
        private readonly EndPoint _proxyAddress;

        private string _strVal;

        /**
     * Creates a new event that indicates a successful connection attempt to the destination address.
     */
        public ProxyConnectionEvent(
            string protocol, string authScheme, EndPoint proxyAddress, EndPoint destinationAddress)
        {
            Protocol = protocol;
            AuthScheme = authScheme;
            _proxyAddress = proxyAddress;
            _destinationAddress = destinationAddress;
        }

        /**
     * Returns the name of the proxy protocol in use.
     */
        public string Protocol { get; }

        /**
     * Returns the name of the authentication scheme in use.
     */
        public string AuthScheme { get; }

        /**
     * Returns the address of the proxy server.
     */
        public T ProxyAddress<T>() where T : EndPoint
        {
            return (T) _proxyAddress;
        }

        /**
     * Returns the address of the destination.
     */

        public T DestinationAddress<T>() where T : EndPoint
        {
            return (T) _destinationAddress;
        }

        public override string ToString()
        {
            if (_strVal != null) return _strVal;

            var buf = new StringBuilder(128)
                .Append(StringUtil.SimpleClassName(this))
                .Append('(')
                .Append(Protocol)
                .Append(", ")
                .Append(AuthScheme)
                .Append(", ")
                .Append(_proxyAddress)
                .Append(" => ")
                .Append(_destinationAddress)
                .Append(')');

            return _strVal = buf.ToString();
        }
    }
}