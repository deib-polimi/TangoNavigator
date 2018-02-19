using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(NumericTextDialogController))]
    public class NumericTextDialogControllerEditor : Editor
    {
        SerializedProperty defaultValue;
        GUIContent defaultValueLabel = new GUIContent("Default Value");

        SerializedProperty saveKey;
        GUIContent saveKeyLabel = new GUIContent("Save Key");

        SerializedProperty OnResult;
        SerializedProperty OnResultText;

        private void OnEnable()
        {
            defaultValue = serializedObject.FindProperty("defaultValue");
            saveKey = serializedObject.FindProperty("saveKey");
            OnResult = serializedObject.FindProperty("OnResult");
            OnResultText = serializedObject.FindProperty("OnResultText");
        }

        public override void OnInspectorGUI()
        {
            var obj = target as NumericTextDialogController;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.title = EditorGUILayout.TextField("Title", obj.title);
            obj.message = EditorGUILayout.TextField("Message", obj.message);
            EditorGUILayout.PropertyField(defaultValue, defaultValueLabel, true);
            obj.maxLength = EditorGUILayout.IntField("Max Length", obj.maxLength);
            obj.enableDecimal = EditorGUILayout.Toggle("Enable Decimal", obj.enableDecimal);
            obj.enableSign = EditorGUILayout.Toggle("Enable Sign", obj.enableSign);

            obj.resultType = (NumericTextDialogController.ResultType)EditorGUILayout.EnumPopup("Result Type", obj.resultType);

            obj.okButton = EditorGUILayout.TextField("Ok Button", obj.okButton);
            obj.cancelButton = EditorGUILayout.TextField("Cancel Button", obj.cancelButton);
            obj.style = EditorGUILayout.TextField("Style", obj.style);

            obj.saveValue = EditorGUILayout.Toggle("Save Value", obj.saveValue);
            EditorGUILayout.PropertyField(saveKey, saveKeyLabel, true);

            switch (obj.resultType)
            {
                case NumericTextDialogController.ResultType.Value:
                    EditorGUILayout.PropertyField(OnResult, true);
                    break;
                case NumericTextDialogController.ResultType.Text:
                    EditorGUILayout.PropertyField(OnResultText, true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
