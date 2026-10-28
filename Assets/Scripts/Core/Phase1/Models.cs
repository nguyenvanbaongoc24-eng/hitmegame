using System;

namespace HitMe.Core
{
    public enum Phase { Placing, Locked, Reveal, Throwing, Resolve, RoundEnd, MatchEnd }
    public enum BotLevel { Random, CenterHunter, Predictor }

    [Serializable]
    public sealed class PlayerState
    {
        public string Id;
        public string Name;
        public bool IsBot;
        public int Hp;
        public Vec2 Position;
        public Vec2 Facing;
        [NonSerialized] public Vec2? LastAim;
        public int HitsLanded;

        public bool IsAlive { get { return Hp > 0; } }

        public PlayerState(string id, string name, bool isBot, int hp, Vec2 position, Vec2 facing)
        {
            Id = id; Name = name; IsBot = isBot; Hp = hp; Position = position; Facing = facing.Normalized;
        }
    }

    [Serializable]
    public sealed class RoundInput
    {
        public string PlayerId;
        public bool HasPlacement;
        public Vec2 Placement;
        public bool HasAim;
        public Vec2 AimDirection;
        public bool IsReady;

        public RoundInput(string playerId) { PlayerId = playerId; }
    }

    public sealed class HitEvent
    {
        public string ShooterId;
        public string TargetId;
        public Vec2 Point;
        public float Distance;
    }

    public sealed class RoundResolution
    {
        public readonly System.Collections.Generic.List<HitEvent> Hits = new System.Collections.Generic.List<HitEvent>();
        public readonly System.Collections.Generic.Dictionary<string, int> Damage = new System.Collections.Generic.Dictionary<string, int>();
    }
}
