using bot.Models;
using Microsoft.EntityFrameworkCore;

namespace bot.Data.Subscriptions
{
	public interface IRepository<T> where T : class
	{
		IQueryable<T> GetAll();
		T Get(int id);
		IQueryable<T> GetByUser(long userId);
		void Create(T item);
		void Delete(long userId, string query);
		bool Exists(long userId, string query);
		//public bool Save();
	}
}
