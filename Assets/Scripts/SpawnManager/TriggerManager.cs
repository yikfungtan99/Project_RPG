using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerManager : MonoBehaviour
{
    [SerializeField] private int whichEnemyGroup = 0;
    [SerializeField] private int howMany = 5;

    private void OnTriggerEnter(Collider other)
    {
        
        StartCoroutine(SpawnMultiple());        
        
        GetComponent<BoxCollider>().enabled = false;
    }

    IEnumerator SpawnMultiple()
    {
        for (int i = 0; i < howMany; i++)
        {
            yield return new WaitForSeconds(2.7f);

            SpawnManager.Instance.Trigger(whichEnemyGroup);
        }
    }
}
