using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RydenCam;
using RydenCam.DialogueGameUI;
using RydenCam.Common;

public class ThirdPersonController : MonoBehaviour
{
    private Animator animator;
    private float movementSpeed = 3.0f;

    DialoguePlayer dialoguePlayer = null;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    //Issue: If ReturnOriginalPosition is enabled, character will return to the point where covo is Triggered, and convo will be retriggered:
    //Not sure if this would be fixed by game's convo trigger conditions
    private void OnTriggerEnter(Collider other)
    { 
        if (other.tag == BranchConstants.Tag_RydenConvo /*OTHER CONDITIONS*/)
        {
            //other.gameObject.GetComponent<DialoguePlayer>().StartConversation();
            dialoguePlayer = other.gameObject.GetComponent<DialoguePlayer>();
            dialoguePlayer?.StartSequence();

        }
    }

    //Update is called once per frame
    void FixedUpdate()
    {
        animator.SetBool("isRunning", false);

        //Prevent movement when dialogue is running
        if (dialoguePlayer == null || !dialoguePlayer.IsDialogueRunning)
        {

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey("w"))
            {
                transform.position += transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed * 2.5f;
                animator.SetBool("isRunning", true);
            }
            else if (Input.GetKey("w") && !Input.GetKey(KeyCode.LeftShift))
            {
                transform.position += transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
                animator.SetBool("isRunning", true);
            }
            else if (Input.GetKey("s"))
            {
                transform.position -= transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
                animator.SetBool("isRunning", true);
            }

            if (Input.GetKey("a") && !Input.GetKey("d"))
            {
                transform.position += transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
                animator.SetBool("isRunning", true);
            }
            else if (Input.GetKey("d") && !Input.GetKey("a"))
            {
                transform.position -= transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
                animator.SetBool("isRunning", true);
            }
        }
    }
}
