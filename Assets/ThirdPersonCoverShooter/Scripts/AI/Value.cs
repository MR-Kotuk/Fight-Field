
using System;
using UnityEngine;

namespace CoverShooter.AI
{
    public enum ValueType
    {
        Unknown,
        Float,
        Vector2,
        Vector3,
        Boolean,
        GameObject,
        Text,
        Array,
        RelativeDirection,
        Speed,
        Facing,
        Team,
        Weapon,
        AudioClip
    }

    public enum ActorTeam
    {
        All,
        Friendly,
        Enemy
    }

    public enum ArithmeticValueType
    {
        Float = ValueType.Float,
        Vector2 = ValueType.Vector2,
        Vector3 = ValueType.Vector3
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class IgnoreNextValuesAttribute : Attribute
    {
        public IgnoreNextValuesAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public class ValueTypeAttribute : Attribute
    {
        public ValueType Type;
        public bool AcceptArrays;

        public ValueTypeAttribute(ValueType type, bool acceptArrays = false)
        {
            Type = type;
            AcceptArrays = acceptArrays;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DefaultValueTypeAttribute : Attribute
    {
        public ValueType Type;

        public DefaultValueTypeAttribute(ValueType type)
        {
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NoFieldNameAttribute : Attribute
    {
        public bool Value;

        public NoFieldNameAttribute(bool value = true)
        {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class NoConstantAttribute : Attribute
    {
        public bool Value;

        public NoConstantAttribute(bool value = true)
        {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class OnlyArithmeticTypeAttribute : Attribute
    {
        public bool Value;

        public OnlyArithmeticTypeAttribute(bool value = true)
        {
            Value = value;
        }
    }

    [Serializable]
    public struct Value
    {
        public ValueType Type;
        public int ID;
        public Vector3 Vector;
        public UnityEngine.Object Object;
        public string Text;
        public Value[] Array;
        public ValueType SubType;

        [NonSerialized]
        public Rect LocalEditorRect;

        public bool IsConstant { get { return ID == 0; } }

        public GameObject GameObject
        {
            get { return Object as GameObject; }
            set { Object = value; }
        }

        public AudioClip AudioClip
        {
            get { return Object as AudioClip; }
            set { Object = value; }
        }

        public bool Bool
        {
            get { return (int)Vector.x != 0; }
            set { Vector.x = value ? 1 : 0; }
        }

        public int Int
        {
            get { return (int)Vector.x; }
            set { Vector.x = value; }
        }

        public int Count
        {
            get { return (int)Vector.x; }
            set { Vector.x = value; }
        }

        public float Float
        {
            get { return Vector.x; }
            set { Vector.x = value; }
        }

        public Vector2 Vector2
        {
            get { return new Vector2(Vector.x, Vector.y); }
            set
            {
                Vector.x = value.x;
                Vector.y = value.y;
            }
        }

        public Vector3 Vector3
        {
            get { return Vector; }
            set { Vector = value; }
        }

        public Direction Direction
        {
            get { return (Direction)(int)Vector.x; }
            set { Vector.x = (int)value; }
        }

        public CharacterSpeed Speed
        {
            get { return (CharacterSpeed)(int)Vector.x; }
            set { Vector.x = (int)value; }
        }

        public CharacterFacing Facing
        {
            get { return (CharacterFacing)(int)Vector.x; }
            set { Vector.x = (int)value; }
        }

        public ActorTeam Team
        {
            get { return (ActorTeam)(int)Vector.x; }
            set { Vector.x = (int)value; }
        }

        public WeaponType Weapon
        {
            get { return (WeaponType)(int)Vector.x; }
            set { Vector.x = (int)value; }
        }

        public static Value Variable(ValueType type, int id)
        {
            var value = new Value();
            value.Type = type;
            value.ID = id;

            return value;
        }

        public static Value Builtin(BuiltinValue b)
        {
            var v = new Value();
            v.ID = -(int)b;
            return v;
        }

        public Value(Vector3 v, ValueType type = ValueType.Vector3)
        {
            ID = 0;
            Type = type;
            SubType = ValueType.Unknown;
            Text = null;
            Vector = v;
            Object = null;
            Array = null;
            LocalEditorRect = new Rect(0, 0, 0, 0);
        }

        public Value(GameObject obj)
        {
            ID = 0;
            Type = ValueType.GameObject;
            SubType = ValueType.Unknown;
            Vector = Vector3.zero;
            Text = null;
            Object = obj;
            Array = null;
            LocalEditorRect = new Rect(0, 0, 0, 0);
        }

        public Value(AudioClip clip)
        {
            ID = 0;
            Type = ValueType.GameObject;
            SubType = ValueType.Unknown;
            Vector = Vector3.zero;
            Text = null;
            Object = clip;
            Array = null;
            LocalEditorRect = new Rect(0, 0, 0, 0);
        }

        public Value(string text)
        {
            ID = 0;
            Type = ValueType.Text;
            SubType = ValueType.Unknown;
            Vector = Vector3.zero;
            Text = text;
            Object = null;
            Array = null;
            LocalEditorRect = new Rect(0, 0, 0, 0);
        }

        public Value(Value[] array, int count, ValueType subtype)
        {
            ID = 0;
            Type = ValueType.Array;
            SubType = subtype;
            Vector = new Vector3(count, 0, 0);
            Text = null;
            Object = null;
            Array = array;

            if (array.Length < count)
                Debug.Assert(array.Length >= count);

            LocalEditorRect = new Rect(0, 0, 0, 0);
        }

        public Value(Vector2 v) : this(new Vector3(v.x, v.y, 0), ValueType.Vector2) { }
        public Value(float f) : this(new Vector3(f, 0, 0), ValueType.Float) { }
        public Value(bool b) : this(new Vector3(b ? 1 : 0, 0, 0), ValueType.Boolean) { }
        public Value(Direction direction) : this(new Vector3((int)direction, 0, 0), ValueType.RelativeDirection) { }
        public Value(CharacterSpeed speed) : this(new Vector3((int)speed, 0, 0), ValueType.Speed) { }
        public Value(CharacterFacing facing) : this(new Vector3((int)facing, 0, 0), ValueType.Facing) { }
        public Value(ActorTeam team) : this(new Vector3((int)team, 0, 0), ValueType.Team) { }
        public Value(WeaponType weapon) : this(new Vector3((int)weapon, 0, 0), ValueType.Weapon) { }

        public bool IsEqual(ref Value other)
        {
            if (ID > 0)
                return ID == other.ID;

            if (Type != other.Type)
                return false;

            switch (Type)
            {
                case ValueType.Unknown: return Vector == other.Vector && GameObject == other.GameObject && Type == other.Type && Array == other.Array;
                case ValueType.Float: return Float == other.Float;
                case ValueType.Vector2: return Vector2 == other.Vector2;
                case ValueType.Vector3: return Vector == other.Vector;
                case ValueType.Boolean: return Bool == other.Bool;
                case ValueType.GameObject: return GameObject == other.GameObject;
                case ValueType.Text: return Text == other.Text;
                case ValueType.RelativeDirection: return Direction == other.Direction;
                case ValueType.Speed: return Speed == other.Speed;
                case ValueType.Facing: return Facing == other.Facing;
                case ValueType.Team: return Team == other.Team;
                case ValueType.Weapon: return Weapon == other.Weapon;
                case ValueType.AudioClip: return AudioClip == other.AudioClip;

                case ValueType.Array:
                    if (Count != other.Count)
                        return false;

                    for (int i = 0; i < Count; i++)
                        if (!Array[i].IsEqual(ref other.Array[i]))
                            return false;

                    return true;
            }

            return false;
        }

        public static void Add(ref Value[] array, ref int count, Value value)
        {
            if (array == null)
            {
                array = new Value[1] { value };
                count = 1;
            }
            else if (array.Length > count)
            {
                array[count] = value;
                count++;
            }
            else
            {
                var old = array;
                array = new Value[old.Length + 1];

                for (int i = 0; i < old.Length; i++)
                    array[i] = old[i];

                array[old.Length] = value;
                count = array.Length;
            }
        }

        public string GetText(Brain brain)
        {
            if (ID != 0)
                return brain.GetText(ID);

            switch (Type)
            {
                case ValueType.Float: return Vector.x.ToString();
                case ValueType.Vector2: return "[" + Vector.x.ToString() + ", " + Vector.y.ToString() + "]";
                case ValueType.Vector3: return "[" + Vector.x.ToString() + ", " + Vector.y.ToString() + ", " + Vector.z.ToString() + "]";
                case ValueType.Boolean: return Bool ? "True" : "False";
                case ValueType.GameObject: return "Object";
                case ValueType.AudioClip: return "AudioClip";
                case ValueType.Text: return Text;
                case ValueType.RelativeDirection: return Direction.ToString();
                case ValueType.Speed: return Speed.ToString();
                case ValueType.Facing: return Facing.ToString();
                case ValueType.Team: return Team.ToString();
                default: return "Unknown";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Value)
                return Equals((Value)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private static bool eq(float a, float b)
        {
            return a >= b - 0.001f && a <= b + 0.001f;
        }

        public bool Equals(Value value)
        {
            if (Type != value.Type)
                return false;

            switch (Type)
            {
                case ValueType.Unknown: return false;
                case ValueType.Float: return eq(Vector.x, value.Vector.x);
                case ValueType.Vector2: return eq(Vector.x, value.Vector.x) && eq(Vector.y, value.Vector.y);
                case ValueType.Vector3: return eq(Vector.x, value.Vector.x) && eq(Vector.y, value.Vector.y) && eq(Vector.z, value.Vector.z);
                case ValueType.Boolean: return Bool == value.Bool;
                case ValueType.GameObject: return GameObject == value.GameObject;
                case ValueType.Text: return Text == value.Text;
                case ValueType.Array:
                    if (Count != value.Count)
                        return false;

                    for (int i = 0; i < Count; i++)
                        if (!Array[i].Equals(value.Array[i]))
                            return false;

                    return true;

                case ValueType.RelativeDirection: return Direction == value.Direction;
                case ValueType.Speed: return Speed == value.Speed;
                case ValueType.Facing: return Facing == value.Facing;
                case ValueType.Team: return Team == value.Team;
                case ValueType.Weapon: return Weapon == value.Weapon;
                case ValueType.AudioClip: return AudioClip == value.AudioClip;
            }

            return false;
        }
    }
}