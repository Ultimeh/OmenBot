using baseBot;

namespace frcqBot
{
    class Program
    {
        //public static AppData appData = new AppData(); //class static pour global variable si besoin

        static async Task Main(string[] args) //string[] args peu etre utile pour argument au demarage:   bot.exe arg1 arg2 arg3
		{
			Bot bot = new Bot();
            Console.WriteLine("Bot is starting ...");
            await bot.StartBot();
		}
    }
}