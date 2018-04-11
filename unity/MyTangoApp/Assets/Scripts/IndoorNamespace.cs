using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace IndoorNavigation {

	/// <summary>
	/// A wall is defined as a line uniting two corners.
	/// </summary>
	public class Wall
	{
		public Corner fromCorner;
		public Corner toCorner;
		public int lineID;

		public Wall(){
			fromCorner = new Corner ();
			toCorner = new Corner ();
			lineID = new int ();
		}

		public Wall (Corner from, Corner to, int id)
		{
			fromCorner = from;
			toCorner = to;
			lineID = id;
		}

		public override string ToString ()
		{
			return string.Format ("{0} {1}", fromCorner, toCorner);
		}

		/// <summary>
		/// Give the two closest intersections of the corridor with walls.
		/// </summary>
		/// <returns>Two Vector3 points.</returns>
		/// <param name="walls">List of all the existing walls.</param>
		/// <param name="midPoint">Point from which you draw the rays which will intersect the walls.</param>
		/// <param name="dirBisect">Direction of the ray.</param>
		public static Vector3[] Intersect (List<Wall> walls, Vector3 midPoint, Vector3 dirBisect, int layerMask){

			Vector3[] intersections = new Vector3[4] { Vector3.zero, Vector3.zero, midPoint, Vector3.zero};
			//float dist = 0;
			float minDist = Mathf.Infinity;
			//float maxDist = Mathf.Infinity;
			midPoint += dirBisect;
			Ray ray = new Ray (midPoint, dirBisect);
			Ray ray2 = new Ray (midPoint, -dirBisect);	

			RaycastHit hitInfo;

			if (Physics.Raycast (ray, out hitInfo, Mathf.Infinity, layerMask)) {
				minDist = hitInfo.distance;
				intersections [0] = hitInfo.point;
				Debug.Log("TRY TO SEND RAYS FROM " + midPoint + " in direction of " + dirBisect);
			}
			if (Physics.Raycast (ray2, out hitInfo, Mathf.Infinity, layerMask)) {
				Debug.Log("TRY TO SEND RAYS FROM " + midPoint + " in direction of " + -dirBisect);
				if (hitInfo.distance < minDist) {
					intersections [1] = intersections [0];
					intersections [0] = hitInfo.point;
				} else {
					intersections [1] = hitInfo.point;
				}
			}
			if (intersections [1] != Vector3.zero) {
				intersections [3] = Vector3.right;
			}
			Debug.Log ("INTERSECTIONS AT " + intersections [0] + " AND " + intersections[1]);

			return intersections;
		}

		// Compute the direction of the line.
		private Vector3 ComputeLineEquation (Corner c1, Corner c2)
		{
			Vector3 direction = c1.point.transform.position - c2.point.transform.position;
			return direction.normalized;
		}

		private float PointToRayDistance (Ray ray, Vector3 point){
			return Vector3.Cross (ray.direction, point - ray.origin).magnitude;
		}

		public List<Vector3[]> ComputeCorridors (List<Wall> walls, float n)
		{

			List<Vector3> directions = new List<Vector3> ();
			List<Vector3[]> corridors = new List<Vector3[]> ();
			//float y = walls [0].fromCorner.point.transform.position.y;
			// Loop on every wall
			foreach (Wall w in walls) {
				directions.Add (ComputeLineEquation (w.fromCorner, w.toCorner));
				Debug.Log ("WALL: " + ComputeLineEquation(w.fromCorner, w.toCorner));
			}



			// Loop on every wall after in the list
			for (int i = 0; i < walls.Count - 1; i++) {
				for (int v = i + 1; v < walls.Count; v++) {
					// If the lines are parallel and the distance is less than 3.0f then draw a mid line
					Wall line1 = walls [i];
					Wall line2 = walls [v];
					Vector3 dir1 = directions [i];
					Vector3 dir2 = directions [v];
					Vector3 dirBisect;
					Vector3 midPoint;
					float dist1;
					float dist2;

					Debug.Log (i + ") ANGLE: " + Vector3.Angle (dir1, dir2));

					// Case of an angle close to 0
					if (Mathf.Abs (Vector3.Angle (dir1, dir2)) < 10.0f) {
						midPoint = (line1.fromCorner.point.transform.position - line2.toCorner.point.transform.position) / 2
							+ line2.toCorner.point.transform.position;
						Debug.Log ("MIDPOINT " + midPoint);
						dirBisect = (directions [i] + directions [v]).normalized;

						dist1 = PointToRayDistance (new Ray (walls [i].fromCorner.point.transform.position,
							walls [i].fromCorner.point.transform.position - walls [i].toCorner.point.transform.position), midPoint);
						dist2 = PointToRayDistance (new Ray(walls [v].fromCorner.point.transform.position,
							walls [v].fromCorner.point.transform.position - walls [v].toCorner.point.transform.position), midPoint);

						Debug.Log ("DIST1: " + dist1 + ", DIST2: " + dist2);

						if (dist1 + dist2 < n) {

							var layerMask = 1 << 8;
							corridors.Add (Intersect (walls, midPoint, dirBisect, layerMask));

						}

					} // Case of an angle close to 180
					else if (Mathf.Abs (180 - Vector3.Angle (dir1, dir2)) < 10.0f) {
						midPoint = (line1.fromCorner.point.transform.position - line2.fromCorner.point.transform.position) / 2
							+ line2.fromCorner.point.transform.position;
						Debug.Log ("MIDPOINT " + midPoint);
						dirBisect = (directions [i] - directions [v]).normalized;

						dist1 = PointToRayDistance (new Ray (walls [i].fromCorner.point.transform.position,
							walls [i].fromCorner.point.transform.position - walls [i].toCorner.point.transform.position), midPoint);
						dist2 = PointToRayDistance (new Ray(walls [v].fromCorner.point.transform.position,
							walls [v].fromCorner.point.transform.position - walls [v].toCorner.point.transform.position), midPoint);

						Debug.Log ("DIST1: " + dist1 + ", DIST2: " + dist2);

						if (dist1 + dist2 < n) {

							var layerMask = 1 << 8;
							corridors.Add (Intersect (walls, midPoint, dirBisect, layerMask));

						}
					}
				}
			}
			return corridors;
		}
	}

	/// <summary>
	/// Class which represent a node. With these nodes we represent the corners of a room/corridor in order to build a connected graph.
	/// </summary>
	public class Corner {
		public GameObject point;
		public Wall wallOn;

		public Corner(){
			point = new GameObject ();
			wallOn = null;
		}

		public Corner (GameObject p)
		{
			point = p;
			wallOn = null;
		}

		public Corner (GameObject p, Wall w)
		{
			point = p;
			wallOn = w;
		}

		public override string ToString ()
		{
			return string.Format ("{0}", point.transform.position);
		}
	}

	public class Floor
	{
		public Plane floorPlane;
		public Vector3 floorCenter;

		public Floor (Plane fp, Vector3 fc)
		{
			floorPlane = fp;
			floorCenter = fc;
		}
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

	/// <summary>
	/// Data container for a wall.
	/// 
	/// Used for serializing/deserializing walls to xml.
	/// </summary>
	[System.Serializable]
	public class WallXML
	{
		[XmlElement ("startCornerID")]
		public int m_startCornerID;

		[XmlElement ("endCornerID")]
		public int m_endCornerID;
	}
}