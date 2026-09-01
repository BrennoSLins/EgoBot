using EgoBot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;

namespace EgoBot;

internal class PlayerRepo
{

        public static async Task<List<Player>> Load()
        {
            if (!File.Exists("Playerlist.json"))
            {
                return new List<Player>();
            }

            string json = await File.ReadAllTextAsync("Playerlist.json");


            return JsonSerializer.Deserialize<List<Player>>(json);
        }

        public static async Task Save(List<Player> players)
        {
            string json = JsonSerializer.Serialize(players, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync("Playerlist.json", json);
        }

        
}

