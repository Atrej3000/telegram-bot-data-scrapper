using bot.Models;
using Microsoft.EntityFrameworkCore;

namespace bot.Data.Subscriptions
{
	public class UOW : IUOW
	{
		private SubscriptionsContext db;
		private PostRepo postRepo;
		private SubscriptionRepo subscriptionRepo;
		private UserRepo userRepo;

		public UOW(IDbContextFactory<SubscriptionsContext> factory)
		{
			db = factory.CreateDbContext();
		}

		public IRepository<Post> Posts
		{
			get
			{
				if (postRepo == null)
					postRepo = new PostRepo(db);
				return postRepo;
			}
		}

		public IRepository<Subscription> Subscriptions
		{
			get
			{
				if (subscriptionRepo == null)
					subscriptionRepo = new SubscriptionRepo(db);
				return subscriptionRepo;
			}
		}

		public IRepository<User> Users
		{
			get
			{
				if (userRepo == null)
					userRepo = new UserRepo(db);
				return userRepo;
			}
		}

		public async Task Save()
		{
			await db.SaveChangesAsync();
		}
	}
}
