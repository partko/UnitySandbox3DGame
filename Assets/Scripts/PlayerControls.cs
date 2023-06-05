using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float flySpeed = 32.0f;
    public float rotateSpeed = 4f; //0.8f;
    public float gravity = 20.0f;

    private Vector3 moveDirection = Vector3.zero;

    private CharacterController controller;
    private Transform playerCamera;

    public bool isFly = false;

    private void Start()
    {
        Cursor.visible = false; // скрыть курсор
        Cursor.lockState = CursorLockMode.Locked; // "заморозить" курсор в одном месте

        isFly = false;
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>().transform;
    }

    void Update()
    {
        if (Input.GetKeyDown("f"))
        {
            isFly = !isFly;
        }

        transform.Rotate(0, Input.GetAxis("Mouse X") * rotateSpeed, 0);

        playerCamera.Rotate(-Input.GetAxis("Mouse Y") * rotateSpeed, 0, 0);
        if (playerCamera.localRotation.eulerAngles.y != 0)
        {
            playerCamera.Rotate(Input.GetAxis("Mouse Y") * rotateSpeed, 0, 0);
        }

        //moveDirection = new Vector3(Input.GetAxis("Horizontal") * speed, moveDirection.y, Input.GetAxis("Vertical") * speed);
        //moveDirection = transform.TransformDirection(moveDirection);


        if (!isFly)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal") * speed, moveDirection.y, Input.GetAxis("Vertical") * speed);
            moveDirection = transform.TransformDirection(moveDirection);

            if (controller.isGrounded)
            {
                if (Input.GetButton("Jump")) moveDirection.y = jumpSpeed;
                else moveDirection.y = 0;
            }

            moveDirection.y -= gravity * Time.deltaTime;
        }
        else
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal") * flySpeed, moveDirection.y, Input.GetAxis("Vertical") * flySpeed);
            moveDirection = transform.TransformDirection(moveDirection);

            if (Input.GetButton("Jump")) moveDirection.y = jumpSpeed;
            else if (Input.GetKey("left shift")) moveDirection.y = -jumpSpeed;
            else moveDirection.y = 0;
        }
        
        controller.Move(moveDirection * Time.deltaTime);
    }
}
