using System;
using System.Collections.Generic;
using System.Text;

namespace EgoBot
{
    public class Config
    {
        public string Token { get; set; } = "";
        
    }

    public enum ResourceType
    {
        vigor,
        ego,
        flow

    }

    public enum Nationality
    {
        ale,
        ita,
        bra,
        mex,
        hol,
        none

    }


}
