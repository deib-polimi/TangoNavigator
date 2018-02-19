using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FantomLib
{
    [CustomEditor(typeof(NotificationController))]
    public class NotificationControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var obj = target as NotificationController;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((MonoBehaviour)target) , typeof(MonoScript), false);
            EditorGUI.EndDisabledGroup();

            obj.title = EditorGUILayout.TextField("Title", obj.title);
            obj.message = EditorGUILayout.TextField("Message", obj.message);

            obj.tapAction = (NotificationController.TapAction)EditorGUILayout.EnumPopup("Tap Action", obj.tapAction);

            switch (obj.tapAction)
            {
                case NotificationController.TapAction.BackToApplication:
                    break;
                case NotificationController.TapAction.OpenURL:
                    obj.url = EditorGUILayout.TextField("URL", obj.url);
                    break;
            }

            obj.iconName = EditorGUILayout.TextField("Icon Name", obj.iconName);
            obj.idTag = EditorGUILayout.TextField("ID Tag", obj.idTag);
            obj.showTimestamp = EditorGUILayout.Toggle("Show Timestamp", obj.showTimestamp);

            EditorUtility.SetDirty(target);
        }
    }
}

