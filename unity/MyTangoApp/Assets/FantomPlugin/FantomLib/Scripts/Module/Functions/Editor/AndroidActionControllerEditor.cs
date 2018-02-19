using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(AndroidActionController))]
    public class AndroidActionControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var obj = target as AndroidActionController;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.action = EditorGUILayout.TextField("Action", obj.action);

            obj.actionType = (AndroidActionController.ActionType)EditorGUILayout.EnumPopup("Action Type", obj.actionType);

            switch (obj.actionType)
            {
                case AndroidActionController.ActionType.URI:
                    obj.uri = EditorGUILayout.TextField("URI", obj.uri);
                    break;
                case AndroidActionController.ActionType.ExtraQuery:
                    obj.extra = EditorGUILayout.TextField("Extra", obj.extra);
                    obj.query = EditorGUILayout.TextField("Query", obj.query);
                    break;
            }

            EditorUtility.SetDirty(target);
        }
    }
}
