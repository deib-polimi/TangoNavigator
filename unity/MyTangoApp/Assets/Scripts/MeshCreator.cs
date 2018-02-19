using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TriangleNet.Geometry;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using IndoorNavigation;
using System.Linq;

public class MeshCreator
{
	private static float CalculateArea(List<Point> points){
		 return Mathf.Abs(points.Take(points.Count - 1)
			.Select((p, i) => (points[i + 1].X - p.X) * (points[i + 1].Y + p.Y))
			.Sum() / 2);
	}

	public static void CreateMesh (GameObject meshContainer, string path, string path2, string adfname)
	{
		var subgraphs = _LoadMarkerFromDisk (path, path2);
		meshContainer.name = adfname + "Mesh";

		InputGeometry geometry = new InputGeometry ();
		TriangleNet.Mesh meshRepresentation;
		MeshFilter mf = meshContainer.GetComponent<MeshFilter> () as MeshFilter;
		Mesh mesh;
		float zOffset = 0.1f;
		float[] maxArea = new float[2]{ 0, 0 };

		List<List<Point>> clusters = new List<List<Point>> ();

		foreach (Graph<Node> s in subgraphs) {
			List<Point> points = new List<Point> ();
			foreach (Node n in s.Nodes) {
				Point tmp = new Point (n.position.x, n.position.z);
				zOffset = n.position.y;
				points.Add (tmp);
			}
			if (points.Count > 2) {
				clusters.Add (points);
			
				// Calculate areas of the clusters. The largest is the external ring, while the others must be holes.
				float num = CalculateArea (points);
				if (num > maxArea [0]) {
					maxArea [0] = num;
					maxArea [1] = clusters.Count - 1;
				}
			}
			Debug.Log (clusters [clusters.Count - 1].Count);
		}

		geometry.AddRing (clusters[(int) maxArea[1]]);
		clusters.RemoveAt ((int) maxArea [1]);

		foreach (List<Point> c in clusters) {
			geometry.AddRingAsHole (c);
		}
		meshRepresentation = new TriangleNet.Mesh();
		meshRepresentation.Triangulate(geometry);

		//		Dictionary<int, float> zOffsets = new Dictionary<int, float>();
		//
		//		foreach(KeyValuePair<int, TriangleNet.Data.Vertex> pair in meshRepresentation.vertices)
		//		{
		//			zOffsets.Add(pair.Key, Random.RandomRange(-zOffset, zOffset));
		//		}

		int triangleIndex = 0;
		List<Vector3> vertices = new List<Vector3>(meshRepresentation.triangles.Count * 3);
		List<int> triangleIndices = new List<int>(meshRepresentation.triangles.Count * 3);

		foreach(KeyValuePair<int, TriangleNet.Data.Triangle> pair in meshRepresentation.triangles)
		{
			TriangleNet.Data.Triangle triangle = pair.Value;

			TriangleNet.Data.Vertex vertex0 = triangle.GetVertex(0);
			TriangleNet.Data.Vertex vertex1 = triangle.GetVertex(1);
			TriangleNet.Data.Vertex vertex2 = triangle.GetVertex(2);

			Vector3 p0 = new Vector3( vertex0.x, zOffset, vertex0.y);
			Vector3 p1 = new Vector3( vertex1.x, zOffset, vertex1.y);
			Vector3 p2 = new Vector3( vertex2.x, zOffset, vertex2.y);

			//			Vector3 p0 = new Vector3( vertex0.x, vertex0.y, zOffsets[vertex0.id]);
			//			Vector3 p1 = new Vector3( vertex1.x, vertex1.y, zOffsets[vertex1.id]);
			//			Vector3 p2 = new Vector3( vertex2.x, vertex2.y, zOffsets[vertex2.id]);

			vertices.Add(p0);
			vertices.Add(p1);
			vertices.Add(p2);

			triangleIndices.Add(triangleIndex + 2);
			triangleIndices.Add(triangleIndex + 1);
			triangleIndices.Add(triangleIndex);

			triangleIndex += 3;
		}
		mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangleIndices.ToArray();
		mesh.RecalculateNormals();
		//GetComponent<MeshFilter>().mesh = mesh;
		mf.mesh = mesh;

//		Exporter e = meshContainer.GetComponent<Exporter> () as Exporter;
//		e.DoExport (true);
//
//		File.WriteAllBytes(Application.persistentDataPath + "/" + adfname + "Mesh", MeshSerializer.WriteMesh (mesh, true));

		meshContainer.SetActive (true);
		meshContainer.GetComponent<NavMeshSurface> ().BuildNavMesh ();

		Debug.Log ("NavMesh created");

		//MeshSaverEditor.SaveMesh (mesh, "meshtest", true, true);

		//ObjExporterScript.MeshToString(GetComponent<MeshFilter>(),
	}

	private static IEnumerable<Graph<Node>> _LoadMarkerFromDisk (string p1, string p2)
	{
		// Attempt to load the exsiting markers from storage.
		string path = p1;

		var serializer = new XmlSerializer (typeof(List<MarkerData>));
		var stream = new FileStream (path, FileMode.Open);

		List<MarkerData> xmlDataList = serializer.Deserialize (stream) as List<MarkerData>;

		// This part is for the wall XML.
		string path2 = p2;
		var serializer2 = new XmlSerializer (typeof(List<WallXML>));
		var stream2 = new FileStream (path2, FileMode.Open);
		List<WallXML> xmlWallList = serializer2.Deserialize (stream2) as List<WallXML>;

		List<Node> nodes = new List<Node> ();

		Graph<Node> graph = new Graph<Node> ();

		foreach (MarkerData m in xmlDataList) {
			Node tmp = new Node (m.m_id, m.m_position);
			Debug.Log (tmp);
			nodes.Add (tmp);
			graph.AddNode (tmp);
		}

		List<Edge> edges = new List<Edge> ();

		foreach (WallXML w in xmlWallList) {
			Node start = nodes.Find (x => x.id == w.m_startCornerID);
			Node end = nodes.Find (x => x.id == w.m_endCornerID);
			Edge tmp = new Edge (start, end);
			Debug.Log (tmp);
			edges.Add (tmp);
			graph.AddArc (start, end);
		}

		var subgraph = graph.GetConnectedComponents ();
		
		return subgraph;


	}

	public class Graph<T>
	{
		public Dictionary<T, HashSet<T>> nodesNeighbors;

		public IEnumerable<T> Nodes {
			get { return nodesNeighbors.Keys; }
		}

		public Graph ()
		{
			this.nodesNeighbors = new Dictionary<T, HashSet<T>> ();
		}

		public void AddNode (T node)
		{
			this.nodesNeighbors.Add (node, new HashSet<T> ());
		}

		public void AddNodes (IEnumerable<T> nodes)
		{
			foreach (var n in nodes)
				this.AddNode (n);
		}

		public void AddArc (T from, T to)
		{
			this.nodesNeighbors [from].Add (to);
			this.nodesNeighbors [to].Add (from);
		}

		public bool ContainsNode (T node)
		{
			return this.nodesNeighbors.ContainsKey (node);
		}

		public IEnumerable<T> GetNeighbors (T node)
		{
			return nodesNeighbors [node];
		}

		public IEnumerable<T> DepthFirstSearch (T nodeStart)
		{
			var stack = new Stack<T> ();
			var visitedNodes = new HashSet<T> ();
			stack.Push (nodeStart);
			while (stack.Count > 0) {
				var curr = stack.Pop ();
				if (!visitedNodes.Contains (curr)) {
					visitedNodes.Add (curr);
					yield return curr;
					foreach (var next in this.GetNeighbors(curr)) {
						if (!visitedNodes.Contains (next))
							stack.Push (next);
					}
				}
			}
		}

		public Graph<T> GetSubGraph (IEnumerable<T> nodes)
		{
			Graph<T> g = new Graph<T> ();
			g.AddNodes (nodes);
			foreach (var n in g.Nodes.ToList()) {
				foreach (var neigh in this.GetNeighbors(n))
					g.AddArc (n, neigh);
			}
			return g;
		}

		public IEnumerable<Graph<T>> GetConnectedComponents ()
		{
			var visitedNodes = new HashSet<T> ();
			var components = new List<Graph<T>> ();

			foreach (var node in this.Nodes) {
				if (!visitedNodes.Contains (node)) {
					var subGraph = GetSubGraph (this.DepthFirstSearch (node));
					components.Add (subGraph);
					visitedNodes.UnionWith (subGraph.Nodes);
				}
			}
			return components;
		}
	}

	public class Node
	{
		public int id;
		public Vector3 position;

		public Node (int i, Vector3 p)
		{
		
			id = i;
			position = p;

		}

		public override string ToString ()
		{
			return string.Format ("Node {0} at {1}", id, position);
		}
	}

	public class Edge
	{
		public Node node1;
		public Node node2;

		public Edge (Node a, Node b)
		{

			node1 = a;
			node2 = b;

		}

		public override string ToString ()
		{
			return string.Format ("Edge between node {0} and {1}", node1, node2);
		}
	}

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
	}
}
