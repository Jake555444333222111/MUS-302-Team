
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 15f;
    public float scrollSpeed = 0.01f;

    [Header("Camera")]
    public float lookSensitivity = 0.15f;

    float horizontalInput;
    float verticalInput;
    float rotationX;
    float rotationY;

    Vector3 moveDirection;

    private void Start()
    {
        // Get the camera's starting rotation
        Vector3 rotation = transform.eulerAngles;

        rotationX = rotation.x;
        rotationY = rotation.y;
    }

    private void Update()
    {
        MyInput();
        MovePlayer();
        CameraLook();
        AdjustSpeed();
    }

    private void MyInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.wKey.isPressed)
            verticalInput = 1f;

        if (Keyboard.current.sKey.isPressed)
            verticalInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;

        if (Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;
    }

    private void MovePlayer()
    {
        // Movement relative to camera direction
        moveDirection = transform.forward * verticalInput
                      + transform.right * horizontalInput;

        // Vertical movement
        if (Keyboard.current.eKey.isPressed)
            moveDirection += Vector3.up;

        if (Keyboard.current.qKey.isPressed)
            moveDirection += Vector3.down;

        // Sprint
        float currentSpeed = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed)
            currentSpeed = sprintSpeed;

        // Apply movement
        transform.position +=
            moveDirection.normalized * currentSpeed * Time.deltaTime;
    }


    private void CameraLook()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        rotationY += mouseDelta.x * lookSensitivity;
        rotationX -= mouseDelta.y * lookSensitivity;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(
            rotationX,
            rotationY,
            0f
        );
    }

    private void AdjustSpeed()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        moveSpeed += scroll * scrollSpeed;
        moveSpeed = Mathf.Clamp(moveSpeed, 1f, 100f);
    }

    private void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
