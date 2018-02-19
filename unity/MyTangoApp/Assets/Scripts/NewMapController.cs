//-----------------------------------------------------------------------
// <copyright file="AreaLearningInGameController.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using Tango;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using IndoorNavigation;
using FantomLib;

/// <summary>
/// AreaLearningGUIController is responsible for the main game interaction.
/// 
/// This class also takes care of loading / save persistent data(marker), and loop closure handling.
/// </summary>
public class NewMapController : MonoBehaviour, ITangoPose, ITangoEvent, ITangoDepth
{
	/// <summary>
	/// Prefabs of different colored markers.
	/// </summary>
	public GameObject[] m_markPrefabs;

	/// <summary>
	/// The point cloud object in the scene.
	/// </summary>
	public TangoPointCloud m_pointCloud;

	/// <summary>
	/// The canvas to place 2D game objects under.
	/// </summary>
	public Canvas m_canvas;

	/// <summary>
	/// The touch effect to place on taps.
	/// </summary>
	public RectTransform m_prefabTouchEffect;

	/// <summary>
	/// Saving progress UI text.
	/// </summary>
	public UnityEngine.UI.Text m_savingText;

	/// <summary>
	/// New map boolean.
	/// </summary>
	public bool m_newMap;

	/// <summary>
	/// Helper for the single choice list of the destinations.
	/// </summary>
	public SingleChoiceDialogController singleChoice;

	/// <summary>
	/// Mesh used for the navigation.
	/// </summary>
	public GameObject mesh;

	/// <summary>
	/// Previous point touched. Used in order to keep track of the last touched element.
	/// </summary>
	private GameObject prevTouchPos = null;

	/// <summary>
	/// List of all the corners.
	/// </summary>
	private List<Corner> corners = new List<Corner> ();

	/// <summary>
	/// List of all the doors which are the destinations.
	/// </summary>
	private List<Corner> doors = new List<Corner> ();

	/// <summary>
	/// Index of the destination selected.
	/// </summary>
	private int currentDestIndex = 0;

	/// <summary>
	/// Boolean which helps to stop the path to be drawn before the destination being set.
	/// </summary>
	private bool currentDestSet = false;

	/// <summary>
	/// List of all the walls.
	/// </summary>
	private List<Wall> walls = new List<Wall> ();

	/// <summary>
	/// List of all the  visual representations of a wall.
	/// </summary>
	private List<GameObject> lines = new List<GameObject> ();

	/// <summary>
	/// Floor plane and center.
	/// </summary>
	private Floor floor;

	/// <summary>
	/// Last selected line. It is used in order to delete a line.
	/// </summary>
	private GameObject selectedLine;

	/// <summary>
	/// Prefab of the visual representation of a wall.
	/// </summary>
	public GameObject line;

	/// <summary>
	/// The Area Description currently loaded in the Tango Service.
	/// </summary>
	[HideInInspector]
	public AreaDescription m_curAreaDescription;

	#if UNITY_EDITOR
	/// <summary>
	/// Handles GUI text input in Editor where there is no device keyboard.
	/// If true, text input for naming new saved Area Description is displayed.
	/// </summary>
	private bool m_displayGuiTextInput;

	/// <summary>
	/// Handles GUI text input in Editor where there is no device keyboard.
	/// Contains text data for naming new saved Area Descriptions.
	/// </summary>
	private string m_guiTextInputContents;

	/// <summary>
	/// Handles GUI text input in Editor where there is no device keyboard.
	/// Indicates whether last text input was ended with confirmation or cancellation.
	/// </summary>
	private bool m_guiTextInputResult;
	#endif

	/// <summary>
	/// If set, then the depth camera is on and we are waiting for the next depth update.
	/// </summary>
	private bool m_findPlaneWaitingForDepth;

	/// <summary>
	/// A reference to TangoARPoseController instance.
	/// 
	/// In this class, we need TangoARPoseController reference to get the timestamp and pose when we place a marker.
	/// The timestamp and pose is used for later loop closure position correction. 
	/// </summary>
	private TangoPoseController m_poseController;

	/// <summary>
	/// List of markers placed in the scene.
	/// </summary>
	private List<GameObject> m_markerList = new List<GameObject> ();

	/// <summary>
	/// Reference to the newly placed marker.
	/// </summary>
	private GameObject newMarkObject = null;

	/// <summary>
	/// Current marker type.
	/// </summary>
	private int m_currentMarkType = 0;

	/// <summary>
	/// If set, this is the selected marker.
	/// </summary>
	private ARMarker m_selectedMarker;

	/// <summary>
	/// If set, this is the rectangle bounding the selected marker.
	/// </summary>
	private Rect m_selectedRect;

	/// <summary>
	/// If the interaction is initialized.
	/// 
	/// Note that the initialization is triggered by the relocalization event. We don't want user to place object before
	/// the device is relocalized.
	/// </summary>
	private bool m_initialized = false;

	/// <summary>
	/// A reference to TangoApplication instance.
	/// </summary>
	private TangoApplication m_tangoApplication;
	//private TangoDynamicMesh m_dynamicMesh;

	private Thread m_saveThread;

	/// <summary>
	/// Unity Start function.
	/// 
	/// We find and assign pose controller and tango application, and register this class to callback events.
	/// </summary>
	public void Start ()
	{
		m_poseController = FindObjectOfType<TangoPoseController> ();
		m_tangoApplication = FindObjectOfType<TangoApplication> ();

		if (m_tangoApplication != null) {
			m_tangoApplication.Register (this);
		}

	}

	/// <summary>
	/// Unity Update function.
	/// 
	/// Mainly handle the touch event and place mark in place.
	/// Used to delete walls/lines as well.
	/// </summary>
	public void Update ()
	{
		if (m_saveThread != null && m_saveThread.ThreadState != ThreadState.Running) {
			// After saving an Area Description or mark data, we reload the scene to restart the game.
			_UpdateMarkersForLoopClosures ();
			_SaveMarkerToDisk ();
			#pragma warning disable 618
			Application.LoadLevel (Application.loadedLevel);
			#pragma warning restore 618
		}

		if (Input.GetKey (KeyCode.Escape)) {
			#pragma warning disable 618
			Application.LoadLevel (Application.loadedLevel);
			#pragma warning restore 618
		}

		if (!m_initialized) {
			return;
		}

		if (EventSystem.current.IsPointerOverGameObject (0) || GUIUtility.hotControl != 0) {
			return;
		}

		if (Input.touchCount == 1) {
			Touch t = Input.GetTouch (0);
			Vector2 guiPosition = new Vector2 (t.position.x, Screen.height - t.position.y);
			Camera cam = Camera.main;
			RaycastHit hitInfo;

			if (t.phase != TouchPhase.Began) {
				return;
			}

			if (m_selectedRect.Contains (guiPosition)) {
				// do nothing, the button will handle it
			} else if (Physics.Raycast (cam.ScreenPointToRay (t.position), out hitInfo)) {
				// We hit a collider.
				Marker (hitInfo);

			} else {
				// Place a new point at that location, clear selections.
				m_selectedMarker = null;
				selectedLine = null;
				prevTouchPos = null;
				StartCoroutine (_WaitForDepthAndFindPlane (t.position));

				// Because we may wait a small amount of time, this is a good place to play a small
				// animation so the user knows that their input was received.
				RectTransform touchEffectRectTransform = Instantiate (m_prefabTouchEffect) as RectTransform;
				touchEffectRectTransform.transform.SetParent (m_canvas.transform, false);
				Vector2 normalizedPosition = t.position;
				normalizedPosition.x /= Screen.width;
				normalizedPosition.y /= Screen.height;
				touchEffectRectTransform.anchorMin = touchEffectRectTransform.anchorMax = normalizedPosition;
			}
		}
	}

	/// <summary>
	/// Application onPause / onResume callback.
	/// </summary>
	/// <param name="pauseStatus"><c>true</c> if the application about to pause, otherwise <c>false</c>.</param>
	public void OnApplicationPause (bool pauseStatus)
	{
		if (pauseStatus && m_initialized) {
			// When application is backgrounded, we reload the level because the Tango Service is disconected. All
			// learned area and placed marker should be discarded as they are not saved.
			#pragma warning disable 618
			Application.LoadLevel (Application.loadedLevel);
			#pragma warning restore 618
		}
	}

	/// <summary>
	/// Unity OnGUI function.
	/// 
	/// Mainly for removing markers or lines.
	/// </summary>
	public void OnGUI ()
	{
		if (m_selectedMarker != null) {
			Renderer selectedRenderer = m_selectedMarker.GetComponent<Renderer> ();

			// GUI's Y is flipped from the mouse's Y
			Rect screenRect = _WorldBoundsToScreen (Camera.main, selectedRenderer.bounds);
			float yMin = Screen.height - screenRect.yMin;
			float yMax = Screen.height - screenRect.yMax;
			screenRect.yMin = Mathf.Min (yMin, yMax);
			screenRect.yMax = Mathf.Max (yMin, yMax);

			if (GUI.Button (screenRect, "<size=30>Delete corner</size>")) {
				corners.Remove (corners.Find(x => x.point.GetInstanceID() == m_selectedMarker.gameObject.GetInstanceID()));
				m_markerList.Remove (m_selectedMarker.gameObject);
				m_selectedMarker.SendMessage ("Hide");
				m_selectedMarker = null;
				selectedLine = null;
				prevTouchPos = null;
				m_selectedRect = new Rect ();
			} else {
				m_selectedRect = screenRect;
			}
		} else {
			m_selectedRect = new Rect ();
		}

		#if UNITY_EDITOR
		// Handle text input when there is no device keyboard in the editor.
		if (m_displayGuiTextInput) {
			Rect textBoxRect = new Rect (100,
				                   Screen.height - 200,
				                   Screen.width - 200,
				                   100);

			Rect okButtonRect = textBoxRect;
			okButtonRect.y += 100;
			okButtonRect.width /= 2;

			Rect cancelButtonRect = okButtonRect;
			cancelButtonRect.x = textBoxRect.center.x;

			GUI.SetNextControlName ("TextField");
			GUIStyle customTextFieldStyle = new GUIStyle (GUI.skin.textField);
			customTextFieldStyle.alignment = TextAnchor.MiddleCenter;
			m_guiTextInputContents = 
				GUI.TextField (textBoxRect, m_guiTextInputContents, customTextFieldStyle);
			GUI.FocusControl ("TextField");

			if (GUI.Button (okButtonRect, "OK")
			    || (Event.current.type == EventType.keyDown && Event.current.character == '\n')) {
				m_displayGuiTextInput = false;
				m_guiTextInputResult = true;
			} else if (GUI.Button (cancelButtonRect, "Cancel")) {
				m_displayGuiTextInput = false;
				m_guiTextInputResult = false;
			}
		}
		#endif
	}

	/// <summary>
	/// Set the marker type.
	/// </summary>
	/// <param name="type">Marker type.</param>
	public void SetCurrentMarkType (int type)
	{
		if (type != m_currentMarkType) {
			m_currentMarkType = type;
		}
	}

	/// <summary>
	/// Save the game.
	/// 
	/// Save will trigger 3 things:
	/// 
	/// 1. Save the Area Description if the learning mode is on.
	/// 2. Bundle adjustment for all marker positions, please see _UpdateMarkersForLoopClosures() function header for 
	///     more details.
	/// 3. Save all markers to xml, save the Area Description if the learning mode is on.
	/// 4. Reload the scene.
	/// </summary>
	public void Save ()
	{
		StartCoroutine (_DoSaveCurrentAreaDescription ());
	}

	/// <summary>
	/// This is called each time a Tango event happens.
	/// </summary>
	/// <param name="tangoEvent">Tango event.</param>
	public void OnTangoEventAvailableEventHandler (Tango.TangoEvent tangoEvent)
	{
		// We will not have the saving progress when the learning mode is off.
		if (!m_tangoApplication.m_areaDescriptionLearningMode) {
			return;
		}

		if (tangoEvent.type == TangoEnums.TangoEventType.TANGO_EVENT_AREA_LEARNING
		    && tangoEvent.event_key == "AreaDescriptionSaveProgress") {
			m_savingText.text = "Saving. " + (float.Parse (tangoEvent.event_value) * 100) + "%";
			Debug.Log ("Saving. " + (float.Parse (tangoEvent.event_value) * 100) + "%");
		}
	}

	/// <summary>
	/// OnTangoPoseAvailable event from Tango.
	/// 
	/// In this function, we only listen to the Start-Of-Service with respect to Area-Description frame pair. This pair
	/// indicates a relocalization or loop closure event happened, base on that, we either start the initialize the
	/// interaction or do a bundle adjustment for all marker position.
	/// </summary>
	/// <param name="poseData">Returned pose data from TangoService.</param>
	public void OnTangoPoseAvailable (Tango.TangoPoseData poseData)
	{
		// This frame pair's callback indicates that a loop closure or relocalization has happened. 
		//
		// When learning mode is on, this callback indicates the loop closure event. Loop closure will happen when the
		// system recognizes a pre-visited area, the loop closure operation will correct the previously saved pose 
		// to achieve more accurate result. (pose can be queried through GetPoseAtTime based on previously saved
		// timestamp).
		// Loop closure definition: https://en.wikipedia.org/wiki/Simultaneous_localization_and_mapping#Loop_closure
		//
		// When learning mode is off, and an Area Description is loaded, this callback indicates a
		// relocalization event. Relocalization is when the device finds out where it is with respect to the loaded
		// Area Description. In our case, when the device is relocalized, the markers will be loaded because we
		// know the relative device location to the markers.
		if (poseData.framePair.baseFrame ==
		    TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION &&
		    poseData.framePair.targetFrame ==
		    TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE &&
		    poseData.status_code == TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID) {
			// When we get the first loop closure/ relocalization event, we initialized all the in-game interactions.
			if (!m_initialized) {
				m_initialized = true;
				if (m_curAreaDescription == null) {
					Debug.Log ("AndroidInGameController.OnTangoPoseAvailable(): m_curAreaDescription is null");
					return;
				}

				_LoadMarkerFromDisk ();
			}
		}
	}

	/// <summary>
	/// This is called each time new depth data is available.
	/// 
	/// On the Tango tablet, the depth callback occurs at 5 Hz.
	/// </summary>
	/// <param name="tangoDepth">Tango depth.</param>
	public void OnTangoDepthAvailable (TangoUnityDepth tangoDepth)
	{
		// Don't handle depth here because the PointCloud may not have been updated yet.  Just
		// tell the coroutine it can continue.
		m_findPlaneWaitingForDepth = false;
	}

	/// <summary>
	/// Actually do the Area Description save.
	/// </summary>
	/// <returns>Coroutine IEnumerator.</returns>
	private IEnumerator _DoSaveCurrentAreaDescription ()
	{
		#if UNITY_EDITOR
		// Work around lack of on-screen keyboard in editor:
		if (m_displayGuiTextInput || m_saveThread != null) {
			yield break;
		}

		m_displayGuiTextInput = true;
		m_guiTextInputContents = "Unnamed";
		while (m_displayGuiTextInput) {
			yield return null;
		}

		bool saveConfirmed = m_guiTextInputResult;
		#else
		if (TouchScreenKeyboard.visible || m_saveThread != null)
		{
		yield break;
		}

		TouchScreenKeyboard kb = TouchScreenKeyboard.Open("Unnamed");
		while (!kb.done && !kb.wasCanceled)
		{
		yield return null;
		}

		bool saveConfirmed = kb.done;
		#endif
		if (saveConfirmed) {
			// Disable interaction before saving.
			m_initialized = false;
			m_savingText.gameObject.SetActive (true);
			if (m_tangoApplication.m_areaDescriptionLearningMode) {
				// The keyboard is not readable if you are not in the Unity main thread. Cache the value here.
				string name;
				#if UNITY_EDITOR
				name = m_guiTextInputContents;
				#else
				name = kb.text;
				#endif

				m_saveThread = new Thread (delegate() {
					// Start saving process in another thread.
					m_curAreaDescription = AreaDescription.SaveCurrent ();
					AreaDescription.Metadata metadata = m_curAreaDescription.GetMetadata ();
					metadata.m_name = name;
					m_curAreaDescription.SaveMetadata (metadata);
				});
				m_saveThread.Start ();
			} else {
				_SaveMarkerToDisk ();
				#pragma warning disable 618
				Application.LoadLevel (Application.loadedLevel);
				#pragma warning restore 618
			}
		}
	}

	/// <summary>
	/// Correct all saved marks when loop closure happens.
	/// 
	/// When Tango Service is in learning mode, the drift will accumulate overtime, but when the system sees a
	/// preexisting area, it will do a operation to correct all previously saved poses
	/// (the pose you can query with GetPoseAtTime). This operation is called loop closure. When loop closure happens,
	/// we will need to re-query all previously saved marker position in order to achieve the best result.
	/// This function is doing the querying job based on timestamp.
	/// </summary>
	private void _UpdateMarkersForLoopClosures ()
	{
		// Adjust mark's position each time we have a loop closure detected.
		foreach (GameObject obj in m_markerList) {
			ARMarker tempMarker = obj.GetComponent<ARMarker> ();
			if (tempMarker.m_timestamp != -1.0f) {
				TangoCoordinateFramePair pair;
				TangoPoseData relocalizedPose = new TangoPoseData ();

				pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_AREA_DESCRIPTION;
				pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
				PoseProvider.GetPoseAtTime (relocalizedPose, tempMarker.m_timestamp, pair);

				Matrix4x4 uwTDevice = TangoSupport.UNITY_WORLD_T_START_SERVICE
				                      * relocalizedPose.ToMatrix4x4 ()
				                      * TangoSupport.DEVICE_T_UNITY_CAMERA;

				Matrix4x4 uwTMarker = uwTDevice * tempMarker.m_deviceTMarker;

				obj.transform.position = uwTMarker.GetColumn (3);
				obj.transform.rotation = Quaternion.LookRotation (uwTMarker.GetColumn (2), uwTMarker.GetColumn (1));
			}
		}
	}

	/// <summary>
	/// Write marker list to an xml file stored in application storage.
	/// </summary>
	private void _SaveMarkerToDisk ()
	{
		// Compose a XML data list.
		List<MarkerData> xmlDataList = new List<MarkerData> ();
		List<WallXML> xmlWallList = new List<WallXML> ();

		foreach (GameObject obj in m_markerList) {
			// Add marks data to the list, we intentionally didn't add the timestamp, because the timestamp will not be
			// useful when the next time Tango Service is connected. The timestamp is only used for loop closure pose
			// correction in current Tango connection.
			MarkerData temp = new MarkerData ();
			temp.m_type = obj.GetComponent<ARMarker> ().m_type;
			temp.m_position = obj.transform.position;
			temp.m_orientation = obj.transform.rotation;
			temp.m_id = obj.GetInstanceID ();
			// The name is useful only for destinations.
			temp.m_name = obj.name;
			xmlDataList.Add (temp);
		}

		// Create also an XML in order to keep track of the existing walls.
		foreach (Wall w in walls) {
			WallXML tmp = new WallXML ();
			tmp.m_startCornerID = w.fromCorner.point.GetInstanceID ();
			tmp.m_endCornerID = w.toCorner.point.GetInstanceID ();
			xmlWallList.Add (tmp);
		}

		string path = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Marker.xml";
		var serializer = new XmlSerializer (typeof(List<MarkerData>));
		using (var stream = new FileStream (path, FileMode.Create)) {
			serializer.Serialize (stream, xmlDataList);
		}

		path = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Wall.xml";
		serializer = new XmlSerializer (typeof(List<WallXML>));
		using (var stream = new FileStream (path, FileMode.Create)) {
			serializer.Serialize (stream, xmlWallList);
		}	
		
	}

	/// <summary>
	/// Load marker list xml from application storage.
	/// </summary>
	private void _LoadMarkerFromDisk ()
	{
		// Attempt to load the existing markers from storage.
		string path = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Marker.xml";

		var serializer = new XmlSerializer (typeof(List<MarkerData>));
		var stream = new FileStream (path, FileMode.Open);

		List<MarkerData> xmlDataList = serializer.Deserialize (stream) as List<MarkerData>;

		if (xmlDataList == null) {
			Debug.Log ("AndroidInGameController._LoadMarkerFromDisk(): xmlDataList is null");
			return;
		}

		m_markerList.Clear ();
		doors.Clear ();
		corners.Clear ();

		// This part is for the wall XML.
		string path2 = Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Wall.xml";
		var serializer2 = new XmlSerializer (typeof(List<WallXML>));
		var stream2 = new FileStream (path2, FileMode.Open);
		List<WallXML> xmlWallList = serializer2.Deserialize (stream2) as List<WallXML>;
		walls.Clear ();
		lines.Clear ();

		foreach (MarkerData mark in xmlDataList) {
			// Instantiate all markers' gameobject.
			GameObject temp = Instantiate (m_markPrefabs [mark.m_type],
				                  mark.m_position,
				                  mark.m_orientation) as GameObject;
			
			temp.tag = "CornersAndWalls";
			Corner corner = new Corner (temp);
			m_markerList.Add (temp);

			// In case this is a corner, then correct the wallXML with the new ID of the corners and add it to the corners' list.
			if (mark.m_type == 0) {
				
				List<WallXML> tmpWallListStart = xmlWallList.FindAll (x => x.m_startCornerID == mark.m_id);
				List<WallXML> tmpWallListEnd = xmlWallList.FindAll (x => x.m_endCornerID == mark.m_id);

				foreach (WallXML w in tmpWallListStart) {
					w.m_startCornerID = temp.GetInstanceID ();
				}
				foreach (WallXML w in tmpWallListEnd) {
					w.m_endCornerID = temp.GetInstanceID ();
				}
				corners.Add (corner);

			} // Otherwise it should be a door/destination. In that case just add it to the destinations' list.
			else if (mark.m_type == 1) {
				temp.name = mark.m_name;
				doors.Add (corner);
			}

		}

		foreach (WallXML w in xmlWallList) {
			Vector3 startPos = m_markerList.Find (x => x.GetInstanceID () == w.m_startCornerID).transform.position;
			Vector3 endPos = m_markerList.Find (x => x.GetInstanceID () == w.m_endCornerID).transform.position;

			// Instantiate all the lines/walls.
			GameObject g = DrawLine (startPos, endPos, 8);
			g.tag = "CornersAndWalls";

			Wall wall = new Wall (corners.Find (x => x.point.GetInstanceID () == w.m_startCornerID),
				            corners.Find (x => x.point.GetInstanceID () == w.m_endCornerID), g.GetInstanceID ());
			walls.Add (wall);

			lines.Add (g);

		}

	}

	/// <summary>
	/// Convert a 3D bounding box represented by a <c>Bounds</c> object into a 2D 
	/// rectangle represented by a <c>Rect</c> object.
	/// </summary>
	/// <returns>The 2D rectangle in Screen coordinates.</returns>
	/// <param name="cam">Camera to use.</param>
	/// <param name="bounds">3D bounding box.</param>
	private Rect _WorldBoundsToScreen (Camera cam, Bounds bounds)
	{
		Vector3 center = bounds.center;
		Vector3 extents = bounds.extents;
		Bounds screenBounds = new Bounds (cam.WorldToScreenPoint (center), Vector3.zero);

		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (+extents.x, +extents.y, +extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (+extents.x, +extents.y, -extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (+extents.x, -extents.y, +extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (+extents.x, -extents.y, -extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (-extents.x, +extents.y, +extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (-extents.x, +extents.y, -extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (-extents.x, -extents.y, +extents.z)));
		screenBounds.Encapsulate (cam.WorldToScreenPoint (center + new Vector3 (-extents.x, -extents.y, -extents.z)));
		return Rect.MinMaxRect (screenBounds.min.x, screenBounds.min.y, screenBounds.max.x, screenBounds.max.y);
	}

	/// <summary>
	/// Wait for the next depth update, then find the plane at the touch position.
	/// </summary>
	/// <returns>Coroutine IEnumerator.</returns>
	/// <param name="touchPosition">Touch position to find a plane at.</param>
	/// <param name="doorCorridorPosition">Door position where to put the door marker.</param>
	/// <param name="wall">Touch position to find a plane at.</param>
	/// <param name="doorCorridorBool">True if we are putting a door marker.</param>
	private IEnumerator _WaitForDepthAndFindPlane (Vector2 touchPosition, Vector3 doorCorridorPosition = new Vector3(), Wall wall = null,
		bool doorCorridorBool = false)
	{
		if (!doorCorridorBool) {
			if (m_currentMarkType == 1 || m_currentMarkType == 2) {
				AndroidHelper.ShowAndroidToastMessage ("Door or corridors must be on walls");
				yield break;
			}
		}

		m_findPlaneWaitingForDepth = true;

		// Turn on the camera and wait for a single depth update.
		m_tangoApplication.SetDepthCameraRate (TangoEnums.TangoDepthCameraRate.MAXIMUM);
		while (m_findPlaneWaitingForDepth) {
			yield return null;
		}

		m_tangoApplication.SetDepthCameraRate (TangoEnums.TangoDepthCameraRate.DISABLED);

		// Find the plane.
		Camera cam = Camera.main;
		Vector3 planeCenter;
		Plane plane;

		// If I'm selecting the floor, set the floor plane
		if (m_currentMarkType == 3) {
			if (!m_pointCloud.FindPlane (cam, touchPosition, out planeCenter, out plane)) {
				yield break;
			}
			m_pointCloud.FindFloor ();
			floor = new Floor (plane, planeCenter);
			Debug.Log ("Height of the floor " + planeCenter.y);
			AndroidHelper.ShowAndroidToastMessage ("Floor found!");
			yield break;
		} else if (!m_pointCloud.m_floorFound) {
			AndroidHelper.ShowAndroidToastMessage ("Please, select first the floor");
			yield break;
		}

		// Find me the point I touched on screen on world coordinates if I'm not putting a door...
		if (!doorCorridorBool) {

			int i = m_pointCloud.FindClosestPoint (cam, touchPosition, 10);
			if (i < 0) {
				yield break;
			}
			planeCenter = m_pointCloud.m_points [i];

			// Put the marker on the floor height
			planeCenter.y = floor.floorCenter.y;


		} //... else my door position is already set as parameter.
		else {
			planeCenter = doorCorridorPosition;
		}

		// Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
		Vector3 up = floor.floorPlane.normal;
		Vector3 forward;
		if (Vector3.Angle (floor.floorPlane.normal, cam.transform.forward) < 175) {
			Vector3 right = Vector3.Cross (up, cam.transform.forward).normalized;
			forward = Vector3.Cross (right, up).normalized;
		} else {
			// Normal is nearly parallel to camera look direction, the cross product would have too much
			// floating point error in it.
			forward = Vector3.Cross (up, cam.transform.right);
		}

		// Instantiate marker object.
		newMarkObject = Instantiate (m_markPrefabs [m_currentMarkType],
			planeCenter,
			Quaternion.LookRotation (forward, up)) as GameObject;
		newMarkObject.tag = "CornersAndWalls";

		ARMarker markerScript = newMarkObject.GetComponent<ARMarker> ();

		markerScript.m_type = m_currentMarkType;
		markerScript.m_timestamp = (float)m_poseController.LastPoseTimestamp;

		Matrix4x4 uwTDevice = Matrix4x4.TRS (m_poseController.transform.position,
			                      m_poseController.transform.rotation,
			                      Vector3.one);
		Matrix4x4 uwTMarker = Matrix4x4.TRS (newMarkObject.transform.position,
			                      newMarkObject.transform.rotation,
			                      Vector3.one);
		markerScript.m_deviceTMarker = Matrix4x4.Inverse (uwTDevice) * uwTMarker;

		m_markerList.Add (newMarkObject);

		prevTouchPos = null;
		selectedLine = null;
		m_selectedMarker = null;

		switch (m_currentMarkType) {
		// Corner case.
		case 0:
			corners.Add (new Corner (newMarkObject));
			break;
		// Door case.
		case 1:
			TouchScreenKeyboard kb = TouchScreenKeyboard.Open ("", TouchScreenKeyboardType.Default, false, false, false, false,
				"Write the name for this destination marker");
			while (!kb.done && !kb.wasCanceled) {
				yield return null;
			}
			newMarkObject.name = kb.text;
			doors.Add (new Corner (newMarkObject, wall));
			break;
		default:
			Debug.Log ("No valid point inserted.");
			break;
		}
	}

	/// <summary>
	/// Function which will decide what to do with a collider hit.
	/// </summary>
	/// <param name="hitInfo">Information about the object we hit.</param>
	public void Marker (RaycastHit hitInfo) {
		
		Debug.Log (hitInfo.collider.GetType());

		// I hit a marker so I have to create a wall in case it's the second marker I hit.
		if (prevTouchPos != null && hitInfo.collider.GetType() == typeof(MeshCollider)) {
			if (prevTouchPos.GetComponent<ARMarker> ().m_type == 0 && hitInfo.collider.gameObject.GetComponent<ARMarker> ().m_type == 0) {
				Vector3 startPos = prevTouchPos.transform.position;
				Vector3 endPos = hitInfo.collider.gameObject.transform.position;

				// Instantiate the line and add it to the list.
				GameObject g = DrawLine(startPos, endPos, 8);

				lines.Add (g);

				Wall wall = new Wall (corners.Find (x => x.point == prevTouchPos), corners.Find (x => x.point == hitInfo.collider.gameObject),
					g.GetInstanceID ());
				walls.Add (wall);

			} else {
				AndroidHelper.ShowAndroidToastMessage ("You have to connect two corners");
			}

			prevTouchPos = null;
			selectedLine = null;
			m_selectedMarker = null;

			return;

		}

		// Found a marker/line for the first time, select it (so long as it isn't disappearing)!
		GameObject tapped = hitInfo.collider.gameObject;

		// I hit a marker, so I select it.
		if (hitInfo.collider.GetType () == typeof(MeshCollider)) {
			// If there was no selected marker, select this; else it will be handled by OnGui()
			if (!tapped.GetComponent<Animation> ().isPlaying) {
				m_selectedMarker = tapped.GetComponent<ARMarker> ();
				prevTouchPos = tapped;
				selectedLine = null;
			}
			AndroidHelper.ShowAndroidToastMessage ("Hide this corner or select another one to create a wall");
			return;

		} // I hit a line
		else if (hitInfo.collider.GetType () == typeof(CapsuleCollider)) {

			// In this case I'm putting a door. Doors can be only on walls.
			if (m_currentMarkType == 1) {
				Wall w = walls.Find(x => x.lineID == hitInfo.collider.gameObject.GetInstanceID ());
				Debug.Log ("Wall is from " + w.fromCorner.point.transform.position + " and you hit on " + hitInfo.point);
				StartCoroutine (_WaitForDepthAndFindPlane(Vector2.zero, ProjectPointOnLineSegment(w.fromCorner.point.transform.position,
					w.toCorner.point.transform.position, hitInfo.point), w, true));
				return;
			}

			// If there was a selected line, remove it; else select it
			if (selectedLine == tapped) {
				walls.Remove (walls.Find (x => x.lineID == selectedLine.GetInstanceID ()));
				lines.Remove (selectedLine); 
				DestroyObject (tapped);
				prevTouchPos = null;
				selectedLine = null;
				m_selectedMarker = null;
			} else {
				selectedLine = tapped;
				prevTouchPos = null;
				m_selectedMarker = null;
				AndroidHelper.ShowAndroidToastMessage ("Select the line again to delete it");
			}
		}
	}

	public void setNewMapBool (bool b)
	{
		m_newMap = b;
		return;
	}

	public void DrawCorridors ()
	{
		StartCoroutine (DrawCorridorsRoutine ());
	}

	/// <summary>
	/// Draw a line.
	/// </summary>
	/// <returns>Line object.</returns>
	/// <param name="startPos">Starting point of the line.</param>
	/// <param name="endPos">Ending point of the line.</param>
	/// <param name="layer">Layer mask of the line.</param>
	private GameObject DrawLine (Vector3 startPos, Vector3 endPos, int layer){
		GameObject g = Instantiate (line);
		g.layer = layer;

		LineRenderer lr = g.GetComponent<LineRenderer> ();
		lr.SetPosition (0, startPos);
		lr.SetPosition (1, endPos);

		CapsuleCollider cc = g.GetComponent<CapsuleCollider> ();
		cc.transform.position = startPos + (endPos - startPos) / 2;
		cc.transform.LookAt (startPos);
		cc.height = (endPos - startPos).magnitude;

		return g;
	}
	/// <summary>
	/// This function returns a point which is a projection from a point to a line.
	/// The line is regarded infinite. If the line is finite, use ProjectPointOnLineSegment() instead.
	/// </summary>
	public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point){		

		//get vector from point on line to point in space
		Vector3 linePointToPoint = point - linePoint;

		float t = Vector3.Dot(linePointToPoint, lineVec);

		return linePoint + lineVec * t;
	}

	/// <summary>
	/// This function finds out on which side of a line segment the point is located.
	/// The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	/// the line segment, project it on the line using ProjectPointOnLine() first.
	/// </summary>
	/// <returns>
	/// Returns 0 if point is on the line segment.
	/// Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	/// Returns 2 if point is outside of the line segment and located on the side of linePoint2.
	/// </returns>
	public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){

		Vector3 lineVec = linePoint2 - linePoint1;
		Vector3 pointVec = point - linePoint1;

		float dot = Vector3.Dot(pointVec, lineVec);

		//point is on side of linePoint2, compared to linePoint1
		if(dot > 0){

			//point is on the line segment
			if(pointVec.magnitude <= lineVec.magnitude){

				return 0;
			}

			//point is not on the line segment and it is on the side of linePoint2
			else{

				return 2;
			}
		}

		//Point is not on side of linePoint2, compared to linePoint1.
		//Point is not on the line segment and it is on the side of linePoint1.
		else{

			return 1;
		}
	}
	/// <summary>
	/// This function returns a point which is a projection from a point to a line segment.
	/// If the projected point lies outside of the line segment, the projected point will 
	/// be clamped to the appropriate line edge.
	/// If the line is infinite instead of a segment, use ProjectPointOnLine() instead.
	/// </summary>
	public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point){

		Vector3 vector = linePoint2 - linePoint1;

		Vector3 projectedPoint = ProjectPointOnLine(linePoint1, vector.normalized, point);

		int side = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, projectedPoint);

		//The projected point is on the line segment
		if(side == 0){

			return projectedPoint;
		}

		if(side == 1){

			return linePoint1;
		}

		if(side == 2){

			return linePoint2;
		}

		//output is invalid
		return Vector3.zero;
	}

	private void setDestination(string dest){
		currentDestIndex = doors.FindIndex (x => x.point.name == dest);
		currentDestSet = true;
	}

	/// <summary>
	/// Create the NavMesh and enable the navigation system.
	/// </summary>
	/// <returns>Coroutine IEnumerator.</returns>
	private IEnumerator DrawCorridorsRoutine ()
	{
		// If the mesh hasn't been created yet then do it.
		if (!mesh.activeSelf) {
			Debug.Log ("Creating mesh...");
			// Put the mesh at the floor level
			mesh.transform.Translate (Vector3.down * (mesh.transform.position.y - floor.floorCenter.y));
			MeshCreator.CreateMesh (mesh, Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Marker.xml",
				Application.persistentDataPath + "/" + m_curAreaDescription.m_uuid + "Wall.xml", m_curAreaDescription.m_uuid);
			GameObject[] gs = GameObject.FindGameObjectsWithTag ("CornersAndWalls");
			// Hide all the markers.
			foreach (GameObject g in gs) {
				g.SetActive (false);
			}
		} // The mesh has been already created, then navigate.
		else {
			Debug.Log ("Showing mesh...");

			List<string> places = new List<string> ();
			foreach (Corner c in doors) {
				places.Add (c.point.name);
			}
			places.Sort ();
			// Show the destinations in a list.
			singleChoice.Show (places.ToArray ());

			singleChoice.OnResult.AddListener ((value) => setDestination (value));
			// Wait for the destination to be set.
			while (!currentDestSet) {
				yield return null;
			}

			// Destroy the previous path
			GameObject[] gs = GameObject.FindGameObjectsWithTag("Path");
			foreach (GameObject game in gs) {
				Destroy (game);
			}

			NavMeshPath path = new NavMeshPath ();
			NavMeshHit hit;
			NavMeshHit hit2;

			Vector3 startingPoint = m_poseController.transform.position;

			// The destination point isn't on the NavMesh, therefore we find the closest point to it on the mesh
			Debug.Log (NavMesh.SamplePosition (startingPoint, out hit, 2f, NavMesh.AllAreas));

			Vector3 destPoint = doors[currentDestIndex].point.transform.position;
			Debug.Log (NavMesh.SamplePosition (destPoint, out hit2, 2f, NavMesh.AllAreas));
			Debug.Log (NavMesh.CalculatePath (hit.position, hit2.position, NavMesh.AllAreas, path));

			GameObject g = DrawLine (path.corners [0], path.corners [1], 9);
			LineRenderer lr = g.GetComponent<LineRenderer> ();
			Vector3[] newPath = new Vector3[path.corners.Length + 1];
			path.corners.CopyTo (newPath, 0);
			newPath [newPath.Length - 1] = destPoint;
			lr.positionCount = newPath.Length;
			lr.SetPositions (newPath);
			lr.material.color = Color.blue;
			lr.widthMultiplier = 10f;
			lr.tag = "Path";

			currentDestSet = false;
		}
		yield break;

	}

	/// <summary>
	/// Data container for marker.
	/// 
	/// Used for serializing/deserializing marker to xml.
	/// </summary>
	[System.Serializable]
	public class MarkerData
	{
		/// <summary>
		/// Marker's type.
		/// 
		/// Red, green or blue markers. In a real game scenario, this could be different game objects
		/// (e.g. banana, apple, watermelon, persimmons).
		/// </summary>
		[XmlElement ("type")]
		public int m_type;

		/// <summary>
		/// Position of this mark, with respect to the origin of the game world.
		/// </summary>
		[XmlElement ("position")]
		public Vector3 m_position;

		/// <summary>
		/// Rotation of this mark.
		/// </summary>
		[XmlElement ("orientation")]
		public Quaternion m_orientation;

		/// <summary>
		/// ID of this mark.
		/// </summary>
		[XmlElement ("id")]
		public int m_id;

		/// <summary>
		/// Name of this mark.
		/// </summary>
		[XmlElement ("name")]
		public string m_name;
	}
}
