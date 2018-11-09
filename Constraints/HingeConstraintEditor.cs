using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(HingeConstraint))]
public class HingeConstraintEditor : RotationConstraintEditor{
	private HingeConstraint _constraint = null;
	public HingeConstraint constraint{
		get{
			if(_constraint == null)
				_constraint = target as HingeConstraint;
			return _constraint;
		}
	}

	private SerializedProperty _autoUpdateP;
	private SerializedProperty _useAngleLimitsP;

	void OnEnable(){
		// Set up serialized properties.
		// Accessing properties through serialized properties rather through
		// target is because undo, multi-editing is handled automatically.
		_autoUpdateP = serializedObject.FindProperty("autoUpdate");
		_useAngleLimitsP = serializedObject.FindProperty("useAngleLimits");
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
        serializedObject.Update ();

		bool anythingChanged = false;

		// auto update checkbox
		EditorGUILayout.PropertyField(_autoUpdateP, new GUIContent("Auto Update"));

		// rotation axis vector field
		EditorGUI.BeginChangeCheck();
		Vector3 axis = EditorGUILayout.Vector3Field(
			"Rotation Axis", constraint.rotationAxis);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Rotation Axis");
			constraint.rotationAxis = axis;
			anythingChanged = true;
		}

		// use angle limits checkbox
		EditorGUILayout.PropertyField(_useAngleLimitsP, new GUIContent("Use Angle Limits"));

		// min angle limits
		EditorGUI.BeginChangeCheck();
		float min = EditorGUILayout.Slider("Min Angle", constraint.minAngle, -180f, 180f);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Min Angle Limit");
			constraint.minAngle = min;
			anythingChanged = true;
		}

		// max angle limits
		EditorGUI.BeginChangeCheck();
		float max = EditorGUILayout.Slider("Max Angle", constraint.maxAngle, -180f, 180);
		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Change Max Angle Limit");
			constraint.maxAngle = max;
			anythingChanged = true;
		}

		if(anythingChanged){
			EditorUtility.SetDirty(constraint);
		}

		// Apply changes to the serializedProperty - always do this in the end
		// of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties ();
	}

	private void DrawAxes(){
		Vector3 p = constraint.transform.position;
		Vector3 axis = ToWorldSpace(constraint.rotationAxis).normalized;
		Vector3 crossAxis = ToWorldSpace(constraint.crossAxis);

		Color c1 = new Color(0.667f, 1.0f, 0.0f, 1.0f);
		Color c2 = new Color(0.667f, 1.0f, 0.0f, 0.1f);

		Handles.color = c1;
		Handles.DrawWireDisc(p, axis, 0.5f);

		DrawArrow(p, axis*0.75f, c1, "Axis");
		DrawArrow(p, crossAxis*0.5f, c1, "0");

		if(constraint.useAngleLimits){
			Vector3 minDir = ToWorldSpace(Quaternion.AngleAxis(constraint.minAngle, constraint.rotationAxis) * constraint.crossAxis);
			Vector3 maxDir = ToWorldSpace(Quaternion.AngleAxis(constraint.maxAngle, constraint.rotationAxis) * constraint.crossAxis);
			DrawArrow(p, minDir*0.75f, c1, "Min");
			DrawArrow(p, maxDir*0.75f, c1, "Max");

			Handles.color = c2;
			Handles.DrawSolidArc(
				p, axis, minDir, constraint.maxAngle - constraint.minAngle, 0.5f);
		}

		if(Application.isPlaying){
			Vector3 newCrossAxis = constraint.transform.TransformDirection(constraint.crossAxis);
			DrawArrow(p, newCrossAxis*0.75f, c1, "Current");
		}
	}

	private Vector3 ToWorldSpace(Vector3 v){
		if(constraint.transform.parent){
			return constraint.transform.parent.rotation * constraint.defaultLocalRotation * v;
		}else{
			return constraint.defaultLocalRotation * v;
		}
	}
}
