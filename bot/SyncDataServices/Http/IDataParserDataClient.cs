namespace bot.SyncDataServices.Http
{
	public interface IDataParserDataClient
	{
		Task<string> ParseData(string query);
	}
}
