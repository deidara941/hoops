using UnityEngine;

public class ReplayCameraFollow : MonoBehaviour {

    [SerializeField] private bool movesWithTarget;
    
    [Tooltip("Only used when movesWithTarget = true")]
    [SerializeField] private Vector3 offsetFromTarget = new(0, 2, -5);

    private Transform _followTarget;
    private bool _isFollowing = false;

    private void Update() {
        if (!_isFollowing) return;

        if (movesWithTarget) {
            Vector3 targetPosition = _followTarget.position + offsetFromTarget;
            transform.position = targetPosition;
        }
        transform.LookAt(_followTarget.position); 
    }

    public void StartFollowing(GameObject followTarget) {
        _followTarget = followTarget.transform;
        _isFollowing = true;
    }

    public void StopFollowing() {
        _isFollowing = false;
        _followTarget = null;
    }
}
