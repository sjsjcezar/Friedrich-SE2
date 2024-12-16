using UnityEngine;

public class TimeConfig : MonoBehaviour
{
    [Header("Initial Time Settings")]
    [Range(0, 23)]
    [Tooltip("Starting hour in 24-hour format (0-23).")]
    public int startingHour = 8;

    [Range(0, 59)]
    [Tooltip("Starting minute (0-59).")]
    public int startingMinute = 0;

    [Range(1, 12)]
    [Tooltip("Starting month (1-12).")]
    public int startingMonth = 1;

    // The maximum day will be determined based on the starting month
    [Tooltip("Starting day based on the starting month.")]
    public int startingDay = 1;

    [Range(0.1f, 10f)]
    [Tooltip("Speed of time progression.")]
    public float timeTick = 1f; // Speed of time progression

    private void Start()
    {
        // Initialize global time with inspector settings

        // Clamp and set starting hour
        GlobalVariables.hours = Mathf.Clamp(startingHour, 0, 23);

        // Clamp and set starting minute
        GlobalVariables.minutes = Mathf.Clamp(startingMinute, 0, 59);

        // Clamp and set starting month
        GlobalVariables.month = Mathf.Clamp(startingMonth, 1, 12);

        // Determine the maximum days in the selected month
        int maxDay = GlobalVariables.monthDays[GlobalVariables.month - 1];

        // Clamp and set starting day based on the month
        GlobalVariables.days = Mathf.Clamp(startingDay, 1, maxDay);

        // Set the time tick
        GlobalVariables.timeTick = Mathf.Clamp(timeTick, 0.1f, 10f);
    }
}