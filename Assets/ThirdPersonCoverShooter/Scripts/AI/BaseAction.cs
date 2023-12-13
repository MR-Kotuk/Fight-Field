using System;

namespace CoverShooter.AI
{
    public struct ValueDesc
    {
        public string Name;
        public ValueType Type;

        public ValueDesc(string name, ValueType type)
        {
            Name = name;
            Type = type;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class SuccessAttribute : Attribute
    {
        public string Name;

        public SuccessAttribute(string name = "Success")
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class FailureAttribute : Attribute
    {
        public string Name;

        public FailureAttribute(string name = "Failure")
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SuccessParameterAttribute : Attribute
    {
        public string Name;
        public ValueType Type;
        public bool IsArray;

        public SuccessParameterAttribute(string name, ValueType type, bool isArray = false)
        {
            Name = name;
            Type = type;
            IsArray = isArray;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class FailureParameterAttribute : Attribute
    {
        public string Name;
        public ValueType Type;
        public bool IsArray;

        public FailureParameterAttribute(string name, ValueType type, bool isArray = false)
        {
            Name = name;
            Type = type;
            IsArray = isArray;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ImmediateAttribute : Attribute
    {
        public bool IsImmediate;

        public ImmediateAttribute(bool isImmediate = true)
        {
            IsImmediate = isImmediate;
        }
    }

    public abstract class BaseAction
    {
        public bool AllowMoveExtension { get { return _allowMoveExtension; } }
        public bool AllowAimExtension { get { return _allowAimExtension; } }
        public bool AllowFireExtension { get { return _allowFireExtension; } }
        public bool AllowCrouchExtension { get { return _allowCrouchExtension; } }

        private bool _allowMoveExtension = false;
        private bool _allowAimExtension = false;
        private bool _allowFireExtension = false;
        private bool _allowCrouchExtension = false;

        public BaseAction()
        {
            _allowMoveExtension = BaseExtension.AllowsMove(GetType());
            _allowAimExtension = BaseExtension.AllowsAim(GetType());
            _allowFireExtension = BaseExtension.AllowsFire(GetType());
            _allowCrouchExtension = BaseExtension.AllowsCrouch(GetType());
        }

        /// <summary>
        /// Called when the node is entered.
        /// </summary>
        public virtual void Enter(State state, int layer, ref ActionState values) { }

        /// <summary>
        /// Called when the node is exited.
        /// </summary>
        public virtual void Exit(State state, int layer, ref ActionState values) { }

        /// <summary>
        /// Called every frame when the node is active.
        /// </summary>
        public abstract AIResult Update(State state, int layer, ref ActionState values);

        /// <summary>
        /// Return trigger that is set off by this node.
        /// </summary>
        public virtual Trigger GetTrigger(Brain brain) { return null; }

        /// <summary>
        /// Return event that is sent by this node. If it is 'trigger' it is ignorred.
        /// </summary>
        public virtual AIEvent GetEvent(Brain brain) { return AIEvent.Trigger; }
    }
}