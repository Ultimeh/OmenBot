using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System.Data;

namespace baseBot
{
    public class BotCommands : BaseCommandModule
    {
		private static readonly ulong omenRole = 1297259916368547933;
		private static readonly ulong lootID = 1299832478286090322;
		private static readonly ulong omenNewsID = 1297279620705685575;
		private static readonly ulong vexID = 114587845716344834;
		private static readonly ulong rekenID = 158460782445723658;
		private static readonly ulong maligozeID = 142830810809106432;
		private static readonly ulong ultimeID = 337741216466862080;
		private static readonly ulong guardian = 1289656595747438652;
		private static readonly ulong advisor = 1289660706857287752;
		private static readonly string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
		private const string ApplicationName = "OmenBot";
		private static string _SpreadsheetId = "1CXGhE6nJUUccRNG6n6LOf-gp0mjz6zdfnceMVA4dUfU";

		private static readonly Dictionary<int, string> _teamDic = new Dictionary<int, string>
		{
			{ 1, "A2,A7,B2,B7, Front Line 1" },
			{ 2, "E2,E7,F2,F7, Front Line 2" },
			{ 3, "I2,I7,J2,J7, Front Line 3" },
			{ 4, "M2,M7,N2,N7, Front Line 4" },
			{ 5, "A11,A16,B11,B16, Mid Line 1" },
			{ 6, "E11,E16,F11,F16, Mid Line 1" },
			{ 7, "I11,I16,J11,J16, Crab Team" },		
			{ 8, "A20,A25,B20,B25, Ranged 1" },
			{ 9, "E20,E25,F20,F25, Ranged 2" },
			{ 10, "I20,I25,J20,J25, Ranged 3" },
			{ 11, "M20,M25,N20,N25, Melee Fill Team" },
			{ 12, "M11,M16,N11,N16, Ranged Fill Team" }	
		};

		private static readonly Dictionary<string, DayOfWeek> _daysOfWeek = new Dictionary<string, DayOfWeek>
		{
			{ "sunday", DayOfWeek.Sunday },
			{ "monday", DayOfWeek.Monday },
			{ "tuesday", DayOfWeek.Tuesday },
			{ "wednesday", DayOfWeek.Wednesday },
			{ "thursday", DayOfWeek.Thursday },
			{ "friday", DayOfWeek.Friday },
			{ "saturday", DayOfWeek.Saturday }
		};

		[Command("help")]
		[Description("Deletes messages")]
		public async Task Help(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			var omen = ctx.Guild.GetRole(omenRole);
			if (!ctx.Member.Roles.Contains(omen)) return;

			await ctx.Member.SendMessageAsync(Bot.AppData.help);
		}

		[Command("purges")]
		[Description("Deletes messages")]
		public async Task purge(CommandContext ctx, int del)
		{
			if (!CheckRights(ctx, lootID)) return;

			var messages = await ctx.Channel.GetMessagesAsync(del + 1);
			await ctx.Channel.DeleteMessagesAsync(messages);
		}

		[Command("item-set")]
		[Description("set item poll")]
		public async Task Item(CommandContext ctx, [Description("item time")] int time, [Description("item slot")] string slot, [Description("item name"), RemainingText] string itemName)
		{
			if (!CheckRights(ctx, lootID)) return;
			if (string.IsNullOrWhiteSpace(slot)) return;
			if (string.IsNullOrWhiteSpace(itemName)) return;

			await ctx.Message.DeleteAsync();
			var omen = ctx.Guild.GetRole(omenRole);

			string formattedTime = "";

			if (time != 0)
			{
				int days = time / 1440;  // Get the number of days (1 day = 1440 minutes)
				int hours = (time % 1440) / 60;  // Get the number of hours from the remaining minutes
				int minutes = time % 60;  // Get the remaining minutes

				if (days != 0) formattedTime = $"{days} days {hours} hours {minutes} minutes Remain until Drawing";
				else if (days == 0 && hours != 0) formattedTime = $"{hours} hours {minutes} minutes Remain until Drawing";
				else if (days == 0 && hours == 0) formattedTime = $"{minutes} minutes Remain until Drawing";
			}
			else formattedTime = $"2 day(s) Remain until Drawing";


			var message = "**Please React with 'Thumbs Up' if this is your Best In Slot!**" + Environment.NewLine +
						  $"Item: {itemName}" + Environment.NewLine +
						  $"Item slot: {slot}" + Environment.NewLine +
						  $"{formattedTime}";


			var sent = await ctx.Channel.SendMessageAsync(/*omen.Mention + Environment.NewLine +*/ message);
			await sent.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:"));


			Bot.AppData.ItemPoll.TryAdd(sent.Id, new List<ulong>());

			TimeSpan threshold = TimeSpan.FromHours(48);
			if (time != 0) threshold = TimeSpan.FromMinutes(time);

			ItemPoll itemPoll = new ItemPoll(sent, threshold);
		}

		[Command("item")]
		[Description("current item poll")]
		public async Task Item(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();

			var omen = ctx.Guild.GetRole(omenRole);
			if (!ctx.Member.Roles.Contains(omen)) return;

			if (Bot.AppData.ItemPoll.Count == 0)
			{
				await ctx.Member.SendMessageAsync("No item polls in progess at the moment!");
				return;
			}

			await ctx.Member.SendMessageAsync("**Current item polls in progess:**");
			
			var loot = await Bot.Client.GetChannelAsync(lootID);

			foreach (var id in Bot.AppData.ItemPoll.Keys)
			{
				var message = await loot.GetMessageAsync(id);

				string content = message.Content;
				var lines = content.Split('\n').ToList();
				lines.RemoveAt(0);

				var update = string.Join('\n', lines);
				await ctx.Member.SendMessageAsync(update);
			}

			await ctx.Member.SendMessageAsync(Environment.NewLine + "**If you need any of these items,**" + Environment.NewLine + $"**Go to {loot.Mention} channel to React with 'Thumbs Up'**");
			Console.WriteLine($"{ctx.Member.DisplayName} used the !item command");
		}

		[Command("list")]
		[Description("get list")]
		public async Task List(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			if (ctx.Guild.Id != 1289323361427787808) return;
			if (ctx.Channel.Id != 1299832478286090322) return;

			await UpdateList();

			List<string> prepareList = new List<string>();

			int maxDiscordMessageLength = 2000 - 8;
			int maxNameLength = Bot.AppData.OmenList.Max(item => item.Name.Length) + 1;  // Adding padding
			int maxRoleLength = Bot.AppData.OmenList.Max(item => item.Role.Length) + 1;  // Adding padding

			foreach (var item in Bot.AppData.OmenList)
			{
				string formattedLine = $"{item.Name.PadRight(maxNameLength)} {item.Role.PadRight(maxRoleLength)} [{item.WinCount}]";
				prepareList.Add(formattedLine);
			}

			var fullMessage = string.Join(Environment.NewLine, prepareList);

			var messagesToSend = new List<string>();

			while (fullMessage.Length > maxDiscordMessageLength)
			{
				// Find the last newline character within the limit
				int splitIndex = fullMessage.LastIndexOf('\n', maxDiscordMessageLength);
				if (splitIndex == -1) splitIndex = maxDiscordMessageLength; // Fallback if no newline found

				// Get the part of the message to send
				messagesToSend.Add(fullMessage.Substring(0, splitIndex));

				// Remove the sent part from the full message
				fullMessage = fullMessage.Substring(splitIndex).Trim();
			}

			// Add any remaining message
			if (fullMessage.Length > 0)
			{
				messagesToSend.Add(fullMessage);
			}

			// Now send each message in messagesToSend separately
			foreach (var message in messagesToSend)
			{
				await ctx.Channel.SendMessageAsync("```" + Environment.NewLine + message + Environment.NewLine + "```");
			}
		}

		[Command("draw")]
		[Description("draw users to win")]
		public async Task Draw(CommandContext ctx, [Description("Count")] int count, [Description("role")] string role = "")
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, lootID)) return;

			if (count < 1)
			{
				await ctx.Channel.SendMessageAsync("The count need to be higher than 0.");
				return;
			}

			if (role != "" && !Bot.AppData.Roles.Contains(role.ToLower()))
			{
				await ctx.Channel.SendMessageAsync($"The class {role} do not exist.");
				return;
			}

			var chat = ctx.Guild.GetChannel(1289323361960460399);

			await UpdateList();

			Users[] userList;

			var msg = "";

			if (role != "") 
			{
				userList = Bot.AppData.OmenList.Where(user => string.Equals(user.Role, role, StringComparison.OrdinalIgnoreCase)).ToArray();
				msg = $"The {count} random winner(s) for the {role}:";		
			}
			else 
			{
				userList = Bot.AppData.OmenList.ToArray();
				msg = $"The {count} random winner(s):";
			}

			if (userList.Count() == 0 && role != "")
			{
				await ctx.Channel.SendMessageAsync($"No members with the class {role}.");
				return;
			}

			if (role != "") await chat.SendMessageAsync($"Random roll for {count} guild member(s) in progress for the class: {role}!");
			else await chat.SendMessageAsync($"Random roll for {count} guild member(s) in progress!");

			count = Math.Min(count, userList.Count());

			for (int i = userList.Count() - 1; i >= 0; i--) // Shuffle the keys
			{
				int j = 0;

				lock (Bot.LockRandom)
				{
					j = Bot.Random.Next(0, i + 1); // Pick a random index
				}
		
				(userList[i], userList[j]) = (userList[j], userList[i]); // Swap elements
			}

			var randomPick = userList.Take(count).ToList();

			List<string> values = new List<string>();

			foreach (var item in randomPick)
			{
				var index = Bot.AppData.OmenList.IndexOf(item);
				Bot.AppData.OmenList[index].WinCount++;
				values.Add(item.Name);
			}

			Bot.ManageDB.SaveList();

			await Task.Delay(4000);
			
			var message = string.Join(Environment.NewLine, values);
			await chat.SendMessageAsync(msg + Environment.NewLine + "```" + Environment.NewLine + message + Environment.NewLine + "```");
		}


		[Command("class")]
		[Description("display all user with given class")]
		public async Task Reset(CommandContext ctx, [Description("Class")] string role = "")
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, lootID)) return;

			role = role.ToLower();

			if (!Bot.AppData.Roles.Contains(role))
			{
				//var roles = string.Join(", ", Bot.AppData.Roles);
				await ctx.Channel.SendMessageAsync($"Invalid class.");
				return;
			}

			List<string> users = new List<string>();

			await UpdateList();

			foreach (var item in Bot.AppData.OmenList)
			{
				if (item.Role.Equals(role, StringComparison.OrdinalIgnoreCase)) users.Add(item.Name);
			}

			var message = string.Join(Environment.NewLine, users);
			await ctx.Channel.SendMessageAsync($"Member(s) with the class '{role}':" + Environment.NewLine + "```" + Environment.NewLine + message + Environment.NewLine + "```");
		}

		[Command("mute")]
		[Description("Mute users in voice")]
		public async Task Mute(CommandContext ctx)
		{
			if (ctx.Guild.Id != 1299241243670216735) return;
			if (ctx.Channel.Id != 1301421180577648640) return;

			var userRole = ctx.Guild.GetRole(1301419835028144128);
			if (!ctx.Member.Roles.Contains(userRole)) return;

			//var roleCall = ctx.Guild.GetRole(1289663652676505693);
			var roleLead = ctx.Guild.GetRole(1301419835028144128);


			foreach (var item in ctx.Guild.Members.Values)
			{
				if (/*!item.Roles.Contains(roleCall) ||*/ !item.Roles.Contains(roleLead))
				{
					if (item.VoiceState != null) 
					{
						await item.SetMuteAsync(true);
						Console.WriteLine(item.DisplayName + "is muted");
					}
				}
			}
		}

		[Command("unmute")]
		[Description("Unmute users in voice")]
		public async Task Unmute(CommandContext ctx)
		{
			if (ctx.Guild.Id != 1299241243670216735) return;
			if (ctx.Channel.Id != 1301421180577648640) return;

			var userRole = ctx.Guild.GetRole(1301419835028144128);
			if (!ctx.Member.Roles.Contains(userRole)) return;

			//var roleCall = ctx.Guild.GetRole(1289663652676505693);
			var roleLead = ctx.Guild.GetRole(1301419835028144128);

			foreach (var item in ctx.Guild.Members.Values)
			{
				if (/*!item.Roles.Contains(roleCall) ||*/ !item.Roles.Contains(roleLead))
				{			
					if (item.VoiceState != null) 
					{
						await item.SetMuteAsync(false);
						Console.WriteLine(item.DisplayName + "is Unmuted");
					}
				}
			}
		}

		[Command("reset")]
		[Description("Reset win count")]
		public async Task Reset(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, lootID)) return;

			foreach (var item in Bot.AppData.OmenList.ToArray())
			{
				item.WinCount = 0;
			}

			Bot.ManageDB.SaveList();
			await ctx.Channel.SendMessageAsync("Win count has been reset to '0' for all members.");
		}

		[Command("ting")]
		[Description("Reset win count")]
		public async Task Ting(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			await ctx.Channel.SendMessageAsync($"Hi Tall-Tinger {ctx.Member.Mention}, please help me find a perfect pumpkin!");
		}

		[Command("team")]
		[Description("team comp (self)")]
		public async Task Self(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();

			var omen = ctx.Guild.GetRole(omenRole);
			if (!ctx.Member.Roles.Contains(omen)) return;

			await ctx.Member.SendMessageAsync("Generating the roster... (may take a few seconds)");
			var embedsList = await GetTeam();

			// Send the first 10 embeds in one message
			if (embedsList.Count <= 10)
			{
				var messageBuilder = new DiscordMessageBuilder().AddEmbeds(embedsList.ToArray());  // Convert list to array
				await ctx.Member.SendMessageAsync(messageBuilder);
			}
			else
			{
				var firstBatch = embedsList.Take(10).ToList();
				var secondBatch = embedsList.Skip(10).ToList();

				// Send the first batch
				var firstBatchMessageBuilder = new DiscordMessageBuilder().AddEmbeds(firstBatch.ToArray());
				await ctx.Member.SendMessageAsync(firstBatchMessageBuilder);
				// Send the second batch
				var secondBatchMessageBuilder = new DiscordMessageBuilder().AddEmbeds(secondBatch.ToArray());
				await ctx.Member.SendMessageAsync(secondBatchMessageBuilder);
			}

			Console.WriteLine($"{ctx.Member.DisplayName} used the !team command");
		}

		[Command("roster")]
		[Description("team comp (no poll)")]
		public async Task Final(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, omenNewsID)) return;			

			var embedsList = await GetTeam();

			var omen = ctx.Guild.GetRole(omenRole);
			await ctx.Channel.SendMessageAsync(omen.Mention + Environment.NewLine + "Here is the team roster based on availability!");

			// Send the first 10 embeds in one message
			if (embedsList.Count <= 10)
			{
				var messageBuilder = new DiscordMessageBuilder().AddEmbeds(embedsList.ToArray());  // Convert list to array
				await ctx.Channel.SendMessageAsync(messageBuilder);
			}
			else
			{
				var firstBatch = embedsList.Take(10).ToList();
				var secondBatch = embedsList.Skip(10).ToList();

				// Send the first batch
				var firstBatchMessageBuilder = new DiscordMessageBuilder().AddEmbeds(firstBatch.ToArray());
				await ctx.Channel.SendMessageAsync(firstBatchMessageBuilder);
				// Send the second batch
				var secondBatchMessageBuilder = new DiscordMessageBuilder().AddEmbeds(secondBatch.ToArray());
				await ctx.Channel.SendMessageAsync(secondBatchMessageBuilder);
			}
		}

		[Command("roster-poll")]
		[Description("team comp poll")]
		public async Task Roster(CommandContext ctx)
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, omenNewsID)) return;
			
			var embedsList = await GetTeam();

			var omen = ctx.Guild.GetRole(omenRole);
			await ctx.Channel.SendMessageAsync(omen.Mention + Environment.NewLine + "Here is the planned team roster! Please select 👍 if you can come or 👎 if you can't on the appropriate team.");

			foreach (var embed in embedsList)
			{
				// Send each embed as a separate message
				var message = await ctx.Channel.SendMessageAsync(embed: embed);

				// Add reactions for thumbs up (show) and thumbs down (hide) for each embed message
				await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsup:"));
				await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":thumbsdown:"));
			}
		}

		[Command("boon")]
		[Description("Boonstone time") ]
		public async Task Boon(CommandContext ctx, [Description("day")] string day = "")
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, omenNewsID)) return;

			if (day == "") 
			{
				await ctx.Member.SendMessageAsync("You need to enter a day of the week. (ex: !boon monday) ");
				return;
			}
		 
			await Event(ctx, day, "Boonstone war!");
		}

		[Command("Rift")]
		[Description("Riftstone time")]
		public async Task Rift(CommandContext ctx, [Description("day")] string day = "")
		{
			await ctx.Message.DeleteAsync();
			if (!CheckRights(ctx, omenNewsID)) return;

			if (day == "")
			{
				await ctx.Member.SendMessageAsync("You need to enter a day of the week. (ex: !rift monday) ");
				return;
			}

			await Event(ctx, day, "Riftstone war!");
		}

		private async Task Event(CommandContext ctx, string day, string name)
		{
			string normalizedDay = day.Trim().ToLower();

			if (!_daysOfWeek.TryGetValue(day.ToLower(), out DayOfWeek targetDay))
			{
				await ctx.Member.SendMessageAsync("Invalid day entered. Please use a valid day of the week (e.g., Monday, Friday).");
				return;
			}

			// Get current Eastern Time (ET)
			TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
			DateTime currentTimeEastern = TimeZoneInfo.ConvertTime(DateTime.Now, easternTimeZone);

			// Calculate the closest occurrence of the entered day at 7:00 PM ET
			DateTime nextDay = currentTimeEastern.Date.AddHours(19); // Start with today's date at 7:00 PM ET

			// If the target day is today but after 7:00 PM, move to next week
			if (currentTimeEastern.DayOfWeek == targetDay && currentTimeEastern.Hour >= 19)
			{
				nextDay = currentTimeEastern.AddDays(7).Date.AddHours(19); // Set it for the same day next week
			}
			else
			{
				// Calculate days to the next target day
				int daysToAdd = ((int)targetDay - (int)currentTimeEastern.DayOfWeek + 7) % 7;
				nextDay = currentTimeEastern.AddDays(daysToAdd).Date.AddHours(19); // Set to 7:00 PM ET on that day
			}

			// Convert the event time to UTC
			DateTime eventTimeUtc = TimeZoneInfo.ConvertTimeToUtc(nextDay, easternTimeZone);

			// Convert to Unix timestamp for Discord
			long unixTimestamp = new DateTimeOffset(eventTimeUtc).ToUnixTimeSeconds();

			var embed = new DiscordEmbedBuilder()
				.WithTitle(name)
				.WithDescription($"The event is scheduled for {day} at <t:{unixTimestamp}:t>") // Time in UTC
				.WithColor(DiscordColor.Gold);

			var omen = ctx.Guild.GetRole(omenRole);
			await ctx.Channel.SendMessageAsync(omen.Mention, embed);
		}

		private async Task<List<DiscordEmbed>> GetTeam()
		{
			GoogleCredential credential;

			using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes); // load credential pour sheet           
			}

			using var service = new SheetsService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			var embedsList = new List<DiscordEmbed>();

			for (int team = 1; team <= 12; team++)
			{
				var info = _teamDic[team].Split(',');
				string teamInfo = "";

				string startName = info[0];
				string endName = info[1];
				string startClass = info[2];
				string endClass = info[3];

				string rangeName = $"PvP Composition!{startName}:{endName}";
				string rangeClass = $"PvP Composition!{startClass}:{endClass}";

				// Fetch data from Column A
				SpreadsheetsResource.ValuesResource.GetRequest requestA = service.Spreadsheets.Values.Get(_SpreadsheetId, rangeName);
				ValueRange responseA = await requestA.ExecuteAsync();
				var valuesA = responseA.Values;

				// Fetch data from Column B
				SpreadsheetsResource.ValuesResource.GetRequest requestB = service.Spreadsheets.Values.Get(_SpreadsheetId, rangeClass);
				ValueRange responseB = await requestB.ExecuteAsync();
				var valuesB = responseB.Values;

				if (valuesA == null) continue;

				if (info.Count() == 5) teamInfo = info[4];
				int count = Math.Min(valuesA.Count, valuesB.Count);

				List<string> tempList = new List<string>();

				for (int i = 0; i < count; i++)
				{
					if (valuesA[i] != null)
					{
						string role = "";
						if (valuesB[i] != null && valuesB[i].Count != 0) role = " | " + valuesB[i][0].ToString().Trim();
						if (valuesA[i].Count != 0) tempList.Add(valuesA[i][0].ToString().Trim() + role);
					}
				}

				var embed = new DiscordEmbedBuilder()
				.WithTitle(teamInfo)
				.WithDescription(string.Join(Environment.NewLine, tempList))
				.WithColor(DiscordColor.Green)
				.Build();

				embedsList.Add(embed);			
			}

			return embedsList;
		}

		private async Task UpdateList()
		{
			GoogleCredential credential;

			using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
			{
				credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes); // load credential pour sheet           
			}

			using var service = new SheetsService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credential,
				ApplicationName = ApplicationName,
			});

			string rangeA = "Roster Information!A2:A";
			string rangeB = "Roster Information!B2:B";

			SpreadsheetsResource.ValuesResource.GetRequest requestA = service.Spreadsheets.Values.Get(_SpreadsheetId, rangeA);
			ValueRange responseA = await requestA.ExecuteAsync();
			var valuesA = responseA.Values;

			SpreadsheetsResource.ValuesResource.GetRequest requestB = service.Spreadsheets.Values.Get(_SpreadsheetId, rangeB);
			ValueRange responseB = await requestB.ExecuteAsync();
			var valuesB = responseB.Values;

			List<Users> tempOmen = new List<Users>();

			if (valuesA == null && valuesB == null)
			{
				return;
			}

			int count = Math.Min(valuesA.Count, valuesB.Count);

			for (int i = 0; i < count; i++)
			{
				tempOmen.Add(new Users { Name = valuesA[i][0]?.ToString().Trim() ?? string.Empty, Role = valuesB[i][0]?.ToString().Trim() ?? string.Empty });
			}

			foreach (var user in tempOmen)
			{
				var existingUser = Bot.AppData.OmenList.FirstOrDefault(u => u.Name == user.Name);

				if (existingUser == null) Bot.AppData.OmenList.Add(new Users { Name = user.Name, Role = user.Role });
				else
				{
					if (existingUser.Role != user.Role) existingUser.Role = user.Role;
				}
			}

			var usersToRemove = Bot.AppData.OmenList.Where(u => !tempOmen.Any(user => user.Name == u.Name)).ToArray();

			foreach (var userToRemove in usersToRemove)
			{
				Bot.AppData.OmenList.Remove(userToRemove);
			}

			Bot.ManageDB.SaveList();
		}

		private bool CheckRights(CommandContext ctx, ulong id)
		{
			var guard = ctx.Guild.GetRole(guardian);
			var advise = ctx.Guild.GetRole(advisor);

			if (ctx.Channel.Id == id && (ctx.User.Id == ultimeID || ctx.User.Id == vexID || ctx.Member.Roles.Contains(guard) || ctx.Member.Roles.Contains(advise))) return true;
			return false;
		}
	}
}
