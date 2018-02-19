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

//	public class Triangulator
//	{
//		private List<Vector2> m_points = new List<Vector2> ();
//
//		public Triangulator (Vector2[] points)
//		{
//			m_points = new List<Vector2> (points);
//		}
//
//		public int[] Triangulate ()
//		{
//			List<int> indices = new List<int> ();
//
//			int n = m_points.Count;
//			if (n < 3)
//				return indices.ToArray ();
//
//			int[] V = new int[n];
//			if (Area () > 0) {
//				for (int v = 0; v < n; v++)
//					V [v] = v;
//			} else {
//				for (int v = 0; v < n; v++)
//					V [v] = (n - 1) - v;
//			}
//
//			int nv = n;
//			int count = 2 * nv;
//			for (int m = 0, v = nv - 1; nv > 2;) {
//				if ((count--) <= 0)
//					return indices.ToArray ();
//
//				int u = v;
//				if (nv <= u)
//					u = 0;
//				v = u + 1;
//				if (nv <= v)
//					v = 0;
//				int w = v + 1;
//				if (nv <= w)
//					w = 0;
//
//				if (Snip (u, v, w, nv, V)) {
//					int a, b, c, s, t;
//					a = V [u];
//					b = V [v];
//					c = V [w];
//					indices.Add (a);
//					indices.Add (b);
//					indices.Add (c);
//					m++;
//					for (s = v, t = v + 1; t < nv; s++, t++)
//						V [s] = V [t];
//					nv--;
//					count = 2 * nv;
//				}
//			}
//
//			indices.Reverse ();
//			return indices.ToArray ();
//		}
//
//		private float Area ()
//		{
//			int n = m_points.Count;
//			float A = 0.0f;
//			for (int p = n - 1, q = 0; q < n; p = q++) {
//				Vector2 pval = m_points [p];
//				Vector2 qval = m_points [q];
//				A += pval.x * qval.y - qval.x * pval.y;
//			}
//			return (A * 0.5f);
//		}
//
//		private bool Snip (int u, int v, int w, int n, int[] V)
//		{
//			int p;
//			Vector2 A = m_points [V [u]];
//			Vector2 B = m_points [V [v]];
//			Vector2 C = m_points [V [w]];
//			if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
//				return false;
//			for (p = 0; p < n; p++) {
//				if ((p == u) || (p == v) || (p == w))
//					continue;
//				Vector2 P = m_points [V [p]];
//				if (InsideTriangle (A, B, C, P))
//					return false;
//			}
//			return true;
//		}
//
//		private bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P)
//		{
//			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
//			float cCROSSap, bCROSScp, aCROSSbp;
//
//			ax = C.x - B.x;
//			ay = C.y - B.y;
//			bx = A.x - C.x;
//			by = A.y - C.y;
//			cx = B.x - A.x;
//			cy = B.y - A.y;
//			apx = P.x - A.x;
//			apy = P.y - A.y;
//			bpx = P.x - B.x;
//			bpy = P.y - B.y;
//			cpx = P.x - C.x;
//			cpy = P.y - C.y;
//
//			aCROSSbp = ax * bpy - ay * bpx;
//			cCROSSap = cx * apy - cy * apx;
//			bCROSScp = bx * cpy - by * cpx;
//
//			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
//		}
//	}

//	public class ObjExporter {
//
//		public static string MeshToString(MeshFilter mf) {
//			Mesh m = mf.mesh;
//			Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;
//
//			StringBuilder sb = new StringBuilder();
//
//			sb.Append("g ").Append(mf.name).Append("\n");
//			foreach(Vector3 v in m.vertices) {
//				sb.Append(string.Format("v {0} {1} {2}\n",v.x,v.y,v.z));
//			}
//			sb.Append("\n");
//			foreach(Vector3 v in m.normals) {
//				sb.Append(string.Format("vn {0} {1} {2}\n",v.x,v.y,v.z));
//			}
//			sb.Append("\n");
//			foreach(Vector3 v in m.uv) {
//				sb.Append(string.Format("vt {0} {1}\n",v.x,v.y));
//			}
//			for (int material=0; material < m.subMeshCount; material ++) {
//				sb.Append("\n");
//				sb.Append("usemtl ").Append(mats[material].name).Append("\n");
//				sb.Append("usemap ").Append(mats[material].name).Append("\n");
//
//				int[] triangles = m.GetTriangles(material);
//				for (int i=0;i<triangles.Length;i+=3) {
//					sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
//						triangles[i]+1, triangles[i+1]+1, triangles[i+2]+1));
//				}
//			}
//			return sb.ToString();
//		}
//
//		public static void MeshToFile(MeshFilter mf, string filename) {
//			using (StreamWriter sw = new StreamWriter(filename)) 
//			{
//				sw.Write(MeshToString(mf));
//			}
//		}
//	}

//	public class Exporter : MonoBehaviour {
//
//		private static int StartIndex = 0;
//
//		public static void Start()
//		{
//			StartIndex = 0;
//		}
//		public static void End()
//		{
//			StartIndex = 0;
//		}
//
//
//		public static string MeshToString(MeshFilter mf, Transform t) 
//		{    
//			Vector3 s = t.localScale;
//			Vector3 p = t.localPosition;
//			Quaternion r = t.localRotation;
//
//
//			int numVertices = 0;
//			Mesh m = mf.sharedMesh;
//			if (!m) {
//				return "####Error####";
//			}
//			Material[] mats = mf.GetComponent<Renderer> ().sharedMaterials;
//
//			StringBuilder sb = new StringBuilder ();
//
//			Vector3[] normals = m.normals; 
//			for (int i=0; i<normals.Length; i++) // remove this if your exported mesh have faces on wrong side
//				normals [i] = -normals [i];
//			m.normals = normals;
//
//			m.triangles = m.triangles.Reverse ().ToArray (); //
//
//			foreach (Vector3 vv in m.vertices) {
//				Vector3 v = t.TransformPoint (vv);
//				numVertices++;
//				sb.Append (string.Format ("v {0} {1} {2}\n", v.x, v.y, -v.z));
//			}
//			sb.Append ("\n");
//			foreach (Vector3 nn in m.normals) {
//				Vector3 v = r * nn;
//				sb.Append (string.Format ("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
//			}
//			sb.Append ("\n");
//			foreach (Vector3 v in m.uv) {
//				sb.Append (string.Format ("vt {0} {1}\n", v.x, v.y));
//			}
//			for (int material=0; material < m.subMeshCount; material ++) {
//				sb.Append ("\n");
//				sb.Append ("usemtl ").Append (mats [material].name).Append ("\n");
//				sb.Append ("usemap ").Append (mats [material].name).Append ("\n");
//
//				int[] triangles = m.GetTriangles (material);
//				for (int i=0; i<triangles.Length; i+=3) {
//					sb.Append (string.Format ("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n", 
//						triangles [i] + 1 + StartIndex, triangles [i + 1] + 1 + StartIndex, triangles [i + 2] + 1 + StartIndex));
//				}
//			}
//
//			for (int i=0; i<normals.Length; i++) // remove this if your exported mesh have faces on wrong side
//				normals [i] = -normals [i];
//			m.normals = normals;
//
//			m.triangles = m.triangles.Reverse ().ToArray (); //
//
//			StartIndex += numVertices;
//			return sb.ToString ();
//		}
//
//		public void DoExport(bool makeSubmeshes)
//		{
//
//			string meshName = gameObject.name;
//			string fileName = Application.persistentDataPath+"/"+gameObject.name+".obj"; // you can also use: "/storage/sdcard1/" +gameObject.name+".obj"
//
//			Start();
//
//			StringBuilder meshString = new StringBuilder();
//
//			meshString.Append("#" + meshName + ".obj"
//				+ "\n#" + System.DateTime.Now.ToLongDateString() 
//				+ "\n#" + System.DateTime.Now.ToLongTimeString()
//				+ "\n#-------" 
//				+ "\n\n");
//
//			Transform t = transform;
//
//			Vector3 originalPosition = t.position;
//			t.position = Vector3.zero;
//
//			if (!makeSubmeshes)
//			{
//				meshString.Append("g ").Append(t.name).Append("\n");
//			}
//			meshString.Append(processTransform(t, makeSubmeshes));
//
//			WriteToFile(meshString.ToString(),fileName);
//
//			t.position = originalPosition;
//
//			End();
//			Debug.Log("Exported Mesh: " + fileName);
//		}
//
//		static string processTransform(Transform t, bool makeSubmeshes)
//		{
//			StringBuilder meshString = new StringBuilder();
//
//			meshString.Append("#" + t.name
//				+ "\n#-------" 
//				+ "\n");
//
//			if (makeSubmeshes)
//			{
//				meshString.Append("g ").Append(t.name).Append("\n");
//			}
//
//			MeshFilter mf = t.GetComponent<MeshFilter>();
//			if (mf)
//			{
//				meshString.Append(MeshToString(mf, t));
//			}
//
//			for(int i = 0; i < t.childCount; i++)
//			{
//				meshString.Append(processTransform(t.GetChild(i), makeSubmeshes));
//			}
//
//			return meshString.ToString();
//		}
//
//		static void WriteToFile(string s, string filename)
//		{
//			using (StreamWriter sw = new StreamWriter(filename)) 
//			{
//				sw.Write(s);
//			}
//		}
//	}
}