using System;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Channels;
using System.Windows.Input;


namespace EgoBot
{
    public static class DI
    {
        public static IServiceProvider Services()
        {
            var services = new ServiceCollection();

            services.AddSingleton<DiscordSocketClient>(provider =>
            {
                var config = new DiscordSocketConfig
                {
                    UseInteractionSnowflakeDate = false,
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages |
                        GatewayIntents.MessageContent
                };

                return new DiscordSocketClient(config);
            });

            services.AddSingleton<InteractionService>(provider =>
            {
                var client = provider.GetRequiredService<DiscordSocketClient>();

                return new InteractionService(client);
            });

            services.AddSingleton<Bot>();
            services.AddSingleton<SlashCommands>();
            services.AddSingleton<PlayerService>();
            services.AddSingleton<PlayerComponents>();
            services.AddSingleton<PlayerModals>();
            services.AddSingleton<EditModalState>();


            return services.BuildServiceProvider();


        }
    }
}
