using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EgoBot
{
    

    public class Skill
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public ResourceType Resource { get; set; }

    }
}
