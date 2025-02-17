using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;          // Speed for movement
    public float lookSensitivity = 2f;    // Sensitivity for mouse rotation
    public float smoothTime = 0.1f;       // Time for smoothing movement

    private Vector3 currentVelocity;      // Used by Lerp for smooth movement
    private Vector3 targetPosition;       // Target position for Lerp movement
    private Quaternion targetRotation;    // Target rotation for Lerp rotation

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        // Update target position based on WASD input
        float moveHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrow
        float moveVertical = Input.GetAxis("Vertical");     // W/S or Up/Down arrow
        Vector3 moveDirection = transform.right * moveHorizontal + transform.forward * moveVertical;

        targetPosition += moveDirection * moveSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothTime);

        // Update target rotation based on mouse movement
        rotationX += Input.GetAxis("Mouse X") * lookSensitivity;
        rotationY -= Input.GetAxis("Mouse Y") * lookSensitivity;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Limit vertical rotation

        targetRotation = Quaternion.Euler(rotationY, rotationX, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, smoothTime);
    }
}
