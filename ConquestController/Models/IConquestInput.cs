using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models
{
    public interface IConquestInput
    {
        public string Faction { get; set; }
        public string Unit { get; set; }
    }
}
