using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using Verse.AI;
using Verse.AI.Group;

namespace ScoobyDooRaids
{
    public class LordToil_AssaultSplittingColonySappers : LordToil
    {
        private static readonly FloatRange EscortRadiusRanged = new FloatRange(15f, 19f);

        private static readonly FloatRange EscortRadiusMelee = new FloatRange(23f, 26f);

        //TODO: Add a minimum distance for melee fighters, so their shields are always on the outside of the clustered group

        private LordToilData_AssaultColonySappers Data
        {
            get
            {
                return (LordToilData_AssaultColonySappers)this.data;
            }
        }

        public override bool AllowSatisfyLongNeeds
        {
            get
            {
                return false;
            }
        }

        public LordToil_AssaultSplittingColonySappers()
        {
            this.data = new LordToilData_AssaultColonySappers();
        }

        public override void Init()
        {
            base.Init();
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.Drafting, OpportunityType.Critical);
        }

        public override void UpdateAllDuties()
        {
            if (!this.Data.sapperDest.IsValid && this.lord.ownedPawns.Any<Pawn>())
            {
                this.Data.sapperDest = GenAI.RandomRaidDest(this.lord.ownedPawns[0].Position, base.Map);
            }
            List<Pawn> list = null;
            if (this.Data.sapperDest.IsValid)
            {
                list = new List<Pawn>();
                for (int i = 0; i < this.lord.ownedPawns.Count; i++)
                {
                    Pawn pawn = this.lord.ownedPawns[i];
                    if (pawn.equipment.Primary != null)
                    {
                        if (pawn.equipment.Primary.GetComp<CompEquippable>().AllVerbs.Any((Verb verb) => verb.verbProps.ai_IsBuildingDestroyer))
                        {
                            list.Add(pawn);
                        }
                    }
                }
                if (list.Count == 0 && this.lord.ownedPawns.Count >= 2)
                {
                    list.Add(this.lord.ownedPawns[0]);
                }
            }


            for (int j = 0; j < this.lord.ownedPawns.Count; j++) 
            {
                
                Pawn pawn2 = this.lord.ownedPawns[j]; 
                if (list != null && list.Contains(pawn2))
                {
                    pawn2.mindState.duty = new PawnDuty(DutyDefOf.Sapper, this.Data.sapperDest, -1f);
                }
                else if (!list.NullOrEmpty<Pawn>())
                {
                    //this if/else statement is where the magic happens. 35 years seems like a good split between attack/ and defend the sapper
                    //Should this create compatability problems, look into pawn.Hashoffset with a module as an alternative.
                    //if (pawn2.ageTracker.AgeBiologicalYears >= 35) 
                    if (pawn2.HashOffset() % 2 == 0)
                    {

                        float randomInRange;

                        if (pawn2.equipment != null && pawn2.equipment.Primary != null && pawn2.equipment.Primary.def.IsRangedWeapon)
                        {
                            randomInRange = LordToil_AssaultSplittingColonySappers.EscortRadiusRanged.RandomInRange;
                        }
                        else
                        {
                            randomInRange = LordToil_AssaultSplittingColonySappers.EscortRadiusMelee.RandomInRange;
                        }
                        pawn2.mindState.duty = new PawnDuty(DutyDefOf.Escort, list.RandomElement<Pawn>(), randomInRange);

                    }
                    else pawn2.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                    
                }
                else
                {
                    pawn2.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
                }
            }
        }

        public override void Notify_ReachedDutyLocation(Pawn pawn)
        {
            this.Data.sapperDest = IntVec3.Invalid;
            this.UpdateAllDuties();
        }
    }
}