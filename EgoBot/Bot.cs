using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using EgoBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Windows.Input;

public class Bot
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceProvider _services;
    private readonly SlashCommands _commands;

    public Bot(
        DiscordSocketClient client,
        InteractionService interactions,
        IServiceProvider services)
    {
        _client = client;
        _interactions = interactions;
        _services = services;
    }


    public async Task StartAsync()
    {

        _client.Log += Log;
        _client.Ready += Ready;
        _client.InteractionCreated += HandleInteraction;


        Console.WriteLine("Conectando...");

        string json = File.ReadAllText("D:\\Work\\Projects\\EgoBot\\EgoBot\\Config\\config.json");
        Config config = JsonSerializer.Deserialize<Config>(json)!;


        await _client.LoginAsync(TokenType.Bot, config.Token);
        
        await _interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);

        await _client.StartAsync();

      

        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg);
        return Task.CompletedTask;
    }

    private async Task Ready()
    {
        Console.WriteLine($"Bot online! ({_client.CurrentUser.Username})");
        //await _interactions.RegisterCommandsGloballyAsync();
        //Console.WriteLine($"{DateTime.Now:HH:mm:ss}Comandos registrados globalmente!");
        
        await _interactions.RegisterCommandsToGuildAsync(543844510317150259);
        await _interactions.RegisterCommandsToGuildAsync(1515097459536298134);


    }

    private async Task HandleInteraction(SocketInteraction interaction)
    {

        try
        {

            if (interaction is SocketMessageComponent component)
            {
                var usuario = component.User;

                Console.WriteLine($"Usuário: {usuario.Username}");
                Console.WriteLine($"ID: {usuario.Id}");
                Console.WriteLine(DateTime.Now.ToString());
            }


            Console.WriteLine($"Handle Interaction arrived! {interaction.Type}");
                var context = new SocketInteractionContext(_client, interaction);


                
                Console.WriteLine("Execute command async finished!");
            var result = await _interactions.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                Console.WriteLine($"ERRO: {result.Error}");
                Console.WriteLine($"MOTIVO: {result.ErrorReason}");
            }



        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        
    }

    public async Task<IMessageChannel> GetMessageChannelID(ulong channelID)
    {

        var channel = _client.GetChannel(channelID) as IMessageChannel;

        if (channel == null)
            return null;
        return channel;
    }

    /* public async Task SendSitAtual(string mensagem)
    {
        Console.WriteLine("Huh?");
        string json = File.ReadAllText("D:\\Work\\Projects\\EgoBot\\EgoBot\\Config\\config.json");
        Config config = JsonSerializer.Deserialize<Config>(json)!;


        var channel = _client.GetChannel(config.SitAtualId) as IMessageChannel;
        Console.WriteLine($"Its:{config.SitAtualId}");

        if (channel != null)
        {
            Console.WriteLine("Fuck");
            await channel.SendMessageAsync(mensagem);
        }


    }

    public async Task SendMessage(string? text = null, Embed? embed = null, MessageComponent? components = null)
    {
        var channel = _client.GetChannel() as IMessageChannel;

        if (channel == null)
            return;

        await channel.SendMessageAsync(
            text: text,
            embed: embed,
            components: components
        );
    }
    public async Task SendPlayerRequest(string? text = null, Embed? embed = null, MessageComponent? components = null)
    {
        Console.WriteLine("Huh?");
        string json = File.ReadAllText("D:\\Work\\Projects\\Shel MeBotDB\\Shel MeBotDB\\Config\\config.json");
        Config config = JsonSerializer.Deserialize<Config>(json)!;


        var channel = _client.GetChannel(config.PlayerRequestId) as IMessageChannel;
        Console.WriteLine($"Its:{config.PlayerRequestId}");

        if (channel != null)
        {
            Console.WriteLine("Fuck");
            await channel.SendMessageAsync(
            text: text,
            embed: embed,
            components: components
        );
        }


    }
    */
}

