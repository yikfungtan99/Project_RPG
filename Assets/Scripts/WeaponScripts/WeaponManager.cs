using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject blood;
    public GameObject sparks;
    [HideInInspector] public float damage;



    protected virtual void OnTriggerEnter(Collider other)
    {
        HurtPeople(other.gameObject);   
    }

    


    public void HurtPeople(GameObject collider)
    {
        if (collider.layer != 8)
        {
            StatesManager states = collider.GetComponent<StatesManager>();
            if (states != null)
            {
                if (!states.isDodge)
                {
                    states.Hurt(damage);
                    Instantiate(blood, transform.position, Quaternion.LookRotation(-transform.forward));
                }
            }
            else
            {
                Instantiate(sparks, transform.position, Quaternion.LookRotation(-transform.forward));
            }

            ShieldScript shieldScript = collider.GetComponent<ShieldScript>();
            if (shieldScript != null)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
