using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(SingleChoiceDialogController))]
    public class SingleChoiceDialogControllerEditor : Editor
    {
        SerializedProperty items;
        GUIContent itemsLabel = new GUIContent("Items");

        SerializedProperty selectedIndex;
        GUIContent selectedIndexLabel = new GUIContent("Selected Index");

        SerializedProperty saveKey;
        GUIContent saveKeyLabel = new GUIContent("Save Key");

        SerializedProperty OnResult;
        SerializedProperty OnResultIndex;
        SerializedProperty OnValueChanged;
        SerializedProperty OnValueIndexChanged;

        private void OnEnable()
        {
            items = serializedObject.FindProperty("items");
            selectedIndex = serializedObject.FindProperty("selectedIndex");
            saveKey = serializedObject.FindProperty("saveKey");
            OnResult = serializedObject.FindProperty("OnResult");
            OnResultIndex = serializedObject.FindProperty("OnResultIndex");
            OnValueChanged = serializedObject.FindProperty("OnValueChanged");
            OnValueIndexChanged = serializedObject.FindProperty("OnValueIndexChanged");
        }

        public override void OnInspectorGUI()
        {
            var obj = target as SingleChoiceDialogController;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.title = EditorGUILayout.TextField("Title", obj.title);

            EditorGUILayout.PropertyField(items, itemsLabel, true);

            EditorGUILayout.PropertyField(selectedIndex, selectedIndexLabel, true);

            obj.resultType = (SingleChoiceDialogController.ResultType)EditorGUILayout.EnumPopup("Result Type", obj.resultType);

            obj.okButton = EditorGUILayout.TextField("Ok Button", obj.okButton);
            obj.cancelButton = EditorGUILayout.TextField("Cancel Button", obj.cancelButton);
            obj.style = EditorGUILayout.TextField("Style", obj.style);

            obj.saveValue = EditorGUILayout.Toggle("Save Value", obj.saveValue);
            EditorGUILayout.PropertyField(saveKey, saveKeyLabel, true);

            switch (obj.resultType)
            {
                case SingleChoiceDialogController.ResultType.Index:
                    EditorGUILayout.PropertyField(OnResultIndex, true);
                    EditorGUILayout.PropertyField(OnValueIndexChanged, true);
                    break;
                case SingleChoiceDialogController.ResultType.Value:
                case SingleChoiceDialogController.ResultType.Text:
                    EditorGUILayout.PropertyField(OnResult, true);
                    EditorGUILayout.PropertyField(OnValueChanged, true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
