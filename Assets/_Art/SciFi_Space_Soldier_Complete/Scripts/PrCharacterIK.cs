using UnityEngine;
using System;
using System.Collections;
using Language.Lua;

[RequireComponent(typeof(Animator))]

public class PrCharacterIK : MonoBehaviour
{
    private Animator _animator;

    public bool ikHandsActive = true;
    public bool ikHeadActive = true;
    public float headWeight = 1f;
    public Transform leftHandTarget = null;
    public Transform lookObj = null;
    private GameObject _headPivot;
    
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _headPivot = new GameObject("HeadPivot")
        {
            transform =
            {
                parent = transform,
                localPosition = new Vector3(0,1.85f,0)
            }
        };
    }

    private void Update()
    {
        if (!lookObj) return;
        _headPivot.transform.LookAt(lookObj);
        var pivotRotY = _headPivot.transform.localRotation.y;
        var dist = Vector3.Distance(_headPivot.transform.position, lookObj.position);
        if (pivotRotY is < 0.65f and > -0.65f && dist < 3.5f)
            headWeight = Mathf.Lerp(headWeight, 1, Time.deltaTime * 2.5f);
        else
            headWeight = Mathf.Lerp(headWeight, 0, Time.deltaTime * 2.5f);
        if (!(dist > 4.0f)) return;
        lookObj = null;
        ikHeadActive = false;
    }
    
    public void OnAnimatorIK(int layerIndex)
    {
        if (!_animator) return;
        //if the IK is active, set the position and rotation directly to the goal. 
        if (ikHandsActive)
        {
            // Set the right hand target position and rotation, if one has been assigned
            if (leftHandTarget == null) return;
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
        //if the IK is not active, set the position and rotation of the hand and head back to the original position
        else
        {
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
        }
        
        if (ikHeadActive)
        {
            // Set the look target position, if one has been assigned
            if (lookObj == null) return;
            _animator.SetLookAtWeight(headWeight);
            _animator.SetLookAtPosition(lookObj.position);
        }
        else
        {
            _animator.SetLookAtWeight(0);
        }
    }
}
