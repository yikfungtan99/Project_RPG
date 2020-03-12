using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatesManager : MonoBehaviour
{
    // Delta Time
    float delta;
    float fixedDelta;

    // Audio Source
    AudioSource audioSource;


    [System.Serializable]
    public class ActionWithCurve
    {
        public string actionAnimationName;
        public AnimationCurve actionAnimationCurve;
        public AudioClip audioClip;
        public float staminaConsumption;
        public float damageMultiplier;
    }



    [Header("Stats")]
    //public float moveSpeed = 3.5f;
    public float slerpSpeed = 8.5f;
    public float actionCurveMultiplier = 4.5f;
    

    [Header("Action Customization")]
    public ActionWithCurve dodgeAction = new ActionWithCurve();
    public List<ActionWithCurve> attackComboListForFire1 = new List<ActionWithCurve>();
    public ActionWithCurve attackActionForFire2 = new ActionWithCurve();
    public ActionWithCurve attackActionForFire3 = new ActionWithCurve();
    public ActionWithCurve aimAttackString;

    [Header("Test Attributes (Temporary)")]
    public InputParent enemyTarget;
    public float maxEnemyTargetDistance = 10;
    [HideInInspector] public StatesManager engagedBy;
    public GameObject deadBody;


    // Components
    /*[HideInInspector]*/ public Animator animator;
    [HideInInspector] public Rigidbody rigidBody;
    [HideInInspector] public RPGManager rpg;
    [HideInInspector] public LayerMask ignoreLayers;

    // Input
    [HideInInspector] public float vertical;
    [HideInInspector] public float horizontal;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool fire1;
    [HideInInspector] public bool fire2;
    [HideInInspector] public bool fire3;
    [HideInInspector] public bool aimFire;
    [HideInInspector] public bool aim;
    [HideInInspector] public bool drink;
    [HideInInspector] public bool lockon;
    [HideInInspector] public bool dodge;
    [HideInInspector] public Vector3 lookPosition;
    [HideInInspector] public Quaternion aimRotation;

    // States
    [HideInInspector] public float moveAmount;
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public bool onGround;
    [HideInInspector] public bool isDodge;
    [HideInInspector] public float damageMultiplier;


    // Action Attributes
    //      -Stationary
    [HideInInspector] public bool isStationaryAction;
    float isStationaryActionBuffer = 0;
    //      -Slow Move
    [HideInInspector] public bool isSlowMoveAction;
    float isSlowMoveActionBuffer = 0;

    // Attack Combo Attributes
    int attComboIndex = 0;
    float attComboTimer = 0;
    float attComboReturnDuration = 0.85f;

    // Action Curve Attributes (For Stationary Actions: Have their own movements)
    float actionAnimation_t = 0;
    AnimationCurve actionAnimationCurve;

    // Dodge
    //bool isMoveWhenDodge = false;
    Vector3 dodgeDir;


    // Public Functions
    public void Hurt(float damage)
    {
        animator.CrossFade("Hurt", 0.2f);
        rpg.health.ModifyCur(-damage);
    }



    // Main Functions
    public void Init()
    {
        rpg = GetComponent<RPGManager>();
        animator = GetComponentInChildren<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        ignoreLayers = ~(1 << 8) | (1 << 9) | (1 << 10) | (1 << 11);

        audioSource = GetComponent<AudioSource>();

        deadBody = GetComponentInChildren<DieScript>().gameObject;
        deadBody.SetActive(false);
        rpg.Init();
    }

    public void Tick(float delta)
    {
        this.delta = delta;

        HandleDeath();

        rpg.Tick();

        HandleCooldowns();

        HandleStaminaActions(); 

        HandleActionCurves();
    }

    public void FixedTick(float fixedDelta)
    {
        this.fixedDelta = fixedDelta;

        UpdateOnGround();

        HandleLockon();

        rigidBody.drag = ((moveAmount > 0 && CanMove()) || !onGround) ? 0 : 4;

        if (!CanMove()) return;

        HandleRotation();

        HandleMovement();

        HandleMoveAnimation();
    }


    // Conditions
    bool CanMove()
    {
        bool canMove = true;

        if (!onGround) canMove = false;
        if (!animator.GetBool("canMove")) canMove = false;


        return canMove;
    }


    // Sub Functions
    #region Sub Functions

    void HandleCooldowns()
    {
        HandleStationaryActionOnceAtATime();
        HandleSlowMoveActionOnceAtATime();
    }

    void HandleDeath()
    {
        if (rpg.health.GetPercentage() <= 0)
        {
            if (engagedBy != null)
                engagedBy.lockon = false;
            animator.SetBool("dead", true);

            deadBody.SetActive(true);
            deadBody.GetComponent<DieScript>().enabled = true;
            //animator.gameObject.SetActive(false);
            Destroy(animator.gameObject);

            rigidBody.velocity = Vector3.zero;
            rigidBody.drag = 4;

            UpdateOnGround();
            rigidBody.useGravity = false;
            GetComponent<CapsuleCollider>().enabled = false;
            SetLayerRecursively(gameObject, 0);
            GetComponent<InputParent>().enabled = false;
        }
    }

    void UpdateOnGround()
    {
        float distanceFromGround = 0.5f;

        Ray ray = new Ray(transform.position + distanceFromGround * Vector3.up, Vector3.down);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, distanceFromGround + 0.2f, ignoreLayers))
        {
            transform.position = hitInfo.point;
            onGround = true;
        }
        else
        {
            onGround = false;
        }

        animator.SetBool("onGround", onGround);
    }

    void HandleLockon()
    {
        if (enemyTarget != null)
        {
            enemyTarget.states.engagedBy = this;

            float enemyTargetDistance = Vector3.Distance(enemyTarget.transform.position, transform.position);
            if (enemyTargetDistance > maxEnemyTargetDistance || !lockon)
            {
                enemyTarget.states.engagedBy = null;
                enemyTarget = null;
            }
        }       

        if (enemyTarget == null)
        {
            lockon = false;
        }
    }

    void HandleMovement()
    {
        if (animator.GetBool("slowMovement")) moveAmount *= 0.5f;
        float curSpeed = rpg.moveSpeed * (sprint ? 1.85f : 1) * moveAmount;
        rigidBody.velocity = moveDirection * curSpeed;
    }

    void HandleRotation()
    {
        if (moveDirection != Vector3.zero)
        {
            Quaternion r = Quaternion.identity;

            if (lockon)
            {
                Vector3 temp = enemyTarget.transform.position - transform.position;
                temp.y = 0;
                r = Quaternion.LookRotation(temp.normalized);                
            }
            else if (aim)
            {
                r = aimRotation;
            }
            else
            {
                r = Quaternion.LookRotation(moveDirection);
            }      


            transform.rotation = Quaternion.Slerp
                (
                    transform.rotation,
                    r,
                    fixedDelta * slerpSpeed
                );
        }        
    }

    void HandleStaminaActions()
    {
        if (sprint)
        {
            if (rpg.EnoughStamina())
                rpg.RegenStamina(-delta);
        }
        else
        {
            rpg.RegenStamina(delta);
        }        

        if (rpg.EnoughStamina())
        {
            HandleStationaryActions();
            HandleSlowMoveActions();
        }
        else
        {
            sprint = false;
        }
    }

    void HandleMoveAnimation()
    {
        if (lockon || aim)
        {
            animator.SetBool("lockon", true);
            animator.SetFloat("vertical", vertical * moveAmount, 0.1f, fixedDelta);
            animator.SetFloat("horizontal", horizontal * moveAmount, 0.1f, fixedDelta);
        }
        else
        {
            animator.SetBool("lockon", false);
            animator.SetFloat("vertical", moveAmount, 0.3f, fixedDelta);
            animator.SetFloat("horizontal", 0);
        }
        
        animator.SetBool("sprint", sprint);
        animator.SetBool("aim", aim);
    }

    void HandleStationaryActions()
    {
        string actionAnimation = null;

        if (!isSlowMoveAction && !isStationaryAction)
        {
            ReturnToFirstComboAfterDuration();
            actionAnimation = HandleStationaryActionTriggerring();
        }

        if (!string.IsNullOrEmpty(actionAnimation))
        {
            animator.CrossFade(actionAnimation, 0.15f);
        }
    }

    void HandleSlowMoveActions()
    {
        string actionAnimation = null;

        if (!isSlowMoveAction && !isStationaryAction)
        {
            actionAnimation = HandleSlowMoveActionTriggerring();
        }

        if (!string.IsNullOrEmpty(actionAnimation))
        {
            animator.Play(actionAnimation);
            
           // animator.CrossFade(actionAnimation, 0.15f);
        }
    }

    void HandleActionCurves()
    {
        if (!animator.GetBool("canMove") && actionAnimationCurve != null)
        {
            actionAnimation_t += delta;
            float zValue = actionAnimationCurve.Evaluate(actionAnimation_t);
            Vector3 direction = Vector3.zero;

            if (isDodge)
            {
                direction = dodgeDir != Vector3.zero ? dodgeDir : -transform.forward;
            }
            else
            {
                direction = transform.forward;
            }

            direction *= zValue;

            rigidBody.velocity = direction * actionCurveMultiplier;
        }
        else
        {
            actionAnimation_t = 0;
        }
    }

    #endregion

    // Sub Sub Functions (Actions)
    #region Sub Sub Functions
    void ReturnToFirstComboAfterDuration()
    {
        if (attComboIndex != 0)
        {
            attComboTimer += fixedDelta;
            if (attComboTimer >= attComboReturnDuration)
            {
                attComboTimer = 0;
                attComboIndex = 0;
            }
        }
        else
        {
            attComboTimer = 0;
        }
    }
    
    void HandleStationaryActionOnceAtATime()
    {
        if (isStationaryAction)
        {
            isStationaryActionBuffer += delta;
            if (isStationaryActionBuffer >= 0.16f
                && animator.GetBool("canMove"))
            {
                isStationaryActionBuffer = 0;
                isStationaryAction = false;
                if (isDodge)
                {
                    isDodge = false;
                }
            }
        }
        else
        {
            isStationaryActionBuffer = 0;
        }
    }


    /// <summary>
    /// Put All Actions that will Stop Movement here
    /// </summary>
    /// <returns></returns>
    string HandleStationaryActionTriggerring()
    {
        string actionAnimation = null;

        if (fire1)
        {
            PerformActionWithCurve(attackComboListForFire1[attComboIndex], ref actionAnimation, ref isStationaryAction);
            if (attComboIndex < attackComboListForFire1.Count - 1)
            {
                attComboIndex++;
            }
            else
            {
                attComboIndex = 0;
            }
        }

        if (dodge && !isDodge)
        {
            //isMoveWhenDodge = !Mathf.Approximately(moveAmount, 0);
            dodgeDir = moveDirection;
            isDodge = true;
            PerformActionWithCurve(dodgeAction, ref actionAnimation, ref isStationaryAction);            
        }

        if (fire2)
        {
            fire2 = false;
            PerformActionWithCurve(attackActionForFire2, ref actionAnimation, ref isStationaryAction);
        }

        if (fire3)
        {
            fire3 = false;
            PerformActionWithCurve(attackActionForFire3, ref actionAnimation, ref isStationaryAction);
        }

        return actionAnimation;
    }

    void HandleSlowMoveActionOnceAtATime()
    {
        if (isSlowMoveAction)
        {
            isSlowMoveActionBuffer += delta;
            if (isSlowMoveActionBuffer >= 0.16f
                && !animator.GetBool("slowMovement"))
            {
                isSlowMoveActionBuffer = 0;
                isSlowMoveAction = false;
            }
        }
        else
        {
            isSlowMoveActionBuffer = 0;
        }
    }

    /// <summary>
    /// Put All Actions that will Slow Down Movement here!
    /// </summary>
    /// <returns></returns>
    string HandleSlowMoveActionTriggerring()
    {
        string actionAnimation = null;

        if (drink)
        {
            isSlowMoveAction = true;
            actionAnimation = "Drink";
        }

        if (aimFire && animator.GetBool("canAimAttack"))
        {
            PerformActionWithCurve(aimAttackString, ref actionAnimation, ref isSlowMoveAction);
        }

        return actionAnimation;
    }

    void PerformActionWithCurve(ActionWithCurve a, ref string actionAnimation, ref bool whichBool)
    {
        whichBool = true;
        damageMultiplier = a.damageMultiplier;
        rpg.stamina.ModifyCur(-a.staminaConsumption);
        audioSource.clip = a.audioClip;
        audioSource.Play();
        actionAnimationCurve = a.actionAnimationCurve;
        actionAnimation = a.actionAnimationName;
    }




    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    #endregion
}
