using System.Security.Cryptography.X509Certificates;

namespace baseBot
{
	public class AppData
	{
		public Dictionary<string, int> UserList { get; set; } = new Dictionary<string, int>();
		public string help = "";
		public string token = "";
	}
}
