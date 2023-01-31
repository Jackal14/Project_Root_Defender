using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class PlayerMovement : MonoBehaviour
{
    //Unity components
    private CharacterController controller;
    private Animator anim;
    private Rigidbody rb;
    private Camera mainCam;
    private Transform refTransform;

    [Header("Rotations")]
    [SerializeField, Tooltip("Controls the speed at which the player rotates"), Range(0, 10)]
    private float smoothInputSpeed = .2f;

    [Space(20)]
    [Header("Movement Variables")]
    private float currentSpeed;
    [SerializeField, Range(0, 10), Tooltip("Controls player walk speed")]
    private float walkSpeed;
    [SerializeField, Range(0, 10), Tooltip("Controls player run speed")]
    private float runSpeed;
    private Vector3 playerVelocity;
    private float rotationSpeed;
    private float gravityValue = -9.81f;
    [Space]
    
    //Input vectors for smoothing player input
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private Vector3 testVelocity;
    
    //
    public LayerMask layer;
    public Transform attackPoint1;
    public Transform attackPoint2;
    public float radius = 1f;

    //Combat variables
    private bool isRunning;
    public bool inCombat = false;
    private bool isAttacking = false;
    private bool detectingAttack = false;
    private bool lockedOn = false;



    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        refTransform = new GameObject().transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Read player input
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        //anim.SetFloat("Horizontal", movement.x, 0.1f, Time.deltaTime);
        //anim.SetFloat("Vertical", movement.y, 0.1f, Time.deltaTime);
        //Apply damping to the vector to create a smoother transition when player input is detected
        if (movement != Vector2.zero)
            currentInputVector = Vector2.SmoothDamp(currentInputVector, movement, ref smoothInputVelocity, smoothInputSpeed);
        else
            currentInputVector = Vector2.SmoothDamp(currentInputVector, new Vector2(0, 0), ref smoothInputVelocity, .03f);
        //currentInputVector = new Vector2(0, 0);
        if(Input.GetButtonDown("Fire1"))
        {
            anim.SetTrigger("Attack");
        }

        //Call the appropriate methods
        HandleMovement(currentInputVector);
    }

    void HandleMovement(Vector2 movement)
    {
        /*//Set running bool to whether the player is holding shift
        isRunning = (sprintControl.activeControl != null) ? true : false;*/

        //Create a move vector with input from the x and y axis
        Vector3 move = new Vector3(movement.x, 0, movement.y);
        //Multiply move by the cameras forward and right
        refTransform.eulerAngles = new Vector3(0f, mainCam.transform.eulerAngles.y, 0f);
        move = refTransform.transform.forward * move.z + mainCam.transform.right * move.x;
        //Hard set move on the y
        move.y = 0f;
        //Move the player controller based on the determined vector
        controller.Move(move * Time.deltaTime * currentSpeed);
        //Set forward to the movement vector
        if (move != Vector3.zero && !lockedOn)
        {
            gameObject.transform.forward = Vector3.SmoothDamp(gameObject.transform.forward, move, ref testVelocity, .025f);
        }
        //Actually move the player
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        //If the player is moving
        if (movement != Vector2.zero)
        {
            //Rotate the player to face the cameras rotation
            //Determine the target angle based on the camera rotation
            float targetAngle = Mathf.Atan2(movement.x, movement.y) * Mathf.Rad2Deg + mainCam.transform.eulerAngles.y;
            //Put that angle into a quaternion
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            //Set the players rotation to the desired angle
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed);

            //Change the speed to walkSpeed
            currentSpeed = walkSpeed;
            //Change the animation to walking
            //anim.SetBool("IsWalking", true);
        }
        //Else move input is zero, i.e. he's idle 
        else
        {
            //Reset the speed
            currentSpeed = 0;
            //Change the animation to idle
            //anim.SetBool("IsWalking", false);
        }

        //Detect if the player is running
        if (isRunning && movement != Vector2.zero)
        {
            //Set the speed to run speed
            currentSpeed = runSpeed;
            //Play the running animation when running
            //anim.SetBool("IsRunning", true);
        }
        //Else the player is walking, set to normal speed
        else
        {
            //Stop the running animation
            //anim.SetBool("IsRunning", false);
        }
    }

    //Function to detect for hits
    //This is called from an animation event on the players attack animation
    public IEnumerator DetectHit()
    {
        detectingAttack = true;
        while (detectingAttack)
        {
            //Right now collision detection is only detected once, animation events should be able to fix this, and a lot of my other issues
            //Creates an array of hits, will create an Overlap Capsule at the attackPoints position, with a radius of size radius, detecting for objects in the selected layer
            Collider[] hits = Physics.OverlapCapsule(attackPoint1.position, attackPoint2.position, radius, layer);

            //If the array of hits contains something (I.E. we did in fact hit something in our intended layer)
            if (hits.Length > 0)
            {
                Debug.Log("Hit detected");
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].tag == "Enemy")
                    {
                        hits[i].GetComponent<Enemy>().TakeDamage();
                    }
                }
                break;
            }
            yield return null;
        }
    }

    //Function to end hit detection
    //This is called from an animation event on the players attack animation
    public void EndDetectHit()
    {
        detectingAttack = false;
    }
}
