using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace baseBot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }
		public static AppData AppData { get; set; } = new AppData();
		public static ManageDB ManageDB { get; set; } = new ManageDB();
		public CommandsNextExtension Commands { get; private set; }
		CancellationTokenSource _cts = new CancellationTokenSource();

		public async Task StartBot()
		{
			SetupClient();

			try
			{
				ManageDB.LoadList();

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
				Token = AppData.token, //obtenu sur le developepr website de discord
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
	}
}
