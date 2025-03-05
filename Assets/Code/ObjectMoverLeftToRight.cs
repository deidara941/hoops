using UnityEngine;

public class ObjectMoverLeftToRight : MonoBehaviour
{
    [SerializeField] private float speed = 5f; 
    [SerializeField] private float distance = 30f; 

    private Vector3 _startPosition;
    private int _direction = 1;
    private float _targetX;

    private void Start() {
        _startPosition = transform.position;
        _targetX = _startPosition.x - distance;
    }

    private void FixedUpdate() {
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(_targetX, _startPosition.y, _startPosition.z), speed);

        if (Mathf.Approximately(transform.position.x, _targetX)) {
            // Switch direction
            _direction *= -1;
            _targetX = _startPosition.x + (_direction * distance);
        }
    }
}
