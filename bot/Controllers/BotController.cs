using bot.Data.Bot;
using bot.Data.Subscriptions;
using bot.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Core.Types;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace bot.Controllers
{
	[Route("api/bot")]
	[ApiController]
	public class BotController : ControllerBase
	{
		public BotController() { }

		[HttpPost]
		public async Task<IActionResult> Post(
		[FromBody] Update update,
		[FromServices] UpdateHandlers handleUpdateService,
		CancellationToken cancellationToken)
		{
			await handleUpdateService.HandleUpdateAsync(update, cancellationToken);
			return Ok();
		}
	}
}