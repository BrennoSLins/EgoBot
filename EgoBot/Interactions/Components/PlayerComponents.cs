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
        

        public PlayerComponents(PlayerService service, InteractionService interactions, PlayerModals modals, EditModalState modalstate, Bot bot)
        {
            _service = service;
            _interactions = interactions;
            _modals = modals;
            _modalstate = modalstate;
            _bot = bot;
            
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

                    var players = await PlayerRepo.Load();

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
            var players = await PlayerRepo.Load();
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

            string info = values[0];
            _modalstate.TryGet(Context.User.Id, out List<string> modlist);
           
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name.Equals(modlist[0], StringComparison.OrdinalIgnoreCase));

            if (player.Status.Contains(info))
            {
                player.Status.Remove(info);
            }

            await PlayerRepo.Save(players);
            await _service.UpdatePlayer(player.Name);
            _modalstate.Remove(Context.User.Id);

            await FollowupAsync($"Jogador {player.Name} editado com sucesso", ephemeral: true);

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

               
        //Skill buttons

        [ComponentInteraction("skills:*")]
        public async Task _skills(string acao)
        {
            switch (acao)
            {
                case "add":
                    var players = await PlayerRepo.Load();

                    var menu = new SelectMenuBuilder()
                    .WithCustomId($"skills:{acao}:select")
                    .WithPlaceholder("Selecione o jogador");

                    foreach (Player player in players)
                    {
                        menu.AddOption($"{player.Name}", $"{player.Name}");
                    }
                    
                    var components = new ComponentBuilder()
                    .WithSelectMenu(menu);

                    await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
                    break;

                case "remove":
                    var rplayers = await PlayerRepo.Load();

                    var rmenu = new SelectMenuBuilder()
                    .WithCustomId($"skills:{acao}:select")
                    .WithPlaceholder("Selecione o jogador");

                    Console.WriteLine($"skills:{acao}:select");

                    foreach (Player player in rplayers)
                    {
                        rmenu.AddOption($"{player.Name}", $"{player.Name}");
                    }

                    var rcomponents = new ComponentBuilder()
                    .WithSelectMenu(rmenu);

                    await RespondAsync("Escolha um jogador:", components: rcomponents.Build(), ephemeral: true);
                    break;
            }
                
        }

        [ComponentInteraction("skills:add:select")]
        public async Task _skilladdselect(string[] values)
        {
            string jogador = values[0];
            _modalstate.Set(Context.User.Id, new List<string> { jogador, "", "", "", "" });
            await RespondWithModalAsync<CreateSkillModal>("create_skill");

        }

        [ComponentInteraction("skills:remove:select")]
        public async Task _skillremoveselect(string[] values)
        {
            string jogador = values[0];
            _modalstate.Set(Context.User.Id, new List<string> { jogador, "", "", "", "" });

            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name == jogador);

            var rmenu = new SelectMenuBuilder()
                    .WithCustomId($"skillsremovefinal")
                    .WithPlaceholder("Selecione a skill");

            foreach (Skill skill in player.Skills)
            {
                rmenu.AddOption($"{skill.Name}", $"{skill.Name}");
            }

            var rcomponents = new ComponentBuilder()
            .WithSelectMenu(rmenu);

            await RespondAsync("Escolha uma skill:", components: rcomponents.Build(), ephemeral: true);


        }

        [ComponentInteraction("skillsremovefinal")]
        public async Task _skillfinalremov(string[] values)
        {
            _modalstate.TryGet(Context.User.Id, out List<string> modlist);
            string chosenskill = values[0];
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name == modlist[0]);

            var deleteskill = player.Skills.FirstOrDefault(p => p.Name == chosenskill);

            player.Skills.Remove(deleteskill);
            await PlayerRepo.Save(players);
            _modalstate.Remove(Context.User.Id);

            await RespondAsync($"Skill {chosenskill} removida de {modlist[0]}");

        }



        //Delete button interactions

        [ComponentInteraction("delselect")]
        public async Task _delete()
        {
            var players = await PlayerRepo.Load();

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

        /*[ComponentInteraction("qkchanger:*:*:*")]
        public async Task _qkchange(string acao, string jogador, string valor)
        {
            switch (acao)
            {
                case "hp":
                    await DeferAsync();
                    Player player = await _service.GetPlayer(jogador);
                    await _service.VigChange(jogador, int.Parse(valor));
                    await _service.UpdatePlayer(jogador);
                    await FollowupAsync($"{jogador} recebeu {valor} de vigor. {player.Vigor} / {player.MaxVigor}", ephemeral: true);
                    break;
                case "mp":
                    await DeferAsync();
                    Player mplayer = await _service.GetPlayer(jogador);
                    await _service.EgoChange(jogador, int.Parse(valor));
                    await _service.UpdatePlayer(jogador);
                    await FollowupAsync($"{jogador} recebeu {valor} de ego. {mplayer.Ego} / {mplayer.MaxEgo}", ephemeral: true);
                    break;
                case "mind":
                    await DeferAsync();
                    Player miplayer = await _service.GetPlayer(jogador);
                    await _service.FlowChange(jogador, int.Parse(valor));
                    await _service.UpdatePlayer(jogador);
                    await FollowupAsync($"{jogador} recebeu {valor} de flow. {miplayer.Flow}", ephemeral: true);
                    break;
            }
        }



        [ComponentInteraction("qkchanger:hp")]
        public async Task _qkchangehp(string[] values)
        {
            string jogador = values[0];
            Player player = await _service.GetPlayer(jogador);

            var button = new ComponentBuilder()
               .WithButton("<<", $"qkchanger:hp:{jogador}:-2", ButtonStyle.Primary)
               .WithButton("​<", $"qkchanger:hp:{jogador}:-1", ButtonStyle.Primary)
               .WithButton(">", $"qkchanger:hp:{jogador}:1", ButtonStyle.Primary)
               .WithButton(">>", $"qkchanger:hp:{jogador}:2", ButtonStyle.Primary);


            await RespondAsync($"Jogador: {jogador} (Vigor antes da troca: {player.Vigor})", components: button.Build(), ephemeral: true);


        }

        [ComponentInteraction("qkchanger:mp")]
        public async Task _qkchangemp(string[] values)
        {
            string jogador = values[0];
            Player player = await _service.GetPlayer(jogador);

            var button = new ComponentBuilder()
               .WithButton("<<", $"qkchanger:mp:{jogador}:-2", ButtonStyle.Primary)
               .WithButton("​<", $"qkchanger:mp:{jogador}:-1", ButtonStyle.Primary)
               .WithButton(">", $"qkchanger:mp:{jogador}:1", ButtonStyle.Primary)
               .WithButton(">>", $"qkchanger:mp:{jogador}:2", ButtonStyle.Primary);


            await RespondAsync($"Jogador: {jogador} (Ego antes da troca: {player.Ego})", components: button.Build(), ephemeral: true);


        }

        [ComponentInteraction("qkchanger:mind")]
        public async Task _qkchangemind(string[] values)
        {
            string jogador = values[0];
            Player player = await _service.GetPlayer(jogador);

            var button = new ComponentBuilder()
               .WithButton("<<", $"qkchanger:mind:{jogador}:-2", ButtonStyle.Primary)
               .WithButton("​<", $"qkchanger:mind:{jogador}:-1", ButtonStyle.Primary)
               .WithButton(">", $"qkchanger:mind:{jogador}:1", ButtonStyle.Primary)
               .WithButton(">>", $"qkchanger:mind:{jogador}:2", ButtonStyle.Primary);


            await RespondAsync($"Jogador: {jogador} (Flow antes da troca: {player.Flow})", components: button.Build(), ephemeral: true);


        }

        [ComponentInteraction("qkchange:*")]
        public async Task _hpbutt(string acao)
        {
            var players = await PlayerRepo.Load();

            Console.WriteLine("FUCKYOU");
            var menu = new SelectMenuBuilder()
        .WithCustomId($"qkchanger:{acao}")
        .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
        .WithSelectMenu(menu);

            await RespondAsync("Escolha um jogador:", components: components.Build(), ephemeral: true);
        }
        */
        //--------------------------Quick Recovery Section--------------------------

        [ComponentInteraction("qkrecov:vigor")]
        public async Task _qkrecover()
        {
            var players = await PlayerRepo.Load();

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
            var players = await PlayerRepo.Load();

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
            var players = await PlayerRepo.Load();

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
            var players = await PlayerRepo.Load();
                        
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
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name.Equals(jogador, StringComparison.OrdinalIgnoreCase));

            player.Vigor = player.MaxVigor;
            player.Ego = player.MaxEgo;
            player.Flow = 0;

            await PlayerRepo.Save(players);
            await _service.UpdatePlayer(jogador);

            await FollowupAsync($"Jogador {jogador} recuperou completamente!", ephemeral: true);

        }

        [ComponentInteraction("qkrecover:hp")]
        public async Task _qkrecoverfunchp(string[] values)
        {
            string jogador = values[0];
            Console.WriteLine($"qkrecoverfunchhp, {jogador}");
            await DeferAsync();
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name.Equals(jogador, StringComparison.OrdinalIgnoreCase));

            player.Vigor = player.MaxVigor;

            await PlayerRepo.Save(players);
            await _service.UpdatePlayer(jogador);

            await FollowupAsync($"Jogador {jogador} recuperou toda vida!", ephemeral: true);


        }

        [ComponentInteraction("qkrecover:mp")]
        public async Task _qkrecoverfuncmp(string[] values)
        {
            string jogador = values[0];

            await DeferAsync();
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name.Equals(jogador, StringComparison.OrdinalIgnoreCase));

            player.Ego = player.MaxEgo;

            await PlayerRepo.Save(players);
            await _service.UpdatePlayer(jogador);

            await FollowupAsync($"Jogador {jogador} recuperou toda mana!", ephemeral: true);

        }

        [ComponentInteraction("qkrecover:mind")]
        public async Task _qkrecoverfuncmind(string[] values)
        {
            string jogador = values[0];

            await DeferAsync();
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name.Equals(jogador, StringComparison.OrdinalIgnoreCase));

            player.Flow = 0;

            await PlayerRepo.Save(players);
            await _service.UpdatePlayer(jogador);

            await FollowupAsync($"Jogador {jogador} recuperou todo mind!", ephemeral: true);

        }
        //--------------------------Transform Section--------------------------


        [ComponentInteraction("transbutt")]
        public async Task _qktransselect()
        {
            var players = await PlayerRepo.Load();

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
