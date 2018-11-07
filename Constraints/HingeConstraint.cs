using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HingeConstraint : RotationConstraint{
	// Apply constraint to rotation.
	// Input is the new local rotation.
	public override Quaternion ApplyConstraint(Quaternion localRotation){
		Quaternion deltaLocalRotation =
			Quaternion.Inverse(defaultLocalRotation) * localRotation;
		Quaternion limitedDeltaLocalRotation = ApplyHingeConstraint(deltaLocalRotation);
		return defaultLocalRotation * limitedDeltaLocalRotation;
	}

	// Make sure rotation is in local space.
	// This is because _rotationAxis is in local space.
	private Quaternion ApplyHingeConstraint(Quaternion localRotation){
		Quaternion newRotation = ConstraintRotation1DOF(localRotation, rotationAxis);

		if(useAngleLimits){
			Vector3 crossAxisNew = newRotation * crossAxis;

			// Vector3.SignedAngle makes sure the angle returns falls in range
			// of [-180, 180].
			float angle = Vector3.SignedAngle(crossAxis, crossAxisNew, rotationAxis);
			angle = Mathf.Clamp(angle, minAngle, maxAngle);
			return Quaternion.AngleAxis(angle, rotationAxis);
		}else{
			return newRotation;
		}
	}

	// Constraint rotation to 1 DOF around given axis.
	private Quaternion ConstraintRotation1DOF(Quaternion rotation, Vector3 axis){
		Quaternion swingRotation, twistRotation;
		DecomposeRotation(rotation, axis, out swingRotation, out twistRotation);
		return twistRotation;
	}
}
