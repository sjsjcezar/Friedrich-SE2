using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCSchedule", menuName = "NPC Schedule")]
public class NPCSchedule : ScriptableObject
{
    [System.Serializable]
    public class ScheduleBlock
    {
        [Header("Schedule Timing")]
        [Tooltip("Start time in 24-hour format (e.g., 8.5 for 8:30 AM)")]
        public float startTime;

        [Tooltip("End time in 24-hour format (e.g., 17.75 for 5:45 PM)")]
        public float endTime;

        [Header("Day of the Week")]
        [Tooltip("Day of the week for this schedule block (1=Mondas, 2=Loredas, ..., 7=Sundar)")]
        [Range(1,7)]
        public int dayOfWeek;

        [Header("Waypoints")]
        [Tooltip("List of waypoint names the NPC will follow")]
        public string[] waypointNames;

        [Header("Backtracking Settings")]
        [Tooltip("Determines if the NPC can backtrack during schedule loops.")]
        public bool canBacktrack = true; // Default to true
    }

    [Tooltip("List of schedule blocks for the NPC")]
    public ScheduleBlock[] scheduleBlocks;
}