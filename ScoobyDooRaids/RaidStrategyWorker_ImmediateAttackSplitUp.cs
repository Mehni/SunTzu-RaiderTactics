﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;
//using Harmony;

//This is either 99 or 99.99% copy paste from vanilla.

namespace ScoobyDooRaids
{
    public class RaidStrategyWorker_ImmediateAttackSplitUp : RaidStrategyWorker
    {
        private static readonly SimpleCurve StrengthChanceFactorCurve = new SimpleCurve
        {
            {
                new CurvePoint(10f, 1f),
                true
            },
            {
                new CurvePoint(50f, 5f),
                true
            }
        };

        public override float SelectionChance(Map map)
        {
            float num = base.SelectionChance(map);
            float strengthRating = map.strengthWatcher.StrengthRating;
            return num * RaidStrategyWorker_ImmediateAttackSplitUp.StrengthChanceFactorCurve.Evaluate(strengthRating);
        }

        public override bool CanUseWith(IncidentParms parms)
        {
            return parms.faction.def.humanlikeFaction && parms.faction.def.techLevel >= TechLevel.Industrial && this.PawnGenOptionsWithSappers(parms.faction).Any<PawnGroupMaker>() && base.CanUseWith(parms);
        }

        public override float MinimumPoints(Faction faction)
        {
            return this.CheapestSapperCost(faction);
        }

        public override float MinMaxAllowedPawnGenOptionCost(Faction faction)
        {
            return this.CheapestSapperCost(faction);
        }

        private float CheapestSapperCost(Faction faction)
        {
            IEnumerable<PawnGroupMaker> enumerable = this.PawnGenOptionsWithSappers(faction);
            if (!enumerable.Any<PawnGroupMaker>())
            {
                Log.Error(string.Concat(new string[]
                {
                    "Tried to get MinimumPoints for ",
                    base.GetType().ToString(),
                    " for faction ",
                    faction.ToString(),
                    " but the faction has no groups with sappers."
                }));
                return 99999f;
            }
            float num = 9999999f;
            foreach (PawnGroupMaker current in enumerable)
            {
                foreach (PawnGenOption current2 in from op in current.options
                                                   where RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(op.kind)
                                                   select op)
                {
                    if (current2.Cost < num)
                    {
                        num = current2.Cost;
                    }
                }
            }
            return num;
        }

        public override bool CanUsePawnGenOption(PawnGenOption opt, List<PawnGenOption> chosenOpts)
        {
            return chosenOpts.Count != 0 || (opt.kind.weaponTags.Count == 1 && RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(opt.kind));
        }

        private IEnumerable<PawnGroupMaker> PawnGenOptionsWithSappers(Faction faction)
        {
            return faction.def.pawnGroupMakers.Where(delegate (PawnGroupMaker gm)
            {
                bool arg_3B_0;
                if (gm.kindDef == PawnGroupKindDefOf.Normal)
                {
                    arg_3B_0 = gm.options.Any((PawnGenOption op) => RaidStrategyWorker_ImmediateAttackSappers.CanBeSapper(op.kind));
                }
                else
                {
                    arg_3B_0 = false;
                }
                return arg_3B_0;
            });
        }

        public static bool CanBeSapper(PawnKindDef kind)
        {
            return !kind.weaponTags.NullOrEmpty<string>() && kind.weaponTags[0] == "GrenadeDestructive";
        }

        public override LordJob MakeLordJob(IncidentParms parms, Map map)
        {
            return new LordJob_SplitUpAssaultColony(parms.faction, true, true, true, false, true);
        }
    }
}
