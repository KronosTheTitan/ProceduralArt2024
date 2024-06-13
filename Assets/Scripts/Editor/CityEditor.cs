using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(City))]
public class CityEditor : Editor
{
	private City city;

	// TODO (1.1): Use OnInspectorGUI to add a button that calls Curve.Apply()

	private void OnEnable()
	{
		city = (City)target;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
	}

	// This method is called by Unity whenever it renders the scene view.
	// We use it to draw gizmos, and deal with changes (dragging objects)
	void OnSceneGUI() {
 		if (city.intersections==null)
			return;

		bool dirty = false;

		// Add new points if needed:
		Event e = Event.current;
		if ((e.type==EventType.KeyDown && e.keyCode == KeyCode.Space)) {
			Debug.Log("Space pressed - trying to add point to curve");
			//dirty |= AddPoint();
		}

		if(city.handles)
			ShowAndMovePoints();
 	}

	// Tries to add a point to the curve, where the mouse is in the scene view.
	// Returns true if a change was made.
	bool AddPoint() {
		bool dirty = false;
		//Transform handleTransform = curve.transform;

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)) {
			Debug.Log("Adding spline point at mouse position: "+hit.point);
			// TODO (1.2): Add this action to the undo list
			//curve.points.Add(handleTransform.InverseTransformPoint(hit.point));
			dirty=true;
		}
		return dirty;
	}

	// Show points in scene view, and check if they're changed:
	bool ShowAndMovePoints() {
		bool dirty = false;
		Transform handleTransform = city.transform;

		Vector3 previousPoint = Vector3.zero;
		for (int i = 0; i < city.intersections.Count; i++)
		{
			Vector3 currentPoint = handleTransform.TransformPoint(city.Conversion(city.intersections[i]));

			// TODO (1.2): Draw a line from previous point to current point, in white


			previousPoint=currentPoint;

			// TODO (1.2): 
			//  Record in the undo list and mark the scene dirty when the handle is moved.

			EditorGUI.BeginChangeCheck();
			currentPoint = Handles.DoPositionHandle(currentPoint, Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				city.intersections[i] = city.Conversion(handleTransform.InverseTransformPoint(currentPoint));
				dirty = true;
			}

		}
		return dirty;
	}
}
