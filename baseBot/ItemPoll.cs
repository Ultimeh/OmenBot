using DSharpPlus.Entities;
using System.Diagnostics;

namespace baseBot
{
	public class ItemPoll
	{
		DiscordMessage _message;
		Task _task;

		public ItemPoll(DiscordMessage msg, TimeSpan threshold)
		{
			_message = msg;
			_task = Task.Run(() => ManagePoll(threshold));
			Bot.AppData.PollTask.Add(_task);
		}

		private async Task ManagePoll(TimeSpan threshold)
		{
			bool stop = false;
			bool sendNotification = false;
	
			PeriodicTimer timeCheck = new PeriodicTimer(TimeSpan.FromSeconds(10));
			Stopwatch stopwatchElapsed = Stopwatch.StartNew();
			Stopwatch stopwatchUpdate = Stopwatch.StartNew();

			TimeSpan remainingTime = TimeSpan.Zero;
			TimeSpan elapsed;

			try
			{
				while (await timeCheck.WaitForNextTickAsync(Bot.AppData.cts.Token))
				{
					elapsed = stopwatchElapsed.Elapsed;
					remainingTime = threshold - elapsed;

					if (remainingTime <= TimeSpan.Zero)
					{
						DiscordMember winner = await RandomCheck();
						string line;

						if (winner != null)
						{
							sendNotification = true;
							line = $"**The Winner is:** {winner.Mention}";
						}
						else line = "**A winner has not been chosen. No bidders!**";

						await UpdateMessage(line, true);
						break;
					}

					if (stopwatchUpdate.Elapsed >= TimeSpan.FromMinutes(2))
					{
						string timeMsg = RemainingTime(remainingTime);
						stop = await UpdateMessage(timeMsg);
						if (stop) break;
						stopwatchUpdate.Restart();
					}
				}
			}
			catch (TaskCanceledException)
			{
				stopwatchElapsed.Stop();
				stopwatchUpdate.Stop();
				timeCheck.Dispose();

				Bot.AppData.TimePoll.TryAdd(_message.Id, remainingTime);
			}

			if (sendNotification)
			{
				var guild = _message.Channel.Guild;
				var vexray = await guild.GetMemberAsync(114587845716344834);
				await vexray.SendMessageAsync("An item poll ended with a winner.");
			}

			stopwatchElapsed.Stop();
			stopwatchUpdate.Stop();
			timeCheck.Dispose();
			Bot.AppData.PollTask.TryTake(out _task);
		}

		private string RemainingTime(TimeSpan remainingTime)
		{
			if (remainingTime.Days > 0) return $"{remainingTime.Days} days {remainingTime.Hours} hours {remainingTime.Minutes} minutes remain until drawing";
			if (remainingTime.Hours > 0) return $"{remainingTime.Hours} hours {remainingTime.Minutes} minutes remain until drawing";
			if (remainingTime.Minutes >= 1) return $"{remainingTime.Minutes} minutes remain until drawing";
			return $"{remainingTime.Seconds} seconds remain until drawing";
		}

		private async Task<bool> UpdateMessage(string msg, bool end = false)
		{
			string content = _message.Content;
			string[] lines = content.Split('\n');
			lines[^1] = msg;

			if (end) lines[0] = "**Poll is over!**";

			try
			{
				await _message.ModifyAsync(string.Join('\n', lines));
				return false;
			}
			catch (Exception)
			{
				Console.WriteLine($"Problem to update an item poll message, stopping ID: {_message.Id}" + Environment.NewLine + _message.Content + Environment.NewLine);
				Bot.AppData.ItemPoll.TryRemove(_message.Id, out _);
				return true;
			}
		}

		private async Task<DiscordMember> RandomCheck()
		{
			var omen = await Bot.Client.GetGuildAsync(1289323361427787808);
			List<DiscordMember> list = new List<DiscordMember>();

			if (Bot.AppData.ItemPoll.TryGetValue(_message.Id, out List<ulong> id))
			{
				foreach (var item in id)
				{
					list.Add(await omen.GetMemberAsync(item));
				}
			}

			Bot.AppData	.ItemPoll.TryRemove(_message.Id, out _);

			if (list.Count != 0)
			{
				lock (Bot.LockRandom)
				{
					return list[Bot.Random.Next(list.Count)];
				}		
			}

			return null;
		}
	}
}
