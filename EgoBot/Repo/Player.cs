using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EgoBot
{
    public class Player
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public Nationality Nationality { get; set; }
        public int Vigor { get; set; } = 5;
        public int MaxVigor { get; set; } = 5;
        public int Ego { get; set; } = 5;
        public int MaxEgo { get; set; } = 5;
        public int Flow { get; set; } = 0;
        public string CharPic { get; set; }
        public bool IsTransformed { get; set; } = false;
        public string TransPic { get; set; }
        public string IconPic { get; set; }
        public ulong MessageId { get; set; }
        public ulong ChannelId { get; set; }
        public List<string> Status { get; set; } = new();
        public string Overall { get; set; }
        public List<Skill> Skills { get; set; } = new();
        



    }
}
