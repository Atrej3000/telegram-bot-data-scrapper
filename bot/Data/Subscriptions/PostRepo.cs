using bot.Models;
using Microsoft.EntityFrameworkCore;

namespace bot.Data.Subscriptions
{
	public class PostRepo : IRepository<Post>
	{
		private readonly SubscriptionsContext _context;

		public PostRepo(SubscriptionsContext context)
		{
			_context = context;
		}

		public void Create(Post item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			_context.Posts.Add(item);
		}

		public void Delete(long userId, string query)
		{
			throw new NotImplementedException();
		}

		public bool Exists(long userId, string query)
		{
			throw new NotImplementedException();
		}

		public Post Get(int id)
		{
			throw new NotImplementedException();
		}

		public IQueryable<Post> GetAll()
		{
			throw new NotImplementedException();
		}

		public IQueryable<Post> GetByUser(long userId)
		{
			return _context.Posts.Where(x => x.Subscription.Id == userId);
		}
	}
}
