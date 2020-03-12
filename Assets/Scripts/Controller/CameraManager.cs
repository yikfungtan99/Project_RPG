using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    float delta;

    public float shoulderDistance = 0.85f;
    public float smoothTime = 0.15f;
    public float mouseSensitivity = 2;
    public Vector2 pitchMinMax = new Vector2(-40, 40);
    public float followSpeed = 8;

    public float lockonPitchAdjustment = 0.7f;

    Transform cameraPivot;
    Transform target;

    float pitch;
    float yaw;
    float smoothX, smoothY, smoothXVel, smoothYVel;


    public InputParent enemyTarget;

    [HideInInspector] public bool lockon;

    float shoulderOffset;
    Vector3 shoulderPos;

    


    public void Init(Transform target)
    {
        this.target = target;
        cameraPivot = Camera.main.transform.parent;
        shoulderOffset = shoulderDistance;
        UpdateShoulderPos();
    }

    public void Tick(float delta)
    {
        this.delta = delta;
        HandleShoulderCamera();
        FollowTarget();
        HandleRotation();



        if (Input.GetKeyDown(KeyCode.L))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

    }
    void FollowTarget()
    {
        Vector3 p = 
            Vector3.Lerp
            (
                transform.position,
                target.position,
                delta * followSpeed
            );
        transform.position = p;
    }

    void HandleRotation()
    {

        if (enemyTarget != null)
        {
            Vector3 yaw = enemyTarget.lockonPosition - transform.position;
            yaw.y = 0;
            transform.rotation = Quaternion.RotateTowards
                (
                    transform.rotation, 
                    Quaternion.LookRotation(yaw.normalized), 
                    delta * 250
                );
            this.yaw = transform.eulerAngles.y;


            Vector3 pitch = (enemyTarget.lockonPosition + Vector3.up * lockonPitchAdjustment) - transform.position;
            pitch.z = Mathf.Sqrt(Mathf.Pow(pitch.z, 2) +Mathf.Pow(pitch.x, 2));
            pitch.x = 0;
            cameraPivot.localRotation = Quaternion.RotateTowards
                (
                    cameraPivot.localRotation,
                    Quaternion.LookRotation(pitch.normalized),
                    delta * 250
                );
            //pitch = cameraPivot.localEulerAngles.x;
        }
        else
        {
            

            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            smoothX = Mathf.SmoothDamp(smoothX, x, ref smoothXVel, delta * smoothTime);
            smoothY = Mathf.SmoothDamp(smoothY, y, ref smoothYVel, delta * smoothTime);

            pitch -= smoothY * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

            yaw += smoothX * mouseSensitivity;

            cameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.rotation = Quaternion.Euler(0, yaw, 0);
        }        
    }

    void UpdateShoulderPos()
    {
        shoulderPos = cameraPivot.localPosition;
        shoulderPos.x = shoulderOffset;
    }

    void HandleShoulderCamera()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            shoulderOffset = -shoulderOffset;
            Vector3 pos = cameraPivot.localPosition;
            pos.x = shoulderOffset;
            UpdateShoulderPos();
        }

        if (cameraPivot.localPosition != shoulderPos)
        {
            cameraPivot.localPosition = Vector3.Lerp
                (
                    cameraPivot.localPosition, 
                    shoulderPos, 
                    delta * followSpeed
                );
        }
    }

}
