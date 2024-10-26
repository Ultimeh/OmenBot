using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

namespace frcqBot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
		CancellationTokenSource _cts = new CancellationTokenSource();

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

			// example de event pratique (code en commantaire plus bas en lier avec ceux ci aussi
			// Client.GuildMemberAdded += Discord_GuildMemberAdded; //event for new user joining the serveur
			// Client.GuildMemberUpdated += Discord_RoleUpdate;  // event for member update (role update)
			// Client.MessageCreated += Discord_NewMessage;  // ereact to some1 message
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
			var config = new DiscordConfiguration
			{
				Token = "discord TOKEN", //obtenu sur le developepr website de discord
				TokenType = TokenType.Bot,
				AutoReconnect = true,
				Intents = DiscordIntents.All,
			};

			Client = new DiscordClient(config);

			Console.WriteLine("Bot is connecting to Discord ...");

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

		//private async Task Discord_NewMessage(DiscordClient sender, MessageCreateEventArgs e)
		//{
		//	if (e.Author.Id == 359393942082551820)
		//	{
		//		await Client.SendMessageAsync(e.Channel, $"blah blah message! qui provient de: {e.Author.Username}");
		//	}
		//}

		//private async Task Discord_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
		//{
		//	//example de message d'accueil a un new user qui connect au serveur

		//	var emo = DiscordEmoji.FromGuildEmote(Client, 658712840198160404);    // 794457218517958726  o7 frcq
		//	var channel = await Client.GetChannelAsync(694123234223980584); // channel accueil

		//	var embed = new DiscordEmbedBuilder()
		//			.WithDescription("Pour commencer je te demanderais simplement de lire attentivement la section <#755519850595483769>. Il est nécessaire de communiquer avec un de nos membres  " +
		//							 "pour avoir accès a <#755528886229401643> et dire un petit mot sur toi au reste de l'équipe." + Environment.NewLine + "Nous sommes malheureusement forcés de procéder ainsi pour éviter les bots et le spamming, merci. " + $"{emo}")
		//			.WithColor(new DiscordColor(191, 255, 0))
		//			.WithAuthor("Bienvenue chez les Feds QC ! ", "https://inara.cz/squadron/6240/", "https://inara.cz/data/wings/logo/6240x3770.png") // image "https://i.imgur.com/E44D9Au.png"
		//			.WithFooter(" 👋 " + e.Member.Username + " #" + e.Member.Discriminator, e.Member.AvatarUrl);

		//	await Task.Delay(150); // delais actificiel pour assurer que le user as generer le channel dans discord
		//	await Client.SendMessageAsync(channel, e.Member.Mention, embed);

		//	string newUser = DateTime.Now + " New User: " + e.Member.DisplayName + " #" + e.Member.Discriminator + " (" + e.Member.Username + ") " + " Joined the Discord Server : automatic welcome message sent to Channel #Accueil.";

		//	Console.WriteLine(newUser);
		//}

		//private async Task Discord_RoleUpdate(DiscordClient sender, GuildMemberUpdateEventArgs e)
		//{
		//	//example de code si changement de role a un user

		//	bool send = true;

		//	if (e.RolesAfter.Count == e.RolesBefore.Count) return;
		//	if (e.RolesAfter.Count < e.RolesBefore.Count) return;
		//	if (e.Member.Roles.Count() > 3) return;

		//	foreach (var item in e.RolesAfter)
		//	{
		//		foreach (DiscordRole roles in e.RolesBefore)
		//		{
		//			if (roles.Id == 592833428207173652 || item.Id == 675992538380763146 || item.Id == 592833348137910275) send = false;
		//		}


		//		if (item.Id == 592833428207173652) //role recrue poour le message
		//		{
		//			foreach (DiscordRole roles in e.RolesBefore)
		//			{
		//				if (roles.Id == 774733480322924566)
		//				{
		//					var accueil = e.Guild.GetRole(774733480322924566);
		//					await e.Member.RevokeRoleAsync(accueil);
		//					break;
		//				}
		//			}
		//		}
		//	}

		//	foreach (var item in e.RolesAfter)
		//	{
		//		if (item.Id == 675992538380763146 || item.Id == 592833348137910275) //role frcq
		//		{
		//			var recrue = e.Guild.GetRole(592833428207173652);

		//			foreach (DiscordRole roles in e.Member.Roles)
		//			{
		//				if (roles == recrue)
		//				{
		//					await e.Member.RevokeRoleAsync(recrue);
		//					break;
		//				}
		//			}
		//		}
		//	}
		//}
	}
}
