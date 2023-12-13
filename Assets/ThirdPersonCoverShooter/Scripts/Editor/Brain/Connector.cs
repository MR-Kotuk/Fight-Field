using CoverShooter.AI;

namespace CoverShooter
{
    public abstract class Connector
    {
        public Location Source;
        public bool IsValid;

        public abstract void Check(LocationType type);

        public virtual bool AcceptValue(Brain brain, ref Value value) { return false; }
        public virtual bool AcceptId(Brain brain, int id) { return false; }
        public virtual bool AcceptIdIndex(Brain brain, int id, int index) { return false; }
        public virtual void Clear(Brain brain) { }
    }

    public class ToValue : Connector
    {
        public ToValue(Location from)
        {
            Source = from;
        }

        public override void Check(LocationType type)
        {
            if (Source.Type != LocationType.Expression &&
                Source.Type != LocationType.TriggerVariable)
            {
                IsValid = false;
                return;
            }

            switch (type)
            {
                case LocationType.ActionValue:
                case LocationType.TriggerInput:
                case LocationType.ExpressionValue:
                case LocationType.ExtensionValue:
                    IsValid = true;
                    break;

                default:
                    IsValid = false;
                    break;
            }
        }

        public override bool AcceptValue(Brain brain, ref Value value)
        {
            if (Source.Type != LocationType.Expression &&
                Source.Type != LocationType.TriggerVariable)
                return false;

            value.ID = Source.Id;
            return true;
        }
    }

    public class ToExpression : Connector
    {
        public ToExpression(Location from)
        {
            Source = from;
        }

        public override void Check(LocationType type)
        {
            if (Source.Type != LocationType.ActionValue &&
                Source.Type != LocationType.ExpressionValue &&
                Source.Type != LocationType.TriggerInput &&
                Source.Type != LocationType.ExtensionValue)
            {
                IsValid = false;
                return;
            }

            switch (type)
            {
                case LocationType.Expression:
                case LocationType.TriggerVariable:
                    IsValid = true;
                    break;

                default:
                    IsValid = false;
                    break;
            }
        }

        public override void Clear(Brain brain)
        {
            AcceptId(brain, 0);
        }

        public override bool AcceptId(Brain brain, int id)
        {
            if (Source.Type == LocationType.TriggerInput)
            {
                var trigger = brain.GetNodeTrigger(Source.Id);

                if (trigger == null)
                    return false;

                if (trigger.Type != NodeTriggerType.Expression)
                    return false;

                trigger.Expression.ID = id;
                return true;
            }
            else if (Source.Type == LocationType.ActionValue)
            {
                var action = brain.GetAction(Source.Id);

                if (action == null)
                    return false;

                var value = action.GetValue(Source.Index);
                value.ID = id;
                action.SetValue(Source.Index, value);

                return true;
            }
            else if (Source.Type == LocationType.ExpressionValue)
            {
                var expression = brain.GetExpression(Source.Id);

                if (expression == null)
                    return false;

                var value = expression.GetValue(Source.Index);
                value.ID = id;
                expression.SetValue(Source.Index, value);

                return true;
            }
            else if (Source.Type == LocationType.ExtensionValue)
            {
                var extension = brain.GetExtension(Source.Id);

                if (extension == null)
                    return false;

                var value = extension.GetValue(Source.Index);
                value.ID = id;
                extension.SetValue(Source.Index, value);

                return true;
            }
            else
                return false;
        }
    }

    public class ToAction : Connector
    {
        public ToAction(Location from)
        {
            Source = from;
        }

        public override void Check(LocationType type)
        {
            IsValid = type == LocationType.Action &&
                      Source.Type == LocationType.Trigger;
        }

        public override void Clear(Brain brain)
        {
            if (Source.Type != LocationType.Trigger)
                return;

            var trigger = brain.GetNodeTrigger(Source.Id);

            if (trigger == null)
                return;

            trigger.Target = 0;
        }

        public override bool AcceptId(Brain brain, int id)
        {
            if (Source.Type != LocationType.Trigger)
                return false;

            var trigger = brain.GetNodeTrigger(Source.Id);

            if (trigger == null)
                return false;

            if (id >= 0)
            {
                var action = brain.GetAction(id);

                if (action == null)
                    return false;
            }

            trigger.Target = id;
            return true;
        }
    }

    public class ToTrigger : Connector
    {
        public ToTrigger(Location from)
        {
            Source = from;
        }

        public override void Check(LocationType type)
        {
            IsValid = type == LocationType.Trigger && 
                      Source.Type == LocationType.Action;
        }

        public override bool AcceptId(Brain brain, int id)
        {
            if (Source.Type != LocationType.Action)
                return false;

            var trigger = brain.GetNodeTrigger(id);

            if (trigger == null)
                return false;

            trigger.Target = Source.Id;
            return true;
        }
    }
}