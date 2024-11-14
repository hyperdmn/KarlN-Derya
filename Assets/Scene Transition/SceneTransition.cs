using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Animator transitionAnimator; // Assign this in the Inspector
    public float transitionDuration = 5f; // Adjust as needed

    private void Start()
    {
        // Start the transition on play
        StartCoroutine(PlayTransition());
    }

    private IEnumerator PlayTransition()
    {
        // Trigger the transition animation
        transitionAnimator.SetTrigger("StartTransition");

        // Wait for the duration of the transition animation
        yield return new WaitForSeconds(transitionDuration);

        // Load the gameplay scene (replace with your gameplay scene name)
        SceneManager.LoadScene("GameplayScene");
    }
}
