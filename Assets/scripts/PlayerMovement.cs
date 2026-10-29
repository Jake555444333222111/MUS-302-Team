using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 3f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Apply drag globally
        rb.linearDamping = groundDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput
                      + orientation.right * horizontalInput;

        rb.AddForce(
            moveDirection.normalized * moveSpeed * 10f,
            ForceMode.Force
        );
    }
}