using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float initialTime = 180f; // Set initial time to 3 minutes (180 seconds)
    private float weekendTime = 300f;           // Set weekend time to 5 minutes (300 seconds)
    public float dayDuration => initialTime;    // Day duration property

    private float remainingTime;
    private bool timerEnded = false;
    
    public DayManager dayManager;               // Reference to the DayManager script

    void Start() 
    {
        if (dayManager != null)
        {
            dayManager.OnDayAdvance += ResetTimer; // Subscribe to day changes
        }
        
        SetInitialTime();                       // Set initial time based on the current day
        remainingTime = initialTime;
        timerEnded = false;
    }

    void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            // Change color to red when the timer reaches 10 seconds
            if (remainingTime <= 10) 
            {
                timerText.color = Color.red;
            } 
            else 
            {
                timerText.color = Color.white;
            }

            // Display the timer in "MM : SS" format
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0:00} : {1:00}", minutes, seconds);
        } 
        else if (!timerEnded) // Only call TimerEnded once
        {
            TimerEnded();
        }
    }

    void TimerEnded()
    {
        Debug.Log("Timer has ended. Resetting to initial time.");
        timerEnded = true;
    }

    public void ResetTimer()
    {
        SetInitialTime();               // Reset the initial time based on the current day
        remainingTime = initialTime;
        timerEnded = false;
    }

    private void SetInitialTime()
    {
        if (dayManager != null)
        {
            // Set to 5 minutes on weekends (Day 6 and Day 7), otherwise use default time
            if (dayManager.currentDay == 6 || dayManager.currentDay == 7)
            {
                initialTime = weekendTime;
            }
            else
            {
                initialTime = 180f; // Reset to 3 minutes on weekdays
            }
        }
    }
}
