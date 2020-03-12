using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowSript : WeaponManager
{
    bool isAlreadyAttacked = false;

    private void OnEnable()
    {
        isAlreadyAttacked = false;
        Destroy(gameObject, 1.45f);
    }

    /*protected override void OnTriggerEnter(Collider other)
    {
        Physics.GetIgnoreLayerCollision(8, 8);

        if (other.gameObject.layer != 8)
        {
            if (!isAlreadyAttacked)
            {
                isAlreadyAttacked = true;
                HurtPeople(other.gameObject);
            }
            else
            {
                Instantiate(sparks, transform.position, Quaternion.LookRotation(-transform.forward));
            }
        }
    }*/


    protected void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.layer != 8)
        {
            if (!isAlreadyAttacked)
            {
                isAlreadyAttacked = true;
                HurtPeople(collision.gameObject);
            }
            else
            {
                Instantiate(sparks, transform.position, Quaternion.LookRotation(-transform.forward));
            }
        }
    }
}
