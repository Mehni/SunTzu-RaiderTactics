using RimWorld;
using System;
using Verse.AI;
using Verse;
using Verse.AI.Group;

namespace ScoobyDooRaids
{

    //poorly named Class that's based on TriggerFilter_NoSapperSapping
    //serves as a function to filter out the group that splits up from the sapper group from defending the sapper.
    public class TriggerFilter_LetTheYoungDie : TriggerFilter
    {
        public override bool AllowActivation(Lord lord, TriggerSignal signal)
        {
            for (int i = 0; i < lord.ownedPawns.Count; i++)
            {
                Pawn pawn = lord.ownedPawns[i];
                if ((pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDefOf.AssaultColony))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
