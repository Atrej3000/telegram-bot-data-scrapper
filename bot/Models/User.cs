using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bot.Models
{
	public class User
	{
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long Id { get; set; }
		public List<Subscription> Subscriptions { get; set; } = new();
	}
}
