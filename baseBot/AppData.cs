using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace baseBot
{
	public class AppData
	{
		//public Dictionary<string, int> UserList { get; set; } = new Dictionary<string, int>();
		public List<Users> OmenList { get; set; } = new List<Users>();

		public List<string> Roles = new List<string>() { "tank", "healer", "melee", "range", "m/r" };

		public string help = "";
		public string token = "";
	}

	public class Users
	{
		public string Name { get; set; }
		public string Role { get; set; }
		public int WinCount { get; set; } = 0;
	}
}
