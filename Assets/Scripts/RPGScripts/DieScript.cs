using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieScript : MonoBehaviour
{
    
    private void OnEnable()
    {
        AwardManager.Instance.GiveXP(33);
        GetComponent<Animator>().SetBool("dead", true);

        Destroy(transform.parent.gameObject, 15);
    }
}
