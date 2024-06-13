using UnityEngine;

public class TextureCycle : MonoBehaviour {
	public float uChange = 0;
	public float vChange = 0.01f;

	void Update () {
		float scalar = Mathf.Min(Time.deltaTime * 100, 2);

		Mesh myMesh = GetComponent<MeshFilter> ().mesh;
		Vector2[] uv = myMesh.uv;
		for (int i = 0; i < uv.Length; i++) {
			uv [i] += new Vector2(uChange * scalar, vChange * scalar);
		}
		myMesh.uv = uv;
	}
}
