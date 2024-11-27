using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using RimWorld;
using rjw;
using Verse;

namespace CrocamedelianAnomaly
{
    [HarmonyPatch(typeof(SexUtility), "ProcessSex")]
    public static class ShamblerAfterSex
    {
        public static bool Prefix(SexProps props)
        {
            if ((!props.pawn.IsShambler && !props.partner.IsShambler) || (props.pawn.IsShambler && props.partner.IsShambler))
            {
                return true;
            }

            Pawn pawn = null;

            if (props.pawn.IsShambler)
            {
                pawn = props.partner;
            }
            else
            {
                pawn = props.pawn;
            }

            Hediff shamblerInfection = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("ShamblerInfection"));
            if (shamblerInfection == null)
            {
                var newHediff = HediffMaker.MakeHediff(HediffDef.Named("ShamblerInfection"), pawn);
                pawn.health.AddHediff(newHediff);
            }
            else
            {
                float severityGain = 0.1f;
                shamblerInfection.Severity += severityGain;
            }

            return true;
        }
    }

}
