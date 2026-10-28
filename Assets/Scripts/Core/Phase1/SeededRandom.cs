namespace HitMe.Core
{
    public sealed class SeededRandom
    {
        private uint _state;
        public SeededRandom(uint seed) { _state = seed == 0 ? 1u : seed; }
        public uint NextUInt() { _state = _state * 1664525u + 1013904223u; return _state; }
        public float NextFloat() { return (NextUInt() & 0x00FFFFFF) / 16777216f; }
        public float Range(float min, float max) { return min + (max - min) * NextFloat(); }
        public int Range(int minInclusive, int maxExclusive) { return minInclusive + (int)(NextFloat() * (maxExclusive - minInclusive)); }
    }
}
