using Microsoft.Extensions.Options;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using bot.SyncDataServices.Http;
using NgrokApi;

namespace bot.Data.Bot
{
	public class ConfigureWebhook : IHostedService
	{
		private readonly ILogger<ConfigureWebhook> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly BotConfiguration _botConfig;
		private readonly INgrokDataClient _ngrokDataClient;

		public ConfigureWebhook(
			ILogger<ConfigureWebhook> logger,
			IServiceProvider serviceProvider,
			IOptions<BotConfiguration> botOptions,
			INgrokDataClient ngrokDataClient)
		{
			_logger = logger;
			_serviceProvider = serviceProvider;
			_botConfig = botOptions.Value;
			_ngrokDataClient = ngrokDataClient;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

			string result = "";

			//try
			//{
			//	result = await _ngrokDataClient.GetNgrokUrl();
			//}
			//catch (Exception ex)
			//{
			//	_logger.LogWarning($"--> Could not get ngrok url: {ex.Message}");
			//	return;
			//}

			var ngrok = new Ngrok("2MIsQdYyx800KYBU68i0TZtnUbP_33tBsmNMzxd3jaq3qBZrh");

			await foreach (var t in ngrok.Endpoints.List())
			{
				result = t.PublicUrl;
				_logger.LogWarning(t.PublicUrl);
			}

			var webhookAddress = $"{result}/{_botConfig.Route}";
			_logger.LogInformation("Setting webhook: {WebhookAddress}", webhookAddress);
			await botClient.SetWebhookAsync(
				url: webhookAddress,
				allowedUpdates: Array.Empty<UpdateType>(),
				cancellationToken: cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			using var scope = _serviceProvider.CreateScope();
			var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

			// Remove webhook on app shutdown
			_logger.LogInformation("Removing webhook");
			await botClient.DeleteWebhookAsync(cancellationToken: cancellationToken);
		}
	}
}