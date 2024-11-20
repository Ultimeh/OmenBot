using System.Text.Json;

namespace baseBot
{
	public class ManageDB
	{
		string pathDB = @".\data\UserList.db";
		private static readonly object LockSave = new object();

		public void LoadList()
		{
			if (File.Exists(@".\data\token.txt"))
			{
				Bot.AppData.token = File.ReadAllText(@".\data\token.txt");
			}

			if (!Directory.Exists(@".\data")) Directory.CreateDirectory(@".\data");

			if (!File.Exists(pathDB)) return;

			try
			{
				var json = File.ReadAllText(pathDB);
				Bot.AppData.OmenList = JsonSerializer.Deserialize<List<Users>>(json);
				Console.WriteLine("UserList Loaded");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			List<string> help = new List<string>();
			help.Add("List or all commands for the Loot Bot:");
			help.Add("-------------------------------------------------------------");
			help.Add("-Display users per Class: '!class class' exemple: !role paladin");
			help.Add("-Draw random user(s): '!draw number' exemple: !draw 2");
			help.Add("-Draw random user(s) by class: '!draw number class' exemple: !draw 2 paladin");
			help.Add("-Show all curent users and their win count: '!list'");
			help.Add("-Reset all Win counts to 0: '!reset'");
			help.Add("-Request latest team comp for yourself: '!team'");

			Bot.AppData.help = "```" + Environment.NewLine + string.Join(Environment.NewLine, help) + Environment.NewLine + "```";
		}

		public void SaveList()
		{
			lock (LockSave)
			{
				try
				{
					var json = JsonSerializer.Serialize(Bot.AppData.OmenList);
					File.WriteAllText(pathDB, json);
				}
				catch (Exception ex) 
				{
					Console.WriteLine(ex.Message);
				}
			}
		}
	}
}
