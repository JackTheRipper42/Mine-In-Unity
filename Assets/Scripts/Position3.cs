using System;
using UnityEngine;

namespace Assets.Scripts
{
    public struct Position3 : IEquatable<Position3>
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        public Position3(int x, int y, int z) : this()
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public int X
        {
            get { return _x; }
        }

        public int Y
        {
            get { return _y; }
        }

        public int Z
        {
            get { return _z; }
        }

        public override bool Equals(object obj)
        {
            if (obj is Position3)
            {
                return Equals(this, (Position3) obj);
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _x;
                hashCode = (hashCode*397) ^ _y;
                hashCode = (hashCode*397) ^ _z;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", _x, _y, _z);
        }

        public bool Equals(Position3 other)
        {
            return Equals(this, other);
        }

        public static Position3 operator +(Position3 left, Position3 right)
        {
            return new Position3(
                left._x + right._x,
                left._y + right._y,
                left._z + right._z);
        }

        public static Position3 operator -(Position3 left, Position3 right)
        {
            return new Position3(
                left._x - right._x,
                left._y - right._y,
                left._z - right._z);
        }

        public static bool operator ==(Position3 left, Position3 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Position3 left, Position3 right)
        {
            return !Equals(left, right);
        }

        public static Position3 From(Vector3 vector3)
        {
            return new Position3(
                Mathf.FloorToInt(vector3.x),
                Mathf.FloorToInt(vector3.y),
                Mathf.FloorToInt(vector3.z));
        }

        private static bool Equals(Position3 a, Position3 b)
        {
            return a._x == b._x && a._y == b._y && a._z == b._z;
        }
    }
}
