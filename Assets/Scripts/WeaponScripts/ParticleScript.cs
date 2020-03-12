using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleScript : MonoBehaviour
{
    

    private void OnEnable()
    {
        GetComponent<AudioSource>().Play();
        
    }
}
