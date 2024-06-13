using UnityEngine;

public class SunCycle : MonoBehaviour {
	public float speed=5;

	void Update () {
		transform.rotation *= Quaternion.Euler (speed * Time.deltaTime, 0, 0);
	}
}
