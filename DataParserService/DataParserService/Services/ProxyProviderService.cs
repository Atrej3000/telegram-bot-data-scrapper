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
            WebProxy proxy = new()
            {
                Address = new Uri($"{_configuration["Proxy:Protocol"]}://{_configuration["Proxy:Domain"]}:{_configuration["Proxy:Port"]}"),
                Credentials = new NetworkCredential($"{_configuration["Proxy:Username"]}", $"{_configuration["Proxy:Password"]}")
            };

            HttpClientHandler handler = new()
            {
                Proxy = proxy,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
            return new HttpClient(handler);
        } 
    }
}

