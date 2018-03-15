using System.Net;

namespace OpenTl.Netty.Socks.Utils
{
    public static class NetUtil
    {
        /**
     * Creates an byte[] based on an ipAddressString. No error handling is performed here.
     */
        public static byte[] CreateByteArrayFromIpAddressString(string ipAddressString)
        {
            return IPAddress.Parse(ipAddressString).GetAddressBytes();
        }
    }
}