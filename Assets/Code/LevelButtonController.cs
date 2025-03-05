using System;
using UnityEngine;

public class LevelButtonController : MonoBehaviour
{
    [SerializeField] private int levelIndex;
    
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            GameController.Instance.ChangeLevel(levelIndex);
        }
    }
}
