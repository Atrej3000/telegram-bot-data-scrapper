namespace DataParserService.Services
{
    public interface IUserAgentProviderService
    {
        Task<List<string>> GetUserAgentsAsync(HttpClient httpClient);
    }
}