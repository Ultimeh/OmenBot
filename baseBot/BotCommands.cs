using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace frcqBot
{
    public class BotCommands : BaseCommandModule
    {
		//example de commands de base
		// await ctx.Member.SendMessageAsync("test", null);  PM message
		//await ctx.Channel.SendMessageAsync(ctx.Member.DisplayName + ": " + "non"); message channel

        // avoir un autre channel pour send msg dans autre channel que le CTX,
		//var channel = ctx.Client.GetChannelAsync(Discordt channel ID);
        //await ctx.Client.SendMessageAsync(channel, "message");

		[Command("aide")]
        [Description("Liste des commandes en PM")]
        public async Task aide(CommandContext ctx)
        {
            if (ctx.Channel.Id == 691207717842321409 || ctx.Channel.Id == 908802539519103007) await ctx.Member.SendMessageAsync("test", null);
            if (ctx.Channel.Id == 812138366237278270) await ctx.Member.SendMessageAsync("test", null);
        }

        [Command("purges")]
        [Description("pour supprimer des messages en masse")]
        public async Task purge(CommandContext ctx, int del)
        {
            if (ctx.Channel.Id == 803487353288917010 || ctx.Channel.Id == 812138366237278270 || ctx.Channel.Id == 691207717842321409 || ctx.Channel.Id == 908802539519103007)
            {
                var messages = await ctx.Channel.GetMessagesAsync(del + 1);
                await ctx.Channel.DeleteMessagesAsync(messages);
            }
        }

		[Command("pass")]
        [Description("add passenger entry")]
        public async Task addPass(CommandContext ctx, [Description("System")] string sys, [Description("inf")] string infs, [RemainingText, Description("faction")] string faction)
        {
			if (ctx.Channel.Id != 691207717842321409) return;

			if (string.IsNullOrEmpty(faction))
			{
				await ctx.Channel.SendMessageAsync(ctx.Member.DisplayName + ": " + "non");
				return;
			}

			if (!int.TryParse(infs, out int inf))
			{
				await ctx.Channel.SendMessageAsync(ctx.Member.DisplayName + ": " + "non");
				return;
			}

			// tache a faire avec les info recu (a été enlever pour example de commande)
			
			await ctx.Channel.SendMessageAsync(ctx.Member.DisplayName + ": " + "message");
		}
	}
}
