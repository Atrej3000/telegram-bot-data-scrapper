using bot.Data.Subscriptions;
using bot.Models;
using bot.SyncDataServices.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public class BackgroundWorker : IHostedService, IDisposable
{
	private readonly ILogger<BackgroundWorker> _logger;
	private readonly IDataParserDataClient _dataParserDataClient;
	private PeriodicTimer _timer;
	private readonly ITelegramBotClient _botClient;
	private readonly IUOW _UOW;
	private Task _timerTask;
	private readonly CancellationTokenSource _cts = new CancellationTokenSource();

	public BackgroundWorker(ILogger<BackgroundWorker> logger, IDataParserDataClient dataParserDataClient, ITelegramBotClient botClient, IUOW UOW)
	{
		_logger = logger;
		_dataParserDataClient = dataParserDataClient;
		_botClient = botClient;
		_UOW = UOW;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogWarning("Background worker starting.");

		_timer = new PeriodicTimer(TimeSpan.FromSeconds(100));

		_timerTask = Task.Run(() => DoWorkAsync(_cts.Token), cancellationToken);

		return Task.CompletedTask;
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogWarning("Background worker stopping.");

		_cts.Cancel();

		// Wait for the background task to complete.
		await Task.WhenAny(_timerTask, Task.Delay(Timeout.Infinite, cancellationToken));

		// Stop the periodic timer
		Dispose();

		_cts.Dispose();
	}

	public void Dispose()
	{
		_timer?.Dispose();
	}

	private async Task DoWorkAsync(CancellationToken cancellationToken)
	{
		try
		{
			_logger.LogWarning("Running background task.");

			string? result;

			while (await _timer.WaitForNextTickAsync())
			{
				foreach (var s in _UOW.Subscriptions.GetAll().ToList())
				{
					result = "";

					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted before ParseData");
						continue;
					}

					// Send Sync Message
					try
					{
						result = await _dataParserDataClient.ParseData(s.query);
					}
					catch (Exception ex)
					{
						_logger.LogWarning($"--> Could not send synchronously: {ex.Message}");
						continue;
					}

					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted before DeserializeObject");
						continue;
					}

					List<Post> posts;
					posts = JsonConvert.DeserializeObject<List<Post>>(result);

					if (posts is null)
					{
						_logger.LogWarning($"NULL for {s.query}");
						continue;
					}

					if (posts.Count == 0)
					{
						_logger.LogWarning($"No data found for {s.query}");
						continue;
					}

					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted before DB.Create");
						continue;
					}

					foreach (Post post in posts)
					{
						if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
						{
							_logger.LogWarning($"Subscription {s.query} was deleted during foreach");
							break;
						}

						post.Subscription = _UOW.Subscriptions.GetByUser(s.User.Id).ToList().Where(x => x.query == s.query).FirstOrDefault();
					}

					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted during foreach or after it");
						continue;
					}

					var second = _UOW.Posts.GetByUser(s.Id).ToList();
					List<Post> substructedPosts = posts.ExceptBy(second.Select(p => p.Uri), p => p.Uri).ToList();

					foreach (var p in substructedPosts)
						_UOW.Posts.Create(p);

					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted before DB.Save");
						continue;
					}

					try
					{
						await _UOW.Save();
					}
					catch (Exception ex)
					{
						_logger.LogError($"BackgroundWorker_UOW.Save() : {ex.Message}");
					}

					foreach (var p in substructedPosts)
						_botClient.SendTextMessageAsync(
						chatId: s.User.Id,
						text: $"{p.Title}\n{p.Price}\n{p.Date}\n{p.Uri}");
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Error running background task: {0}", ex.Message);

			StopAsync(cancellationToken).Wait(cancellationToken);
			StartAsync(cancellationToken).Wait(cancellationToken);
		}
	}
}
