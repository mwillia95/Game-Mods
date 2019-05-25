using System.Collections.Generic;
using Klei.AI;

namespace DuplicantLifeExpectancy
{
    public static class LifeStagePreset
    {
        public const string FirstState = "NewlyPrinted";
        public class Preset
        {
            public string ID;
            public string Description;
            public string Name;
            public MathUtil.MinMax BeginsIn;
            public MathUtil.MinMax LastsFor;
            private bool Is_Bad;
            public string NextPreset;
            public bool DeathStage;
            public List<AttributeModifier> Modifiers = new List<AttributeModifier>();
            public Effect GetEffect(float length)
            {
                Effect e = new Effect(ID, Name, Description, length, true, true, Is_Bad);
                e.SelfModifiers = Modifiers;
                return e;
            }

            public Preset(string _id, string _description, string _name, MathUtil.MinMax _beginsIn, MathUtil.MinMax _lastsFor, bool is_bad, List<AttributeModifier> _modifiers, string _nextPreset, bool _deathStage = false)
            {
                ID = _id;
                Description = _description;
                Name = _name;
                BeginsIn = _beginsIn;
                LastsFor = _lastsFor;
                Is_Bad = is_bad;
                Modifiers = _modifiers;
                NextPreset = _nextPreset;
                _deathStage = false;
            }
        }

        public static Effect GetPresetEffect(string id, float length)
        {
            foreach(Preset p in Presets)
            {
                if(p.ID == id)
                {
                    return p.GetEffect(length);
                }
            }
            return null;
        }

        public static List<Effect> GetGenericEffects()
        {
            List<Effect> effects = new List<Effect>();
            foreach(Preset p in Presets)
            {
                effects.Add(p.GetEffect(1f));
            }
            return effects;
        }
        public static string GetNextPreset(string id)
        {
            foreach(Preset p in Presets)
            {
                if (p.ID == id)
                    return p.NextPreset;
            }
            return "";
        }
        private static List<Preset> _presets;

        public static List<Preset> Presets
        {
            get
            {
                if(_presets == null)
                {
                    BuildPresets();
                }
                return _presets;
            }
        }

        public static void BuildPresets()
        {
            List<Preset> p = new List<Preset>();
            p.Add(NewlyPrintedPreset());
            p.Add(NewlyAdjustedPreset());
            p.Add(MidLifeCrisisPreset());
            p.Add(AgingPreset());
            p.Add(ElderPreset());
            p.Add(DyingOfAgePreset());
            _presets = p;
        }

        private static Preset NewlyPrintedPreset()
        {
            string id = "NewlyPrinted";
            string name = "Newly Printed";
            string description = "This Duplicant was recently printed. Their body just needs some time to adjust.";
            string nextPreset = "NewlyAdjusted";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(0, 0);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(1, 1);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -2f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Learning.Id, -1f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.AirConsumptionRate.Id, -0.33f, name, true));
            modifiers.Add(new AttributeModifier("CaloriesDelta", (100f/60f)*300f, name));
            modifiers.Add(new AttributeModifier("StaminaDelta", -(30f/600f), name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -0.5f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, true, modifiers, nextPreset);

            return p;
        }

        private static Preset NewlyAdjustedPreset()
        {
            string id = "NewlyAdjusted";
            string name = "Newly Adjusted";
            string description = "This Duplicant is starting to catch up. They are picking up things quicker than normal.";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(0, 0);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(8, 12);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, 2f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Learning.Id, 2f, name));
            modifiers.Add(new AttributeModifier(Db.Get().AttributeConverters.TrainingSpeed.Id, 100f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, 2f, name));
            modifiers.Add(new AttributeModifier("CaloriesDelta", (100f / 60f) * -100f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, false, modifiers, "MidLifeCrisis");

            return p;
        }

        private static Preset MidLifeCrisisPreset()
        {
            string id = "MidLifeCrisis";
            string name = "Midlife Crisis";
            string description = "This Duplicant is becoming uncomfortable with the same-old same-old. He/she needs extra stimulation to stay happy.";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(50, 75);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(8, 12);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, -4f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, true, modifiers, "Aging");

            return p;
        }

        

        private static Preset AgingPreset()
        {
            string id = "Aging";
            string name = "Aging";
            string description = "This Duplicant is starting to feel the effects of old age.";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(35, 50);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(30, 50);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -3f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Learning.Id, -2f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.QualityOfLife.Id, -2f, name));
            modifiers.Add(new AttributeModifier("CaloriesDelta", (100f / 60f) * 100f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, true, modifiers, "Elder");

            return p;
        }

        private static Preset ElderPreset()
        {
            string id = "Elder";
            string name = "Elder";
            string description = "This Duplicant is approaching the end of their lifetime.";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(0, 0);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(20, 40);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -5f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Learning.Id, -4f, name));
            modifiers.Add(new AttributeModifier("CaloriesDelta", (100f / 60f) * 100f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.AirConsumptionRate.Id, -0.25f, name, true));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -0.5f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, true, modifiers, "DyingOfAge");
            return p;
        }

        private static Preset DyingOfAgePreset()
        {
            string id = "DyingOfAge";
            string name = "Dying of Old Age";
            string description = "This Duplicant is within a few cycles of passing away from old age.";
            MathUtil.MinMax beginsIn = new MathUtil.MinMax(0, 0);
            MathUtil.MinMax lastsFor = new MathUtil.MinMax(3, 3);
            List<AttributeModifier> modifiers = new List<AttributeModifier>();
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Athletics.Id, -7f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.Learning.Id, -6f, name));
            modifiers.Add(new AttributeModifier("CaloriesDelta", (100f / 60f) * 400f, name));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.AirConsumptionRate.Id, -0.25f, name, true));
            modifiers.Add(new AttributeModifier(Db.Get().Attributes.GermResistance.Id, -1.0f, name));
            Preset p = new Preset(id, description, name, beginsIn, lastsFor, true, modifiers, "", true);
            return p;
        }


    }
}
