using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AngleConstraint : MonoBehaviour {
	[HideInInspector]
	[SerializeField]
	private Transform _childJoint = null;
	// Public properties are not serialized, only public fields are
	// serialized. Properties are actually methods.
	// This is for making undo work properly.
	public Transform ChildJoint{
		get{
			return _childJoint;
		}
		set{
			_childJoint = value;
			if(_childJoint){
				twistAxis = (_childJoint.position-transform.position).normalized;
				twistAxis = Quaternion.Inverse(transform.rotation) * twistAxis;
			}
		}
	}

	// all axes coordinates are in local space
	public Vector3 twistAxis = Vector3.forward;
	public Vector3 rotateAxis = Vector3.right;

	private Quaternion _prevRotation;

	void Reset (){
		// Reset() is called when script is attached.

		if(transform.childCount == 1){
			// If there's only one child object, set it as child joint.
			ChildJoint = transform.GetChild(0);
		}
	}

	// Use this for initialization
	void Start () {
		_prevRotation = transform.localRotation;
	}
	
	// Update is called once per frame
	//void Update () {
	void LateUpdate () {
		ApplyConstraint();
		_prevRotation = transform.localRotation;
	}

	public void ApplyConstraint(){
		// Let qNew = qOld * deltaQ, so deltaQ = inverse(qOld) * qNew
		// The reason why we are not using qNew = deltaQ * qOld, is because
		// in the first equation, deltaQ gets applied first, so everyting is
		// in local space without any rotation. It's easier for us to
		// construct the replacement of deltaQ.
		Quaternion deltaQ = Quaternion.Inverse(_prevRotation) * transform.localRotation;
		Vector3 newTwistAxis = deltaQ * twistAxis;

		//Quaternion swingQ, twistQ;
		//DecomposeRotation(deltaQ, twistAxis, out swingQ, out twistQ);

		// transform.localRotation = swingQ * twistQ * _prevRotation;

		// apply constraint
		// project twist axis before and after rotation on the plane
		// perpendicular to rotation axis.
		Vector3 projTwistAxis = ProjectDirection(twistAxis, rotateAxis);
		Vector3 projNewTwistAxis = ProjectDirection(newTwistAxis, rotateAxis);
		Debug.Log(Vector3.Dot(projNewTwistAxis, rotateAxis));
		Debug.Log(Vector3.Dot(projTwistAxis, rotateAxis));

		//Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(projTwistAxis));
		//Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(projNewTwistAxis), Color.green);

		Quaternion newSwingQ = Quaternion.FromToRotation(projTwistAxis, projNewTwistAxis);

		transform.localRotation = _prevRotation * newSwingQ;
	}

	private void DecomposeRotation(Quaternion q, Vector3 axis,
		out Quaternion swing, out Quaternion twist){
		// Decompose rotation q into two quaternions.
		// One swing axis from old direction to new direction. 
		// The other represent twist rotation around axis.
		// let q = swingQ * twistQ;
		// therefore, twistQ = inverse(swingQ) * q;
		Vector3 newAxis = q * axis;
		swing = Quaternion.FromToRotation(axis, newAxis);
		twist = Quaternion.Inverse(swing) * q;
	}

	private Vector3 ProjectDirection(Vector3 dir, Vector3 axis ){
		// Project dir to the plane perpendicular to the axis.
		dir.Normalize();
		axis.Normalize();

		// componet parallel to axis.
		Vector3 d1 = Vector3.Dot(dir, axis) * axis;

		// componet perpendicular to axis.
		return  (dir - d1).normalized;
	}

	void CalculateRotationAxis(){
		// rotation axis and twist axis should alway be perpendicular to
		// each other.
		twistAxis = (_childJoint.position - transform.position).normalized;

		rotateAxis = new Vector3(twistAxis.y, twistAxis.x, 0.0f);
	}
}
