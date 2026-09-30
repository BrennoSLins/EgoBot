using EgoBot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.IO;

namespace EgoBot;

public class PlayerRepo
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task Update(Func<List<Player>, Task> action)
    {
        await _semaphore.WaitAsync();

        try
        {
            var players = await Load();

            await action(players);

            await Save(players);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in {action.Method.Name}: {ex.Message}");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    

    public async Task<List<Player>> Load()
        {
            if (!File.Exists("Playerlist.json"))
            {
                return new List<Player>();
            }

            string json = await File.ReadAllTextAsync("Playerlist.json");


            return JsonSerializer.Deserialize<List<Player>>(json);
        }

     public async Task Save(List<Player> players)
        {
            string json = JsonSerializer.Serialize(players, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync("Playerlist.json", json);
        }

        
}

