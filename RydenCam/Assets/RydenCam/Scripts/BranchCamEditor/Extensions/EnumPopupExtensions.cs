using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Assets.RydenCam.Scripts.BranchCamEditor.Extensions
{
    public static class EnumPopupExtensions
    {
        // The custom EnumPopup that supports filtering
        public static T EnumPopup<T>(T selected, bool filterEnabled, int width, params T[] filterValues) where T : Enum
        {
            // Get all enum values
            Array allEnumValues = Enum.GetValues(typeof(T));

            // Filter the values based on the condition
            Array filteredEnumValues = filterEnabled ? filterValues : allEnumValues;

            // Convert the filtered values into a string array for the popup
            string[] enumNames = Array.ConvertAll((T[])filteredEnumValues, e => e.ToString());

            // Find the current selected index
            int selectedIndex = Array.IndexOf(filteredEnumValues, selected);

            // Display the popup
            int newIndex = EditorGUILayout.Popup(selectedIndex, enumNames, GUILayout.Width(width));

            // Update and return the selected enum value
            return (T)filteredEnumValues.GetValue(newIndex);

        }
    }
}