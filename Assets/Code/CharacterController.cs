using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    private Rigidbody rb;

    private Vector2 moveV, lookV;
    private Vector3 jumpDirection; 


    public Transform Ball;
    public Transform PosOvedHead;
    public Transform PosDribble;
    public Transform RespawnPoint;

    private float moveSpeed = 10.0f;
    private float chargeTime = 0f;
    private bool isChargingThrow = false;
    private bool isGrounded = true;
    private bool isBallInHands = true;

    public float moveBaseSpeed = 10.0f;
    public float moveJumpSpeed = 5.0f;
    public float maxThrowForce = 100.0f;
    public float minThrowForce = 30.0f;
    public float chargeSpeed = 20.0f;
    public float ballGravity = 20.0f;
    public float jumpForce = 8f; 


    void Start() {
        rb = GetComponent<Rigidbody>();
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
    }

    void FixedUpdate() {
        if (!isBallInHands) {
            //Adding more gravity for ball
            Rigidbody ballRb = Ball.GetComponent<Rigidbody>();
            ballRb.AddForce(Vector3.down * ballGravity, ForceMode.Acceleration);
        }
    }

    public void movePlayer() {

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

    void ThrowBall() {
        isBallInHands = false;

        Rigidbody ballRb = Ball.GetComponent<Rigidbody>();
        ballRb.isKinematic = false;
        ballRb.useGravity = false;

        Vector3 throwDirection = transform.forward + Vector3.up * 1.5f;
        float throwForce = Mathf.Lerp(minThrowForce, maxThrowForce, chargeTime / maxThrowForce);

        ballRb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

        chargeTime = 0f;
    }

    void RespawnBall() {
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

        // Do jump
        if (context.performed && isGrounded) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }
}
