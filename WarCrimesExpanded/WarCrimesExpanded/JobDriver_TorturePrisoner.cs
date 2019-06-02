using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace WarCrimesExpanded
{
    public class JobDriver_TorturePrisoner : JobDriver
    {
        private int numMeleeAttacksMade;

        protected Pawn Talkee => (Pawn)job.targetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
            => pawn.Reserve(TargetA, job, errorOnFailed: errorOnFailed);

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.job.maxNumMeleeAttacks = 5;

            this.FailOn(() => !Talkee.IsPrisonerOfColony || !Talkee.guest.PrisonerIsSecure);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Goto.Goto(TargetIndex.A, PathEndMode.Touch);
            yield return Cease(Talkee);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                if (pawn.meleeVerbs.TryMeleeAttack(Talkee, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
                {
                    RestUtility.WakeUp(Talkee);
                    numMeleeAttacksMade++;
                    ReadyForNextToil();
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            yield return FreeMovement(Talkee);
            yield return InsultRecruitee(pawn, Talkee);

            yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            yield return InsultRecruitee(pawn, Talkee);

            yield return Cease(Talkee);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                if (pawn.meleeVerbs.TryMeleeAttack(Talkee, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
                {
                    numMeleeAttacksMade++;
                    if (numMeleeAttacksMade >= job.maxNumMeleeAttacks || Rand.Chance(0.3f))
                    {
                        ReadyForNextToil();
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A);
            yield return FreeMovement(Talkee);
            yield return InsultRecruitee(pawn, Talkee);

            yield return Cease(Talkee);
            yield return Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, delegate
            {
                if (pawn.meleeVerbs.TryMeleeAttack(Talkee, job.verbToUse) && pawn.CurJob != null && pawn.jobs.curDriver == this)
                {
                    numMeleeAttacksMade++;
                    if (numMeleeAttacksMade >= job.maxNumMeleeAttacks)
                    {
                        ReadyForNextToil();
                    }
                }
            }).FailOnDespawnedOrNull(TargetIndex.A);

            yield return Toils_Interpersonal.GotoPrisoner(pawn, Talkee, Talkee.guest.interactionMode);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(pawn);
            yield return Toils_Interpersonal.GotoInteractablePosition(TargetIndex.A).FailOn(() => !Talkee.guest.ScheduledForInteraction);
            yield return FreeMovement(Talkee);
            yield return Toils_Interpersonal.SetLastInteractTime(TargetIndex.A);

            yield return GainThoughts(pawn, Talkee);
            yield return Toils_Interpersonal.TryRecruit(TargetIndex.A);
        }

        private static Toil InsultRecruitee(Pawn pawn, Pawn talkee)
        {
            Toil toil = Toils_General.Do(() =>
                {
                    if (pawn.interactions.TryInteractWith(talkee, InteractionDefOf.Insult))
                    {
                        pawn.records.Increment(RecordDefOf.PrisonersChatted);
                    }
                }
            );
            toil.FailOn(() => !talkee.guest.ScheduledForInteraction);
            toil.socialMode = RandomSocialMode.Off;
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = 350;
            return toil;
        }

        private static Toil Cease(Pawn talkee) => new Toil
        {
            initAction = () => talkee.stances.SetStance(new Stance_Cooldown(2000, null, null)),
            atomicWithPrevious = true,
        };

        private static Toil FreeMovement(Pawn talkee) => new Toil
        {
            initAction = () => talkee.stances.SetStance(new Stance_Mobile()),
            atomicWithPrevious = true,
        };

        private static Toil GainThoughts(Pawn pawn, Pawn talkee) => new Toil
        {
            initAction = () => talkee.needs.mood.thoughts.memories.TryGainMemory(WCE_DefOf.WCE_TorturedMe, pawn),
            atomicWithPrevious = true,
        };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref numMeleeAttacksMade, "numMeleeAttacksMade", 0);
        }
    }
}
