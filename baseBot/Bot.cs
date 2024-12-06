using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace baseBot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
		private static readonly ulong leaderID = 1289655024666017843;
		public static readonly Random Random = new Random();
		public static readonly object LockRandom = new object();
		public static AppData AppData { get; set; } = new AppData();
		public static ManageDB ManageDB { get; set; } = new ManageDB();
		public static string _SpreadsheetId = "1CXGhE6nJUUccRNG6n6LOf-gp0mjz6zdfnceMVA4dUfU";
		public CommandsNextExtension Commands { get; private set; }
		CancellationTokenSource _cts = new CancellationTokenSource();

		public static ConcurrentDictionary<ulong, List<ulong>> ItemPoll = new ConcurrentDictionary<ulong, List<ulong>>();

		public async Task StartBot()
		{
			SetupClient();

			try
			{
				await Client.ConnectAsync(); //connection a Discord
				Console.WriteLine("Connected: Bot is ready." + Environment.NewLine);
			}
			catch (Exception ex)
			{
				ExitApp(ex.Message);
			}

			SetEvents();

			try
			{
				await Task.Delay(-1, _cts.Token); //garde le app running en attente de commande tant que le token pas canceller	
			}
			catch (OperationCanceledException) //catch le ctrl+c exception pour message de closing diferent
			{
				ExitApp("Bot shutdown completed.");
			}
		}
		 
		private void ExitApp(string message)
		{
			Console.WriteLine(message);
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
			Environment.Exit(0);
		}

		private void SetEvents()
		{
			Console.CancelKeyPress += CancelKey;  //ctrl + C
			Client.GuildMemberAdded += Discord_GuildMemberAdded;
			Client.GuildMemberRemoved += Client_GuildMemberRemoved;
			Client.MessageReactionAdded += Client_MessageReactionAdded;
			Client.MessageReactionRemoved += Client_MessageReactionRemoved;
		}

		private Task Client_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
		{
			if (args.User.IsBot) return Task.CompletedTask;

			if (ItemPoll.TryGetValue(args.Message.Id, out List<ulong> nameID))
			{
				nameID.Add(args.User.Id);
			}

			return Task.CompletedTask;
		}


		private Task Client_MessageReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
		{
			if (args.User.IsBot) return Task.CompletedTask;

			if (ItemPoll.TryGetValue(args.Message.Id, out List<ulong> nameID))
			{
				if (nameID.Contains(args.User.Id)) nameID.Remove(args.User.Id);
			}

			return Task.CompletedTask;
		}

		private void CancelKey(object sender, ConsoleCancelEventArgs e)
		{
			Console.WriteLine("Ctrl + C by user. Closing the bot...");
			e.Cancel = true; // empeche la console fermer auto apres le ctrl+c (permet de faire code en bas et gerer la closing)
							 // do stuff before closing au besoin
			_cts.Cancel(); // cancel le CTS pour creaker le await -1
		}

		private void SetupClient()
        {
			ManageDB.LoadList();

			var config = new DiscordConfiguration
			{
				Token = AppData.token, //obtenu sur le developepr website de discord
				TokenType = TokenType.Bot,
				AutoReconnect = true,
				Intents = DiscordIntents.All,
				MinimumLogLevel = LogLevel.Error
			};

			Client = new DiscordClient(config);

			Console.WriteLine("Bot is connecting to Discord ...");
			//Client.UpdateCurrentUserAsync("botname")

			var commandConfig = new CommandsNextConfiguration
			{
				StringPrefixes = ["!"],
				EnableDms = true,
				EnableMentionPrefix = true,
				EnableDefaultHelp = false,
			};

			//gere les commandes recu via discord
			Commands = Client.UseCommandsNext(commandConfig);
			Commands.RegisterCommands<BotCommands>();
		}

		private async Task Discord_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		{
			if (e.Guild.Id != 1289323361427787808) return;
			if (e.Member.IsBot) return;

			var channel = await sender.GetChannelAsync(1289653768371310625);
			var vexray = await Client.GetUserAsync(114587845716344834);
			var advisor = e.Guild.GetRole(1289660706857287752);
			var guardian = e.Guild.GetRole(1289656595747438652);

			var message = "We are an anti-elitist and non-hardcore guild engaging in both PvE and PvP content, striving to do our best." + Environment.NewLine + "We welcome everyone with a similar mindset who is looking for a friendly and stress-free place." + Environment.NewLine +
				          $"Feel free to contact our GM {vexray.Mention}, Advisors {advisor.Mention} or Guardians {guardian.Mention}.";

			var embed = new DiscordEmbedBuilder()
		   .WithDescription(message)
		   .WithColor(new DiscordColor(191, 255, 0))
		   .WithAuthor("Welcome to Omen!")
		   .WithFooter(" 👋 " + e.Member.Username, e.Member.AvatarUrl);

			await Task.Delay(150);
			await channel.SendMessageAsync(e.Member.Mention, embed);
		}

		private async Task Client_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
		{
			if (args.Guild.Id != 1289323361427787808) return;
			if (args.Member.IsBot) return;

			var guild = args.Guild;
			var leaderChannel = guild.GetChannel(leaderID);
			var vexray = await guild.GetMemberAsync(114587845716344834);
			var ultime = await guild.GetMemberAsync(337741216466862080);

			var user = args.Member.Username;
			var display = args.Member.DisplayName;
			var message = $"UserName: {user}, DisplayName: {display} has left the server";		

			await ultime.SendMessageAsync(message);
			await vexray.SendMessageAsync(message);
			await leaderChannel.SendMessageAsync(message);
		}
	}
}
