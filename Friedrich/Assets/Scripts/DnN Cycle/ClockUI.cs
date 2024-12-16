using UnityEngine;
using TMPro;

public class ClockUI : MonoBehaviour
{
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI dayText;
    public TextMeshProUGUI dateText;
    public TextMeshProUGUI monthText;
    public TextMeshProUGUI yearText;

    private void Update()
    {
        if (clockText != null)
        {
            // Determine AM or PM
            string period = GlobalVariables.hours >= 12 ? "PM" : "AM";

            // Convert 24-hour format to 12-hour format
            int displayHour = GlobalVariables.hours % 12;
            if (displayHour == 0)
                displayHour = 12;

            // Format the time string with leading zeros
            string timeString = string.Format("{0:00}:{1:00} {2}", displayHour, GlobalVariables.minutes, period);

            // Update the clock UI
            clockText.text = timeString;
        }

        if (dayText != null)
        {
            // Display day of the week
            string dayName = GlobalVariables.dayOfWeek >=1 && GlobalVariables.dayOfWeek <=7
                ? GlobalVariables.dayNames[GlobalVariables.dayOfWeek - 1]
                : "Unknown";

            dayText.text = dayName;
        }

        if (dateText != null)
        {
            // Display current day
            dateText.text = $"{GlobalVariables.days}";
        }

        if (monthText != null)
        {
            // Display current month
            string monthName = GlobalVariables.month >=1 && GlobalVariables.month <=12
                ? GlobalVariables.monthNames[GlobalVariables.month - 1]
                : "Unknown";

            monthText.text = monthName;
        }

        if (yearText != null)
        {
            // Display current year
            yearText.text = $"Year {GlobalVariables.year}";
        }
    }
}