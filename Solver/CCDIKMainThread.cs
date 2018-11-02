using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCDIKMainThread : MonoBehaviour {
	// Cyclic Coordinate Descent (CCD) IK
	// Implement IK using main thread.
	// reference: https://www.youtube.com/watch?v=MA1nT9RAF3k

	public Transform goal;
	public Transform effector;
	public Transform rootBone;

	public float sqrDistError = 0.0001f;

	[Range(0, 1)]
	public float weight = 1.0f;
	public int maxIterationCount = 10;

	List<Transform> _bones;

	// Use this for initialization
	void Start () {
		_getBoneHierachy();

	}

	private void _getBoneHierachy(){
		// Create a list of bones starting at the effector.
		_bones = new List<Transform>();
		Transform current = effector;

		// Add bones up the hierarchy until the base bone is reached.
		while(current != null){
			_bones.Add(current);

			if(current == rootBone)
				return;

			current = current.parent;
		}
		throw new UnityException(
			"Root bone is not an ancestor of Effector. IK will fail.");
	}

	// Update is called once per frame
	void LateUpdate () {
		Solve();
	}

	void Solve(){
		Vector3 goalPos = goal.position;
		Vector3 effectorPos = _bones[0].position;
		Vector3 targetPos = Vector3.Lerp(effectorPos, goalPos, weight);

		float sqrDistance;
		int iterationCount = 0;

		// The method provided here is an improved version of the original
		// algorithm. The closer to effector, more times the bone gets rotated.
		// _bone[0] -> effector
		// What it's here is:
		// rotate _bone[1], rotate _bone[2]
		// rotate _bone[1], rotate _bone[2], rotate _bone[3]
		// rotate _bone[1], rotate _bone[2], rotate _bone[3], rotate _bone[4]
		// ......
		do{
			for(int i=0; i<_bones.Count-2; i++){
				for(int j=1; j<i+3 && j<_bones.Count; j++){
					RotateBone(_bones[0], _bones[j], targetPos);
					sqrDistance = (_bones[0].position - targetPos).sqrMagnitude;

					if(sqrDistance<= sqrDistError)
						return;
				}
			}
			iterationCount ++;
		}while(iterationCount <= maxIterationCount);
	}

	public static void RotateBone(Transform effector, Transform bone, Vector3 goalPos){
		Vector3 effectorPos = effector.position;
		Vector3 bonePos = bone.position;

		Vector3 boneToEffector = effectorPos - bonePos;
		Vector3 boneToGoal = goalPos - bonePos;

		Quaternion fromToRotation = Quaternion.FromToRotation(boneToEffector, boneToGoal);

		// apply the new rotation first, then apply the old rotation.
		// This is because the new rotation is created based on no rotation
		// applied to the object.
		Quaternion newRotation = fromToRotation * bone.rotation;

		// check whether any constraint attached to the bone
		AngleConstraint constraint = bone.GetComponent<AngleConstraint>();
		if(constraint){
			constraint.ApplyConstraint(newRotation);
		}else{
			bone.rotation = newRotation;
		}
	}
}
