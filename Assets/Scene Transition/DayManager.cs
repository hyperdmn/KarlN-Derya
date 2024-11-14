using UnityEngine;
using TMPro;
using System;

public class DayManager : MonoBehaviour
{
    public TextMeshProUGUI dayText;  // Reference to the TextMeshProUGUI component
    public int currentDay = 1;       // Tracks the current day

    public event Action OnDayAdvance; // Event to notify day change

    private void Start()
    {
        UpdateDayText();               // Initialize day text at the start
    }

    public void AdvanceDay()
    {
        currentDay++;                  // Increment the current day
        if (currentDay > 7)            // If the day exceeds 7, reset it to 1
        {
            currentDay = 1;
        }                
        UpdateDayText();               // Update the displayed day text
        OnDayAdvance?.Invoke();        // Trigger the day change event
    }

    private void UpdateDayText()
    {
        if (dayText != null)
        {
            dayText.text = $"Day {currentDay}"; // This will update to "Day 2", "Day 3", etc.
        }
    }
}
