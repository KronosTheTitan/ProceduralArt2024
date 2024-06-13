using UnityEngine;

public class MeshBreathe : MonoBehaviour {
	[Range(1,200)]
	public int timeFactor=60;
	public float strength=0.01f;
	float timer;

	void Start () {
		timer = 0;
	}

	void Update () {
		float scalar = Mathf.Min(Time.deltaTime * 100,2);
		timer += scalar;
		Mesh myMesh = GetComponent<MeshFilter> ().mesh;
		Vector3[] verts = myMesh.vertices;
		Vector3[] norms = myMesh.normals;
		for (int i = 0; i < verts.Length; i++) {
			verts [i] += scalar * strength * norms [i] * (Mathf.Cos (timer * Mathf.PI / timeFactor));
		}
		myMesh.vertices = verts;
	}
}
