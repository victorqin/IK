﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CanEditMultipleObjects]
[CustomEditor(typeof(AngleConstraint))]
public class AngleConstraintEditor : Editor{
	// The default access for everything in C# is "the most restricted access
	// you could declare for that member". So private keyword can be omitted.
	SerializedProperty _twistAxisP;
	SerializedProperty _rotateAxisP;

	bool _editingRotateAxis = false;
	bool _editingTwistAxis = false;
	bool _showAxis = true;

	// This is for tracking the rotation of axis
	// when editing it using Unity's rotate handle.
	Quaternion _twistAxisQ;
	Quaternion _rotateAxisQ;

	// record the axes corresponding _twistAxisQ and _rotateAxisQ
	Vector3 _savedTwistAxis;
	Vector3 _savedRotateAxis;

	void OnEnable(){
		AngleConstraint constraint = target as AngleConstraint;

		// Set up serialized properties.
		// Accessing properties through serialized properties rather through
		// target is because undo, multi-editing is handled automatically.
		_rotateAxisP = serializedObject.FindProperty("rotateAxis");
		_twistAxisP = serializedObject.FindProperty("twistAxis");

		_rotateAxisQ = constraint.transform.rotation;
		_twistAxisQ = constraint.transform.rotation;
		_savedRotateAxis = constraint.rotateAxis;
		_savedTwistAxis = constraint.twistAxis;
	}

	void ShowAxis(AngleConstraint constraint){
		// draw rotation axis
		Vector3 axis = constraint.transform.TransformDirection(
			constraint.rotateAxis);
		Handles.color = Color.red;
		Handles.DrawLine(constraint.transform.position-axis,
			constraint.transform.position+axis);

		// draw twist axis
		axis = constraint.transform.TransformDirection(
			constraint.twistAxis);
		Handles.color = Color.yellow;
		Handles.DrawLine(constraint.transform.position,
			constraint.transform.position+axis);
	}

	void EditTwistAxis(AngleConstraint constraint){
		EditorGUI.BeginChangeCheck();
		//_twistAxisQ = Handles.RotationHandle(_twistAxisQ, constraint.transform.position);

		// Let q2 = Handles.RotationHandle(q1);
		// q2 is not the delta rotation between q1 and q2. q2 is the modified
		// rotation of q1.
		_twistAxisQ = Handles.RotationHandle(_twistAxisQ, constraint.transform.position);

		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Rotate Twist Axis");
			constraint.twistAxis = Quaternion.Inverse(constraint.transform.rotation) * _twistAxisQ * _savedTwistAxis;
		}
	}

	void EditRotateAxis(AngleConstraint constraint){
		EditorGUI.BeginChangeCheck();
		_rotateAxisQ = Handles.RotationHandle(_rotateAxisQ, constraint.transform.position);

		if (EditorGUI.EndChangeCheck()){
			Undo.RecordObject(constraint, "Rotate Rotation Axis");
			constraint.rotateAxis= Quaternion.Inverse(constraint.transform.rotation) * _rotateAxisQ * _savedRotateAxis;
		}
	}

	public void OnSceneGUI(){
		AngleConstraint constraint = target as AngleConstraint;

		if(_showAxis){
			ShowAxis(constraint);
		}

		// enable twist axis editing
		if(_editingTwistAxis){
			EditTwistAxis(constraint);
		}

		// enable rotation axis editing
		if(_editingRotateAxis){
			EditRotateAxis(constraint);
		}
	}

	public override void OnInspectorGUI(){
		AngleConstraint constraint = target as AngleConstraint;

		// toggle to show axis
		_showAxis = EditorGUILayout.Toggle("Show Axis", _showAxis);

		// Since ChildJoint is a property (actually a method) of AngleConstraint,
		// it cannot be serialized. So we have to access it through target.
		// The bad thing is we have to do all the undo dirty stuff on our own.
		EditorGUI.BeginChangeCheck();
		Transform t = EditorGUILayout.ObjectField("Child Joint",
			constraint.ChildJoint, typeof(Transform), true) as Transform;
		if (EditorGUI.EndChangeCheck()){
			// since ChildJoint property cannot be serialized, 
			// make sure its counterpart private field is serialized.
			// Otherwise Undo.RecordObject won't record anything.
			Undo.RecordObject(constraint, "Change Child Joint");
			constraint.ChildJoint = t;
		}

		// Use serialized properties to access other public fields of
		// AngleConstraint. 

		// Update the serializedProperty - always do this in the beginning
		// of OnInspectorGUI.
        serializedObject.Update ();

		// twist axis
		// If child joint is specified, then only display twist axis (read only).
		// Otherwise, let user edit twist axis.
		EditorGUILayout.BeginHorizontal();
		if(constraint.ChildJoint){
			_editingTwistAxis = false;
			GUI.enabled = false;
			EditorGUILayout.PropertyField(_twistAxisP, new GUIContent("Twist Axis"));
			_editingTwistAxis = GUILayout.Toggle(_editingTwistAxis, "Edit", "Button",
			GUILayout.Width(50));
			GUI.enabled = true;
		}else{
			EditorGUILayout.PropertyField(_twistAxisP, new GUIContent("Twist Axis"));
			_editingTwistAxis = GUILayout.Toggle(_editingTwistAxis, "Edit", "Button",
				GUILayout.Width(50));
		}
		EditorGUILayout.EndHorizontal();
		if(_editingTwistAxis){
			_editingRotateAxis = false;
		}

		// rotate axis
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PropertyField(_rotateAxisP, new GUIContent("Rotate Axis"));
		_editingRotateAxis = GUILayout.Toggle(_editingRotateAxis, "Edit", "Button",
			GUILayout.Width(50));
		EditorGUILayout.EndHorizontal();
		if(_editingRotateAxis){
			_editingTwistAxis = false;
		}

		// Apply changes to the serializedProperty - always do this in the end
		// of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties ();
	}
}
