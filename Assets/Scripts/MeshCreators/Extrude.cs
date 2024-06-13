using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Extrude : MeshCreator {
	public float height = 1;
	public bool ModifySharedMesh = false;

	public override void RecalculateMesh() {
		Curve curve = GetComponent<Curve>();
		if (curve==null)
			return;
		List<Vector3> points = curve.points;
		if (points.Count < 2) {
			Debug.Log("Cannot triangulate polygons with less than 3 vertices");
			return;
		}
		// Copy the inspector array to a list that's going to be modified:
		List<Vector2> polygon = new List<Vector2>();
		for (int i = 0; i<points.Count; i++) {
			polygon.Add(new Vector2(points[i].x, points[i].y));
		}

		// Create a list of indices 0..n-1:
		List<int> indices = new List<int>();
		for (int i = 0; i < polygon.Count; i++) {
			indices.Add(i);
		}
		// This list is going to contain the vertex indices of the triangles: (3 integers per triangle)
		List<int> triangles = new List<int>();

		// Compute the triangulation of [polygon], store it in [triangles]:
		TriangulatePolygon(triangles, polygon, indices);

		MeshBuilder builder = new MeshBuilder();

		// Add front face:
		for (int i = 0; i < points.Count; i++) {
			// TODO: Add uvs
			builder.AddVertex(new Vector3(points[i].x, points[i].y, 0));
		}
		for (int t = 0; t < triangles.Count; t += 3) {
			builder.AddTriangle(triangles[t], triangles[t+1], triangles[t+2]);
			//Debug.Log ("Adding triangle " + triangles [t] + "," + triangles [t + 1] + "," + triangles [t + 2]);
		}
		// Add back face:
		int n = points.Count;
		for (int i = 0; i < points.Count; i++) {
			// TODO: Add uvs
			builder.AddVertex(new Vector3(points[i].x, points[i].y, height));
		}
		for (int t = 0; t < triangles.Count; t += 3) {
			builder.AddTriangle(n+triangles[t+2], n+triangles[t+1], n+triangles[t]);
		}

		// Add sides:
		for (int i = 0; i < points.Count; i++) {
			int j = (i + 1) % points.Count;
			// TODO: Add uvs
			// front vertices:
			int v1 = builder.AddVertex(new Vector3(points[i].x, points[i].y, 0));
			int v2 = builder.AddVertex(new Vector3(points[j].x, points[j].y, 0));
			// back vertices:
			int v3 = builder.AddVertex(new Vector3(points[i].x, points[i].y, height));
			int v4 = builder.AddVertex(new Vector3(points[j].x, points[j].y, height));
			// Add quad:
			builder.AddTriangle(v1, v3, v2);
			builder.AddTriangle(v2, v3, v4);
		}
		ReplaceMesh(builder.CreateMesh(), ModifySharedMesh);
	}

	// *IF* [polygon] respresents a simple polygon (no crossing edges), given in clockwise order, then 
	// this method will return in [triangles] a triangulation of the polygon, using the vertex indices from [indices]
	// If the assumption is not satisfied, the output is undefined.
	void TriangulatePolygon(List<int> triangles, List<Vector2> polygon, List<int> indices) {
		for (int i = 0; i < polygon.Count; i++) {
			int i2 = (i + 1) % polygon.Count;
			int i3 = (i + 2) % polygon.Count;
			Vector2 u = polygon[i];
			Vector2 v = polygon[i2];
			Vector2 w = polygon[i3];

			// TODO: Check whether the polygon corner at point v is less than 180 degrees - if not continue the for loop (with the next value for i)

			// TODO: Check whether there are no other points of the polygon inside the triangle u,v,w - if not continue the for loop (with the next value for i)

			// (Hint: see the methods below!!! :-)  )

			// Add a triangle on u,v,w:
			triangles.Add(indices[i]);
			triangles.Add(indices[i2]);
			triangles.Add(indices[i3]);

			polygon.RemoveAt(i2); // remove v from point list (keep u and w!)
			indices.RemoveAt(i2); // also remove the corresponding index from the index list
			if (polygon.Count < 3)
				return; // The polygon is now fully triangulated

			// continue with a smaller polygon - restart the for loop:
			i=-1;
		}
		throw new Exception("No suitable triangulation found - is the polygon simple and clockwise?");
	}

	// Returns true if p1,p2 and p3 form a clockwise triangle (returns false if anticlockwise, or all three on the same line)
	bool Clockwise(Vector2 p1, Vector2 p2, Vector2 p3) {
		Vector2 difference1 = (p2 - p1);
		Vector2 difference2 = (p3 - p2);
		// Take the dot product of the (normal of difference1) and (difference2):
		return (-difference1.y * difference2.x + difference1.x * difference2.y) < 0;
	}

	// Returns true if [testPoint] lies inside, or on the boundary, of the triangle given by the points p1,p2 and p3.
	bool InsideTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Vector3 testPoint) {
		if (Clockwise(p1, p2, p3))
			return !Clockwise(p2, p1, testPoint) && !Clockwise(p3, p2, testPoint) && !Clockwise(p1, p3, testPoint);
		else
			return !Clockwise(p1, p2, testPoint) && !Clockwise(p2, p3, testPoint) && !Clockwise(p3, p1, testPoint);
	}
}
