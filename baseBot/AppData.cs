using System.Collections.Concurrent;

namespace baseBot
{
	public class AppData
	{
		//public Dictionary<string, int> UserList { get; set; } = new Dictionary<string, int>();
		public List<Users> OmenList { get; set; } = new List<Users>();
		public ConcurrentBag<Task> PollTask = new ConcurrentBag<Task>();

		public CancellationTokenSource cts = new CancellationTokenSource();

		public List<string> Roles = new List<string>()
		{
			"scorpion",
			"outrider",
			"raider",
			"scout",
			"battleweaver",
			"fury",
			"paladin",
			"ravager",
			"crusader",
			"ranger",
			"sentinel",
			"berserker",
			"warden",
			"disciple",
			"templar",
			"infiltrator",
			"liberator",
			"seeker",
			"spellblade",
			"invocator",
			"darkblighter"
		};

		public string help = "";
		public string token = "";

		public ConcurrentDictionary<ulong, List<ulong>> ItemPoll = new ConcurrentDictionary<ulong, List<ulong>>();
		public ConcurrentDictionary<ulong, TimeSpan> TimePoll { get; set; } = new ConcurrentDictionary<ulong, TimeSpan>();
	}

	public class Users
	{
		public string Name { get; set; }
		public string Role { get; set; }
		public int WinCount { get; set; } = 0;
	}
}
