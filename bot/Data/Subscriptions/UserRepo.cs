using bot.Models;
using Microsoft.EntityFrameworkCore;

namespace bot.Data.Subscriptions
{
	public class UserRepo : IRepository<User>
	{
		private readonly SubscriptionsContext _context;

		public UserRepo(SubscriptionsContext context)
		{
			_context = context;
		}

		public void Create(User item)
		{
			//_context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Users ON;");
			if (item == null)
				throw new ArgumentNullException(nameof(item));
			if (!Exists(item.Id))
				_context.Users.Add(item);
		}

		public void Delete(long userId, string query)
		{
			throw new NotImplementedException();
		}

		public bool Exists(long userId, string query = null)
		{
			return _context.Users.Any(x => x.Id == userId);
		}

		public User Get(int id)
		{
			return _context.Users.Where(x => x.Id == id).FirstOrDefault();
		}

		public IQueryable<User> GetAll()
		{
			throw new NotImplementedException();
		}

		public IQueryable<User> GetByUser(long userId)
		{
			throw new NotImplementedException();
		}
	}
}