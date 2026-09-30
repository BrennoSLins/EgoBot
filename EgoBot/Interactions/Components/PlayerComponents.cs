using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Markup;
using System.Xml.Linq;
using static EgoBot.PlayerModals;

namespace EgoBot
{
    public class PlayerComponents : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerService _service;
        private readonly InteractionService _interactions;
        private readonly PlayerModals _modals;
        private readonly EditModalState _modalstate;
        private readonly Bot _bot;
        private readonly PlayerRepo _repo;
        

        public PlayerComponents(PlayerService service, InteractionService interactions, PlayerModals modals, EditModalState modalstate, Bot bot, PlayerRepo repo)
        {
            _service = service;
            _interactions = interactions;
            _modals = modals;
            _modalstate = modalstate;
            _bot = bot;
            _repo = repo;
        }

        //--------------------------------Player management section-----------------------------

        [ComponentInteraction("add")]
        public async Task _add()
        {
            await RespondWithModalAsync<CreatePlayerModal>("create_player");
        }

        //Edit button interactions

        [ComponentInteraction("editbutt")]
        public async Task _editt()
        {

            var button = new ComponentBuilder()
               .WithButton("✏️ Renomear/Alterar atributos", $"edit:rename", ButtonStyle.Primary)
               .WithButton("​📝​ Atribuir informações extras", $"edit:info", ButtonStyle.Primary)
               .WithButton("🖼 Alterar imagem", $"edit:altpic", ButtonStyle.Primary);


            await RespondAsync(components: button.Build(), ephemeral: true);


        }

        [ComponentInteraction("edit:*")]
        public async Task _edittomodal(string acao)
        {
            switch (acao)
            {
                case "rename":
                    Console.WriteLine($"edit:{acao}");
                    await RespondWithModalAsync<EditPlayerModal>("medit");
                    break;
            

                case "info":

                    var button = new ComponentBuilder()
                    .WithButton("➕ Adicionar informações", $"editinfo:add", ButtonStyle.Success)
                    .WithButton("🗑️ ​Remover informações", $"editinfo:remove", ButtonStyle.Danger);
                    

                    await RespondAsync(components: button.Build(), ephemeral: true);
                    break;

                case "altpic":
                    await RespondWithModalAsync<AltPicModal>("maltpic");
                    break;
            }
        }

        [ComponentInteraction("editinfo:*")]
        public async Task _editinfo(string acao)
        {
            switch (acao)
            {
                case "add":

                    await RespondWithModalAsync<AddStatusModal>("maddinfo");
                    break;

                case "remove":

                    var players = await _repo.Load();

                    Console.WriteLine("FUCKYOU");
                    var menu = new SelectMenuBuilder()
                .WithCustomId($"editinfo:{acao}:p")
                .WithPlaceholder("Escolha um jogador");

                    foreach (Player player in players)
                    {
                        menu.AddOption($"{player.Name}", $"{player.Name}");
                    }

                    var components = new ComponentBuilder()
                .WithSelectMenu(menu);

                    await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
                    break;
            }
        }

        [ComponentInteraction("editinfo:remove:p")]
        public async Task _editinfo(string[] values)
        {
            string jogador = values[0];
            var players = await _repo.Load();
            Player? player = await _service.GetPlayer(jogador);

            _modalstate.Set(Context.User.Id, new List<string> { jogador, "", "", "", "" });
            
            var menu = new SelectMenuBuilder()
        .WithCustomId($"removeinfo:def")
        .WithPlaceholder("Escolha a informação");

            foreach (string info in player.Status)
            {
                menu.AddOption($"{info}", $"{info}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha a informação:", components: components.Build(), ephemeral: true);

        }

        [ComponentInteraction("removeinfo:def")]
        public async Task _removeinfodef(string[] values)
        {
            await DeferAsync();


            string playername = "";
            string info = values[0];
            _modalstate.TryGet(Context.User.Id, out List<string> modlist);

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(modlist[0], StringComparison.OrdinalIgnoreCase));

                if (player.Status.Contains(info))
                {
                    player.Status.Remove(info);
                }

                string playername = player.Name;
                return Task.CompletedTask;
            });
           
                        
            await _service.UpdatePlayer(playername);
            _modalstate.Remove(Context.User.Id);

            await FollowupAsync($"Jogador {playername} editado com sucesso", ephemeral: true);

        }

        [ComponentInteraction("editselect")]
        public async Task _componenttofunc(string[] values)
        {
            string jogador = values[0];
            Console.WriteLine("editselected");

            _modalstate.TryGet(Context.User.Id, out List<string> modallist);
            
            await _service.EditPlayer(modallist, jogador);
            await _service.UpdatePlayer(jogador);
            _modalstate.Remove(Context.User.Id);

            await RespondAsync($"Jogador {jogador} editado com sucesso", ephemeral: true);

        }

        [ComponentInteraction("addinfoselect")]
        public async Task _addinfoselect(string[] values)
        {
            string jogador = values[0];
            _modalstate.TryGet(Context.User.Id, out List<string> modallist);

            await _service.AddStatus(modallist, jogador);
            await _service.UpdatePlayer(jogador);
            _modalstate.Remove(Context.User.Id);

            await RespondAsync($"Informações adicionados ao jogador {jogador}.", ephemeral: true);
        }

      
        //Delete button interactions

        [ComponentInteraction("delselect")]
        public async Task _delete()
        {
            var players = await _repo.Load();

            var menu = new SelectMenuBuilder()
        .WithCustomId("del_player")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador para editar:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("del_player")]
        public async Task _delplayer(string[] values)
        {
            string jogador = values[0];
            await _service.DeletePlayer(jogador);
            await RespondAsync($"Jogador {jogador} deletado com sucesso.", ephemeral: true);
        }

        //Alt pic button interactions

        [ComponentInteraction("altpicselect")]
        public async Task _altpicselect(string[] values)
        {
            string jogador = values[0];
            _modalstate.TryGet(Context.User.Id, out List<string> modallist);

            await DeferAsync();
            await _service.AltPlayerPic(modallist, jogador);
            await _service.UpdatePlayer(jogador);
            _modalstate.Remove(Context.User.Id);

            await FollowupAsync($"Imagem do jogador {jogador} alterada com sucesso.", ephemeral: true);
        }

        [ComponentInteraction("close")]
        public async Task _close()
        {
            await RespondAsync("Close button clicked!", ephemeral: true);
        }

        //--------------------------Quick Change Section--------------------------

        [ComponentInteraction("qc:*:*")]
        public async Task _qkchange(string jogador, string acao)
        {
            await DeferAsync();
            switch (acao)
            {
                //Vigor cases
                case "vg-2":
                    
                    await _service.VigChange(jogador, -2);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "vg-1":
                    
                    await _service.VigChange(jogador, -1);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "vg+1":
                    
                    await _service.VigChange(jogador, 1);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "vg+2":
                    
                    await _service.VigChange(jogador, 2);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                
                //Ego cases
                case "ego-2":
                    
                    await _service.EgoChange(jogador, -2);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "ego-1":
                    
                    await _service.EgoChange(jogador, -1);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "ego+1": 
                    
                    await _service.EgoChange(jogador, 1);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "ego+2":
                    
                    await _service.EgoChange(jogador, 2);
                    await _service.UpdatePlayer(jogador);
                    
                    break;

                //Flow cases
                case "flow-2":
                    
                    await _service.FlowChange(jogador, -20);
                    await _service.UpdatePlayer(jogador);
                   
                    break;
                case "flow-1":
                    
                    await _service.FlowChange(jogador, -10);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "flow+1":
                   
                    await _service.FlowChange(jogador, 10);
                    await _service.UpdatePlayer(jogador);
                    
                    break;
                case "flow+2":
                    
                    await _service.FlowChange(jogador, 20);
                    await _service.UpdatePlayer(jogador);
                    
                    break;

            }
            Console.WriteLine($"QCPanel used by {Context.User.GlobalName}, alterou {jogador} , ação {acao}");
           
        }
                      
        
        //--------------------------Quick Recovery Section--------------------------

        [ComponentInteraction("qkrecov:vigor")]
        public async Task _qkrecover()
        {
            var players = await _repo.Load();

            Console.WriteLine("FUCKYOU");
            var menu = new SelectMenuBuilder()
        .WithCustomId($"qkrecover:vigor")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("qkrecov:ego")]
        public async Task _qkrecovermp()
        {
            var players = await _repo.Load();

            Console.WriteLine("FUCKYOU");
            var menu = new SelectMenuBuilder()
        .WithCustomId($"qkrecover:ego")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("qkrecov:flow")]
        public async Task _qkrecovermind()
        {
            var players = await _repo.Load();

            Console.WriteLine("FUCKYOU");
            var menu = new SelectMenuBuilder()
        .WithCustomId($"qkrecover:flow")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("qkrecov:all")]
        public async Task _qkrecoverall()
        {
            var players = await _repo.Load();
                        
            var menu = new SelectMenuBuilder()
        .WithCustomId($"qkrecover:all")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("qkrecover:all")]
        public async Task _qkrecoverallmind(string[] values)
        {
            string jogador = values[0];

            await DeferAsync();

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(jogador, StringComparison.OrdinalIgnoreCase));

                player.Vigor = player.MaxVigor;
                player.Ego = player.MaxEgo;
                player.Flow = 0;

                return Task.CompletedTask;
            });
                        
            await _service.UpdatePlayer(jogador);

            await FollowupAsync($"Jogador {jogador} recuperou completamente!", ephemeral: true);

        }

       
        //--------------------------Transform Section--------------------------


        [ComponentInteraction("transbutt")]
        public async Task _qktransselect()
        {
            var players = await _repo.Load();

            Console.WriteLine("FUCKYOU");
            var menu = new SelectMenuBuilder()
            .WithCustomId($"transform")
            .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
            .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }

        [ComponentInteraction("transform")]
        public async Task _transform(string[] values)
        {
            string jogador = values[0];
            Player? player = await _service.GetPlayer(jogador);

            await DeferAsync();
            int tcheck = await _service.Transform(jogador);

            if (tcheck == 3)
            {
                await _service.UpdatePlayer(jogador);
                await FollowupAsync($"{jogador} não tem transformação.", ephemeral: true);
            }
            else if (tcheck == 1)
            {
                await _service.UpdatePlayer(jogador);
                await FollowupAsync($"{jogador} transformado!", ephemeral: true);
            }
            else if (tcheck == 0)
            {
                await _service.UpdatePlayer(jogador);
                await FollowupAsync($"{jogador} teve a transformação revertida!", ephemeral: true);
            }
        }

        
    }
}
