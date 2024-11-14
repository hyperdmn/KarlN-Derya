using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Include this for TextMeshPro support

public class TransitionController : MonoBehaviour
{
    public GameObject sceneCanvas;                    // Reference to the Scene Transition Canvas
    public GameObject timerCanvas;                     // Reference to the Timer Canvas
    public float transitionDuration = 5f;              // Duration of the transition animation
    public Timer timerScript;                           // Reference to the Timer script
    public Animator transitionAnimator;                 // Animator for Scene Transition
    public DayManager dayManager;                       // Reference to the DayManager script

    private Text dayText;                              // Text component for displaying the day
    private int currentDay = 1;                        // Current day tracker

    private void Start()
    {
        dayText = sceneCanvas.GetComponentInChildren<Text>(); // Fetch the Text component
        dayManager = FindObjectOfType<DayManager>();         // Automatically find the DayManager in the scene
        UpdateDayText();                                     // Initialize the day text
        StartCoroutine(TransitionSequence());                // Start the transition sequence
    }

    private IEnumerator TransitionSequence()
    {
        sceneCanvas.SetActive(true);                         // Show the scene canvas
        UpdateDayText();                                    // Update the day text at the beginning

        yield return new WaitForSeconds(transitionDuration); // Wait for the transition duration

        sceneCanvas.SetActive(false);                        // Hide the scene canvas
        timerCanvas.SetActive(true);                         // Show the timer canvas

        timerScript.ResetTimer();                            // Reset the timer at the beginning

        StartCoroutine(DayCycle());                          // Start the day cycle coroutine
    }

    private IEnumerator DayCycle()
    {
        while (true)
        {
            // Wait for the full duration of the timer before resetting
            yield return new WaitForSeconds(timerScript.dayDuration);

            // Prepare for day transition
            timerCanvas.SetActive(false);
            currentDay++; // Increment the day first

            // Update the day text in DayManager
            dayManager.AdvanceDay(); // Call the DayManager to update the day
            UpdateDayText();         // Update the day text with the new day number before transition

            // Show day transition and update day text
            yield return StartCoroutine(ShowDayTransition());

            // Reset timer and re-enable the timer display
            timerScript.ResetTimer();
            timerCanvas.SetActive(true);
        }
    }

    private IEnumerator ShowDayTransition()
    {
        UpdateDayText();                                  // Update day text before transition
        sceneCanvas.SetActive(true);                      // Show the scene canvas
        transitionAnimator.SetTrigger("StartTransition"); // Trigger the transition animation

        yield return new WaitForSeconds(transitionDuration); // Wait for the transition duration

        sceneCanvas.SetActive(false);                      // Hide the scene canvas after transition
    }

    private void UpdateDayText()
    {
        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}";          // Set the day text
        }
    }
}
