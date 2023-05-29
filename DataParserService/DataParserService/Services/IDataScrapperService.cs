using DataParserService.Models;

namespace DataParserService.Services
{
    public interface IDataScrapperService
    {
        Task ParseDataPage(HashSet<Post> posts, string category, int page, string userAgent);
    }
}