using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CoverShooter.AI;

namespace CoverShooter
{
    public static class BrainEditorUtil
    {
        public static GUISkin Skin
        {
            get
            {
                CheckSetup();
                return _skin;
            }
        }

        private static Texture2D _actionOutput;

        private static GUISkin _skin;
        private static Dictionary<string, int> _map = new Dictionary<string, int>();

        public static float ActionInputSize = 15;
        public static float ActionOutputSize = 15;
        public static float ValueInputSize = 15;
        public static float ExpressionOutputSize = 15;
        public static float ExitWidth = 150;
        public static float ExitHeight = 30;
        public static float FailWidth = 150;
        public static float FailHeight = 30;
        public static float PropertyExtraWidth = 75;

        private static bool _isSetup;

        public static GUIStyle GetStyle(string name)
        {
            int value;
            if (_map.TryGetValue(name, out value))
                if (value >= 0 && value < _skin.customStyles.Length)
                    return _skin.customStyles[value];

            for (int i = 0; i < _skin.customStyles.Length; i++)
                if (_skin.customStyles[i].name == name)
                {
                    _map[name] = i;
                    return _skin.customStyles[i];
                }

            return null;
        }

        public static void UpdateSlotSizes()
        {
            ActionInputSize = GetStyle("ActionInputLeftOk").fixedWidth;
            ActionOutputSize = GetStyle("ActionOutputOk").fixedWidth;
            ValueInputSize = GetStyle("ValueInputOk").fixedWidth;
            ExpressionOutputSize = GetStyle("ExpressionOutputOk").fixedWidth;
            ExitWidth = GetStyle("ExitWindow").fixedWidth;
            ExitHeight = GetStyle("ExitWindow").fixedHeight;
            FailWidth = GetStyle("FailWindow").fixedWidth;
            FailHeight = GetStyle("FailWindow").fixedHeight;
        }

        public static void CheckSetup()
        {
            if (_isSetup)
            {
                UpdateSlotSizes();
                return;
            }

            _skin = (GUISkin)EditorGUIUtility.Load("AiEditor/Skin.guiskin");
            _actionOutput = (Texture2D)EditorGUIUtility.Load("ThirdPersonCoverShooter/out.png");
            UpdateSlotSizes();

            _isSetup = true;
        }

        public static void HorizontalSpace()
        {
            CheckSetup();
            GUILayout.Label(new GUIContent(), GetStyle("HorizontalSpace"));
        }

        public static bool VariableButton()
        {
            CheckSetup();
            return GUILayout.Button(new GUIContent(), GetStyle("Variable"));
        }

        public static bool AddButton()
        {
            CheckSetup();
            return GUILayout.Button(new GUIContent(), GetStyle("Add"));
        }

        public static bool DeleteButton()
        {
            CheckSetup();
            return GUILayout.Button(new GUIContent(), GetStyle("Delete"));
        }

        public static bool UpButton()
        {
            CheckSetup();
            return GUILayout.Button(new GUIContent(), GetStyle("Up"));
        }

        public static bool DownButton()
        {
            CheckSetup();
            return GUILayout.Button(new GUIContent(), GetStyle("Down"));
        }

        public static bool ValueInput(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ValueInputOk") : GetStyle("ValueInputNone"));
        }

        public static bool ExpressionOutput(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ExpressionOutputOk") : GetStyle("ExpressionOutputNone"));
        }

        public static bool ActionInputLeft(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ActionInputLeftOk") : GetStyle("ActionInputLeftNone"));
        }

        public static bool ActionInputTop(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ActionInputTopOk") : GetStyle("ActionInputTopNone"));
        }

        public static bool ActionInputBottom(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ActionInputBottomOk") : GetStyle("ActionInputBottomNone"));
        }

        public static bool ActionOutput(Rect rect, bool isConnected)
        {
            CheckSetup();
            return GUI.Button(rect, new GUIContent(), isConnected ? GetStyle("ActionOutputOk") : GetStyle("ActionOutputNone"));
        }

        public static bool WindowTitle(string name, bool canDelete = false)
        {
            CheckSetup();
            GUILayout.BeginHorizontal(GetStyle("WindowTitleBackground"));
            GUILayout.Label(name, GetStyle("WindowTitleText"));

            bool result = true;

            if (canDelete)
                if (DeleteButton())
                    result = false;

            GUILayout.EndHorizontal();

            return result;
        }

        public static bool BoxTitle(string name, bool canDelete = false)
        {
            CheckSetup();
            GUILayout.BeginHorizontal(GetStyle("BoxTitleBackground"));
            GUILayout.Label(name, GetStyle("BoxTitleText"));

            bool result = true;

            if (canDelete)
                if (DeleteButton())
                    result = false;

            GUILayout.EndHorizontal();

            return result;
        }

        public static bool NoContentBox(string name, bool canDelete = false)
        {
            CheckSetup();
            GUILayout.BeginHorizontal(GetStyle("NoContentBox"));
            GUILayout.Label(name, GetStyle("BoxTitleText"));

            bool result = true;

            if (canDelete)
                if (DeleteButton())
                    result = false;

            GUILayout.EndHorizontal();

            return result;
        }

        public static GUIStyle GetTitledBox()
        {
            return GetStyle("TitledBox");
        }

        public static float GetEventWidth(AIEvent e)
        {
            return GetStyle("DropDown").CalcSize(new GUIContent("some event name")).x;
        }

        public static void EditEvent(ref AIEvent e)
        {
            e = (AIEvent)EditorGUILayout.EnumPopup(e, GetStyle("DropDown")); 
        }

        public static float GetValueWidth(ref Value value)
        {
            switch (value.Type)
            {
                case AI.ValueType.Boolean: return Skin.toggle.CalcSize(new GUIContent("True")).x;
                case AI.ValueType.Float: return Skin.textField.CalcSize(new GUIContent("9999.99")).x;
                case AI.ValueType.Vector2: return Skin.textField.CalcSize(new GUIContent("9999.99")).x * 2 + 10;
                case AI.ValueType.Vector3: return Skin.textField.CalcSize(new GUIContent("9999.99")).x * 3 + 10;
                case AI.ValueType.GameObject: return Skin.textField.CalcSize(new GUIContent("some object name")).x;
                case AI.ValueType.AudioClip: return Skin.textField.CalcSize(new GUIContent("some object name")).x;
                case AI.ValueType.RelativeDirection: return GetStyle("DropDown").CalcSize(new GUIContent("enum value")).x;
                case AI.ValueType.Speed: return GetStyle("DropDown").CalcSize(new GUIContent("enum value")).x;
                case AI.ValueType.Facing: return GetStyle("DropDown").CalcSize(new GUIContent("enum value")).x;
                case AI.ValueType.Team: return GetStyle("DropDown").CalcSize(new GUIContent("enum value")).x;
                case AI.ValueType.Weapon: return GetStyle("DropDown").CalcSize(new GUIContent("enum value")).x;
                case AI.ValueType.Text: return Skin.textField.CalcSize(new GUIContent(value.Text)).x;
            }

            return 10;
        }

        public static void EditValueWithStyle(ref Value value, bool allowSceneObjects)
        {
            switch (value.Type)
            {
                case AI.ValueType.Boolean: value.Bool = EditorGUILayout.Toggle(value.Bool, Skin.toggle); break;
                case AI.ValueType.Float: value.Float = EditorGUILayout.FloatField(value.Float, Skin.textField); break;
                case AI.ValueType.Vector2:
                    {
                        var v = value.Vector2;
                        GUILayout.BeginHorizontal();
                        v.x = EditorGUILayout.FloatField(v.x, Skin.textField);
                        v.y = EditorGUILayout.FloatField(v.y, Skin.textField);
                        GUILayout.EndHorizontal();

                        value.Vector2 = v;
                    }
                    break;
                case AI.ValueType.Vector3:
                    {
                        var v = value.Vector3;
                        GUILayout.BeginHorizontal();
                        v.x = EditorGUILayout.FloatField(v.x, Skin.textField);
                        v.y = EditorGUILayout.FloatField(v.y, Skin.textField);
                        v.z = EditorGUILayout.FloatField(v.z, Skin.textField);
                        GUILayout.EndHorizontal();

                        value.Vector3 = v;
                    }
                    break;
                case AI.ValueType.GameObject: value.GameObject = (GameObject)EditorGUILayout.ObjectField(value.GameObject, typeof(GameObject), allowSceneObjects); break;
                case AI.ValueType.AudioClip: value.AudioClip = (AudioClip)EditorGUILayout.ObjectField(value.AudioClip, typeof(AudioClip), allowSceneObjects); break;
                case AI.ValueType.RelativeDirection: value.Direction = (Direction)EditorGUILayout.EnumPopup(value.Direction, GetStyle("DropDown")); break;
                case AI.ValueType.Speed: value.Speed = (CharacterSpeed)EditorGUILayout.EnumPopup(value.Speed, GetStyle("DropDown")); break;
                case AI.ValueType.Facing: value.Facing = (CharacterFacing)EditorGUILayout.EnumPopup(value.Facing, GetStyle("DropDown")); break;
                case AI.ValueType.Team: value.Team = (ActorTeam)EditorGUILayout.EnumPopup(value.Team, GetStyle("DropDown")); break;
                case AI.ValueType.Weapon: value.Weapon = (WeaponType)EditorGUILayout.EnumPopup(value.Weapon, GetStyle("DropDown")); break;
                case AI.ValueType.Text: value.Text = GUILayout.TextField(value.Text, Skin.textField); break;
            }
        }

        public static void EditValue(ref Value value, bool allowSceneObjects)
        {
            switch (value.Type)
            {
                case AI.ValueType.Boolean: value.Bool = EditorGUILayout.Toggle(value.Bool); break;
                case AI.ValueType.Float: value.Float = EditorGUILayout.FloatField(value.Float); break;
                case AI.ValueType.Vector2:
                    {
                        var v = value.Vector2;
                        GUILayout.BeginHorizontal();
                        v.x = EditorGUILayout.FloatField(v.x);
                        v.y = EditorGUILayout.FloatField(v.y);
                        GUILayout.EndHorizontal();

                        value.Vector2 = v;
                    }
                    break;
                case AI.ValueType.Vector3:
                    {
                        var v = value.Vector3;
                        GUILayout.BeginHorizontal();
                        v.x = EditorGUILayout.FloatField(v.x);
                        v.y = EditorGUILayout.FloatField(v.y);
                        v.z = EditorGUILayout.FloatField(v.z);
                        GUILayout.EndHorizontal();

                        value.Vector3 = v;
                    }
                    break;
                case AI.ValueType.GameObject: value.GameObject = (GameObject)EditorGUILayout.ObjectField(value.GameObject, typeof(GameObject), allowSceneObjects); break;
                case AI.ValueType.AudioClip: value.AudioClip = (AudioClip)EditorGUILayout.ObjectField(value.AudioClip, typeof(AudioClip), allowSceneObjects); break;
                case AI.ValueType.RelativeDirection: value.Direction = (Direction)EditorGUILayout.EnumPopup(value.Direction); break;
                case AI.ValueType.Speed: value.Speed = (CharacterSpeed)EditorGUILayout.EnumPopup(value.Speed); break;
                case AI.ValueType.Facing: value.Facing = (CharacterFacing)EditorGUILayout.EnumPopup(value.Facing); break;
                case AI.ValueType.Team: value.Team = (ActorTeam)EditorGUILayout.EnumPopup(value.Team); break;
                case AI.ValueType.Weapon: value.Weapon = (WeaponType)EditorGUILayout.EnumPopup(value.Weapon); break;
                case AI.ValueType.Text: value.Text = GUILayout.TextField(value.Text); break;
            }
        }
    }
}