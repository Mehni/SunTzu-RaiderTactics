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
    public class LordJob_SplitUpAssaultColony : LordJob
    {
        private Faction assaulterFaction;

        private bool canKidnap = true;

        private bool canTimeoutOrFlee = true;

        private bool sappers;

        private bool useAvoidGridSmart;

        private bool canSteal = true;

        private static readonly IntRange AssaultTimeBeforeGiveUp = new IntRange(26000, 38000);

        private static readonly IntRange SapTimeBeforeGiveUp = new IntRange(33000, 38000);

        public LordJob_SplitUpAssaultColony()
        {
        }

        public LordJob_SplitUpAssaultColony(Faction assaulterFaction, bool canKidnap = true, bool canTimeoutOrFlee = true, bool sappers = false, bool useAvoidGridSmart = false, bool canSteal = true)
        {
            this.assaulterFaction = assaulterFaction;
            this.canKidnap = canKidnap;
            this.canTimeoutOrFlee = canTimeoutOrFlee;
            this.sappers = sappers;
            this.useAvoidGridSmart = useAvoidGridSmart;
            this.canSteal = canSteal;
        }

        public override StateGraph CreateGraph()
        {
            StateGraph stateGraph = new StateGraph();
            LordToil lordToil = null;
            LordToil lordToil2 = null;
            if (this.sappers)
            {
                lordToil = new LordToil_AssaultSplittingColonySappers();
                if (this.useAvoidGridSmart)
                {
                    lordToil.avoidGridMode = AvoidGridMode.Smart;
                }
                stateGraph.AddToil(lordToil);
                lordToil2 = new LordToil_DefendPoint(false);
                stateGraph.AddToil(lordToil2);
                Transition transition = new Transition(lordToil, lordToil2);
                // the following line is literally the only thing that isn't straight-up copy-pasted: add the filter to prevent the split-off group from guarding the sapper
                transition.AddTrigger(new Trigger_PawnHarmed(1f, false).WithFilter(new TriggerFilter_NoSapperSapping()).WithFilter(new TriggerFilter_LetTheYoungDie())); 
                transition.AddPreAction(new TransitionAction_SetDefendLocalGroup());
                stateGraph.AddTransition(transition);
                Transition transition2 = new Transition(lordToil2, lordToil);
                transition2.AddTrigger(new Trigger_TicksPassedWithoutHarm(600));
                stateGraph.AddTransition(transition2);
            }
            LordToil lordToil3 = new LordToil_AssaultColony();
            if (this.useAvoidGridSmart)
            {
                lordToil3.avoidGridMode = AvoidGridMode.Smart;
            }
            stateGraph.AddToil(lordToil3);
            LordToil_ExitMap lordToil_ExitMap = new LordToil_ExitMap(LocomotionUrgency.Jog, false);
            lordToil_ExitMap.avoidGridMode = AvoidGridMode.Smart;
            stateGraph.AddToil(lordToil_ExitMap);
            if (this.sappers)
            {
                Transition transition3 = new Transition(lordToil, lordToil3);
                transition3.AddSource(lordToil2);
                transition3.AddTrigger(new Trigger_NoFightingSappers());
                stateGraph.AddTransition(transition3);
            }
            if (this.assaulterFaction.def.humanlikeFaction)
            {
                if (this.canTimeoutOrFlee)
                {
                    Transition transition4 = new Transition(lordToil3, lordToil_ExitMap);
                    if (lordToil != null)
                    {
                        transition4.AddSource(lordToil);
                    }
                    transition4.AddTrigger(new Trigger_TicksPassed((!this.sappers) ? LordJob_SplitUpAssaultColony.AssaultTimeBeforeGiveUp.RandomInRange : LordJob_SplitUpAssaultColony.SapTimeBeforeGiveUp.RandomInRange));
                    transition4.AddPreAction(new TransitionAction_Message("MessageRaidersGivenUpLeaving".Translate(new object[]
                    {
                        this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                        this.assaulterFaction.Name
                    })));
                    stateGraph.AddTransition(transition4);
                    Transition transition5 = new Transition(lordToil3, lordToil_ExitMap);
                    if (lordToil != null)
                    {
                        transition5.AddSource(lordToil);
                    }
                    FloatRange floatRange = new FloatRange(0.25f, 0.35f);
                    float randomInRange = floatRange.RandomInRange;
                    transition5.AddTrigger(new Trigger_FractionColonyDamageTaken(randomInRange, 900f));
                    transition5.AddPreAction(new TransitionAction_Message("MessageRaidersSatisfiedLeaving".Translate(new object[]
                    {
                        this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                        this.assaulterFaction.Name
                    })));
                    stateGraph.AddTransition(transition5);
                }
                if (this.canKidnap)
                {
                    LordToil startingToil = stateGraph.AttachSubgraph(new LordJob_Kidnap().CreateGraph()).StartingToil;
                    Transition transition6 = new Transition(lordToil3, startingToil);
                    if (lordToil != null)
                    {
                        transition6.AddSource(lordToil);
                    }
                    transition6.AddPreAction(new TransitionAction_Message("MessageRaidersKidnapping".Translate(new object[]
                    {
                        this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                        this.assaulterFaction.Name
                    })));
                    transition6.AddTrigger(new Trigger_KidnapVictimPresent());
                    stateGraph.AddTransition(transition6);
                }
                if (this.canSteal)
                {
                    LordToil startingToil2 = stateGraph.AttachSubgraph(new LordJob_Steal().CreateGraph()).StartingToil;
                    Transition transition7 = new Transition(lordToil3, startingToil2);
                    if (lordToil != null)
                    {
                        transition7.AddSource(lordToil);
                    }
                    transition7.AddPreAction(new TransitionAction_Message("MessageRaidersStealing".Translate(new object[]
                    {
                        this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                        this.assaulterFaction.Name
                    })));
                    transition7.AddTrigger(new Trigger_HighValueThingsAround());
                    stateGraph.AddTransition(transition7);
                }
            }
            Transition transition8 = new Transition(lordToil3, lordToil_ExitMap);
            if (lordToil != null)
            {
                transition8.AddSource(lordToil);
            }
            transition8.AddTrigger(new Trigger_BecameColonyAlly());
            transition8.AddPreAction(new TransitionAction_Message("MessageRaidersLeaving".Translate(new object[]
            {
                this.assaulterFaction.def.pawnsPlural.CapitalizeFirst(),
                this.assaulterFaction.Name
            })));
            stateGraph.AddTransition(transition8);
            return stateGraph;
        }

        public override void ExposeData()
        {
            Scribe_References.Look<Faction>(ref this.assaulterFaction, "assaulterFaction", false);
            Scribe_Values.Look<bool>(ref this.canKidnap, "canKidnap", true, false);
            Scribe_Values.Look<bool>(ref this.canTimeoutOrFlee, "canTimeoutOrFlee", true, false);
            Scribe_Values.Look<bool>(ref this.sappers, "sappers", false, false);
            Scribe_Values.Look<bool>(ref this.useAvoidGridSmart, "useAvoidGridSmart", false, false);
            Scribe_Values.Look<bool>(ref this.canSteal, "canSteal", true, false);
        }
    }
}
