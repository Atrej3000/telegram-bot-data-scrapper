using bot.Models;
using Microsoft.EntityFrameworkCore;

namespace bot.Data.Subscriptions
{
	public class SubscriptionRepo : IRepository<Subscription>
	{
		private readonly SubscriptionsContext _context;

		public SubscriptionRepo(SubscriptionsContext context)
		{
			_context = context;
		}

		public void Create(Subscription item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			_context.Subscriptions.Add(item);
		}

		public void Delete(long userId, string query)
		{
			var subscription = _context.Subscriptions.Include(x => x.User).Where(x => x.User.Id == userId && x.query == query);
			_context.Subscriptions.RemoveRange(subscription);
		}

		public bool Exists(long userId, string query)
		{
			return (_context.Subscriptions?.Include(x => x.User).Any(x => x.User.Id == userId && x.query == query)).GetValueOrDefault();
		}

		public Subscription Get(int id)
		{
			return _context.Subscriptions.Where(x => x.Id == id).FirstOrDefault();
		}

		public IQueryable<Subscription> GetAll()
		{
			return _context.Subscriptions.Include(x => x.User).OrderByDescending(x => x.date);
		}

		public IQueryable<Subscription> GetByUser(long userId)
		{
			return _context.Subscriptions.Include(x => x.User).Where(x => x.User.Id == userId);
		}
	}
}
