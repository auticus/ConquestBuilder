using System;
using System.Collections.Generic;
using System.Text;

namespace ConquestController.Models
{
    public class UnitOptionModel
    {
        public string Faction { get; set; }
        public string Unit { get; set; }
        public string Upgrade { get; set; }
        public int Points { get; set; }
        public string Tag { get; set; }
        public int ArmyLimit { get; set; }

        public override string ToString()
        {
            return Upgrade;
        }
    }
}
