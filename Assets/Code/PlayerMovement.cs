using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerMovement : MonoBehaviour {
    [SerializeField] private float controllerDeadZone = .001f;
    [SerializeField] private float gravity = 100f;
    [SerializeField] private float movementSpeed = 25f;
    [SerializeField] private float jumpForce = 30f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 70f;
    [SerializeField] private float dashDuration = .25f;
    [SerializeField] private float dashCooldownSeconds = .7f;
    [SerializeField] AnimationCurve dashSpeedCurve;

    private Rigidbody _rigidbody;

    private Vector3 _movementVector;
    private Vector3 _lookVector;
    private Vector3 _dashVector = Vector3.zero;
    
    private bool _isGrounded = true;
    private bool _isDashing;
    private bool _isDashOnCooldown;

    private void FixedUpdate() {
        // apply custom gravity value
        _rigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        Vector3 movementVector = _movementVector.sqrMagnitude > controllerDeadZone ? _movementVector : Vector3.zero;

        // movement
        if (_isDashing) {
            _rigidbody.linearVelocity = Vector3.up * _rigidbody.linearVelocity.y + _dashVector * dashForce;
        } else {
            _rigidbody.linearVelocity = Vector3.up * _rigidbody.linearVelocity.y + movementVector * movementSpeed;
        }

        // rotation
        Vector3 lookVector = _lookVector.sqrMagnitude > controllerDeadZone ? _lookVector : movementVector;
        if (lookVector.sqrMagnitude > controllerDeadZone) {
            _rigidbody.rotation = Quaternion.Lerp(
                _rigidbody.rotation,
                Quaternion.LookRotation(lookVector),
                rotationSpeed * Time.fixedDeltaTime
            );
        }
    }

    public void OnMove(InputAction.CallbackContext context) {
        Vector2 inputVector = context.ReadValue<Vector2>();
        _movementVector = new Vector3 { x = inputVector.x, z = inputVector.y };
    }

    public void OnLook(InputAction.CallbackContext context) {
        Vector2 inputVector = context.ReadValue<Vector2>();
        _lookVector = new Vector3 { x = inputVector.x, z = inputVector.y };
    }

    public void OnJump(InputAction.CallbackContext context) {
        if (!_isGrounded || !context.started) return;

        _rigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        _isGrounded = false;
    }

    public void OnDash(InputAction.CallbackContext context) {
        if (_isDashOnCooldown || !context.started) return;

        StartCoroutine(PerformDash());
    }

    private IEnumerator PerformDash() {
        _isDashing = true;
        _isDashOnCooldown = true;
        float elapsed = 0f;

        Vector3 dashVector = _movementVector.sqrMagnitude > controllerDeadZone
            ? _movementVector
            : _rigidbody.transform.forward;

        while (elapsed < dashDuration) {
            // Curve-based speed modulation
            float speedMultiplier = dashSpeedCurve.Evaluate(elapsed / dashDuration);
            _dashVector = dashVector * speedMultiplier;

            elapsed += Time.deltaTime;
            yield return null;
        }

        _isDashing = false;
        _dashVector = Vector3.zero;
        
        yield return new WaitForSeconds(dashCooldownSeconds);
        _isDashOnCooldown = false;
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Ground")) {
            _isGrounded = true;
        }
    }

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }
}