namespace bot.SyncDataServices.Http
{
	public interface INgrokDataClient
	{
		Task<string> GetNgrokUrl();
	}
}