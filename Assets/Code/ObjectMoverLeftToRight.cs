using UnityEngine;

public class ObjectMoverLeftToRight : MonoBehaviour
{
    [SerializeField] private float speed = 6f; 
    [SerializeField] private float distance = 10f;

    private Vector3 _startPosition;
    private int _direction = 1;
    private float _targetX;
    private bool _initialized = false;

    private void FixedUpdate() {
        if (!GameController.Instance.LevelLoaded) return; 
        
        if (!_initialized) {
            _startPosition = transform.position; 
            _targetX = _startPosition.x - distance;
            _initialized = true;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(_targetX, _startPosition.y, _startPosition.z), speed * Time.fixedDeltaTime);

        if (Mathf.Approximately(transform.position.x, _targetX)) {
            _direction *= -1;
            _targetX = _startPosition.x + (_direction * distance);
        }
    }
}