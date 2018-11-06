using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(HingeConstraint))]
public class HingeConstraintEditor : Editor{

	private HingeConstraint _constraint = null;
	public HingeConstraint constraint{
		get{
			if(_constraint == null)
				_constraint = target as HingeConstraint;
			return _constraint;
		}
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

	}

	private void DrawAxes(){
		Vector3 p = constraint.transform.position;
		Vector3 axis = constraint.transform.TransformDirection(constraint.rotationAxis).normalized;

		Vector3 crossAxis = ToWorldSpace(constraint.crossAxis);

		Vector3 minDir = ToWorldSpace(Quaternion.AngleAxis(constraint.minAngle, constraint.rotationAxis) * constraint.crossAxis);
		Vector3 maxDir = ToWorldSpace(Quaternion.AngleAxis(constraint.maxAngle, constraint.rotationAxis) * constraint.crossAxis);

		Color c1 = new Color(0.667f, 1.0f, 0.0f, 1.0f);
		Color c2 = new Color(0.667f, 1.0f, 0.0f, 0.1f);

		Handles.color = c1;
		Handles.DrawWireDisc(p, axis, 0.5f);

		DrawArrow(p, axis*0.75f, c1, "Axis");
		DrawArrow(p, crossAxis*0.5f, c1, "0");
		DrawArrow(p, minDir*0.75f, c1, "Min");
		DrawArrow(p, maxDir*0.75f, c1, "Max");

		if(Application.isPlaying){
			Vector3 newCrossAxis = constraint.transform.TransformDirection(constraint.crossAxis);
			DrawArrow(p, newCrossAxis*0.75f, c1, "Current");
		}

		Handles.color = c2;
		Handles.DrawSolidArc(
			p, axis, minDir, constraint.maxAngle - constraint.minAngle, 0.5f);
	}

	private void DrawArrow(Vector3 position, Vector3 direction, Color color,
		string label = "", float size = 0.02f){
		Handles.color = color;
		Handles.DrawLine(position, position + direction);
		Handles.SphereHandleCap(0, position + direction, Quaternion.identity, size, EventType.Repaint);

		if(label != ""){
			GUIStyle style = new GUIStyle();
			style.normal.textColor = color;
			Handles.Label(position+direction, label, style);
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
