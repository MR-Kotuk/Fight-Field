using System;
using System.Reflection;
using UnityEngine;

namespace CoverShooter.AI
{
    [Serializable]
    public class ExpressionNode : LayerNode
    {
        public BaseExpression Expression { get { return (BaseExpression)Attachment; } }

        public ExpressionNode(BaseExpression expression)
            : base(expression)
        {
        }

        public Value Evaluate(int id, State state)
        {
            return Expression.Evaluate(id, state);
        }
    }
}