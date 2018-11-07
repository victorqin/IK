using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RotationConstraint : MonoBehaviour {
	public bool autoUpdate = true;

	[SerializeField]		//for recording undo step
	private Vector3 _rotationAxis = Vector3.forward;

	// Here we make _crossAxis serialized. If not, value change due to
	// change of rotationAxis through inspector editor won't be recorded.
	// Leaving _crossAxis to it's initial value.
	[SerializeField]
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

	// range of minAngle is [-180, 180]
	public float minAngle{
		get{
			return _minAngle;
		}
		set{
			if(value > _maxAngle){
				Debug.LogError("minAngle should NOT be greater than maxAngle.");
				return;
			}
			value = NormalizeAngle(value);
			_minAngle = value;
		}
	}

	// range of maxAngle is [-180, 180]
	public float maxAngle{
		get{
			return _maxAngle;
		}
		set{
			if(value < _minAngle){
				Debug.LogError("maxAngle should NOT be smaller than minAngle.");
				return;
			}
			value = NormalizeAngle(value);
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

	public abstract Quaternion ApplyConstraint (Quaternion localRotation);

	public Quaternion ApplyConstraint(Quaternion localRotation, out bool isChanged){
		Quaternion q = ApplyConstraint(localRotation);
		if (q == localRotation){
			isChanged = false;
		}else{
			isChanged = true;
		}
		return q;
	}

	void Awake() {
		SetDefaultLocalRotation();
	}

	void LateUpdate() {
		if(autoUpdate){
			transform.localRotation = ApplyConstraint(transform.localRotation);
		}
	}

	// normalize angle to [-180, 180]
	protected float NormalizeAngle(float angle){
		while(angle< -180.0f){
			angle += 360.0f;
		}
		while(angle> 180.0f){
			angle -= 360.0f;
		}
		return angle;
	}

	// Decompose rotation into swing and twist rotation.
	// Note twist is applied first, then swing.
	// rotation = swing * twist
	// so twist = inverse(swing) * rotation
	protected void DecomposeRotation(Quaternion rotation, Vector3 axis,
		out Quaternion swingRotation, out Quaternion twistRotation){
		swingRotation = Quaternion.FromToRotation(axis, rotation * axis);
		twistRotation = Quaternion.Inverse(swingRotation) * rotation;
	}

	// Set the default rotation of the object.
	// All rotation constraint is based on the default rotation.
	private void SetDefaultLocalRotation(){
		_defaultLocalRotation = transform.localRotation;
		_defaultLocalRotationSet = true;
	}

}
