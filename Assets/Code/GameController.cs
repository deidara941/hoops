using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {
    
    public static GameController Instance { get; private set; } // 🔹 Singleton instance

    [SerializeField] private Transform mainPanel;
    [SerializeField] private List<GameObject> levels;
    [SerializeField] private float rotationDuration = 2f;
    [SerializeField] private AnimationCurve rotationCurve;

    [SerializeField] private GameObject player;
    
    public bool LevelLoaded { get; private set; } = false;

    private int _currentLevel = 0;
    private bool _isRotating = false;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void ChangeLevel(int levelIndex) {
        LevelLoaded = false;
        player.SetActive(false);
        levels[levelIndex].SetActive(true);
        StartCoroutine(RotateAndSwitchLevel(levelIndex));
    }

    private IEnumerator RotateAndSwitchLevel(int newLevelIndex) {
        if (_isRotating) yield break; 
        _isRotating = true;

        Quaternion startRotation = mainPanel.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 180);
        
        float elapsed = 0f;

        while (elapsed < rotationDuration) {
            float t = elapsed / rotationDuration;
            float curveValue = rotationCurve.Evaluate(t);
            mainPanel.rotation = Quaternion.Slerp(startRotation, targetRotation, curveValue);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainPanel.rotation = targetRotation;

        levels[_currentLevel].SetActive(false);
        
        _currentLevel = newLevelIndex;
        LevelLoaded = true;
        player.SetActive(true);

        _isRotating = false; 
    }
}