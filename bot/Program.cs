using bot.Controllers;
using bot.Data.Bot;
using bot.Data.Subscriptions;
using bot.Models;
using bot.SyncDataServices.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

// Setup Bot configuration
var botConfigurationSection = builder.Configuration.GetSection(BotConfiguration.Configuration);
builder.Services.Configure<BotConfiguration>(botConfigurationSection);
var botConfiguration = botConfigurationSection.Get<BotConfiguration>();

builder.Services.AddHttpClient("telegram_bot_client")
				.AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
				{
					BotConfiguration? botConfig = sp.GetConfiguration<BotConfiguration>();
					TelegramBotClientOptions options = new(botConfig.BotToken);
					return new TelegramBotClient(options, httpClient);
				});

builder.Services.AddDbContextFactory<SubscriptionsContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("SubscriptionsContext1")
	?? throw new InvalidOperationException("Connection string 'SubscriptionsContext' not found.")));

builder.Services.AddScoped<UpdateHandlers>();

builder.Services.AddHostedService<ConfigureWebhook>();

//builder.Services.AddSingleton<IRepository<Post>, PostRepo>();
//builder.Services.AddSingleton<IRepository<Subscription>, SubscriptionRepo>();
//builder.Services.AddSingleton<IRepository<User>, UserRepo>();

builder.Services.AddSingleton<IUOW, UOW>();

builder.Services.AddHostedService<BackgroundWorker>();

builder.Services.AddHttpClient<IDataParserDataClient, HttpDataParserDataClient>(client =>
{
	client.Timeout = Timeout.InfiniteTimeSpan; // set timeout to infinite
});

builder.Services.AddHttpClient<INgrokDataClient, HttpNgrokDataClient>();

builder.Services
	.AddControllers()
	.AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<SubscriptionsContext>();
	db.Database.Migrate();
}

app.MapBotWebhookRoute<BotController>(route: botConfiguration.Route);

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
