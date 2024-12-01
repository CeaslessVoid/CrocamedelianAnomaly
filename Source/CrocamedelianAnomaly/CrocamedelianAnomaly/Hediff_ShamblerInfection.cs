using HarmonyLib;
using RimWorld;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace CrocamedelianAnomaly
{
    public class Hediff_ShamblerInfection : HediffWithComps
    {

        //public override void Tick()
        //{
        //    base.Tick();

        //    if (this.pawn.IsHashIntervalTick(1000) && this.Severity >= 0.99)
        //    {
        //        pawn.Kill(null);

        //        Hediff shamblerHediff = pawn.health.hediffSet.GetFirstHediffOfDef(this.def);
        //        if (shamblerHediff != null)
        //        {
        //            pawn.health.RemoveHediff(shamblerHediff);
        //        }

        //        Hediff submittingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Hediff_Submitting"));
        //        if (submittingHediff != null)
        //        {
        //            pawn.health.RemoveHediff(submittingHediff);
        //        }
        //    }

        //}


    }

    [HarmonyPatch(typeof(Pawn), "Kill")]
    public static class Pawn_Kill_Patch
    {

        public static void Postfix(Pawn __instance)
        {
            Hediff shamblerInfection = __instance.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("ShamblerInfection"));
            if (shamblerInfection != null)
            {
                MutantUtility.ResurrectAsShambler(__instance, 60000, Faction.OfEntities);
            }

            Hediff submittingHediff = __instance.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Hediff_Submitting"));
            if (submittingHediff != null)
            {
                __instance.health.RemoveHediff(submittingHediff);
            }

        }

    }

}
