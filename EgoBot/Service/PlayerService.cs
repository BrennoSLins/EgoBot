using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Microsoft.VisualBasic;
using EgoBot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Xml.Linq;
using System.Diagnostics.CodeAnalysis;

namespace EgoBot
{
    public class PlayerService
    {
        private Bot _bot;
        

        public PlayerService(Bot bot)
        {
            _bot = bot;
           
        }

        public async Task<int> VigChange(string pname, int value)
        {
            try
            {
                
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));


                if (player != null)
                {
                    player.Vigor += value;
                    await PlayerRepo.Save(players);
                    return player.Vigor;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    return -404;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in VigorChange: {ex.Message}");
                return -404;
            }

        }

        public async Task<int> EgoChange(string pname, int value)
        {
            try
            {
                
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    player.Ego += value;
                    await PlayerRepo.Save(players);
                    return player.Ego;
                }
                else
                {
                    Console.WriteLine("Who is this neguinho?");
                    return -404;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EgoChange: {ex.Message}");
                return -404;
            }
        }

        public async Task<int> FlowChange(string pname, int value)
        {
            try
            {
                pname = pname.ToLower();
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    player.Flow += value;
                    await PlayerRepo.Save(players);
                    return player.Flow;
                }
                else
                {
                    Console.WriteLine("Who is this neguinho?");
                    return -404;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FlowChange: {ex.Message}");
                return -404;
            }



        }

        public async Task<int> CreatePlayer(string pname, int age, Nationality natio, string overall)
           {
               try
               {

                   var players = await PlayerRepo.Load();
                   var player = players.FirstOrDefault(p => p.Name == pname);

                   if (player != null)
                   {
                       Console.WriteLine("CreatePlayer: Player already exists.");
                       return 0;
                   }

                   Player newplayer = new Player();
                   newplayer.Name = pname;

                   newplayer.Age = age;

                   newplayer.Nationality = natio;

                   newplayer.Overall = overall;
                          
                   
                   players.Add(newplayer);


                   await PlayerRepo.Save(players);

                   return 1;
               }
               catch (Exception ex)
               {
                   Console.WriteLine($"Error in CreatePlayer: {ex.Message}");
                   return 0;
               }


           }

        public async Task EditPlayer(List<string> Info, string pname)
        {
            try
            {

                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name == pname);

                if (player == null)
                {
                    Console.WriteLine("EditPlayer: Player dont exist.");
                    return;
                }

                string vig = Info.ElementAtOrDefault(1) ?? "";
                string ego = Info.ElementAtOrDefault(2) ?? "";
                string age = Info.ElementAtOrDefault(4) ?? "";

                string newname = Info.ElementAtOrDefault(0) ?? "";
                string overall = Info.ElementAtOrDefault(3) ?? "";

                


                if (newname != "")
                    player.Name = newname;

                if (vig != "")
                {
                    int newvig;
                    while (!int.TryParse(vig, out newvig))
                    {
                        Console.WriteLine("EditPlayer: Vigor não foi um número int");
                    }
                    
                    player.Vigor = newvig;
                    player.MaxVigor = newvig;
                }
                    
                if (ego != "")
                {
                    int newego;

                    if (!int.TryParse(ego, out newego))
                    {
                        Console.WriteLine("EditPlayer: Ego não foi um número int");
                        return;
                    }

                    player.Ego = newego;
                    player.MaxEgo = newego;
                }

                if (overall != "")
                {
                    player.Overall = overall;
                }

                if (age != "")
                {
                    int newage;
                    if (!int.TryParse(age, out newage))
                    {
                        Console.WriteLine("EditPlayer: Idade não foi um número int");
                    }
                    player.Age = newage;
                }
                                                       
                                                
                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in EditPlayer: {ex.Message}");
                return;
            }


        }

        public async Task DeletePlayer(string pname)
        {
            try
            {

                var players = await PlayerRepo.Load();

                players.RemoveAll(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeletPlayer: {ex.Message}");
                return;
            }
        }

        public async Task<Player?> GetPlayer(string pname)
        {
            try
            {

                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    Console.WriteLine("GetPlayer: Player not found");
                    return null;
                }

                if (player.Name == pname)
                {
                    return player;
                }
                else
                {
                    return null;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPlayer: {ex.Message}");
                return null;
            }
        }

        public async Task<Embed?> ShowEmbed(string pname)
        {
            try
            {
                var player = await GetPlayer(pname);
                                

                if (player == null)
                {
                    Console.WriteLine("ShowEmbed: Player not found");
                    return null;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithAuthor(name: $"{player.Name}: {player.Age}yo", iconUrl: $"{player.IconPic}")
                    .WithFooter(string.Join(Environment.NewLine, player.Overall));


                embed.ImageUrl = player.CharPic;

                switch (player.Nationality)
                {
                    case Nationality.ale:
                        embed.ThumbnailUrl = "https://raw.githubusercontent.com/BrennoSLins/Imagedump/refs/heads/main/KickingBallsProEgoEvolution/Players/Flags/Alemanha.webp";
                        break;

                    case Nationality.ita:
                        embed.ThumbnailUrl = "https://raw.githubusercontent.com/BrennoSLins/Imagedump/refs/heads/main/KickingBallsProEgoEvolution/Players/Flags/Italy.png";
                        break;

                    case Nationality.bra:
                        embed.ThumbnailUrl = "https://raw.githubusercontent.com/BrennoSLins/Imagedump/refs/heads/main/KickingBallsProEgoEvolution/Players/Flags/Brasil.png";
                        break;

                    case Nationality.mex:
                        embed.ThumbnailUrl = "https://raw.githubusercontent.com/BrennoSLins/Imagedump/refs/heads/main/KickingBallsProEgoEvolution/Players/Flags/Meixco.webp";
                        break;

                    case Nationality.hol:
                        embed.ThumbnailUrl = "https://raw.githubusercontent.com/BrennoSLins/Imagedump/refs/heads/main/KickingBallsProEgoEvolution/Players/Flags/Holanda.webp";
                        break;
                }       
                
                embed.Color = Color.Blue;

                embed.AddField("❤️ Vigor", $"{player.Vigor} / {player.MaxVigor}", true);
                               
                embed.AddField("⚫ Ego", $"{player.Ego} / {player.MaxEgo}", true);

                string flowBar;

                if (player.Flow >= 100)
                    flowBar = "🟧🟧🟧🟧🟧";
                else if (player.Flow >= 80)
                    flowBar = "🟧🟧🟧🟧⬛";
                else if (player.Flow >= 60)
                    flowBar = "🟧🟧🟧⬛⬛";
                else if (player.Flow >= 40)
                    flowBar = "🟧🟧⬛⬛⬛";
                else if (player.Flow >= 20)
                    flowBar = "🟧⬛⬛⬛⬛";
                else
                    flowBar = "⬛⬛⬛⬛⬛";

                embed.AddField(
                    "🔥 Flow",
                    $"{flowBar}\n{player.Flow} / 100",
                    true
                );




                return embed.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ShowPlayer: {ex.Message}");
                return null;
            }
        }

        public async Task SetMessageID(string pname, IUserMessage messageId)
        {
            try
            {
                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name == pname);

                if (player == null)
                {
                    Console.WriteLine("Jogador não existe: Task SetMessageID");
                    return;
                }

                if (player.MessageId != messageId.Id)
                {
                    player.MessageId = messageId.Id;
                }

                if (player.ChannelId != messageId.Channel.Id)
                {
                    player.ChannelId = messageId.Channel.Id;
                }



                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SetMessageID: {ex.Message}");
                return;
            }

        }
               
        public async Task<int> VigSet(string pname, int value)
        {
            try
            {
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    player.Vigor = value;
                    await PlayerRepo.Save(players);
                    return player.Vigor;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    return -404;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreatePlayer: {ex.Message}");
                return -404;
            }

        }

        public async Task<int> EgoSet(string pname, int value)
        {
            try
            {

                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    player.Ego = value;
                    await PlayerRepo.Save(players);
                    return player.Ego;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    return -404;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MPSet: {ex.Message}");
                return -404;
            }

        }

        public async Task<int> FlowSet(string pname, int value)
        {
            try
            {
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null)
                {
                    player.Flow = value;
                    await PlayerRepo.Save(players);
                    return player.Flow;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    return -404;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in FlowSet: {ex.Message}");
                return -404;
            }
        }

        public async Task AddStatus(List<string> Info, string pname)
        {
            try
            {
                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    Console.WriteLine("Player não encontrado");
                    return;
                }

                string info1 = Info.ElementAtOrDefault(0) ?? "";
                string info2 = Info.ElementAtOrDefault(1) ?? "";
                string info3 = Info.ElementAtOrDefault(2) ?? "";
                string info4 = Info.ElementAtOrDefault(3) ?? "";
                string info5 = Info.ElementAtOrDefault(4) ?? "";

                

                if (!player.Status.Contains(info1))
                    player.Status.Add(info1);

                if (info2 != "" && !player.Status.Contains(info2))
                    player.Status.Add(info2);
                                   
                if (info3 != "" && !player.Status.Contains(info3))
                    player.Status.Add(info3);

                if (info4 != "" && !player.Status.Contains(info4))
                    player.Status.Add(info4);

                if (info5 != "" && !player.Status.Contains(info5))
                    player.Status.Add(info5);

                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddInfo: {ex.Message}");
                return;
            }
        }

        public async Task RemoveStatus(string pname)
        {
            try
            {

                List<Player> players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    Console.WriteLine("Player não encontrado");
                    return;
                }

                player.Status.Clear();

                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveStatus: {ex.Message}");
                return;
            }
        }

        public async Task UpdatePlayer(string pname)
        {
            try
            {
                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                

                if (player == null)
                {
                    Console.WriteLine("Player not found.");
                    return;
                }

                pname = player.Name;

                ulong MessageId = player.MessageId;
                ulong ChannelId = player.ChannelId;

                IMessageChannel channel = await _bot.GetMessageChannelID(ChannelId);

                if (channel == null)
                    return;

                var msg = await channel.GetMessageAsync(MessageId) as IUserMessage;

                if (msg == null)
                    return;

                Embed? novoEmbed = await ShowEmbed(pname);

                if (novoEmbed == null)
                    return;

                await msg.ModifyAsync(x =>
                {
                    x.Embed = novoEmbed;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePlayer: {ex.Message}");
                return;
            }


        }

        public async Task AltPlayerPic(List<string> modallist, string pname)
        {
            try
            {
                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                string fuck1 = modallist.ElementAtOrDefault(0) ?? "";
                Console.WriteLine($"{fuck1}");
                string fuck2 = modallist.ElementAtOrDefault(1) ?? "";
                Console.WriteLine($"{fuck2}");
                string fuck3 = modallist.ElementAtOrDefault(2) ?? "";
                Console.WriteLine($"{fuck3}");



                if (fuck1 != "")
                {
                    player.CharPic = fuck1;
                }

                if (fuck2 != "")
                {
                    player.TransPic = fuck2;
                }

                if (fuck3 != "")
                {
                    player.IconPic = fuck3;
                }
                                

                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AltPlayerPic: {ex.Message}");
                return;
            }
        }

        public async Task<Embed?> RegisteredPlayers()
        {
            try
            {
                List<Player> players = await PlayerRepo.Load();

                if (players.Count == 0)
                {
                    Console.WriteLine("Nenhum jogador registrado.");
                    return null;
                }

                EmbedBuilder embed = new EmbedBuilder()
                    .WithTitle("Jogadores Registrados")
                    .WithColor(Color.Blue);
                foreach (var player in players)
                {
                    embed.AddField(player.Name, "\u200B");
                }

                return embed.Build();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RegisteredPlayers: {ex.Message}");
                return null;
            }


        }

        public async Task<int> Transform(string pname)
        {
            try
            {
                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null && (player.TransPic == null || player.TransPic == ""))
                {
                    return 3;
                }

                else if (player != null && !player.IsTransformed)
                {
                    player.IsTransformed = true;
                    
                    await PlayerRepo.Save(players);
                    return 1;
                }
                else if (player != null && player.IsTransformed)
                {
                    player.IsTransformed = false;
                    await PlayerRepo.Save(players);
                    return 0;
                }
                
                return 3;

                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Transform: {ex.Message}");
                return 3;
            }
        }

        public async Task RenamePlayer(string pname, string nname)
        {
            try
            {
                var players = await PlayerRepo.Load();
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Name = nname;
                }
                else
                {
                    return;
                }
                await PlayerRepo.Save(players);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RenamePlayer: {ex.Message}");
                return;
            }
        }
             
        public async Task<Skill> CreateSkill(string name, string desc, int amount, ResourceType resource)
        {
            Skill newskill = new Skill();

            newskill.Name = name;
            newskill.Description = desc;
            newskill.Cost = amount;
            newskill.Resource = resource;

            return newskill;
        }

        public async Task<int> UseSkill(string pname, string pskill)
        {
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name == pname);
            Skill? skill = player.Skills.FirstOrDefault(p => p.Name.Equals(pskill, StringComparison.OrdinalIgnoreCase));

            switch (skill.Resource)
            {
                case ResourceType.vigor: 
                    if (player.Vigor >= skill.Cost)
                    {
                        player.Vigor += skill.Cost;
                        await PlayerRepo.Save(players);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                                                                                
                case ResourceType.ego: 
                    if (player.Ego >= skill.Cost)
                    {
                        player.Ego += skill.Cost;
                        await PlayerRepo.Save(players);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }

                case ResourceType.flow: 
                    if (player.Flow >= skill.Cost)
                    {
                        player.Flow += skill.Cost;
                        await PlayerRepo.Save(players);
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }

                
                default:
                    return 3;

            }

            
        }

        public async Task<Embed?> ShowSkill(string pname, string pskill)
        {
            var players = await PlayerRepo.Load();
            var player = players.FirstOrDefault(p => p.Name == pname);
            Skill? skill = player.Skills.FirstOrDefault(p => p.Name.Equals(pskill, StringComparison.OrdinalIgnoreCase));

            var embed = new EmbedBuilder()
            .WithTitle($"{skill.Name}:")
            .WithDescription($"{skill.Description}")
            .AddField($"[{skill.Cost} de {skill.Resource} ao uso]", "\u200B", inline: false)
            .WithColor(Color.DarkBlue);

            return embed.Build();
        }
    }
}
