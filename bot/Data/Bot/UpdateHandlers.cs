using bot.Data.Subscriptions;
using bot.Models;
using bot.SyncDataServices.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace bot.Data.Bot
{
	public partial class UpdateHandlers
	{
		private readonly ITelegramBotClient _botClient;
		private readonly ILogger<UpdateHandlers> _logger;
		private readonly IDataParserDataClient _dataParserDataClient;
		private readonly IUOW _UOW;

		public UpdateHandlers(ITelegramBotClient botClient, ILogger<UpdateHandlers> logger, IDataParserDataClient dataParserDataClient, IUOW UOW)
		{
			_botClient = botClient;
			_logger = logger;
			_dataParserDataClient = dataParserDataClient;
			_UOW = UOW;
		}

		public Task HandleErrorAsync(Exception exception, CancellationToken cancellationToken)
		{
			var ErrorMessage = exception switch
			{
				ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
				_ => exception.ToString()
			};

			_logger.LogWarning("HandleError: {ErrorMessage}", ErrorMessage);
			return Task.CompletedTask;
		}

		public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
		{
			var handler = update switch
			{
				{ Message: { } message } => BotOnMessageReceived(message, cancellationToken),
				{ EditedMessage: { } message } => BotOnMessageReceived(message, cancellationToken),
				{ CallbackQuery: { } callbackQuery } => BotOnCallbackQueryReceived(callbackQuery, cancellationToken),
				_ => UnknownUpdateHandlerAsync(update, cancellationToken)
			};

			await handler;
		}

		private async Task BotOnMessageReceived(Message message, CancellationToken cancellationToken)
		{
			_logger.LogWarning("Receive message type: {MessageType}", message.Type);
			if (message.Text is not { } messageText)
				return;
			var action = messageText.Split(' ')[0] switch
			{
				"/menu" => SendMenuInlineKeyboard(_botClient, message, cancellationToken),
				"/start" => OnStart(_botClient, message, cancellationToken),
				_ => ProcessInput(_botClient, message, cancellationToken)
			};

			await action;

			// Send inline keyboard
			// You can process responses in BotOnCallbackQueryReceived handler
			static async Task SendMenuInlineKeyboard(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
			{
				await botClient.SendChatActionAsync(
					chatId: message.Chat.Id,
					chatAction: ChatAction.Typing,
					cancellationToken: cancellationToken);

				InlineKeyboardMarkup inlineKeyboard = new(
					new[]
					{
						InlineKeyboardButton.WithCallbackData("New Template", "new"),
						InlineKeyboardButton.WithCallbackData("My Templates", "list")
					});

				await botClient.SendTextMessageAsync(
					   chatId: message.Chat.Id,
					   text: "Menu:",
					   replyMarkup: inlineKeyboard,
					   cancellationToken: cancellationToken);
			}

			async Task ProcessInput(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
			{
				if (!MyRegex().IsMatch(message.Text))
				{
					await botClient.SendTextMessageAsync(
											chatId: message.From.Id,
											text: "One or more characters are not allowed",
											cancellationToken: cancellationToken);

					await Menu(botClient, message, cancellationToken);

					return;
				}
				if (_UOW.Subscriptions.Exists(message.From.Id, message.Text))
				{
					await botClient.SendTextMessageAsync(
									chatId: message.From.Id,
									text: "Template already exists",
									cancellationToken: cancellationToken);

					await Menu(botClient, message, cancellationToken);

					return;
				}
				if (_UOW.Subscriptions.GetByUser(message.From.Id).Count() >= 10)
				{
					await botClient.SendTextMessageAsync(
									chatId: message.From.Id,
									text: "You have reached maximum number of templates",
									cancellationToken: cancellationToken);

					await Menu(botClient, message, cancellationToken);

					return;
				}

				Models.User user = _UOW.Users.Get((int)message.Chat.Id);
				Subscription s = new Subscription { User = user, date = DateTime.Now, query = message.Text };

				_UOW.Subscriptions.Create(s);
				await _UOW.Save();

				await botClient.SendTextMessageAsync(
							chatId: message.From.Id,
							text: "Successfully added template",
							cancellationToken: cancellationToken);

				await Menu(botClient, message, cancellationToken);

				FirstParse(s);
			}

			async Task OnStart(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
			{
				_UOW.Users.Create(new Models.User { Id = message.From.Id });
				await _UOW.Save();

				await Menu(botClient, message, cancellationToken);
			}
		}

		// Process Inline Keyboard callback data
		private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken cancellationToken)
		{
			_logger.LogWarning("Received inline keyboard callback from: {CallbackQueryId}", callbackQuery.Id);

			InlineKeyboardMarkup inlineKeyboard = new(
					new[]
					{
						InlineKeyboardButton.WithCallbackData("Delete", "delete")
					});

			ForceReplyMarkup forceReplyMarkup = new ForceReplyMarkup();
			forceReplyMarkup.InputFieldPlaceholder = "Write your template query:";

			switch (callbackQuery.Data)
			{
				case "new":
					await _botClient.SendChatActionAsync(
						chatId: callbackQuery.Message!.Chat.Id,
						chatAction: ChatAction.Typing,
						cancellationToken: cancellationToken);

					await _botClient.SendTextMessageAsync(
							chatId: callbackQuery.Message!.Chat.Id,
							text: "Write your template query:",
							replyMarkup: forceReplyMarkup,
							cancellationToken: cancellationToken);

					try
					{
						await _botClient.DeleteMessageAsync(
							chatId: callbackQuery.Message.Chat.Id,
							messageId: callbackQuery.Message.MessageId);
					}
					catch
					{
						_logger.LogError("ERROR deleting msg in 'new'");
					}

					break;
				case "list":
					var userSubs = _UOW.Subscriptions.GetByUser(callbackQuery.Message!.Chat.Id);

					if (userSubs.Any())
					{
						await _botClient.SendTextMessageAsync(
								chatId: callbackQuery.Message!.Chat.Id,
								text: "Your templates:",
								cancellationToken: cancellationToken);

						foreach (var subscription in userSubs)
							await _botClient.SendTextMessageAsync(
								chatId: callbackQuery.Message!.Chat.Id,
								text: subscription.query,
								replyMarkup: inlineKeyboard,
								cancellationToken: cancellationToken);
					}
					else
						await _botClient.SendTextMessageAsync(
							chatId: callbackQuery.Message!.Chat.Id,
							text: "You currently have 0 templates",
							cancellationToken: cancellationToken);

					try
					{
						await _botClient.DeleteMessageAsync(
							chatId: callbackQuery.Message.Chat.Id,
							messageId: callbackQuery.Message.MessageId);
					}
					catch
					{
						_logger.LogError("ERROR deleting msg in 'list'");
					}

					break;
				case "delete":
					if (_UOW.Subscriptions.Exists(callbackQuery.Message!.Chat.Id, callbackQuery.Message.Text))
					{
						_UOW.Subscriptions.Delete(callbackQuery.Message!.Chat.Id, callbackQuery.Message.Text);
						await _UOW.Save();
						await _botClient.AnswerCallbackQueryAsync(
							callbackQueryId: callbackQuery.Id,
							text: "Successfully deleted",
							cancellationToken: cancellationToken);
					}
					else
					{
						await _botClient.AnswerCallbackQueryAsync(
							callbackQueryId: callbackQuery.Id,
							text: "Error: There is no such template",
							cancellationToken: cancellationToken);
					}

					try
					{
						await _botClient.DeleteMessageAsync(
							chatId: callbackQuery.Message.Chat.Id,
							messageId: callbackQuery.Message.MessageId);
					}
					catch
					{
						_logger.LogError("ERROR deleting msg in 'delete'");
					}

					break;
			}
		}

		private Task UnknownUpdateHandlerAsync(Update update, CancellationToken cancellationToken)
		{
			_logger.LogWarning("No such command: {UpdateType}", update.Type);
			return Task.CompletedTask;
		}

		private async Task FirstParse(Subscription s)
		{
			string? result = "";

			if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
			{
				_logger.LogWarning($"Subscription {s.query} was deleted before ParseData");
				return;
			}

			// Send Sync Message
			try
			{
				result = await _dataParserDataClient.ParseData(s.query);
			}
			catch (Exception ex)
			{
				_logger.LogWarning($"--> Could not send synchronously: {ex.Message}");
				return;
			}

			if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
			{
				_logger.LogWarning($"Subscription {s.query} was deleted before DeserializeObject");
				return;
			}

			List<Post> posts;
			posts = JsonConvert.DeserializeObject<List<Post>>(result);

			if (posts is null)
			{
				_logger.LogWarning($"NULL for {s.query}");
				return;
			}

			if (posts.Count == 0)
			{
				_logger.LogWarning($"No data found for {s.query}");
				return;
			}

			if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
			{
				_logger.LogWarning($"Subscription {s.query} was deleted before DB.Create");
				return;
			}

			foreach (Post post in posts)
			{
				if (post.Price is not null)    //REMOVE
				{
					if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
					{
						_logger.LogWarning($"Subscription {s.query} was deleted during foreach");
						return;
					}

					post.Subscription = _UOW.Subscriptions.GetByUser(s.User.Id).Where(x => x.query == s.query).FirstOrDefault();

					_UOW.Posts.Create(post);
				}
			}

			if (!_UOW.Subscriptions.Exists(s.User.Id, s.query))
			{
				_logger.LogWarning($"Subscription {s.query} was deleted before DB.Save");
				return;
			}

			await _UOW.Save();
		}

		private async Task Menu(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
		{
			const string usage = "Click to open menu:\n" +
								 "/menu";

			await botClient.SendTextMessageAsync(
											chatId: message.From.Id,
											text: usage,
											cancellationToken: cancellationToken);
		}

		[GeneratedRegex("^[\\p{L}\\p{N}\\p{P}\\s]+$")]
		private static partial Regex MyRegex();
	}
}
