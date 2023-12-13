using System;

namespace CoverShooter.AI
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AllowMoveAttribute : Attribute
    {
        public bool Allow;

        public AllowMoveAttribute(bool allow = true)
        {
            Allow = allow;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AllowAimAndFireAttribute : Attribute
    {
        public bool Allow;

        public AllowAimAndFireAttribute(bool allow = true)
        {
            Allow = allow;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AllowFireAttribute : Attribute
    {
        public bool Allow;

        public AllowFireAttribute(bool allow = true)
        {
            Allow = allow;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AllowAimAttribute : Attribute
    {
        public bool Allow;

        public AllowAimAttribute(bool allow = true)
        {
            Allow = allow;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class AllowCrouchAttribute : Attribute
    {
        public bool Allow;

        public AllowCrouchAttribute(bool allow = true)
        {
            Allow = allow;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ExtensionAttribute : Attribute
    {
        public ExtensionClass Class;

        public ExtensionAttribute(ExtensionClass class_)
        {
            Class = class_;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class FolderAttribute : Attribute
    {
        public string Folder;

        public FolderAttribute(string folder)
        {
            Folder = folder;
        }
    }

    public enum ExtensionClass
    {
        Move,
        Aim,
        Fire,
        AimAndFire,
        Crouch
    }

    public abstract class BaseExtension
    {
        public ExtensionClass Class { get { return _class; } }
        public string Name { get { return _name; } }

        private ExtensionClass _class;
        private string _name;

        public BaseExtension()
        {
            _name = GetName(GetType());
            _class = GetClass(GetType());
        }

        public virtual void Begin(State state, int layer, ref ExtensionState values) { }
        public virtual void Update(State state, int layer, ref ExtensionState values) { }
        public virtual void End(State state, int layer, ref ExtensionState values) { }

        public static ExtensionClass GetClass(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            if (attributes != null)
                for (int i = 0; i < attributes.Length; i++)
                    if (attributes[i] is ExtensionAttribute)
                        return ((ExtensionAttribute)attributes[i]).Class;

            return ExtensionClass.Move;
        }

        public static bool AllowsMove(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            if (attributes != null)
                for (int i = 0; i < attributes.Length; i++)
                    if (attributes[i] is AllowMoveAttribute)
                        return ((AllowMoveAttribute)attributes[i]).Allow;

            return false;
        }

        public static bool AllowsAim(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            if (attributes != null)
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i] is AllowAimAndFireAttribute)
                        return ((AllowAimAndFireAttribute)attributes[i]).Allow;
                    else if (attributes[i] is AllowAimAttribute)
                        return ((AllowAimAttribute)attributes[i]).Allow;
                }

            return false;
        }

        public static bool AllowsFire(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            if (attributes != null)
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i] is AllowAimAndFireAttribute)
                        return ((AllowAimAndFireAttribute)attributes[i]).Allow;
                    else if (attributes[i] is AllowFireAttribute)
                        return ((AllowFireAttribute)attributes[i]).Allow;
                }

            return false;
        }

        public static bool AllowsCrouch(Type type)
        {
            var attributes = type.GetCustomAttributes(false);

            if (attributes != null)
                for (int i = 0; i < attributes.Length; i++)
                {
                    if (attributes[i] is AllowCrouchAttribute)
                        return ((AllowCrouchAttribute)attributes[i]).Allow;
                    else if (attributes[i] is AllowCrouchAttribute)
                        return ((AllowCrouchAttribute)attributes[i]).Allow;
                }

            return false;
        }

        public static string GetName(Type type)
        {
            var name = type.Name;

            if (name.Length > "Extension".Length && name.LastIndexOf("Extension") == name.Length - "Extension".Length)
                return name.Substring(0, name.Length - "Extension".Length);
            else
                return name;
        }
    }

    public struct ExtensionState
    {
        public float Time;
        public int Count;
    }
}