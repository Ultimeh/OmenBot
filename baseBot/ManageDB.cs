using System.Collections.Concurrent;
using System.Text.Json;

namespace baseBot
{
	public class ManageDB
	{
		string pathDB = @".\data\UserList.db";
		string PathTimePoll = @".\data\TimePoll.db";
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

			List<string> help =
			[
				"List or all commands for the Loot Bot:",
				"-------------------------------------------------------------",
				"-Display users per Class: '!class class' exemple: !role paladin",
				"-Draw random user(s): '!draw number' exemple: !draw 2",
				"-Draw random user(s) by class: '!draw number class' exemple: !draw 2 paladin",
				"-Show all curent users and their win count: '!list'",
				"-Reset all Win counts to 0: '!reset'",
				"-Request latest team comp for yourself: '!team'",
				"-See current item poll : '!item'",
			];

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

		public void SaveItemPoll()
		{
			try
			{
				var json = JsonSerializer.Serialize(Bot.AppData.TimePoll);
				File.WriteAllText(PathTimePoll, json);
				Console.WriteLine(Bot.AppData.TimePoll.Count + " Item polls saved");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}

		public void LoadItemPoll()
		{
			try
			{
				var json = File.ReadAllText(PathTimePoll);
				Bot.AppData.TimePoll = JsonSerializer.Deserialize<ConcurrentDictionary<ulong, TimeSpan>>(json);
				Console.WriteLine("Item polls / time Loaded");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
