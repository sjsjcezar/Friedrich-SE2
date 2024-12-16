using System;
using UnityEngine;

public class ScheduleManager : MonoBehaviour
{
    public static event Action<NPCSchedule, int> OnScheduleStart;
    public static event Action<NPCSchedule, int> OnScheduleEnd;

    [Tooltip("List of all NPC schedules in the game")]
    public NPCSchedule[] allSchedules;

    private float previousTime;

    private void Start()
    {
        previousTime = GlobalVariables.hours + (GlobalVariables.minutes / 60f);
    }

    private void Update()
    {
        // Current time in decimal
        float currentTime = GlobalVariables.hours + (GlobalVariables.minutes / 60f);

        foreach (var schedule in allSchedules)
        {
            for (int i = 0; i < schedule.scheduleBlocks.Length; i++)
            {
                var block = schedule.scheduleBlocks[i];

                // Only process schedule blocks that match the current day of the week
                if (block.dayOfWeek != GlobalVariables.dayOfWeek)
                    continue;

                // Check for schedule start
                if (previousTime < block.startTime && currentTime >= block.startTime)
                {
                    OnScheduleStart?.Invoke(schedule, i);
                }

                // Check for schedule end
                if (previousTime < block.endTime && currentTime >= block.endTime)
                {
                    OnScheduleEnd?.Invoke(schedule, i);
                }
            }
        }

        previousTime = currentTime;
    }
}