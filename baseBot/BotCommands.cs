using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace baseBot
{
    public class BotCommands : BaseCommandModule
    {
		private static readonly Random random = new Random();
		private static readonly ulong channelID = 1299832478286090322;
		private static readonly ulong vexID = 114587845716344834;
		private static readonly ulong rekenID = 158460782445723658;
		private static readonly ulong maligozeID = 142830810809106432;
		private static readonly ulong ultimeID = 337741216466862080;

		[Command("help")]
		[Description("Deletes messages")]
		public async Task Help(CommandContext ctx)
		{
			if (!CheckRights(ctx)) return;

			var messages = await ctx.Channel.GetMessagesAsync(1);
			await ctx.Channel.DeleteMessagesAsync(messages);

			await ctx.Member.SendMessageAsync(Bot.AppData.help);
		}

		[Command("purges")]
		[Description("Deletes messages")]
		public async Task purge(CommandContext ctx, int del)
		{
			if (!CheckRights(ctx)) return;

			var messages = await ctx.Channel.GetMessagesAsync(del + 1);
			await ctx.Channel.DeleteMessagesAsync(messages);
		}

		[Command("add")]
		[Description("add new user")]
		public async Task AddUser(CommandContext ctx, [Description("UserName")] string name, string role = "")
        {
			if (!CheckRights(ctx)) return;
			if (string.IsNullOrWhiteSpace(name)) return;

			if (string.IsNullOrWhiteSpace(role))
			{
				await ctx.Channel.SendMessageAsync($"You must set a role to create a user: '!add {name} role");
				return;
			}

			role = role.ToLower();

			if (!Bot.AppData.Roles.Contains(role))
			{
				await ctx.Channel.SendMessageAsync("Invalid Role (accepted roles: tank, healer, melee, range, m/r)");
				return;
			}

			foreach(var item in Bot.AppData.OmenList)
			{
				if (name.ToLower() == item.Name.ToLower())
				{
					await ctx.Channel.SendMessageAsync($"user: {name} is already in the list.");
					return;
				}
			}

			Bot.AppData.OmenList.Add(new Users { Name = name, Role = role });

			Bot.ManageDB.SaveList();
			await ctx.Channel.SendMessageAsync($"Added user: {name}.");
		}

		[Command("del")]
		[Description("remove user")]
		public async Task DelUser(CommandContext ctx, [Description("UserName")] string name = "")
		{
			if (!CheckRights(ctx)) return;
			if (string.IsNullOrEmpty(name)) return;

			foreach (var item in Bot.AppData.OmenList.ToArray())
			{
				if (name.ToLower() == item.Name.ToLower())
				{
					Bot.AppData.OmenList.Remove(item);
					Bot.ManageDB.SaveList();
					await ctx.Channel.SendMessageAsync($"User removed: {name}.");
					return;
				}
			}

			await ctx.Channel.SendMessageAsync($"User: {name} not found.");		
		}

		[Command("list")]
		[Description("Show the current list")]
		public async Task UserList(CommandContext ctx)
		{
			if (!CheckRights(ctx)) return;

			if (Bot.AppData.OmenList.Count == 0)
            {
				await ctx.Channel.SendMessageAsync("User list is empty.");
				return;
			}

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

			//await ctx.Channel.SendMessageAsync( $"{fullMessage.Length} caracters " );

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
			if (!CheckRights(ctx)) return;

			if (count < 1)
			{
				await ctx.Channel.SendMessageAsync("The count need to be higher than 0.");
				return;
			}

			var userList = new List<Users>();
			string msg = "";

			if (!string.IsNullOrWhiteSpace(role))
			{
				role = role.ToLower();

				if (!Bot.AppData.Roles.Contains(role))
				{
					var roles = string.Join(", ", Bot.AppData.Roles);
					await ctx.Channel.SendMessageAsync($"Invalid Role (accepted roles: {roles})");
					return;
				}

				foreach (var item in Bot.AppData.OmenList)
				{
					if (item.Role == role) userList.Add(item);
					msg = $"The {count} random winner(s) [{role}]:";
				}
			}

			if (role == "") 
			{
				userList = Bot.AppData.OmenList;
				msg = $"The {count} random winner(s):";
			}

			if (userList.Count == 0)
			{
				await ctx.Channel.SendMessageAsync($"No user with the role: {role}");
				return;
			}

			count = Math.Min(count, userList.Count);

			for (int i = userList.Count - 1; i >= 0; i--) // Shuffle the keys
			{
				int j = random.Next(0, i + 1); // Pick a random index
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

			var message = string.Join(Environment.NewLine, values);
			await ctx.Channel.SendMessageAsync(msg + Environment.NewLine + "```" + Environment.NewLine + message + Environment.NewLine + "```");
		}

		[Command("reset")]
		[Description("reset wins for all users")]
		public async Task Reset(CommandContext ctx)
		{
			if (!CheckRights(ctx)) return;

			foreach (var item in Bot.AppData.OmenList)
			{
				item.WinCount = 0;
			}

			Bot.ManageDB.SaveList();
			await ctx.Channel.SendMessageAsync("All Win counts reset to 0.");
		}

		[Command("role")]
		[Description("display all user with given role")]
		public async Task Reset(CommandContext ctx, [Description("Role")] string role = "")
		{
			if (!CheckRights(ctx)) return;

			role = role.ToLower();

			if (!Bot.AppData.Roles.Contains(role))
			{
				var roles = string.Join(", ", Bot.AppData.Roles);
				await ctx.Channel.SendMessageAsync($"Invalid Role (accepted roles: {roles}");
				return;
			}

			List<string> users = new List<string>();

			foreach (var item in Bot.AppData.OmenList)
			{
				if (item.Role == role) users.Add(item.Name);
				if (item.Role == "m/r" && (role == "melee" || role == "range")) users.Add(item.Name);
			}

			var message = string.Join(Environment.NewLine, users);
			await ctx.Channel.SendMessageAsync($"User(s) with the role '{role}':" + Environment.NewLine + "```" + Environment.NewLine + message + Environment.NewLine + "```");
		}

		[Command("edit")]
		[Description("change user role")]
		public async Task EditRole(CommandContext ctx, [Description("UserName")] string name = "", [Description("Role")] string role = "")
		{
			if (!CheckRights(ctx)) return;
			if (string.IsNullOrEmpty(name)) return;
			if (string.IsNullOrEmpty(role)) return;

			name = name.ToLower();
			role = role.ToLower();

			if (!Bot.AppData.Roles.Contains(role))
			{
				await ctx.Channel.SendMessageAsync("Invalid Role (accepted roles: tank, healer, melee, range, m/r)");
				return;
			}

			foreach (var item in Bot.AppData.OmenList)
			{
				if (item.Name.ToLower() == name)
				{
					item.Role = role;
					Bot.ManageDB.SaveList();
					await ctx.Channel.SendMessageAsync($"{item.Name} role changed to '{role}'");
					return;
				}
			}

			await ctx.Channel.SendMessageAsync($"User: {name} not found.");
		}

		private bool CheckRights(CommandContext ctx)
		{
			if (ctx.Channel.Id == channelID && (ctx.User.Id == vexID || ctx.User.Id == rekenID || ctx.User.Id == ultimeID || ctx.User.Id == maligozeID)) return true;
			return false;
		}
	}
}
