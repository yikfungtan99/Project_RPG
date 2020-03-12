using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : InputParent
{

    float delta;
    float fixedDelta;

    LayerMask playerMask;
    public InputManager hostileTarget;
    public float stopDistance = 1.55f;



    float attackTimer = 0;
    float randomInterval = 1;
    int randomAttack = 1;


    private void Start()
    {        
        states = GetComponent<StatesManager>();
        states.Init();
        playerMask = ~((1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 8) | (1 << 9) | (1 << 11));

        //states.aim = true;
    }

    private void Update()
    {
        delta = Time.deltaTime;


        
        GetInput();
        states.Tick(delta);
    }

    private void FixedUpdate()
    {
        fixedDelta = Time.fixedDeltaTime;
        states.FixedTick(fixedDelta);
        UpdateLockonPosition();
    }

    

    private void GetInput()
    {
        if (hostileTarget == null)
        {
            states.moveAmount = 0;
            ResetInputs();
            return;
        }

        if (!hostileTarget.enabled)
        {
            states.moveAmount = 0;
            ResetInputs();
            return;
        }
        
        states.lookPosition = hostileTarget.transform.position + Vector3.up * 0.85f;

        Vector3 heading = hostileTarget.transform.position - transform.position;
        states.moveDirection = heading.normalized;
        states.aimRotation = Quaternion.LookRotation(heading.normalized);
        states.moveAmount = heading.magnitude > stopDistance ? 1 : 0;

        if (heading.magnitude < stopDistance + 1)
        {
            attackTimer += delta;
            if (attackTimer > randomInterval)
            {
                attackTimer = 0;
                randomInterval = UnityEngine.Random.Range(0.7f, 3.4f);


                randomAttack = UnityEngine.Random.Range(1, 7);

                switch (randomAttack)
                {
                    case 1:
                        states.fire3 = true;
                        break;
                    case 2:
                        states.fire2 = true;
                        break;
                    case 3:
                        states.dodge = true;
                        break;
                    default:
                        states.fire1 = true;
                        break;
                }
            }
            if (attackTimer > 0.01f)
            {
                ResetInputs();
            }
        }
        
    }

    void ResetInputs()
    {
        states.fire3 = false;
        states.fire2 = false;
        states.dodge = false;
        states.fire1 = false;
    }

}
