using System.Collections.Generic;
using UnityEngine;
using Handout;

public class WarpMeshAlongSpline : MeshCreator
{
	public Mesh InputMesh;
	public Vector3 MeshOrigin;
	public float MeshScale;
	public Vector2 TextureScale;
	public bool ComputeUVs;
	public bool ModifySharedMesh;

	public override void RecalculateMesh() {
		Curve curve = GetComponent<Curve>();
		if (curve==null)
			return;
		List<Vector3> points = curve.points;

		Debug.Log("Recalculating spline mesh");

		MeshBuilder builder = new MeshBuilder();
		if (points.Count<2) {
			GetComponent<MeshFilter>().mesh = builder.CreateMesh(true);
			return;
		}

		Bounds bounds = InputMesh.bounds;
		Vector3 max = bounds.max;
		Vector3 min = bounds.min;

		// First, compute directions & orientations for each line segment of the curve:
		// TODO: for a better looking curve: for each curve point, first compute the *average direction* (normalized) of *both* incident line segments,
		//   and then use that direction to compute the orientation of the point:
		var localOrientation = new List<Quaternion>();
		for (int i = 0; i < points.Count-1; i++) {
			// Compute a unit length vector from the current point to the next:
			Vector3 lineSegmentDirection = (points[i+1]-points[i]).normalized;
			// Store a matching orientation (computing an orientation requires a forward direction vector and an up direction vector):
			localOrientation.Add(Quaternion.LookRotation(lineSegmentDirection,Vector3.up));
		}

		// Loop over all line segments in the curve:
		for (int i = 0; i < points.Count-1; i++) {
			// For each line segment, add a rotated version of the input mesh to the output mesh, using the localOrientation as rotation:
			int numVerts = InputMesh.vertexCount;
			for (int j = 0; j<InputMesh.vertexCount; j++) {
				// Map z coordinate to a number t from 0 to 1 (assuming the mesh bounds are correct):
				float t = (InputMesh.vertices[j].z - min.z) / (max.z - min.z);

				// Center and scale the input mesh vertices, using the values given in the inspector:
				Vector3 inputV = (InputMesh.vertices[j] - MeshOrigin) * MeshScale;
				// Set the z-coordinate to zero:
				inputV.Scale(new Vector3(1, 1, 0));

				// Use the value t to linearly interpolate between the start and end points of the line segment:
				// Choose one of the two lines below - they are completely equivalent!
				// Vector3 interpolatedLineSegmentPoint = Vector3.Lerp(points[i], points[i+1], t);
				Vector3 interpolatedLineSegmentPoint = points[i]*(1-t) + points[i+1] * t; // Lerp = the weighted average between two vectors

				// TODO: interpolate the orientations as well, not just the points!
				Vector3 rotatedXYModelCoordinate = localOrientation[i] * inputV;
				
				builder.AddVertex(
					interpolatedLineSegmentPoint + rotatedXYModelCoordinate,
					InputMesh.uv[j]/TextureScale
				);
			}
			// TODO: Take submeshes into account:
			int numTris = InputMesh.triangles.Length;
			for (int j = 0; j < numTris; j+=3) {
				builder.AddTriangle(
					InputMesh.triangles[j] + numVerts * i,
					InputMesh.triangles[j+1] + numVerts * i,
					InputMesh.triangles[j+2] + numVerts * i
				);
			}
		}

		Mesh mesh=builder.CreateMesh(true);
		var autoUV = GetComponent<AutoUv>();
		if (autoUV!=null && autoUV.enabled && ComputeUVs) {
			autoUV.UpdateUVs(mesh);
		}
		ReplaceMesh(mesh, ModifySharedMesh);
	}
}
