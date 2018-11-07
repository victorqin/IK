using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(ConeConstraint))]
public class ConeConstraintEditor : RotationConstraintEditor{
	private ConeConstraint _constraint = null;
	public ConeConstraint constraint{
		get{
			if(_constraint == null)
				_constraint = target as ConeConstraint;
			return _constraint;
		}
	}

	private SerializedProperty _autoUpdateP;
	private SerializedProperty _useAngleLimitsP;
	private SerializedProperty _useConeLimitP;

	void OnEnable(){
		_autoUpdateP = serializedObject.FindProperty("autoUpdate");
		_useAngleLimitsP = serializedObject.FindProperty("useAngleLimits");
		_useConeLimitP = serializedObject.FindProperty("useConeLimit");
	}

	void OnSceneGUI(){
		if(!Application.isPlaying){
			// when not in running mode, object's current local rotation is
			// the default local rotation.
			constraint.defaultLocalRotation = constraint.transform.localRotation;
		}
		DrawAxes();
	}

	public override void OnInspectorGUI(){
		// Update the serializedProperty - always do this in the beginning
		// of OnInspectorGUI.
        serializedObject.Update();

		// auto update checkbox
		EditorGUILayout.PropertyField(_autoUpdateP, new GUIContent("Auto Update"));

		// rotation axis vector field
		EditorGUI.BeginChangeCheck();
		Vector3 axis = EditorGUILayout.Vector3Field(
			"Rotation Axis", constraint.rotationAxis);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Rotation Axis");
			constraint.rotationAxis = axis;
		}

		// use cone limit checkbox
		EditorGUILayout.PropertyField(_useConeLimitP, new GUIContent("Use Cone Limits"));

		// cone angle
		EditorGUI.BeginChangeCheck();
		float coneAngle = EditorGUILayout.Slider("Cone Angle", constraint.coneAngle, 0f, 90f);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Cone Angle");
			constraint.coneAngle = coneAngle;
		}

		// use angle limits checkbox
		EditorGUILayout.PropertyField(_useAngleLimitsP, new GUIContent("Use Angle Limits"));

		// min angle limits
		EditorGUI.BeginChangeCheck();
		float min = EditorGUILayout.Slider("Min Angle", constraint.minAngle, -180f, 180f);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Min Angle Limit");
			constraint.minAngle = min;
		}

		// max angle limits
		EditorGUI.BeginChangeCheck();
		float max = EditorGUILayout.Slider("Max Angle", constraint.maxAngle, -180f, 180);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Max Angle Limit");
			constraint.maxAngle = max;
		}

		// Apply changes to the serializedProperty - always do this in the end
		// of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties ();
	}

	private void DrawAxes(){
		Vector3 p = constraint.transform.position;
		Vector3 axis = ToWorldSpace(constraint.rotationAxis).normalized;
		Vector3 axisSwinged = ToWorldSpaceWithSwing(constraint.rotationAxis).normalized;

		Vector3 crossAxis = ToWorldSpace(constraint.crossAxis);
		Vector3 crossAxisSwinged = ToWorldSpaceWithSwing(constraint.crossAxis);

		Vector3 rotatedAxis = constraint.transform.TransformDirection(
			constraint.rotationAxis).normalized;

		//color for axis related to twist rotation, green-yellow
		Color c1 = new Color(0.667f, 1.0f, 0.0f, 1.0f);
		Color c2 = new Color(0.667f, 1.0f, 0.0f, 0.1f);

		// color for axis related to swing rotation, yellow
		Color c3 = Color.yellow;
		Color c4 = new Color(1.0f, 1.0f, 0.0f, 0.1f);

		Handles.color = c1;

		// draw circle
		Handles.DrawWireDisc(p, axisSwinged, 0.5f);

		// rotation axis
		DrawArrow(p, axis*0.75f, c1, "Axis");

		// axis where 0 degree starts
		DrawArrow(p, crossAxisSwinged*0.5f, c1, "0");

		// draw cone
		Handles.color = c3;
		Vector3 coneDir = Mathf.Cos(constraint.coneAngle/180*Mathf.PI) * axis +
			Mathf.Sin(constraint.coneAngle/180*Mathf.PI) * crossAxis;
		Handles.DrawWireArc(p, axis, coneDir, 360, 0.5f);
		Handles.color = c4;
		Handles.DrawSolidArc(p, axis, coneDir, 360, 0.5f);

		// angle limit axes
		if(constraint.useAngleLimits){
			Handles.color = c1;
			Vector3 minDir = ToWorldSpaceWithSwing(Quaternion.AngleAxis(
				constraint.minAngle, constraint.rotationAxis) * constraint.crossAxis);
			Vector3 maxDir = ToWorldSpaceWithSwing(Quaternion.AngleAxis(
				constraint.maxAngle, constraint.rotationAxis) * constraint.crossAxis);
			DrawArrow(p, minDir*0.75f, c1, "Min");
			DrawArrow(p, maxDir*0.75f, c1, "Max");

			Handles.color = c2;
			Handles.DrawSolidArc(
				p, axisSwinged, minDir, constraint.maxAngle - constraint.minAngle, 0.5f);
		}

		if(Application.isPlaying){
			Vector3 newCrossAxis = constraint.transform.TransformDirection(constraint.crossAxis);
			DrawArrow(p, newCrossAxis*0.75f, c1, "Current");
			DrawArrow(p, axisSwinged*0.75f, c3, "Current Axis");
		}
	}

	private Vector3 ToWorldSpace(Vector3 v){
		if(constraint.transform.parent){
			return constraint.transform.parent.rotation * constraint.defaultLocalRotation * v;
		}else{
			return constraint.defaultLocalRotation * v;
		}
	}

	private Vector3 ToWorldSpaceWithSwing(Vector3 v){
		if(constraint.transform.parent){
			return constraint.transform.parent.rotation *
				constraint.defaultLocalRotation * constraint.swingRotation * v;
		}else{
			return constraint.defaultLocalRotation * constraint.swingRotation * v;
		}
	}
}
