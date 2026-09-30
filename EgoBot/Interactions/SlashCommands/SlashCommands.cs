using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Numerics;
using System.Text;
using static System.Net.WebRequestMethods;

namespace EgoBot
{
    public class SlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerService _service;
        private readonly InteractionService _interactions;
        private readonly Bot _bot;
        private readonly PlayerRepo _repo;

        public SlashCommands(PlayerService service, InteractionService interactions, Bot bot, PlayerRepo repo)
        {
            _service = service;
            _interactions = interactions;
            _bot = bot;
            _repo = repo;
        }

        //-------------------------------Console Commands---------------------------

        [SlashCommand("pcontrol", "Implanta o menu de controle de jogador.")]
        public async Task Butt()
        {
            var button = new ComponentBuilder()
                .WithButton("➕ Adicionar", "add", ButtonStyle.Success, row: 0)
                .WithButton("✏️ Editar", "editbutt", ButtonStyle.Primary, row: 0)
                .WithButton("🗑️ Excluir", "delselect", ButtonStyle.Danger, row: 0)
                .WithButton("🐦‍🔥 Transform", "transbutt", ButtonStyle.Secondary, row: 0)

                //Inventory/skill section
                .WithButton("➕ Adicionar skill", $"skills:add", ButtonStyle.Success, row: 1)
                .WithButton("🗑️ ​Remover skill", $"skills:remove", ButtonStyle.Danger, row: 1)

                //Quick Recovery Section
                .WithButton("❤️💯Recuperar Vigor", "qkrecov:vigor", ButtonStyle.Danger, row: 2)
                .WithButton("⚫💯Recuperar Ego", "qkrecov:ego", ButtonStyle.Primary, row: 2)
                .WithButton("🔥💯Recuperar Flow", "qkrecov:flow", ButtonStyle.Success, row: 2)
                .WithButton("💯💯Recuperar Tudo", "qkrecov:all", ButtonStyle.Secondary, row: 2);


            await RespondAsync(components: button.Build());

        }

       
        [SlashCommand("update", "Atualiza todos os jogadores, armas e itens")]
        public async Task _MassUpdate()
        {
            await DeferAsync();

            var players = await _repo.Load();
            
            
            foreach (var player in players)
            {
                await _service.UpdatePlayer(player.Name);
            }
           
            await FollowupAsync("Atualizados", ephemeral: true);
        }

        //---------------------------Player Commands Section----------------------------------------

        [SlashCommand("pshow", "Implanta a carta do jogador. (Atualiza constantemente)")]
        public async Task _ShowPlayer([Autocomplete(typeof(PlayerAutocompleteHandler))] string Jogador)
        {
            Embed? embed = await _service.ShowEmbed(Jogador);
            await RespondAsync(embed: embed);

            IUserMessage sentemb = await GetOriginalResponseAsync();

            await _service.SetMessageID(Jogador, sentemb);
            await _service.UpdatePlayer(Jogador);
        }

        [SlashCommand("psee", "Mostra a carta do jogador. (Não atualiza)")]
        public async Task _SeePlayer([Autocomplete(typeof(PlayerAutocompleteHandler))] string Jogador)
        {
            Embed? embed = await _service.ShowEmbed(Jogador);
            await RespondAsync(embed: embed);
        }

        [SlashCommand("pregistrados", "Mostra todos os jogadores registrados")]
        public async Task _RegPlayers()
        {
            Embed? embed = await _service.RegisteredPlayers();
            if (embed == null)
            {
                await RespondAsync("Sem jogadores");
            }
            else
            {
                await RespondAsync(embed: embed);
            }
        }

        [SlashCommand("help", "Lista todos os comandos")]
        public async Task Help()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Comandos");

            foreach (var command in _interactions.SlashCommands)
            {
                embed.AddField(
                    $"/{command.Name}",
                    command.Description,
                    false);
            }

            await RespondAsync(embed: embed.Build(), ephemeral: true);

        }


        //--------------------------------Quick Change Commands---------------------------

        [SlashCommand("qcpanel", "Implanta o painel de alteração rápida")]
        public async Task _qcpanel([Autocomplete(typeof(PlayerAutocompleteHandler))] string Jogador)
        {
            var button = new ComponentBuilder()
               //Vigor change section
               .WithButton("<<", $"qc:{Jogador}:vg-2", ButtonStyle.Danger, row: 0)
               .WithButton("<", $"qc:{Jogador}:vg-1", ButtonStyle.Danger, row: 0)
               .WithButton(">", $"qc:{Jogador}:vg+1", ButtonStyle.Danger, row: 0)
               .WithButton(">>", $"qc:{Jogador}:vg+2", ButtonStyle.Danger, row: 0)

               //Ego change section
               .WithButton("<<", $"qc:{Jogador}:ego-2", ButtonStyle.Secondary, row: 1)
               .WithButton("<", $"qc:{Jogador}:ego-1", ButtonStyle.Secondary, row: 1)
               .WithButton(">", $"qc:{Jogador}:ego+1", ButtonStyle.Secondary, row: 1)
               .WithButton(">>", $"qc:{Jogador}:ego+2", ButtonStyle.Secondary, row: 1)

               //Flow change section
               .WithButton("<<", $"qc:{Jogador}:flow-2", ButtonStyle.Primary, row: 2)
               .WithButton("<", $"qc:{Jogador}:flow-1", ButtonStyle.Primary, row: 2)
               .WithButton(">", $"qc:{Jogador}:flow+1", ButtonStyle.Primary, row: 2)
               .WithButton(">>", $"qc:{Jogador}:flow+2", ButtonStyle.Primary, row: 2);


            await RespondAsync(components: button.Build(), ephemeral: true);
        }

        [SlashCommand("setvigor", "Define o vigor do jogador")]
        public async Task _SetVigor([Autocomplete(typeof(PlayerAutocompleteHandler))] string Jogador, int Vigor)
        {
            var usuario = Context.User;
            Console.WriteLine($"Comando sethp executado por: {usuario.Username}");
            Console.WriteLine($"{Jogador}jogador, {Vigor}vigor");

            int erchek = await _service.VigSet(Jogador, Vigor);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.", ephemeral: true);
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await RespondAsync($"{Jogador} agora tem {Vigor} de HP.", ephemeral: true);
        }

        [SlashCommand("setego", "Define o ego do jogador")]
        public async Task _SetMP([Autocomplete(typeof(PlayerAutocompleteHandler))]string Jogador, int Ego)
        {
            var usuario = Context.User;
            Console.WriteLine($"Comando setmp executado por: {usuario.Username}");
            Console.WriteLine($"{Jogador}jogador, {Ego}ego");

            int erchek = await _service.EgoSet(Jogador, Ego);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.", ephemeral: true);
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await RespondAsync($"{Jogador} agora tem {Ego} de MP.", ephemeral: true);
        }

        [SlashCommand("setflow", "Define o flow do jogador")]
        public async Task _SetMind([Autocomplete(typeof(PlayerAutocompleteHandler))]string Jogador, int Flow)
        {
            var usuario = Context.User;
            Console.WriteLine($"Comando setmind executado por: {usuario.Username}");
            Console.WriteLine($"{Jogador}jogador, {Flow}flow");

            int erchek = await _service.FlowSet(Jogador, Flow);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.", ephemeral: true);
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await RespondAsync($"{Jogador} agora tem {Flow} de Flow.", ephemeral: true);

        }

        [SlashCommand("vigor", "Altera o vigor do jogador")]
        public async Task _HPChange([Autocomplete(typeof(PlayerAutocompleteHandler))]string Jogador, int Vigor)
        {
            await DeferAsync(ephemeral: true);
            var usuario = Context.User;
            Console.WriteLine($"Comando hp executado por: {usuario.Username}");
            Console.WriteLine($"{Jogador}, {Vigor} vigor");

            int erchek = await _service.VigChange(Jogador, Vigor);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.", ephemeral: true);
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await FollowupAsync($"{Jogador} recebeu {Vigor} de vigor.");

        }

        [SlashCommand("ego", "Altera o ego do jogador")]
        public async Task _MPChange([Autocomplete(typeof(PlayerAutocompleteHandler))]string Jogador, int Ego)
        {
            await DeferAsync(ephemeral: true);
            var usuario = Context.User;
            Console.WriteLine($"Comando mp executado por: {usuario.Username}");
            Console.WriteLine($"{Jogador}jogador, {Ego}ego");

            int erchek = await _service.EgoChange(Jogador, Ego);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.", ephemeral: true);
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await FollowupAsync($"{Jogador} recebeu {Ego} de ego.");
        }

        [SlashCommand("flow", "Altera o flow do jogador")]
        public async Task _MindChange([Autocomplete(typeof(PlayerAutocompleteHandler))]string Jogador, int Flow)
        {
            await DeferAsync(ephemeral: true);
            Console.WriteLine($"{Jogador}jogador, {Flow}flow");
            var usuario = Context.User;
            Console.WriteLine($"Comando mind executado por: {usuario.Username}");

            int erchek = await _service.FlowChange(Jogador, Flow);
            if (erchek == -404)
            {
                await RespondAsync("Jogador não encontrado.");
                return;
            }
            await _service.UpdatePlayer(Jogador);
            await FollowupAsync($"{Jogador} recebeu {Flow} de flow.");
        }
                
        [SlashCommand("healall", "Recupera turo e toros.")]
        public async Task test4()
        {
            await DeferAsync(ephemeral: true);
            List<string> names = new List<string>();

            await _repo.Update(async players =>
            {
                foreach (Player player in players)
                {
                    player.Vigor = player.MaxVigor;
                    player.Ego = player.MaxEgo;
                    player.Flow = 0;
                }

                foreach (Player player in players)
                {
                    names.Add(player.Name);
                }
            });

            foreach (string name in names)
            {
                await _service.UpdatePlayer(name);
            }
            
           

            await FollowupAsync($"Jogadores recuperados completamente!");

        }

        //----------------------------------Auto Complete Handlers---------------------------------

        public class PlayerAutocompleteHandler : AutocompleteHandler
        {
            private readonly PlayerRepo _repo;
            public PlayerAutocompleteHandler(PlayerRepo repo)
            {
                _repo = repo;
            }

            public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
                IInteractionContext context,
                IAutocompleteInteraction autocompleteInteraction,
                IParameterInfo parameter,
                IServiceProvider services)
            {
                var players = await _repo.Load();

                var input = autocompleteInteraction.Data.Current.Value?.ToString() ?? "";

                var suggestions = players
                    .Where(p => p.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
                    .Take(25)
                    .Select(p => new AutocompleteResult(p.Name, p.Name));

                return AutocompletionResult.FromSuccess(suggestions);
            }
        }
                       
    }

}
