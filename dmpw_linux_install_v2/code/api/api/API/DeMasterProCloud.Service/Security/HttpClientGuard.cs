using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;

namespace DeMasterProCloud.Service.Security
{
    public static class HttpClientGuard
    {
        private static readonly string[] AllowHosts = new[] { "api.example.com", "service.internal" };

        public static void EnsureSafeUri(Uri uri)
        {
            if (uri is null) throw new ArgumentNullException(nameof(uri));
            if (!AllowHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Outbound host not allowed.");

            foreach (var ip in Dns.GetHostAddresses(uri.Host))
            {
                if (IPAddress.IsLoopback(ip)) throw new InvalidOperationException("Loopback address not allowed.");
                if (ip.ToString().StartsWith("169.254.") || ip.IsIPv6LinkLocal || ip.IsIPv6Multicast)
                    throw new InvalidOperationException("Link-local/multicast not allowed.");
            }
        }

        public static HttpRequestMessage CreateSafeGet(string url)
        {
            var uri = new Uri(url);
            EnsureSafeUri(uri);
            return new HttpRequestMessage(HttpMethod.Get, uri);
        }
    }
}
