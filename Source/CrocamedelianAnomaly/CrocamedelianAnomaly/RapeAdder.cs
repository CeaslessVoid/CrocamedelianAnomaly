using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using rjw;
using System.Reflection;
using Verse.AI;

namespace CrocamedelianAnomaly
{

    [HarmonyPatch(typeof(JobGiver_RapeEnemy), "TryGiveJob")]
    public static class JobGiver_RapeEnemy_TryGiveJob_Patch
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            if (pawn != null && !pawn.IsShambler)
            {
                return true;
            }

            if (RJWSettings.DebugRape) ModLog.Message(" JobGiver_RapeEnemy::TryGiveJob( " + xxx.get_pawnname(pawn) + " ) called0");
            if (pawn.health.hediffSet.HasHediff(HediffDef.Named("Hediff_RapeEnemyCD")) || !pawn.health.capacities.CanBeAwake)
                __result = null;

            if (!xxx.can_rape(pawn)) __result = null;
            if (RJWSettings.DebugRape) ModLog.Message(" JobGiver_RapeEnemy::TryGiveJob( " + xxx.get_pawnname(pawn) + " ) can rape");

            JobDef_RapeEnemy rapeEnemyJobDef = null;

            int? highestPriority = null;

            foreach (JobDef_RapeEnemy job in DefDatabase<JobDef_RapeEnemy>.AllDefs)
            {
                if (job.CanUseThisJobForPawn(pawn))
                {
                    if (highestPriority == null)
                    {
                        rapeEnemyJobDef = job;
                        highestPriority = job.priority;
                    }
                    else if (job.priority > highestPriority)
                    {
                        rapeEnemyJobDef = job;
                        highestPriority = job.priority;
                    }
                }
            }

            if (rapeEnemyJobDef == null)
            {
                if (RJWSettings.DebugRape) ModLog.Message(" JobGiver_RapeEnemy::TryGiveJob( " + xxx.get_pawnname(pawn) + " ) no valid rapeEnemyJobDef found");
                __result = null;
            }

            if (RJWSettings.DebugRape) ModLog.Message(" JobGiver_RapeEnemy::ChoosedJobDef( " + xxx.get_pawnname(pawn) + " ) - " + rapeEnemyJobDef?.ToString() + " choosen");
            Pawn victim = rapeEnemyJobDef?.FindVictim(pawn, pawn.Map);

            if (RJWSettings.DebugRape) ModLog.Message(" JobGiver_RapeEnemy::FoundVictim( " + xxx.get_pawnname(victim) + " )");

            pawn.health.AddHediff(HediffDef.Named("Hediff_RapeEnemyCD"), null, null, null);

            __result = victim != null ? JobMaker.MakeJob(rapeEnemyJobDef, victim) : null;

            return false;

        }
    }

    [HarmonyPatch(typeof(JobDriver_RapeEnemy), "FindVictim")]
    public static class JobDriver_RapeEnemyFindVictim_Patch
    {
        public static bool Prefix(Pawn rapist, Map m, ref Pawn __result)
        {
            if (!rapist.IsShambler && m != null)
            {
                return true;
            }

            if (rapist == null || m == null)
            {
                __result = null;
                return false;
            }

            if (!xxx.can_rape(rapist))
            {
                __result = null;
                return false;
            }

            List<Pawn> validTargets = new List<Pawn>();
            float min_fuckability = 0.10f;
            float avg_fuckability = 0f;
            var valid_targets = new Dictionary<Pawn, float>();
            Pawn chosentarget = null;

            IEnumerable<Pawn> targets = m.mapPawns.AllPawnsSpawned.Where(x
                => !x.IsForbidden(rapist) && x != rapist && x.HostileTo(rapist)
                && RJWReplacer.IsValidTarget(rapist, x))
                .ToList();

            if (RJWSettings.DebugRape) ModLog.Message($" targets {targets.Count()}");

            if (targets.Any(x => RJWReplacer.IsBlocking(rapist, x)))
            {
                __result = null;
                return false;
            }

            foreach (var target in targets)
            {
                if (!Pather_Utility.cells_to_target_rape(rapist, target.Position))
                {
                    if (RJWSettings.DebugRape) ModLog.Message($" {xxx.get_pawnname(target)} too far (cells) = {rapist.Position.DistanceTo(target.Position)}, skipping");
                    continue;
                }

                float fuc = RJWReplacer.GetFuckability(rapist, target);

                if (fuc > min_fuckability)
                {
                    if (Pather_Utility.can_path_to_target(rapist, target.Position))
                        valid_targets.Add(target, fuc);
                    else
                        if (RJWSettings.DebugRape) ModLog.Message($" {xxx.get_pawnname(target)} too far (path), skipping");
                }
                else
                    if (RJWSettings.DebugRape) ModLog.Message($" {xxx.get_pawnname(target)} fuckability too low = {fuc}, skipping");

            }
            if (RJWSettings.DebugRape) ModLog.Message($" fuckable targets {valid_targets.Count()}");

            if (valid_targets.Any())
            {
                avg_fuckability = valid_targets.Average(x => x.Value);
                if (RJWSettings.DebugRape) ModLog.Message($" avg_fuckability {avg_fuckability}");

                var valid_targetsFiltered = valid_targets.Where(x => x.Value >= avg_fuckability);
                if (RJWSettings.DebugRape) ModLog.Message($" targets above avg_fuckability {valid_targetsFiltered.Count()}");

                if (valid_targetsFiltered.Any())
                    chosentarget = valid_targetsFiltered.RandomElement().Key;
            }

            __result =  chosentarget;
            return false;
        }


    }

    public class RJWReplacer
    {
        public static bool IsValidTarget(Pawn rapist, Pawn target)
        {

            if (!RJWSettings.bestiality_enabled)
            {
                if (xxx.is_animal(target) && xxx.is_human(rapist))
                {
                    return false;
                }
                if (xxx.is_animal(rapist) && xxx.is_human(target))
                {
                    return false;
                }
            }

            if (!RJWSettings.animal_on_animal_enabled)
                if ((xxx.is_animal(target) && xxx.is_animal(rapist)))
                {
                    return false;
                }

            if ((xxx.is_mechanoid(rapist) && xxx.is_animal(target)) || (xxx.is_animal(rapist) && xxx.is_mechanoid(target)))
                return false;

            if (target.CurJob?.def == xxx.gettin_raped || target.CurJob?.def == xxx.gettin_loved)
            {
                return false;
            }

            return Can_rape_Easily(target) &&
                (xxx.is_human(target) || xxx.is_animal(target)) &&
                rapist.CanReserveAndReach(target, PathEndMode.OnCell, Danger.Some, xxx.max_rapists_per_prisoner, 0);
        }

        public static bool Can_rape_Easily(Pawn pawn)
        {
            return xxx.can_get_raped(pawn) && !pawn.IsBurning();
        }

        public static bool IsBlocking(Pawn rapist, Pawn target)
        {
            return (!target.Downed && rapist.CanSee(target)) || (!rapist.IsShambler);
        }

        public static float GetFuckability(Pawn rapist, Pawn target)
        {
            float fuckability = 0;
            if (target.health.hediffSet.HasHediff(xxx.submitting))
            {
                fuckability = 2 * SexAppraiser.would_fuck(rapist, target, invert_opinion: true, ignore_bleeding: true, ignore_gender: true);
            }
            else if (SexAppraiser.would_rape(rapist, target))
            {
                fuckability = SexAppraiser.would_fuck(rapist, target, invert_opinion: true, ignore_bleeding: true, ignore_gender: true);
            }

            if (RJWSettings.DebugRape) ModLog.Message($"JobDriver_RapeEnemy::GetFuckability({xxx.get_pawnname(rapist)}, {xxx.get_pawnname(target)})");

            return fuckability;
        }
    }

    internal class JobDef_RapeEnemy : JobDef
    {
        public List<JobDef> interruptJobs;
        public List<string> TargetDefNames = new List<string>();
        public int priority = 0;

        protected JobDriver_RapeEnemy instance
        {
            get
            {
                if (_tmpInstance == null)
                {
                    _tmpInstance = (JobDriver_RapeEnemy)Activator.CreateInstance(driverClass);
                }
                return _tmpInstance;
            }
        }

        private JobDriver_RapeEnemy _tmpInstance;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            interruptJobs = new List<JobDef> { null, JobDefOf.LayDown, JobDefOf.Wait_Wander, JobDefOf.GotoWander, JobDefOf.AttackMelee };
        }

        public virtual bool CanUseThisJobForPawn(Pawn rapist)
        {
            bool busy = !interruptJobs.Contains(rapist.CurJob?.def);
            if (RJWSettings.DebugRape) ModLog.Message(" JobDef_RapeEnemy::CanUseThisJobForPawn( " + xxx.get_pawnname(rapist) + " ) - busy:" + busy + " with current job: " + rapist.CurJob?.def?.ToString());
            if (busy) return false;

            return instance.CanUseThisJobForPawn(rapist);
        }

        public virtual Pawn FindVictim(Pawn rapist, Map m)
        {
            return instance.FindVictim(rapist, m);
        }
    }

}
