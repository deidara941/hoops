using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BallReplay : MonoBehaviour {
    [SerializeField] private Camera replayCamera;
    private Camera _mainCamera;
    private List<FrameData> _recordedFrames = new List<FrameData>();
    
    private Rigidbody _rb;
    
    private bool _isRecording = false;
    private bool _isReplaying = false;
    private int _replayIndex = 0;
    [SerializeField] private float replaySpeed = 0.5f;
    private float _replayFrameRate;
    private float _replayTimer = 0f;
    
    private bool _hasHitFirstTrigger = false;
    
    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        _replayFrameRate = Time.fixedDeltaTime / replaySpeed; // Adjust frame rate for slow motion
    }

    private void FixedUpdate() {
        if (_isRecording) {
            // Store position and rotation every frame
            _recordedFrames.Add(new FrameData(transform.position, transform.rotation));
        }
    }

    private void Update() {
        if (!_isReplaying || _replayIndex >= _recordedFrames.Count) return;
        _replayTimer += Time.deltaTime;

        if (!(_replayTimer >= _replayFrameRate)) return;
        
        // Update position at slowed-down speed
        transform.position = _recordedFrames[_replayIndex].Position;
        transform.rotation = _recordedFrames[_replayIndex].Rotation;
        _replayIndex++;
        _replayTimer = 0f;  
    }
    
    public void StartRecording() {
        _recordedFrames.Clear();
        _isRecording = true;
    }
    
    public void StopRecordingAndPlayReplay() {
        
        if (_isRecording) {
            _isRecording = false;
        }

        if (_recordedFrames.Count <= 0) return;

        // Reset time scale before replay
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;

        // Start replay
        _isReplaying = true;
        _replayIndex = 0;
        _replayTimer = 0f;

        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null) {
            ballCollider.enabled = false;
        }
        
        // Disable CameraFollow on Main Camera
        CameraFollow cameraFollowMain = _mainCamera.GetComponent<CameraFollow>();
        if (cameraFollowMain != null) {
            cameraFollowMain.enabled = false;
        }

        // Ensure cameras switch properly
        if (replayCamera != null && _mainCamera != null) {
            _mainCamera.gameObject.SetActive(false);
            replayCamera.gameObject.SetActive(true);
            
            ReplayCameraFollow cameraFollowReplay = replayCamera.GetComponent<ReplayCameraFollow>();
            if (cameraFollowReplay != null) {
                cameraFollowReplay.StartFollowing();
            }  
            
        } else {
            Debug.LogError("Replay Camera or Main Camera is missing! Assign them in the Inspector.");
            return;
        }

        // Disable physics during replay
        _rb.isKinematic = true;

        // Calculate replay duration
        float replayDuration = _recordedFrames.Count * _replayFrameRate;
        Invoke(nameof(EndReplay), replayDuration);
    }

    private void EndReplay() {
        _isReplaying = false;

        // Restore ball physics
        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null) {
            ballCollider.enabled = true;
        }
        
        // Switch back to the Main Camera and enable CameraFollow again
        if (replayCamera != null && _mainCamera != null) {
            
            ReplayCameraFollow cameraFollowReplay = replayCamera.GetComponent<ReplayCameraFollow>();
            if (cameraFollowReplay != null) {
                cameraFollowReplay.StopFollowing();
            }
            
            replayCamera.gameObject.SetActive(false);
            _mainCamera.gameObject.SetActive(true);

            // Re-enable CameraFollow script after replay
            CameraFollow cameraFollowMain = _mainCamera.GetComponent<CameraFollow>();
            if (cameraFollowMain != null) {
                cameraFollowMain.enabled = true;
            }

            Debug.Log("Switched back to Main Camera");
        } else {
            Debug.LogError("Replay Camera or Main Camera is missing!");
        }
    }

    // Struct to store position and rotation at each frame
    private struct FrameData {
        public Vector3 Position;
        public Quaternion Rotation;

        public FrameData(Vector3 pos, Quaternion rot) {
            Position = pos;
            Rotation = rot;
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("FirstTrigger")) {
            _hasHitFirstTrigger = true;
            Debug.Log("Ball hit first trigger!");
        } 
        else if (other.CompareTag("SecondTrigger") && _hasHitFirstTrigger) {
            Debug.Log("Ball hit second trigger after first! Starting replay...");
            _hasHitFirstTrigger = false; // Reset for the next shot
            StopRecordingAndPlayReplay();
        }
    }
}