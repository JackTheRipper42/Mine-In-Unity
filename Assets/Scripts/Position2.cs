using System;

namespace Assets.Scripts
{
    public struct Position2 : IEquatable<Position2>
    {
        private readonly int _x;
        private readonly int _z;

        public Position2(int x, int z) : this()
        {
            _x = x;
            _z = z;
        }

        public int X
        {
            get { return _x; }
        }

        public int Z
        {
            get { return _z; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Position2)
            {
                return Equals(this, (Position2) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_x*397) ^ _z;
            }
        }

        public static bool operator ==(Position2 left, Position2 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Position2 left, Position2 right)
        {
            return !Equals(left, right);
        }

        public bool Equals(Position2 other)
        {
            return Equals(this, other);
        }

        private static bool Equals(Position2 a, Position2 b)
        {
            return a._x == b._x && a._z == b._z;
        }
    }
}
