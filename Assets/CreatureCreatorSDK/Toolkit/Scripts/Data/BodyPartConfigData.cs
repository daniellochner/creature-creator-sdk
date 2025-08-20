using System.Collections.Generic;
using System;

namespace DanielLochner.Assets.CreatureCreator
{
    [Serializable]
    public class BodyPartConfigData : ItemConfigData
    {
        public SaveType Type;
        public Diet Diet;
        public int Complexity;
        public int Health;
        public float Weight;
        public float Speed;
        public List<AbilityType> Abilities;

        public override string Singular => "Body Part";

        public enum SaveType
        {
            Detail,
            Tail,
            Weapon,
            Wing,
            Foot,
            Hand,
            Ear,
            Eye,
            Mouth,
            Nose,
            Arm,
            Leg
        }

        public enum AbilityType
        {
            Bite1,
            Bite2,
            Bite3,
            Command,
            Dance1,
            Dance2,
            Dance3,
            Drop,
            Eat,
            BreatheFire,
            Flap1,
            Flap2,
            Growl,
            Hear,
            Hold,
            Jump1,
            Jump2,
            Jump3,
            Shoot,
            Spit,
            Luminesce,
            NightVision,
            Ping,
            See,
            Smell,
            Spin1,
            Spin2,
            Spin3,
            Sprint1,
            Sprint2,
            Sprint3,
            Strike1,
            Strike2,
            Strike3,
            Swim,
            Walk
        }
    }
}