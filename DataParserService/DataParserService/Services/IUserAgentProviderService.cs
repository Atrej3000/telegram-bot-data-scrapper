namespace DataParserService.Services
{
    public interface IUserAgentProviderService
    {
        Task<List<string>> GetUserAgents(HttpClient httpClient);
    }
}