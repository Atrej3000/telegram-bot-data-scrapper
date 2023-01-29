namespace DataParserService.Services
{
    public interface IProxyProviderService
    {
        HttpClient CreateClient();
    }
}