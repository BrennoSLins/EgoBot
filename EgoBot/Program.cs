using Discord;
using EgoBot;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

var provider = DI.Services();

var bot = provider.GetRequiredService<Bot>();

await bot.StartAsync();






