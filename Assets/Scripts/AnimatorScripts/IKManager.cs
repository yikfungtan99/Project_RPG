using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKManager : MonoBehaviour
{
    //public Vector3 lookPosition;
    Animator animator;
    StatesManager states;
    public Transform lookPosTrans;

    private void Start()
    {
        animator = GetComponent<Animator>();
        states = GetComponentInParent<StatesManager>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (lookPosTrans == null)
        {
            if (states.aim || (states.isStationaryAction && !states.isDodge))
            {
                Vector3 hor = states.lookPosition - transform.position;
                hor.y = 0;
                hor = hor.normalized;
                states.transform.rotation = Quaternion.Slerp(states.transform.rotation, Quaternion.LookRotation(hor), Time.deltaTime * 8.5f);
                animator.SetLookAtWeight(1, 0.85f, 0.15f);
                animator.SetLookAtPosition(states.lookPosition);
            }

            if (states.lockon && states.enemyTarget != null)
            {
                animator.SetLookAtWeight(1, 0.85f, 0.15f);
                animator.SetLookAtPosition(states.lookPosition + Vector3.up * 1.2f);
            }
        }
        else
        {
            if (states.aim)
            {
                animator.SetLookAtWeight(1, 0.85f, 0.15f);
                animator.SetLookAtPosition(lookPosTrans.position);
            }

            if (states.lockon && states.enemyTarget != null)
            {
                animator.SetLookAtWeight(1, 0.85f, 0.15f);
                animator.SetLookAtPosition(lookPosTrans.position);
            }
        }
        

       
    }



}
