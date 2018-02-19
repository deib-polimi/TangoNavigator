using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(DialogItemParameter))]
    public class DialogItemParameterEditor : Editor
    {
        SerializedProperty toggleItems;
        GUIContent toggleItemsLabel = new GUIContent("Toggle Items");

        private void OnEnable()
        {
            toggleItems = serializedObject.FindProperty("toggleItems");
        }

        public override void OnInspectorGUI()
        {
            var obj = target as DialogItemParameter;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target), typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.type = (DialogItemType)EditorGUILayout.EnumPopup("Item Type", obj.type);

            switch (obj.type)
            {
                case DialogItemType.Divisor:
                    obj.lineHeight = EditorGUILayout.FloatField("Line Height", obj.lineHeight);
                    obj.lineColor = EditorGUILayout.ColorField("Line Color", obj.lineColor);
                    break;
                case DialogItemType.Text:
                    obj.text = EditorGUILayout.TextField("Text", obj.text);
                    obj.textColor = EditorGUILayout.ColorField("Text Color", obj.textColor);
                    obj.backgroundColor = EditorGUILayout.ColorField("Background Color", obj.backgroundColor);
                    obj.align = (DialogItemParameter.TextAlign)EditorGUILayout.EnumPopup("Align", obj.align);
                    break;
                case DialogItemType.Switch:
                    obj.key = EditorGUILayout.TextField("Key", obj.key);
                    obj.text = EditorGUILayout.TextField("Text", obj.text);
                    obj.defaultChecked = EditorGUILayout.Toggle("isOn", obj.defaultChecked);
                    obj.textColor = EditorGUILayout.ColorField("Text Color", obj.textColor);
                    break;
                case DialogItemType.Slider:
                    obj.key = EditorGUILayout.TextField("Key", obj.key);
                    obj.text = EditorGUILayout.TextField("Text", obj.text);
                    obj.value = EditorGUILayout.FloatField("Value", obj.value);
                    obj.minValue = EditorGUILayout.FloatField("Min Value", obj.minValue);
                    obj.maxValue = EditorGUILayout.FloatField("Max Value", obj.maxValue);
                    obj.digit = EditorGUILayout.IntField("Digit", obj.digit);
                    obj.textColor = EditorGUILayout.ColorField("Text Color", obj.textColor);
                    break;
                case DialogItemType.Toggle:
                    obj.key = EditorGUILayout.TextField("Key", obj.key);
                    EditorGUILayout.PropertyField(toggleItems, toggleItemsLabel, true);
                    obj.checkedIndex = EditorGUILayout.IntField("Selected Index", obj.checkedIndex);
                    obj.textColor = EditorGUILayout.ColorField("Text Color", obj.textColor);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
