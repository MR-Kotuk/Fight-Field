using System;

namespace CoverShooter.AI
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class MinimumCountAttribute : Attribute
    {
        public int Count;

        public MinimumCountAttribute(int count)
        {
            Count = count;
        }
    }

    public abstract class BaseExpression
    {
        public abstract Value Evaluate(int id, State state);

        public virtual string GetText(Brain brain)
        {
            return GetType().Name + "()";
        }

        public abstract ValueType GetReturnType(Brain brain);
    }
}