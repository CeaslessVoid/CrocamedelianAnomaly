using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrocamedelianAnomaly
{
    public class CrocamedelianAnomaly : Mod
    {
        public CrocamedelianAnomaly(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("crocamedelian.anomaly");
            harmony.PatchAll();
        }
    }
}
