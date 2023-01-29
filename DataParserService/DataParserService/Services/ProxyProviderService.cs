using System;
using System.Net;
using System.Net.Http;
using System.Reflection.Metadata;

namespace DataParserService.Services
{
    public class ProxyProviderService : IProxyProviderService
    {
        private readonly IConfiguration _configuration;
        public ProxyProviderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HttpClient CreateClient()
        {
            var proxy = new WebProxy
            {
                Address = new Uri($"{_configuration["Proxy:Protocol"]}://{_configuration["Proxy:Domain"]}:{_configuration["Proxy:Port"]}"),
                Credentials = new NetworkCredential($"{_configuration["Proxy:Username"]}", $"{_configuration["Proxy:Password"]}")
            };

            var handler = new HttpClientHandler
            {
                Proxy = proxy,
            };

            handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            return new HttpClient(handler);
        } 
    }
}

