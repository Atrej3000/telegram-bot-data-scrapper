using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bot.Models;
using System.Diagnostics.Metrics;

public class SubscriptionsContext : DbContext
{
	public SubscriptionsContext(DbContextOptions<SubscriptionsContext> options)
		: base(options) { }

	public DbSet<bot.Models.User> Users { get; set; } = default!;
	public DbSet<bot.Models.Subscription> Subscriptions { get; set; } = default!;
	public DbSet<bot.Models.Post> Posts { get; set; } = default!;
}
