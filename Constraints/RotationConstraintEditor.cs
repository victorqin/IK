using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class RotationConstraintEditor : Editor{
	protected void DrawArrow(Vector3 position, Vector3 direction, Color color,
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
}