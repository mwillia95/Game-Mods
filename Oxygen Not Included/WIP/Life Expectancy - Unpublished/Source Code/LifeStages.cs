using System;
using KSerialization;
using Klei.AI;
using UnityEngine;
namespace DuplicantLifeExpectancy
{
    [SerializationConfig(MemberSerialization.OptIn)]
    class LifeStages : KMonoBehaviour, ISaveLoadable
    {
        [Serializable]
        public class LifeStage
        {
            public string ID;
            public float LastsFor;
            public float BeginsOn;
            public bool Started;
            private bool DeathStage = false;
            public float EndsAfter => LastsFor + BeginsOn;
            public string NextStage => LifeStagePreset.GetNextPreset(this.ID);

            public LifeStage(string id)
            {
                this.ID = id;
                foreach (LifeStagePreset.Preset p in LifeStagePreset.Presets)
                {
                    if (id == p.ID)
                    {
                        LastsFor = Mathf.Floor(p.LastsFor.Get());
                        BeginsOn = Mathf.Floor(p.BeginsIn.Get()) + GameClock.Instance.GetCycle();
                        Started = false;
                        DeathStage = p.DeathStage;
                    }
                }

            }

            public LifeStage(string _id, float _lastsFor, float _beginsOn, bool _started, bool _deathStage = false)
            {
                ID = _id;
                LastsFor = _lastsFor;
                BeginsOn = _beginsOn;
                Started = _started;
                DeathStage = _deathStage;
            }

            public LifeStage()
            {
                ID = "";
            }

            public Effect GetEffect()
            {
                return LifeStagePreset.GetPresetEffect(ID, LastsFor * 600f);
            }

            public Effect GetEffect(float addTime)
            {
                return LifeStagePreset.GetPresetEffect(ID, (LastsFor * 600f) + addTime);
            }

            public bool IsDying()
            {
                return DeathStage;
            }
        }
        [Serialize]
        public LifeStage currentStage;
        private MinionIdentity minion;
        private Effects effects;
        private Attributes attributes;
        [Serialize]
        private bool lifeCyclesDone = false;
        public static bool postLoadChecked = false;

        public void PostLoadCheck()
        {
            BeginFirstStage();
            
        }

        private void BeginFirstStage()
        {
            if (currentStage == null)
            {
                float currCycle = GameClock.Instance.GetCycle();
                float currTime = GameClock.Instance.GetTime();
                currentStage = new LifeStage(LifeStagePreset.FirstState);
                if (currentStage.BeginsOn <= currCycle)
                {
                    Effect e = currentStage.GetEffect(currTime * -1);
                    effects.Add(e, true);
                    currentStage.Started = true;
                }
            }
        }
        protected override void OnPrefabInit()
        {
        }

        protected override void OnSpawn()
        {
            minion = GetComponent<MinionIdentity>();
            effects = GetComponent<Effects>();
            attributes = minion.GetAttributes();
            if(currentStage == null && postLoadChecked)
            {
                //If postLoadChecked is true, this must be a newly printed Duplicant
                BeginFirstStage();
            }

            GameClock.Instance.Subscribe((int)GameHashes.NewDay, OnNewDay);
        }

        protected override void OnCleanUp()
        {
            GameClock.Instance.Unsubscribe((int)GameHashes.NewDay, OnNewDay);
            base.OnCleanUp();
        }       

        private void OnNewDay(object data)
        {

            if (lifeCyclesDone)
                return;
            if (currentStage == null || currentStage.ID == "")
            {
                DebugUtil.LogWarningArgs($"[DuplicantLifeExpectancy]: Life Stage for Duplicant {gameObject.GetProperName()} was null during OnNewDay callback. Should have already been set.");
                currentStage = new LifeStage(LifeStagePreset.FirstState);
            }
            float currCycle = GameClock.Instance.GetCycle();
            if (!currentStage.Started)
            {
                if (currentStage.BeginsOn >= currCycle)
                {
                    Effect e = currentStage.GetEffect();
                    EffectInstance instance = effects.Add(e, true);

                    currentStage.Started = true;
                }
            }
            else
            {
                if (currCycle >= currentStage.EndsAfter)
                {
                    string nextStage = currentStage.NextStage;
                    if (nextStage != "")
                    {
                        currentStage = new LifeStage(currentStage.NextStage);
                        if (currentStage.BeginsOn == currCycle)
                        {
                            effects.Add(currentStage.GetEffect(), true);
                            currentStage.Started = true;
                        }
                        
                    }
                    else
                    {
                        if (currentStage.IsDying())
                        {
                            DeathMonitor.Instance smi = gameObject.GetSMI<DeathMonitor.Instance>();
                            if(smi != null)
                            {
                                Death OldAge = new Death("OldAge", Db.Get().Deaths, "Old Age", "{Target} has died of old age", "dead_on_back", "dead_on_back");
                                smi.Kill(OldAge);
                            }
                        }
                        else
                        {
                            //There is no next stage. The current stage will be re-applied
                            Effect e = currentStage.GetEffect();
                            effects.Add(e, true);
                            currentStage.BeginsOn = currCycle;
                        }
                    }
                }
            }

        }
    }
}
