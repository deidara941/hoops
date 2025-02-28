using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Ball")]
    public Transform Ball;
    public Transform PosOvedHead;
    public Transform PosDribble;
    public Transform RespawnPoint;
    private bool isBallInHands = true;
    public float ballGravity = 20.0f;

    [Header("MovingAndLooking")] 
    private Vector2 moveV, lookV;

    private float moveSpeed = 10.0f;
    public float moveBaseSpeed = 13.0f;
    public float moveJumpSpeed = 5.0f;

    [Header("Throwing")]
    private bool isChargingThrow = false;
    public float maxThrowForce = 40.0f;
    public float minThrowForce = 10.0f;
    public float chargeSpeed = 20.0f;
    private float chargeTime = 0f;

    [Header("Jumping")]
    private Vector3 jumpDirection;
    private bool isGrounded = true;
    public float jumpForce = 8.0f;

    [Header("Dashing")]
    private bool isDashing = false;
    public float dashForce = 35.0f;
    public float dashUpwardForce = 0.2f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 3.0f;
    private float dashCooldownTimer = 0f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update() {

        movePlayer();

        if (isBallInHands) {
            if (isChargingThrow) {
                // Ball above head
                Ball.position = PosOvedHead.position;
            }

            else {
                //Dribbling
                Ball.position = PosDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.time * 5));
            }
        }

        if (isChargingThrow) {
            chargeTime += Time.deltaTime * chargeSpeed;
            chargeTime = Mathf.Clamp(chargeTime, 0f, maxThrowForce); // Prevent overcharging
        }

        if (dashCooldownTimer > 0) {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate() {
        if (!isBallInHands) {
            //Adding more gravity for ball
            Rigidbody ballRb = Ball.GetComponent<Rigidbody>();
            ballRb.AddForce(Vector3.down * ballGravity, ForceMode.Acceleration);
        }
    }

    public void movePlayer() {

        if (isDashing) return;

        Vector3 aimDirection = new Vector3(lookV.x, 0f, lookV.y);
        Vector3 movement = new Vector3(moveV.x, 0f, moveV.y);

        if (isGrounded) {
            //Save the direction of player
            jumpDirection = movement;
        }

        // Aiming
        if (aimDirection.sqrMagnitude > 0.001f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(aimDirection), 0.15f);
        }
        else if (movement.sqrMagnitude > 0.001f) {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        }

        transform.Translate(jumpDirection * moveSpeed * Time.deltaTime, Space.World);
    }

    public void ThrowBall() {

        if (isDashing) return;

        isBallInHands = false;

        Rigidbody ballRb = Ball.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.useGravity = false;

        Vector3 throwDirection = transform.forward + Vector3.up * 1.2f;
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeTime / maxThrowForce);

        ballRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

        chargeTime = 0f;
    }

    public void RespawnBall() {
        Ball.position = RespawnPoint.position; 
        Rigidbody ballRb = Ball.GetComponent<Rigidbody>();

        ballRb.isKinematic = false;

        ballRb.linearVelocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        isBallInHands = false;
    }


    private void OnTriggerEnter(Collider other) {

        // Pickup ball
        if (!isBallInHands && other.CompareTag("Ball")) {
            isBallInHands = true;
            Ball.GetComponent<Rigidbody>().isKinematic = true;
            Ball.GetComponent<Rigidbody>().useGravity = false;
        }

        if (other.CompareTag("Ground")) {
            isGrounded = true;
        }
    }


    public void onMove(InputAction.CallbackContext context) {
        moveV = context.ReadValue<Vector2>();
    }

    public void onLook(InputAction.CallbackContext context) {
        lookV = context.ReadValue<Vector2>();
    }

    public void onRestart(InputAction.CallbackContext context) {
        if (context.performed) {
            RespawnBall();
        }
    }
    public void onThrow(InputAction.CallbackContext context) {

        if (isDashing) return;

        float triggerValue = context.ReadValue<float>();

        if (isBallInHands) {
            if (context.started) {
                isChargingThrow = true;
                chargeTime = 0f;
                moveSpeed = moveJumpSpeed;
            }

            if (context.canceled) {
                ThrowBall();
                isChargingThrow = false;
                moveSpeed = moveBaseSpeed;
            }
        }
    }

    public void onJump(InputAction.CallbackContext context) {

        if (isDashing) return;

        // Do jump
        if (context.performed && isGrounded) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    public void onDash(InputAction.CallbackContext context) {
        if (context.performed && dashCooldownTimer <= 0 && !isDashing) {
            StartCoroutine(Dash());
        }
    }
    IEnumerator Dash() {
        isDashing = true;
        moveSpeed = 0f; // Disable movement

        Vector3 dashDirection = new Vector3(moveV.x, 0f, moveV.y).normalized;
        if (dashDirection == Vector3.zero) {
            dashDirection = transform.forward; // Dash forward if no input
        }

        rb.linearVelocity = dashDirection * dashForce; // Apply dash force

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector3.zero; // Stop dash
        moveSpeed = moveBaseSpeed; // Re-enable movement
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }
}
