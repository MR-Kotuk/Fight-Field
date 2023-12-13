using UnityEngine;

namespace CoverShooter
{
    public enum LocationType
    {
        Trigger,
        TriggerInput,
        TriggerVariable,
        Action,
        ActionValue,
        ExtensionValue,
        Expression,
        ExpressionValue
    }

    public struct Location
    {
        public LocationType Type;
        public int Id;
        public int Index;

        public static Location Trigger(int id)
        {
            Location location;
            location.Type = LocationType.Trigger;
            location.Id = id;
            location.Index = 0;

            return location;
        }

        public static Location TriggerInput(int id)
        {
            Location location;
            location.Type = LocationType.TriggerInput;
            location.Id = id;
            location.Index = 0;

            return location;
        }

        public static Location TriggerVariable(int id)
        {
            Location location;
            location.Type = LocationType.TriggerVariable;
            location.Id = id;
            location.Index = 0;

            return location;
        }

        public static Location Action(int id)
        {
            Location location;
            location.Type = LocationType.Action;
            location.Id = id;
            location.Index = 0;

            return location;
        }

        public static Location ActionValue(int id, int index)
        {
            Location location;
            location.Type = LocationType.ActionValue;
            location.Id = id;
            location.Index = index;

            return location;
        }

        public static Location ExtensionValue(int id, int index)
        {
            Location location;
            location.Type = LocationType.ExtensionValue;
            location.Id = id;
            location.Index = index;

            return location;
        }

        public static Location ExpressionValue(int id, int index)
        {
            Location location;
            location.Type = LocationType.ExpressionValue;
            location.Id = id;
            location.Index = index;

            return location;
        }

        public static Location Expression(int id)
        {
            Location location;
            location.Type = LocationType.Expression;
            location.Id = id;
            location.Index = 0;

            return location;
        }
    }
}