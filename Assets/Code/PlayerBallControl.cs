using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBallControl : MonoBehaviour {

    [Header("Arms")]
    [SerializeField] private Transform leftArm;
    [SerializeField] private Transform rightArm;
    
    [Header("Ball positions")]
    [SerializeField] private Transform posDribble;
    [SerializeField] private Transform posOverHead;
    
    [Header("Throwing")]
    [SerializeField] private float maxThrowForce = 35.0f;
    [SerializeField] private float minThrowForce = 10.0f;
    [SerializeField] private float chargeSpeed = 70.0f;
    [SerializeField] private float chargeSlowMotion = 0.3f;
    [SerializeField] private float playerSpeedRetentionFraction = 0.1f;
    
    private Rigidbody _player;
    private Rigidbody _ball;
    private bool _hasBall;

    private BallReplay _ballReplay;
    
    private bool _isChargingThrow;
    private float _chargeTime;

    private void Update() {
        if (!_hasBall) return;

        if (_isChargingThrow) {
            _ball.position = posOverHead.position;
            
            _chargeTime += Time.deltaTime * chargeSpeed;
            _chargeTime = Mathf.Clamp(_chargeTime, 0f, maxThrowForce);
        } else {
            _ball.position = posDribble.position + Vector3.up * Mathf.Abs(Mathf.Sin(Time.fixedTime * 5));
        }
    }
    
    public void OnThrow(InputAction.CallbackContext context) {
        if (!_hasBall) return;
        
        // initiate charge
        if (context.started) {
            _isChargingThrow = true;
            _chargeTime = 0f;
            
            PutArmsUp();
            EnableSlowMotion();
        }

        // throw
        if (context.canceled) {
            ThrowBall();
            _isChargingThrow = false;
            
            RemoveBallFromHands();
            DisableSlowMotion();
            
            // Start recording for replay
            if (_ballReplay != null) {
                _ballReplay.StartRecording();
            }
        }
    }

    private void EnableSlowMotion() {
        Time.timeScale = chargeSlowMotion;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }

    private void DisableSlowMotion() {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
    
    private void ThrowBall() {
        if (!_hasBall) return;

        _ball.isKinematic = false;
        _ball.detectCollisions = true;

        Vector3 throwDirection = transform.forward + Vector3.up * 1.2f;
        float throwForce = Mathf.Max(minThrowForce, _chargeTime);

        _ball.AddForce(
            _player.linearVelocity * playerSpeedRetentionFraction + throwDirection * throwForce,
            ForceMode.Impulse
        );
    }
    
    private void OnCollisionEnter(Collision other) {
        if (_hasBall || !other.gameObject.CompareTag("Ball")) return;
        
        _ball = other.gameObject.GetComponent<Rigidbody>();
        _ballReplay = _ball.GetComponent<BallReplay>();
        _hasBall = true;
        
        _ball.isKinematic = true;
        _ball.detectCollisions = false;
        
        PutRightArmInDribblePosition();
    }
    
    public void OnRestart(InputAction.CallbackContext context) {
        if (!context.started) return;
        
        RemoveBallFromHands();
    }

    private void RemoveBallFromHands() {
        _ball = null;
        _hasBall = false;
        PutArmsDown();
    }

    private void PutArmsUp() {
        leftArm.localEulerAngles = Vector3.right * 180;
        rightArm.localEulerAngles = Vector3.right * 180;
    }

    private void PutArmsDown() {
        leftArm.localEulerAngles = Vector3.right * 0;
        rightArm.localEulerAngles = Vector3.right * 0;
    }

    private void PutRightArmInDribblePosition() {
        rightArm.localEulerAngles = Vector3.forward * 30;
    }

    private void Start() {
        _player = GetComponent<Rigidbody>();
    }

}