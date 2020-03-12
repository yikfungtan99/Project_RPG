using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : InputParent
{
    float delta;
    float fixedDelta;
    CameraManager cameraManager;
    Camera mainCam;

    float fireTimer = 0;
    float fire2StopTimer = 0;
    float fire3StopTimer = 0;
    float sprintTimer = 0;

    LayerMask enemyLayerMask;

    
    bool canDodgeAttack = false;
    float canDodgeAtttackTimer = 0;


    private void Start()
    {
        mainCam = Camera.main;
        states = GetComponent<StatesManager>();
        states.Init();
        cameraManager = Camera.main.transform.parent.parent.GetComponent<CameraManager>();
        cameraManager.Init(transform);

        enemyLayerMask = ~((1 << 0) | (1 << 1) | (1 << 2) | (1 << 4) | (1 << 5) | (1 << 8) | (1 << 9) | (1 << 10));
        states.ignoreLayers = ~(1 << 8) | (1 << 9) | (1 << 10) | (1 << 11);
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
        cameraManager.Tick(fixedDelta);
    }


    void GetInput()
    {
        InputLookPosition();
        states.aim = Input.GetButton("Aim");
        if (states.aim)
        {
            states.aimRotation = cameraManager.transform.rotation;
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 38, delta * 130);
        }
        else
        {
            mainCam.fieldOfView = Mathf.MoveTowards(mainCam.fieldOfView, 70, delta * 130);
        }

        states.vertical = Input.GetAxis("Vertical");
        states.horizontal = Input.GetAxis("Horizontal");
        UpdateMoveAmountAndDirection();

        states.drink = Input.GetButtonDown("Drink");
        SettleAttacks();      
       
        HancleHoldTapSameButton("Sprint", ref states.sprint, ref states.dodge, ref sprintTimer);
        HandleLockonInput();        
    }



    void SettleAttacks()
    {

        if (!states.aim)
        {
            if (fire3StopTimer < 0.1f)
            {
                HandleDodgeAttack();
            }
            if (!states.fire3 && fire2StopTimer < 0.1f)
            {
                HancleHoldTapSameButton("Fire1", ref states.fire2, ref states.fire1, ref fireTimer);
            }
        }
        else
        {
            states.aimFire = Input.GetButtonDown("Fire1");
        }




        if (states.fire2)
        {
            fire2StopTimer += delta;
            if (fire2StopTimer >= 0.1f)
            {
                states.fire2 = false;
                fire2StopTimer = 0;
            }
        }
        if (states.fire3)
        {
            fire3StopTimer += delta;
            if (fire3StopTimer >= 0.1f)
            {
                states.fire3 = false;
                fire3StopTimer = 0;
            }
        }
    }




    void UpdateMoveAmountAndDirection()
    {
        states.moveAmount = Mathf.Clamp01
            (
                Mathf.Abs(states.horizontal) + Mathf.Abs(states.vertical)
            );

        Vector3 v = states.vertical * cameraManager.transform.forward;
        Vector3 h = states.horizontal * cameraManager.transform.right;
        states.moveDirection = (v + h).normalized;
    }

    void HancleHoldTapSameButton(string button, ref bool hold, ref bool tap, ref float timer)
    {
        if (timer < 0.25f)
        {
            tap = Input.GetButtonUp(button);
        }
        else
        {
            hold = true;
        }


        if (Input.GetButton(button))
        {
            timer += delta;
        }
        else
        {
            timer = 0;
        }


        if (Input.GetButtonUp(button))
        {
            hold = false;
        }
    }

    void HandleLockonInput()
    {
        if (Input.GetButtonDown("Lockon"))
        {
            states.lockon = !states.lockon;
            if (states.lockon)
            {
                GetEnemyTarget();
            }
        }
        if (states.sprint)
        {
            states.lockon = false;
        }
        if (!states.lockon)
        {
            cameraManager.enemyTarget = null;
        }
        if(states.enemyTarget != null)
        {
            cameraManager.enemyTarget = states.enemyTarget;
        }
    }

    void GetEnemyTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 200, enemyLayerMask);

        for (int i = 0; i < colliders.Length; i++)
        {
            EnemyInput enemy = colliders[i].GetComponent<EnemyInput>();

            if (enemy != null)
            {
                if (states.enemyTarget == null)
                {
                    states.enemyTarget = enemy;
                    states.engagedBy = states;
                }
                else
                {
                    Vector3 to = colliders[i].transform.position - transform.position;
                    float newAngle = Vector3.SignedAngle(cameraManager.transform.forward, to, cameraManager.transform.up);

                    to = states.enemyTarget.lockonPosition - transform.position;
                    float oldAngle = Vector3.SignedAngle(cameraManager.transform.forward, to, cameraManager.transform.up);

                    if (Mathf.Abs(newAngle) < Mathf.Abs(oldAngle))
                    {
                        states.enemyTarget = enemy;
                        states.engagedBy = states;
                    }
                }
            }       
            
            
        }
    }

    
    void InputLookPosition()
    {
        if (states.aim || states.isStationaryAction)
        {
            Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
            float distance = 150;
            RaycastHit hitInfo;

            

            if (Physics.Raycast(ray, out hitInfo, distance, states.ignoreLayers))
            {
                if (hitInfo.collider.name != "ShieldCollider" && hitInfo.collider.name != "Controller Warrior")
                    states.lookPosition = hitInfo.point;
                //Debug.Log(hitInfo.collider.name);
            }
            else
            {
                states.lookPosition = mainCam.transform.position + mainCam.transform.forward * distance;
            }
        }
       
        if (states.lockon && states.enemyTarget != null)
        {
            states.lookPosition = states.enemyTarget.lockonPosition;
        }
    }


    void HandleDodgeAttack()
    {
        if (states.dodge)
        {
            canDodgeAttack = true;
        }

        if (canDodgeAttack && states.animator.GetBool("canMove"))
        {
            canDodgeAtttackTimer += delta;
            states.fire3 = Input.GetButton("Fire1");
        }

        if (canDodgeAtttackTimer >= 0.4f)
        {
            states.fire3 = false;
            canDodgeAttack = false;
            canDodgeAtttackTimer = 0;
        }
    }
}
