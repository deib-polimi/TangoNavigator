using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using UnityEditor;
//using UnityEditor.SceneManagement;

public class MainMenuStart : MonoBehaviour {

	public void loadStart() {
//		#if UNITY_EDITOR
//		EditorSceneManager.LoadScene ("MainScene");
//		#else
		SceneManager.LoadScene ("MainScene2");
//		#endif
	}

}
