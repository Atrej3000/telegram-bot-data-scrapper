using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
	public class Subscription
	{
		[Key]
		[Required]
		public int Id { get; set; }
		[Required]
		public DateTime date { get; set; }
		[Required]
		public string query { get; set; }
		//[Required]
		//public long UserId { get; set; }
		public User User { get; set; }
		public List<Post> Posts { get; set; } = new();

	}
}
