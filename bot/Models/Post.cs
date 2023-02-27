using System.ComponentModel.DataAnnotations;

namespace bot.Models
{
	public class Post
	{
		[Key]
		[Required]
		public int Id { get; set; }
		[Required]
		public string? Title { get; set; }
		[Required]
		public string? Uri { get; set; }
		[Required]
		public string? Price { get; set; }
		[Required]
		public DateTime Date { get; set; }
		//[Required]
		//public string SubscriptionId { get; set; }
		public Subscription Subscription { get; set; }

	}
}
