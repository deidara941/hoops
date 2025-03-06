using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallReplay : MonoBehaviour {
    [SerializeField] private List<Camera> replayCameras;
    [SerializeField][Range(0.1f, 1f)]  private float replaySpeed = 0.5f;
    
    private Camera _mainCamera;
    private readonly List<FrameData> _recordedFrames = new();
    
    private Rigidbody _rb;
    private readonly List<ReplayCamera> _replayCameras = new();
    
    private bool _isRecording = false;
    
    private bool _hasHitFirstTrigger = false;

    private float _recordingStart;
    private float _totalRecordingTime;
    
    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        
        if (replayCameras.Count == 0 || _mainCamera == null) {
            throw new InvalidOperationException("Replay Camera or Main Camera is missing! Assign them in the Inspector.");
        }
        
        foreach (Camera replayCamera in replayCameras) {
            var replayCameraFollower = replayCamera.GetComponent<ReplayCameraFollow>();
            if (replayCameraFollower == null) {
                throw new InvalidOperationException("Replay camera does not have a ReplayCameraFollow component.");
            }
            _replayCameras.Add(new ReplayCamera(replayCamera, replayCameraFollower));
        }
    }

    private void FixedUpdate() {
        if (!_isRecording) return;
        
        // Store position and rotation every frame
        _recordedFrames.Add(new FrameData(transform.position, transform.rotation));
    }

    public void StartRecording() {
        _recordedFrames.Clear();
        _isRecording = true;
        
        _recordingStart = Time.time;
    }
    
    private void StopRecordingAndPlayReplay() {
        _isRecording = false;
        if (_recordedFrames.Count <= 0) return;

        BallPhysicsEnabled(false);

        _totalRecordingTime = Time.time - _recordingStart;
        
        // Start replay with first camera
        StartCoroutine(Replay(0));
    }

    private IEnumerator Replay(int cameraIndex) {
        // End replay if no more cameras are available
        if (cameraIndex >= _replayCameras.Count) {
            EndReplay();
            yield break;
        }

        ReplayCamera replayCamera = _replayCameras[cameraIndex];
        EnableReplayCamera(replayCamera);

        float timePerFrame = _totalRecordingTime / _recordedFrames.Count;
        
        for (var index = 0; index < _recordedFrames.Count - 1; index++) {
            float elapsedTime = 0f;
            
            while (elapsedTime < timePerFrame) {
                // move
                transform.position = Vector3.Lerp(
                    _recordedFrames[index].Position,
                    _recordedFrames[index + 1].Position,
                    elapsedTime / timePerFrame
                );
                
                // rotate
                transform.rotation = Quaternion.Lerp(
                    _recordedFrames[index].Rotation,
                    _recordedFrames[index + 1].Rotation,
                    elapsedTime / timePerFrame
                );
                
                elapsedTime += Time.deltaTime * replaySpeed;
                yield return null;
            }
        }
        
        // Continue replay with next camera in list
        StartCoroutine(Replay(cameraIndex + 1));
    }
    
    private void EndReplay() {
        // Restore ball physics
        BallPhysicsEnabled(true);
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        
        DisableAllReplayCameras();
        MainCameraEnabled(true);

        Debug.Log("Switched back to Main Camera");
    }

    private void EnableReplayCamera(ReplayCamera replayCamera) {
        MainCameraEnabled(false);
        DisableAllReplayCameras();
        
        // only enable the current replay camera
        replayCamera.camera.gameObject.SetActive(true);
        replayCamera.follower.StartFollowing(gameObject);
    }

    private void DisableAllReplayCameras() {
        foreach (ReplayCamera rc in _replayCameras) {
            rc.camera.gameObject.SetActive(false);
            rc.follower.StopFollowing();
        }
    }

    private void MainCameraEnabled(bool isEnabled) {
        _mainCamera.gameObject.SetActive(isEnabled);
    }

    private void BallPhysicsEnabled(bool isEnabled) {
        _rb.isKinematic = !isEnabled;
        
        Collider ballCollider = GetComponent<Collider>();
        if (ballCollider != null) {
            ballCollider.enabled = isEnabled;
        }
    }
    
    private void OnTriggerEnter(Collider other) {
        switch (other.tag) {
            case "FirstTrigger": {
                _hasHitFirstTrigger = true;
                Debug.Log("Ball hit first trigger!");
                break;
            }
            case "SecondTrigger": {
                if (!_hasHitFirstTrigger) break;
                
                Debug.Log("Ball hit second trigger after first! Starting replay...");
                _hasHitFirstTrigger = false; // Reset for the next shot
                StopRecordingAndPlayReplay();
                break;
            }
        }
    }
    
    // Struct to store position and rotation at each frame
    private struct FrameData {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public FrameData(Vector3 pos, Quaternion rot) {
            Position = pos;
            Rotation = rot;
        }
    }

    private struct ReplayCamera {
        public readonly Camera camera;
        public readonly ReplayCameraFollow follower;
        
        public ReplayCamera(Camera camera, ReplayCameraFollow follower) {
            this.camera = camera;
            this.follower = follower;
        }
    }
}