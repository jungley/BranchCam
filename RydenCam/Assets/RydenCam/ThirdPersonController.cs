using UnityEngine;
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
        if (animator == null)
        {
            Debug.LogWarning("[RydenCam] ThirdPersonController: No Animator found. Movement animations will be skipped.");
        }
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.CompareTag(BranchConstants.Tag_RydenConvo))
        {
            dialoguePlayer = other.gameObject.GetComponent<DialoguePlayer>();
            dialoguePlayer?.StartSequence();
        }
    }

    void FixedUpdate()
    {
        if (animator != null)
            animator.SetBool("isRunning", false);

        if (dialoguePlayer != null && dialoguePlayer.IsDialogueRunning)
            return;

        bool isMoving = false;

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            transform.position += transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed * 2.5f;
            isMoving = true;
        }
        else if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.LeftShift))
        {
            transform.position += transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
            isMoving = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position -= transform.TransformDirection(Vector3.forward) * Time.deltaTime * movementSpeed;
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            transform.position += transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
            isMoving = true;
        }
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            transform.position -= transform.TransformDirection(Vector3.left) * Time.deltaTime * movementSpeed;
            isMoving = true;
        }

        if (animator != null)
            animator.SetBool("isRunning", isMoving);
    }
}
