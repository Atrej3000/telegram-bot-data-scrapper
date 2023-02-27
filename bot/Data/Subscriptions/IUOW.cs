using bot.Models;

namespace bot.Data.Subscriptions
{
	public interface IUOW
	{
		IRepository<Post> Posts { get; }
		IRepository<Subscription> Subscriptions { get; }
		IRepository<User> Users { get; }
		Task Save();
	}
}
