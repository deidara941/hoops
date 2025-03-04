using UnityEngine;
using UnityEngine.InputSystem;

public class BallController : MonoBehaviour {

    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private Transform respawnPosition;
    [SerializeField] private float gravity = 40f;

    private Rigidbody _rigidbody;

    private void FixedUpdate() {
        _rigidbody.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }
    
    private void OnRestart(InputAction.CallbackContext context) {
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        
        _rigidbody.position = respawnPosition.position;
    }

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Awake() => inputActions.FindAction("Reset").started += OnRestart;
    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    
}