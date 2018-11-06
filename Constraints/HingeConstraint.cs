using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HingeConstraint : MonoBehaviour {
	public bool autoUpdate = true;

	// rotation axis
	private Vector3 _rotationAxis = Vector3.forward;
	private Vector3 _crossAxis = Vector3.up;	// where 0 degree starts
	public Vector3 rotationAxis{
		get{
			return _rotationAxis;
		}
		set{
			_rotationAxis  = value;

			// Get an arbitary vector, but make sure it's not equal to _rotationAxis.
			Vector3 v = new Vector3(_rotationAxis.z, _rotationAxis.x, _rotationAxis.y);
			if (v == _rotationAxis){
				v = Vector3.up;
			}
				_crossAxis = Vector3.Cross(v, _rotationAxis).normalized;
		}
	}
	public Vector3 crossAxis{
		get{
			return _crossAxis;
		}
	}

	// use min and max rotation angle limit around rotation axis
	public bool useAngleLimits = true;

	// range of _minAngle, _maxAngle is [-180, 180]
	[SerializeField]
	private float _minAngle = -90.0f;
	[SerializeField]
	private float _maxAngle = 90.0f;
	public float minAngle{
		get{
			return _minAngle;
		}
		set{
			value = NormalizeAngle(value);
			if(value > _maxAngle){
				Debug.LogError("minAngle should NOT be greater than maxAngle.");
				return;
			}
			_minAngle = value;
		}
	}
	public float maxAngle{
		get{
			return _maxAngle;
		}
		set{
			value = NormalizeAngle(value);
			if(value < _maxAngle){
				Debug.LogError("maxAngle should NOT be smaller than minAngle.");
				return;
			}
			_maxAngle = NormalizeAngle(value);
		}
	}

	// default local rotation
	private Quaternion _defaultLocalRotation;
	private bool _defaultLocalRotationSet = false;
	public Quaternion defaultLocalRotation{
		get{
			if(!_defaultLocalRotationSet){
				SetDefaultLocalRotation();
			}
			return _defaultLocalRotation;
		}
		set{
			_defaultLocalRotation = value;
			_defaultLocalRotationSet = true;
		}
	}

	// Set the default rotation of the object.
	// All rotation constraint is based on the default rotation.
	private void SetDefaultLocalRotation(){
		_defaultLocalRotation = transform.localRotation;
		_defaultLocalRotationSet = true;
	}

	void Awake() {
		SetDefaultLocalRotation();
	}

	// Update is called once per frame
	void LateUpdate() {
		if(autoUpdate){
			transform.localRotation = ApplyConstraint(transform.localRotation);
		}
	}

	// Apply constraint to rotation.
	// Input is the new local rotation.
	// Return true if rotation is modified due to the constraint.
	// Make sure rotation is in local space.
	public Quaternion ApplyConstraint(Quaternion localRotation){
		Quaternion deltaLocalRotation =
			Quaternion.Inverse(defaultLocalRotation) * localRotation;
		Quaternion limitedDeltaLocalRotation = LimitHinge(deltaLocalRotation);
		return defaultLocalRotation * limitedDeltaLocalRotation;
	}

	public Quaternion ApplyConstraint(Quaternion localRotation, out bool isChanged){
		Quaternion q = ApplyConstraint(localRotation);
		if (q == localRotation){
			isChanged = false;
		}else{
			isChanged = true;
		}
		return q;
	}

	// Make sure rotation is in local space.
	// This is because _rotationAxis is in local space.
	private Quaternion LimitHinge(Quaternion localRotation){
		Quaternion newRotation = ConstraintRotation1DOF(localRotation, _rotationAxis);

		if(useAngleLimits){
			Vector3 crossAxisNew = newRotation * _crossAxis;

			// Vector3.SignedAngle makes sure the angle returns falls in range
			// of [-180, 180].
			float angle = Vector3.SignedAngle(_crossAxis, crossAxisNew, _rotationAxis);
			angle = Mathf.Clamp(angle, _minAngle, _maxAngle);
			return Quaternion.AngleAxis(angle, _rotationAxis);
		}else{
			return newRotation;
		}
	}

	// normalize angle to [-180, 180]
	private float NormalizeAngle(float angle){
		while(angle<= -180.0f){
			angle += 360.0f;
		}
		while(angle>= 180.0f){
			angle -= 360.0f;
		}
		return angle;
	}

	// Constraint rotation to 1 DOF around given axis.
	private Quaternion ConstraintRotation1DOF(Quaternion rotation, Vector3 axis){
		// Let rotation = swing * twist. twist is the rotation we want to get.
		// twist = inverse(swing) * rotation
		// FromToRotation(rotation*axis, axis) is actually inverse(swing).
		return Quaternion.FromToRotation(rotation * axis, axis) * rotation;
	}
}
