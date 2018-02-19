using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(MultiChoiceDialogController))]
    public class MultiChoiceDialogControllerEditor : Editor
    {
        SerializedProperty items;
        GUIContent itemsLabel = new GUIContent("Items");

        SerializedProperty saveKey;
        GUIContent saveKeyLabel = new GUIContent("Save Key");

        SerializedProperty OnResult;
        SerializedProperty OnResultIndex;
        SerializedProperty OnValueChanged;
        SerializedProperty OnValueIndexChanged;

        private void OnEnable()
        {
            items = serializedObject.FindProperty("items");
            saveKey = serializedObject.FindProperty("saveKey");
            OnResult = serializedObject.FindProperty("OnResult");
            OnResultIndex = serializedObject.FindProperty("OnResultIndex");
            OnValueChanged = serializedObject.FindProperty("OnValueChanged");
            OnValueIndexChanged = serializedObject.FindProperty("OnValueIndexChanged");
        }

        public override void OnInspectorGUI()
        {
            var obj = target as MultiChoiceDialogController;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.title = EditorGUILayout.TextField("Title", obj.title);

            EditorGUILayout.PropertyField(items, itemsLabel, true);

            obj.resultType = (MultiChoiceDialogController.ResultType)EditorGUILayout.EnumPopup("Result Type", obj.resultType);

            obj.okButton = EditorGUILayout.TextField("Ok Button", obj.okButton);
            obj.cancelButton = EditorGUILayout.TextField("Cancel Button", obj.cancelButton);
            obj.style = EditorGUILayout.TextField("Style", obj.style);

            obj.saveValue = EditorGUILayout.Toggle("Save Value", obj.saveValue);
            EditorGUILayout.PropertyField(saveKey, saveKeyLabel, true);

            switch (obj.resultType)
            {
                case MultiChoiceDialogController.ResultType.Index:
                    EditorGUILayout.PropertyField(OnResultIndex, true);
                    EditorGUILayout.PropertyField(OnValueIndexChanged, true);
                    break;
                case MultiChoiceDialogController.ResultType.Value:
                case MultiChoiceDialogController.ResultType.Text:
                    EditorGUILayout.PropertyField(OnResult, true);
                    EditorGUILayout.PropertyField(OnValueChanged, true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
