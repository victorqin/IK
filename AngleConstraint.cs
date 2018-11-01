using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AngleConstraint : MonoBehaviour {
	[HideInInspector]
	[SerializeField]
	private Transform _childJoint = null;
	// public properties are not serialized, only public fields are
	// serialized. Properties are actually methods.
	public Transform ChildJoint{
		get{
			return _childJoint;
		}
		set{
			_childJoint = value;
			if(_childJoint){
				twistAxis = (_childJoint.position-transform.position).normalized;
			}
		}
	}

	[Range(0, 360)]
	public float rotAxisAdjustAngle= 0.0f;

	// all axes coordinates are in local space
	public Vector3 twistAxis = Vector3.forward;
	public Vector3 rotateAxis = Vector3.right;

	void Reset (){
		// Reset() is called when script is attached.

		if(transform.childCount == 1){
			// If there's only one child object, set it as child joint.
			ChildJoint = transform.GetChild(0);
		}
	}

	// Use this for initialization
	void Start () {
		twistAxis = (_childJoint.position - transform.position).normalized;

		// get the twistAxis without rotation
		twistAxis = Quaternion.Inverse(transform.rotation) * twistAxis;

	}
	
	// Update is called once per frame
	void Update () {
		transform.rotation = ApplyConstraint(transform.rotation);

	}

	public Quaternion ApplyConstraint(Quaternion q){
		// The idea is to decompose rotation q into two quaternions.
		// One swing twist axis from old direction to new direction. 
		// The other represent rotation around twistAxis.
		// let q = swingQ * twistQ;
		// therefore, twistQ = reverse(swingQ) * q;
		Quaternion swingQ, twistQ;

		Vector3 newTwistAxis = q * twistAxis;

		swingQ = Quaternion.FromToRotation(twistAxis, newTwistAxis);
		twistQ = Quaternion.Inverse(swingQ) * q;

		Debug.Log("q: " + q);
		Debug.Log("swing: " + swingQ);
		Debug.Log("twistQ: " + twistQ);

		return swingQ * twistQ;

		// apply constraint
		// project 
	}



	void CalculateRotationAxis(){
		// rotation axis and twist axis should alway be perpendicular to
		// each other.
		twistAxis = (_childJoint.position - transform.position).normalized;

		rotateAxis = new Vector3(twistAxis.y, twistAxis.x, 0.0f);



	}


	void OnDrawGizmosSelected(){
		// draw twist axis
		//Gizmos.color = Color.yellow;
		//Gizmos.DrawLine(transform.position, childJoint.position);

		// draw rotation axis
		//Gizmos.color = Color.red;
		//Gizmos.DrawLine(transform.position - rotateAxis, 
			//transform.position + rotateAxis);

		//Vector3 rotateAxisWorld = transform.TransformDirection(rotateAxis);
		//Gizmos.DrawLine(
			//transform.position + rotateAxisWorld,
			//transform.position - rotateAxisWorld); 
		
		//Gizmos.color = Color.red;
		//Vector3 axis;
		//transform.rotation.ToAngleAxis(out angle, out axis);
		//Gizmos.DrawLine(transform.position, transform.position + axis);

		//Gizmos.color = Color.blue;
		//Vector3 twistAxisWorld = transform.TransformDirection(twistAxis);
		//Gizmos.DrawLine(transform.position, transform.position + twistAxisWorld);

		//Quaternion baseQ = Quaternion.FromToRotation(Vector3.forward, rotateAxis);
		//transform.localRotation = baseQ * Quaternion.AngleAxis(rotateAngle, twistAxis);

	}
}
