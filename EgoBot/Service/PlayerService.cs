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
        private PlayerRepo _repo;
        

        public PlayerService(Bot bot, PlayerRepo repo)
        {
            _bot = bot;
            _repo = repo;
        }

        public async Task<int> VigChange(string pname, int value)
        {
            int result = 0;     
            
                await _repo.Update(players =>
                {
                    var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                    if (player != null)
                    {
                        player.Vigor += value;
                        result = 1;
                        return Task.FromResult(0);
                    }
                    else
                    {
                        Console.WriteLine("Player not found");
                        result = -404;
                        return Task.FromResult(-404);
                    }
                                      
                    
                });

            return result;
        }

        public async Task<int> EgoChange(string pname, int value)
        {
            int result = 0;

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Ego += value;
                    result = 1;
                    return Task.FromResult(0);
                }
                else
                {
                    Console.WriteLine("Player not found");
                    result = -404;
                    return Task.FromResult(-404);
                }
            });

            return result;
            
        }

        public async Task<int> FlowChange(string pname, int value)
        {
            int result = 0;

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Flow += value;
                    result = 1;
                    return Task.FromResult(0);
                }
                else
                {
                    Console.WriteLine("Player not found");
                    result = -404;
                    return Task.FromResult(-404);
                }
            });

            return result;



        }

        public async Task<int> CreatePlayer(string pname, int age, Nationality natio, string overall)
           {
            int result = 0;
            
            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    Console.WriteLine("CreatePlayer: Player already exists.");
                    result = -404;
                    return Task.FromResult(-404);
                }
                Player newplayer = new Player();

                newplayer.Name = pname;
                newplayer.Age = age;
                newplayer.Nationality = natio;
                newplayer.Overall = overall;

                players.Add(newplayer);
                result = 1;
                return Task.FromResult(1);
            });

            return result;
           }

        public async Task EditPlayer(List<string> Info, string pname)
        {
            
                string vig = Info.ElementAtOrDefault(1) ?? "";
                string ego = Info.ElementAtOrDefault(2) ?? "";
                string age = Info.ElementAtOrDefault(4) ?? "";

                string newname = Info.ElementAtOrDefault(0) ?? "";
                string overall = Info.ElementAtOrDefault(3) ?? "";

                await _repo.Update(players =>
                { 

                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (newname != "")
                { 
                    player.Name = newname;
                }

                if (vig != "")
                {
                    int newvig;
                    if (!int.TryParse(vig, out newvig))
                    {
                        Console.WriteLine("EditPlayer: Vigor não foi um número int. Valor atribuido como 0");
                        player.Vigor = 0;
                    }

                    player.Vigor = newvig;
                    player.MaxVigor = newvig;
                }

                if (ego != "")
                {
                    int newego;

                    if (!int.TryParse(ego, out newego))
                    {
                        Console.WriteLine("EditPlayer: Ego não foi um número int. Valor atribuido como 0");
                        player.Ego = 0;
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
                        Console.WriteLine("EditPlayer: Idade não foi um número int. Valor atribuido como 0");
                        player.Age = 0;
                    }
                    player.Age = newage;

                }
                
                    return Task.CompletedTask;
            });
             

        }

        public async Task DeletePlayer(string pname)
        {
            await _repo.Update(players =>
            {
                players.RemoveAll(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                return Task.CompletedTask;
            });
            
        }

        public async Task<Player?> GetPlayer(string pname)
        {
            try
            {

                var players = await _repo.Load();
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
            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player == null)
                {
                    Console.WriteLine("SetMessageID: Player not found");
                    return Task.CompletedTask;
                }
                if (player.MessageId != messageId.Id)
                {
                    player.MessageId = messageId.Id;
                }
                if (player.ChannelId != messageId.Channel.Id)
                {
                    player.ChannelId = messageId.Channel.Id;
                }

                return Task.CompletedTask;
            });
            
           
        }
               
        public async Task<int> VigSet(string pname, int value)
        {
            int result = 0;

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Vigor = value;
                    return Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    result = -404;
                    return Task.FromResult(-404);
                }
                
            });

            return result;

        }

        public async Task<int> EgoSet(string pname, int value)
        {
            int result = 0;

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Ego = value;
                    return Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    result = -404;
                    return Task.FromResult(-404);
                }
                
            });

            return result;
          
        }

        public async Task<int> FlowSet(string pname, int value)
        {
            int result = 0;

            await _repo.Update(players =>
            {
                
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player != null)
                {
                    player.Flow = value;
                    return Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine("Player not found");
                    result = -404;
                    return Task.CompletedTask;
                }
            });

            return result;
        }

        public async Task AddStatus(List<string> Info, string pname)
        {           
                
                string info1 = Info.ElementAtOrDefault(0) ?? "";
                string info2 = Info.ElementAtOrDefault(1) ?? "";
                string info3 = Info.ElementAtOrDefault(2) ?? "";
                string info4 = Info.ElementAtOrDefault(3) ?? "";
                string info5 = Info.ElementAtOrDefault(4) ?? "";


            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

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

                return Task.CompletedTask;
            });
                         
                            
        }

        public async Task RemoveStatus(string pname)
        {

            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player == null)
                {
                    Console.WriteLine("Player não encontrado");
                    return Task.CompletedTask;
                }

                player.Status.Clear();
                return Task.CompletedTask;
            });
            
        }

        public async Task UpdatePlayer(string pname)
        {
            try
            {
                var players = await _repo.Load();
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
           
                
                string fuck1 = modallist.ElementAtOrDefault(0) ?? "";
                Console.WriteLine($"{fuck1}");
                string fuck2 = modallist.ElementAtOrDefault(1) ?? "";
                Console.WriteLine($"{fuck2}");
                string fuck3 = modallist.ElementAtOrDefault(2) ?? "";
                Console.WriteLine($"{fuck3}");


            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));
                if (player == null)
                {
                    Console.WriteLine("AltPic: Player not found.");
                    return Task.CompletedTask;
                }

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
                return Task.CompletedTask;
            });
         
                
        }
            

        public async Task<Embed?> RegisteredPlayers()
        {
            try
            {
                List<Player> players = await _repo.Load();

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
            int result = 0;
            await _repo.Update(players =>
            {
                var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

                if (player != null && (player.TransPic == null || player.TransPic == ""))
                {
                    result = 3; //Player não possui imagem de transformação
                    return Task.FromResult(3);
                }

                else if (player != null && !player.IsTransformed)
                {
                    player.IsTransformed = true;
                    result = 1; //Transformado
                    return Task.FromResult(1);

                }
                else if (player != null && player.IsTransformed)
                {
                    player.IsTransformed = false;
                    result = 2; //Destransformado
                    return Task.FromResult(2);
                }
                else
                {
                    result = -404; //Player não encontrado
                    return Task.FromResult(-404);
                }
            });

            return result;
                
         
        }

        public async Task RenamePlayer(string pname, string nname)
        {

            await _repo.Update(players => {

            var player = players.FirstOrDefault(p => p.Name.Equals(pname, StringComparison.OrdinalIgnoreCase));

            if (player != null)
            {
                player.Name = nname;
                return Task.CompletedTask;
            }
            else
            {
                return Task.CompletedTask;
            }

            });
        }
                  

    }
}
