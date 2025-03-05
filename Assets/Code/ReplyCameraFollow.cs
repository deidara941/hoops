using UnityEngine;

public class ReplayCameraFollow : MonoBehaviour {
    public Transform ball;
    public Vector3 offset = new Vector3(0, 2, -5);

    private bool _isFollowing = false;

    private void Update() {
        if (!_isFollowing) return;
        
        if (ball) {
            Vector3 targetPosition = ball.position + offset;
            transform.position = targetPosition;
            transform.LookAt(ball.position); 
        }
    }

    public void StartFollowing() {
        _isFollowing = true;
    }

    public void StopFollowing() {
        _isFollowing = false;
    }
}
