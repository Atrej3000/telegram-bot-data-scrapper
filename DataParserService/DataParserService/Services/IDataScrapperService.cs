using DataParserService.Models;

namespace DataParserService.Services
{
    public interface IDataScrapperService
    {
        void ParseDataPage(HashSet<Post> posts, string category, int page, string userAgent);
    }
}