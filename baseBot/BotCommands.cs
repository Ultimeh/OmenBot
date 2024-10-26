using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace baseBot
{
    public class BotCommands : BaseCommandModule
    {
		private static readonly Random random = new Random();

        [Command("purges")]
        [Description("Deletes messages")]
        public async Task purge(CommandContext ctx, int del)
        {
            if (ctx.Channel.Id == 803487353288917010 || ctx.Channel.Id == 812138366237278270 || ctx.Channel.Id == 691207717842321409 || ctx.Channel.Id == 908802539519103007)
            {
                var messages = await ctx.Channel.GetMessagesAsync(del + 1);
                await ctx.Channel.DeleteMessagesAsync(messages);
            }
        }

		[Command("add")]
        [Description("add new user")]
        public async Task AddUser(CommandContext ctx, [RemainingText, Description("UserName")] string name)
        {
			if (ctx.Channel.Id != 691207717842321409) return;
            if (string.IsNullOrEmpty(name)) return;

            Bot.AppData.UserList.Add(name, 0);
			Bot.ManageDB.SaveList();

			await ctx.Channel.SendMessageAsync($"Added user: {name}.");
		}

		[Command("del")]
		[Description("remove user")]
		public async Task DelUser(CommandContext ctx, [RemainingText, Description("UserName")] string name)
		{
			if (ctx.Channel.Id != 691207717842321409) return;
			if (string.IsNullOrEmpty(name)) return;

            foreach (var item in Bot.AppData.UserList.ToArray())
            {
                if (item.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
					Bot.AppData.UserList.Remove(item.Key);
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
			if (ctx.Channel.Id != 691207717842321409) return;

			if (Bot.AppData.UserList.Count == 0)
            {
				await ctx.Channel.SendMessageAsync("User list is empty.");
				return;
			}

			List<string> prepareList = new List<string>();

			foreach (var item in Bot.AppData.UserList)
			{
				prepareList.Add(item.Key +  $" Win count: {item.Value}");
			}

			var message = string.Join(Environment.NewLine, prepareList);
			await ctx.Channel.SendMessageAsync("```" + message + "```");
		}

		[Command("draw")]
		[Description("draw users to win")]
		public async Task Draw(CommandContext ctx, [Description("Count")] int count = 1)
		{
			if (ctx.Channel.Id != 691207717842321409) return;

			if (count < 1)
			{
				await ctx.Channel.SendMessageAsync("The coutn need to be higher than 0.");
				return;
			}

			var keys = Bot.AppData.UserList.Keys.ToList();
			count = Math.Min(count, keys.Count);

			for (int i = keys.Count - 1; i > 0; i--)
			{
				int j = random.Next(0, i + 1); // Pick a random index
				(keys[i], keys[j]) = (keys[j], keys[i]); // Swap elements
			}

			var randomKeys = keys.Take(count).ToList();
			List<string> values = new List<string>();

			foreach (var key in randomKeys)
			{
				Bot.AppData.UserList[key]++;
				values.Add(key);
			}

			Bot.ManageDB.SaveList();

			var message = string.Join(Environment.NewLine, values);
			await ctx.Channel.SendMessageAsync($"The {count} random winner(s):" + Environment.NewLine + "```" + message + "```");
		}

		[Command("reset")]
		[Description("reset wins for all users")]
		public async Task Reset(CommandContext ctx)
		{
			if (ctx.Channel.Id != 691207717842321409) return;
			
			foreach (var key in Bot.AppData.UserList.Keys.ToArray())
			{
				Bot.AppData.UserList[key] = 0;
			}

			Bot.ManageDB.SaveList();

			await ctx.Channel.SendMessageAsync("All Win counts reset to 0.");
		}
	}
}
