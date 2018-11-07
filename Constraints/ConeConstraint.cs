using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConeConstraint : RotationConstraint{
	public bool useConeLimit = true;

	[SerializeField]
	private float _coneAngle = 30.0f;
	public float coneAngle{
		get{
			return _coneAngle;
		}
		set{
			if(value<0.0f || value>180f){
				Debug.LogError(string.Format(
					"coneAngle should be within [0, 180], but {0} is given.",
					value));
			}
			_coneAngle = value;
		}
	}

	private Quaternion _swingRotation = Quaternion.identity;
	public Quaternion swingRotation{
		get{
			return _swingRotation;
		}
	}
	private Quaternion _twistRotation = Quaternion.identity;
	public Quaternion twistRotation{
		get{
			return _twistRotation;
		}
	}

	public override Quaternion ApplyConstraint(Quaternion localRotation){
		Quaternion deltaLocalRotation =
			Quaternion.Inverse(defaultLocalRotation) * localRotation;
		Quaternion limitedDeltaLocalRotation = ApplyConeConstraint(deltaLocalRotation);
		return defaultLocalRotation * limitedDeltaLocalRotation;
	}

	private Quaternion ApplyConeConstraint(Quaternion localRotation){
		DecomposeRotation(localRotation, rotationAxis,
			out _swingRotation, out _twistRotation);

		if(useAngleLimits){
			// limit twist rotation
			Vector3 crossAxisNew = _twistRotation * crossAxis;
			float angle = Vector3.SignedAngle(crossAxis, crossAxisNew, rotationAxis);
			angle = Mathf.Clamp(angle, minAngle, maxAngle);
			_twistRotation = Quaternion.AngleAxis(angle, rotationAxis);
		}

		if(useConeLimit){
			// limit swing rotation
			Vector3 rotationAxisNew = _swingRotation * rotationAxis;
			float angle = Vector3.Angle(rotationAxis, rotationAxisNew);
			if(angle > coneAngle){
				_swingRotation = Quaternion.AngleAxis(
					coneAngle, Vector3.Cross(rotationAxis, rotationAxisNew));
			}
		}
		return _swingRotation * _twistRotation;
	}

	void LateUpdate(){
		if(autoUpdate){
			transform.localRotation = ApplyConstraint(transform.localRotation);
		}else{
			DecomposeRotation(transform.localRotation, rotationAxis,
				out _swingRotation, out _twistRotation);
		}
	}
}
