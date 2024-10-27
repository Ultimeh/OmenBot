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

			if (!File.Exists(pathDB))
			{
				Console.WriteLine("No UserList file found");

				if (File.Exists(@".\data\default.txt"))
				{
					try
					{
						var lines = File.ReadLines(@".\data\default.txt");

						foreach (var line in lines)
						{
							var data = line.Split('-');
							Bot.AppData.OmenList.Add(new Users { Name = data[0], Role = data[1] });
						}

						Bot.ManageDB.SaveList();
						Console.WriteLine("Loaded default list");
					}

					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);
					}
				}

				return;
			}

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
			help.Add("Add new user: '!add name role' exemple: !add bob melee (tank, healer, melee, range, melee/range)");
			help.Add("Delete existing user: '!del name' exemple: !del bob");	
			help.Add("Display users per Role: '!role role' exemple: !role tank (tank, healer, melee, range, melee/range)");
			help.Add("Change user Role: '!edit name role' exemple: !edit bob healer (tank, healer, melee, range, melee/range)");
			help.Add("Draw random user(s): '!draw number' exemple: !draw 2 ('!draw' without a number will be '1' by default)");
			help.Add("Show all curent users and their win count: '!list'");
			help.Add("Reset all Win counts to 0: '!reset'");
			help.Add("Delete message(s): '!purges number' exemple: !purges 5");

			Bot.AppData.help = string.Join(Environment.NewLine, help);
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
