#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class EnumPopupExtensions
    {
        public static T EnumPopup<T>(T selected, bool filterEnabled, int width, params T[] filterValues) where T : Enum
        {
            Array allEnumValues = Enum.GetValues(typeof(T));
            Array filteredEnumValues = filterEnabled ? filterValues : allEnumValues;
            string[] enumNames = Array.ConvertAll((T[])filteredEnumValues, e => e.ToString());

            int selectedIndex = Array.IndexOf(filteredEnumValues, selected);
            if (selectedIndex < 0) selectedIndex = 0;

            int newIndex = EditorGUILayout.Popup(selectedIndex, enumNames, GUILayout.Width(width));
            if (newIndex < 0 || newIndex >= filteredEnumValues.Length) newIndex = 0;

            return (T)filteredEnumValues.GetValue(newIndex);
        }
    }
}
#endif
