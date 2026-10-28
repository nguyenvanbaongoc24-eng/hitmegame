using System;

namespace HitMe.Core
{
    [Serializable]
    public struct Vec2 : IEquatable<Vec2>
    {
        public float X;
        public float Y;

        public Vec2(float x, float y) { X = x; Y = y; }
        public static Vec2 Zero { get { return new Vec2(0f, 0f); } }
        public static Vec2 Up { get { return new Vec2(0f, 1f); } }
        public float LengthSquared { get { return X * X + Y * Y; } }
        public float Length { get { return (float)Math.Sqrt(LengthSquared); } }
        public Vec2 Normalized { get { var length = Length; return length > 0.00001f ? this / length : Up; } }
        public static float Dot(Vec2 a, Vec2 b) { return a.X * b.X + a.Y * b.Y; }
        public static float Distance(Vec2 a, Vec2 b) { return (a - b).Length; }
        public static Vec2 operator +(Vec2 a, Vec2 b) { return new Vec2(a.X + b.X, a.Y + b.Y); }
        public static Vec2 operator -(Vec2 a, Vec2 b) { return new Vec2(a.X - b.X, a.Y - b.Y); }
        public static Vec2 operator *(Vec2 a, float scalar) { return new Vec2(a.X * scalar, a.Y * scalar); }
        public static Vec2 operator /(Vec2 a, float scalar) { return new Vec2(a.X / scalar, a.Y / scalar); }
        public bool Equals(Vec2 other) { return Math.Abs(X - other.X) < 0.0001f && Math.Abs(Y - other.Y) < 0.0001f; }
        public override bool Equals(object obj) { return obj is Vec2 && Equals((Vec2)obj); }
        public override int GetHashCode() { return X.GetHashCode() ^ (Y.GetHashCode() << 2); }
        public override string ToString() { return string.Format("({0:0.###}, {1:0.###})", X, Y); }
    }
}
