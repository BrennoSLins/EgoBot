using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using EgoBot;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Net;
using System.Numerics;
using System.Security.AccessControl;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;

namespace EgoBot
{
    public class PlayerModals : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly PlayerService _service;
        private readonly InteractionService _interactions;
        private readonly EditModalState _modalstate;
        private readonly PlayerRepo _repo;



        public PlayerModals(PlayerService service, InteractionService interactions, EditModalState modalstate, PlayerRepo repo)
        {
            _service = service;
            _interactions = interactions;
            _modalstate = modalstate;
            _repo = repo;
        }

        public class CreatePlayerModal : IModal
        {
            public string Title => "Informações do Jogador";

            [InputLabel("Nome")]
            [ModalTextInput("nome")]
            public string Nome { get; set; }

            [InputLabel("Idade")]
            [ModalTextInput("idade")]
            public int Idade { get; set; }

            [InputLabel("Nacionalidade")]
            [ModalTextInput("country", placeholder: "Digite as três primeiras letras do país.")]
            public string Country { get; set; }

            [InputLabel("Overall")]
            [ModalTextInput("flow",
                TextInputStyle.Paragraph,
                placeholder: "Digite o overall do personagem...",
                maxLength: 4000)]
            public string Overall { get; set; }


        }

        public class EditPlayerModal : IModal
        {
            public string Title => "Informações do Jogador";


            [InputLabel("Novo nome")]
            [ModalTextInput("newname")]
            [RequiredInput(false)]
            public string? NewName { get; set; }

            [InputLabel("Vigor")]
            [ModalTextInput("vigor")]
            [RequiredInput(false)]
            public string? Vigor { get; set; }

            [InputLabel("Ego")]
            [ModalTextInput("ego")]
            [RequiredInput(false)]
            public string? Ego { get; set; }


            [InputLabel("Overall")]
            [ModalTextInput("flow",
                TextInputStyle.Paragraph,
                placeholder: "Digite o overall do personagem...",
                maxLength: 4000)]
            [RequiredInput(false)]
            public string? Overall { get; set; }

            [InputLabel("Idade")]
            [ModalTextInput("idade")]
            [RequiredInput(false)]
            public string? Idade { get; set; }



        }

        public class AddStatusModal : IModal
        {
            public string Title => "Digite as informações extras";


            [InputLabel("Informação")]
            [ModalTextInput("info1")]
            public string info1 { get; set; }

            [InputLabel("Informação")]
            [ModalTextInput("info2")]
            [RequiredInput(false)]
            public string? info2 { get; set; }

            [InputLabel("Informação")]
            [ModalTextInput("info3")]
            [RequiredInput(false)]
            public string? info3 { get; set; }

            [InputLabel("Informação")]
            [ModalTextInput("info4")]
            [RequiredInput(false)]
            public string? info4 { get; set; }

            [InputLabel("Informação")]
            [ModalTextInput("info5")]
            [RequiredInput(false)]
            public string? info5 { get; set; }


        }

        public class AltPicModal : IModal
        {
            public string Title => "Mande o link da imagem.";

            [InputLabel("Foto do personagem")]
            [ModalTextInput("charpic")]
            [RequiredInput(false)]
            public string? MCharPic { get; set; }

            [InputLabel("Foto da transformação")]
            [ModalTextInput("transpic")]
            [RequiredInput(false)]
            public string? MTransPic { get; set; }

            [InputLabel("Foto do ícone")]
            [ModalTextInput("iconpic")]
            [RequiredInput(false)]
            public string? MIconPic { get; set; }



        }

        public class CreateSkillModal : IModal
        {
            public string Title => "Criando skill para personagem.";

            [InputLabel("Nome da skill")]
            [ModalTextInput("skillname")]
            public string Name { get; set; }

            [InputLabel("Descrição da skill")]
            [ModalTextInput("skilldesc",
                TextInputStyle.Paragraph,
                placeholder: "Digite a descrição da habilidade...",
                maxLength: 4000)]
            public string Desc { get; set; }

            [InputLabel("Custo da skill")]
            [ModalTextInput("skillcost")]
            public int Cost { get; set; }

            [InputLabel("Recurso usado")]
            [ModalTextInput("skillresource", placeholder: "Vigor, Ego, Flow")]
            public string Resource { get; set; }


        }


        //Modal interaction handlers
        //Create player handler

        [ModalInteraction("create_player")]
        public async Task _createPlayer(CreatePlayerModal modal)
        {
            Nationality nationality;

            switch (modal.Country.ToLower())
            {
                case "ale":
                    nationality = Nationality.ale;
                    break;

                case "ita":
                    nationality = Nationality.ita;
                    break;

                case "bra":
                    nationality = Nationality.bra;
                    break;

                case "mex":
                    nationality = Nationality.mex;
                    break;
                case "hol":
                    nationality = Nationality.hol;
                    break;
                default:
                    nationality = Nationality.none;
                    break;
            }

            int check = await _service.CreatePlayer(modal.Nome, modal.Idade, nationality, modal.Overall);
            if (check == -404)
            {
                await RespondAsync("Algo deu errado. Tente novamente.", ephemeral: true);
            }
            else
            {
                await RespondAsync($"Jogador {modal.Nome} criado com sucesso!", ephemeral: true);
            }
            
        }

        //Edit player handlers


        [ModalInteraction("medit")]
        public async Task _editPlayerMod(EditPlayerModal modal)
        {
            
            _modalstate.Set(Context.User.Id, new List<string> { modal.NewName, modal.Vigor, modal.Ego, modal.Overall, modal.Idade });
            
            List<Player> players = await _repo.Load();

            var menu = new SelectMenuBuilder();
            menu.WithCustomId($"editselect")
                .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
                .WithSelectMenu(menu);

            await RespondAsync("Escolhe o jogador que será editado:", components: components.Build(), ephemeral: true);

        }

        [ModalInteraction("maddinfo")]
        public async Task _maddInfo(AddStatusModal modal)
        {
            _modalstate.Set(Context.User.Id, new List<string> { modal.info1, modal.info2, modal.info3, modal.info4, modal.info5 });
            
            List<Player> players = await _repo.Load();

            var menu = new SelectMenuBuilder();
            menu.WithCustomId($"addinfoselect")
                .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
                .WithSelectMenu(menu);

            await RespondAsync("Escolha o jogador:", components: components.Build(), ephemeral: true);


        }

        [ModalInteraction("maltpic")]
        public async Task _maltPic(AltPicModal modal)
        {
            _modalstate.Set(Context.User.Id, new List<string> { modal.MCharPic, modal.MTransPic, modal.MIconPic });
            
            List<Player> players = await _repo.Load();

            var menu = new SelectMenuBuilder();
            menu.WithCustomId($"altpicselect")
                .WithPlaceholder("Escolha um jogador");

            foreach (Player player in players)
            {
                menu.AddOption($"{player.Name}", $"{player.Name}");
            }

            var components = new ComponentBuilder()
                .WithSelectMenu(menu);

            await RespondAsync("Escolha o jogador:", components: components.Build(), ephemeral: true);


        }

        

    } 

    public class EditModalState
    {
        private readonly ConcurrentDictionary<ulong, List<string>> _states = new();
        public void Set(ulong userId, List<string> list)
        {
            _states[userId] = list;
        }

        public bool TryGet(ulong userId, out List<string> state)
        {
            return _states.TryGetValue(userId, out state);
        }

        public void Remove(ulong userId)
        {
            _states.TryRemove(userId, out _);
        }
    }
}
