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

			TimeSpan remainingTime = TimeSpan.Zero;

			PeriodicTimer timeCheck = new PeriodicTimer(TimeSpan.FromSeconds(10));
			Stopwatch stopwatchElapsed = Stopwatch.StartNew();
			Stopwatch stopwatchUpdate = Stopwatch.StartNew();

			remainingTime = threshold - stopwatchElapsed.Elapsed;

			try
			{
				while (await timeCheck.WaitForNextTickAsync(Bot.AppData.cts.Token))
				{
					remainingTime = threshold - stopwatchElapsed.Elapsed;

					if (remainingTime <= TimeSpan.Zero)
					{
						var winner = await RandomCheck();
						string line;

						if (winner != null)
						{
							sendNotification = true;
							line = $"**The Winner is:** {winner}";
						}
						else line = "**A winner has not been chosen. No bidders!**";

						await UpdateMessage(line, true);
						break;
					}

					if (stopwatchUpdate.Elapsed >= TimeSpan.FromMinutes(2))
					{
						stop = await UpdateMessage(RemainingTime(remainingTime));
						if (stop) break;
						stopwatchUpdate.Restart();
					}
				}
			}
			catch (OperationCanceledException)
			{
				Bot.AppData.TimePoll.TryAdd(_message.Id, remainingTime);
				stopwatchElapsed.Stop();
				stopwatchUpdate.Stop();
				timeCheck.Dispose();
				return;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			if (sendNotification)
			{
				var guild = _message.Channel.Guild;
				var vexray = await guild.GetMemberAsync(114587845716344834);
				var zestra = await guild.GetMemberAsync(138888245235679232);
				var msg = "An item poll ended with a winner.";

				await vexray.SendMessageAsync(msg);
				await zestra.SendMessageAsync(msg);
			}

			stopwatchElapsed.Stop();
			stopwatchUpdate.Stop();
			timeCheck.Dispose();
			Bot.AppData.ItemPoll.TryRemove(_message.Id, out _);
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

		private async Task<string> RandomCheck()
		{
			if (Bot.AppData.ItemPoll.TryGetValue(_message.Id, out List<ulong> idList))
			{
				if (idList.Count == 0) return null;

				ulong winID = 0;

				lock (Bot.LockRandom)
				{
					winID = idList[Bot.Random.Next(idList.Count)];
				}

				var omen = await Bot.Client.GetGuildAsync(1289323361427787808);

				for (int i = 0; i < idList.Count; i++)
				{
					try
					{				
						var winner = await omen.GetMemberAsync(winID);
						return $"**{winner.DisplayName}**";
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex.Message);

						if (idList.Count - 1 != i)
						{
							lock (Bot.LockRandom)
							{
								winID = idList[Bot.Random.Next(idList.Count)];
							}
						}
					}
				}
			}

			return null;
		}
	}
}
