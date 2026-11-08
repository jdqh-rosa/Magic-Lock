using UnityEditor;
using UnityEngine;

namespace MagicLock.Radial.Editor
{
    /// <summary>Draws an <see cref="Optional{T}"/> as a checkbox followed by its value, greyed out while unchecked.</summary>
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalDrawer : PropertyDrawer
    {
        private const float ToggleWidth = 18f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"), label, true);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty enabled = property.FindPropertyRelative("enabled");
            SerializedProperty value = property.FindPropertyRelative("value");

            EditorGUI.BeginProperty(position, label, property);

            Rect indented = EditorGUI.IndentedRect(position);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var toggleRect = new Rect(indented.x, position.y, ToggleWidth, EditorGUIUtility.singleLineHeight);
            enabled.boolValue = EditorGUI.Toggle(toggleRect, enabled.boolValue);

            var valueRect = new Rect(indented.x + ToggleWidth, position.y, indented.width - ToggleWidth, position.height);
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Max(60f, labelWidth - (indented.x - position.x) - ToggleWidth);
            using (new EditorGUI.DisabledScope(!enabled.boolValue)) {
                EditorGUI.PropertyField(valueRect, value, label, true);
            }

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
}
