using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(SelectDialogController))]
    public class SelectDialogControllerEditor : Editor
    {
        SerializedProperty items;
        GUIContent itemsLabel = new GUIContent("Items");

        SerializedProperty OnResult;
        SerializedProperty OnResultIndex;

        private void OnEnable()
        {
            items = serializedObject.FindProperty("items");
            OnResult = serializedObject.FindProperty("OnResult");
            OnResultIndex = serializedObject.FindProperty("OnResultIndex");
        }

        public override void OnInspectorGUI()
        {
            var obj = target as SelectDialogController;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.title = EditorGUILayout.TextField("Title", obj.title);

            EditorGUILayout.PropertyField(items, itemsLabel, true);

            obj.resultType = (SelectDialogController.ResultType)EditorGUILayout.EnumPopup("Result Type", obj.resultType);

            obj.style = EditorGUILayout.TextField("Style", obj.style);


            switch (obj.resultType)
            {
                case SelectDialogController.ResultType.Index:
                    EditorGUILayout.PropertyField(OnResultIndex, true);
                    break;
                case SelectDialogController.ResultType.Value:
                case SelectDialogController.ResultType.Text:
                    EditorGUILayout.PropertyField(OnResult, true);
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
