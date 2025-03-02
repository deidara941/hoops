using System.Collections;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {

    public TMP_Text scoreText;

    private void Start() {
        scoreText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Ball")) {
            StartCoroutine(ShowScoreEffect());
        }
    }

    private IEnumerator ShowScoreEffect() {
        scoreText.gameObject.SetActive(true);
        Color originalColor = scoreText.color;
        scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        Vector3 originalPosition = scoreText.transform.localPosition;

        float fadeDuration = 1f;
        float displayTime = 1.0f;
        float shakeAmount = 0.2f;

        // Fade in + Shake effect
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float alpha = t / fadeDuration;
            scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            scoreText.transform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * shakeAmount;

            yield return null;
        }

        // Ensure final position is correct
        scoreText.transform.localPosition = originalPosition;

        // Hold the text visible
        yield return new WaitForSeconds(displayTime);

        // Fade out
        for (float t = 0; t < fadeDuration; t += Time.deltaTime) {
            float alpha = 1 - (t / fadeDuration);
            scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Hide the text
        scoreText.gameObject.SetActive(false);
    }
}