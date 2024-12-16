using UnityEngine;
using System.Collections;

public class NPCBehavior : MonoBehaviour
{
    [Tooltip("Assign the NPC's schedule here")]
    public NPCSchedule npcSchedule;

    private bool isActive = false;
    private bool scheduleEnding = false;
    private int currentScheduleIndex = 0;
    private Coroutine currentMovementCoroutine = null;

    // Tracking the current waypoint index
    private int currentWaypointIndex = -1;
    private int loopsCompleted = 0;
    private int requiredLoops = 2; // Current loop + one additional loop

    private void OnEnable()
    {
        ScheduleManager.OnScheduleStart += HandleScheduleStart;
        ScheduleManager.OnScheduleEnd += HandleScheduleEnd;
    }

    private void OnDisable()
    {
        ScheduleManager.OnScheduleStart -= HandleScheduleStart;
        ScheduleManager.OnScheduleEnd -= HandleScheduleEnd;
    }

    private void HandleScheduleStart(NPCSchedule schedule, int scheduleIndex)
    {
        if (schedule == npcSchedule)
        {
            Debug.Log($"{gameObject.name} Schedule Start: Block {scheduleIndex}");
            isActive = true;
            scheduleEnding = false;
            currentScheduleIndex = scheduleIndex;
            loopsCompleted = 0;
            currentMovementCoroutine = StartCoroutine(FollowSchedule(schedule.scheduleBlocks[scheduleIndex]));
        }
    }

    private void HandleScheduleEnd(NPCSchedule schedule, int scheduleIndex)
    {
        if (schedule == npcSchedule && isActive)
        {
            Debug.Log($"{gameObject.name} Schedule End Triggered: Block {scheduleIndex}");
            isActive = false;
            scheduleEnding = true;

            // Retrieve the current schedule block
            var block = schedule.scheduleBlocks[scheduleIndex];

            // If backtracking is disabled, set loopsCompleted to requiredLoops to trigger return
            if (!block.canBacktrack)
            {
                loopsCompleted = requiredLoops;
            }
        }
    }

    private IEnumerator FollowSchedule(NPCSchedule.ScheduleBlock block)
    {
        bool canBacktrack = block.canBacktrack;
        int direction = 1; // 1 for forward, -1 for backward
        int index = (currentWaypointIndex == -1) ? 0 : currentWaypointIndex;

        while (isActive || (scheduleEnding && loopsCompleted < requiredLoops))
        {
            if (index >= block.waypointNames.Length || index < 0)
            {
                Debug.LogError($"{gameObject.name}: Waypoint index {index} is out of bounds.");
                yield break;
            }

            string waypointName = block.waypointNames[index];
            Transform waypoint = WaypointManager.Instance.GetWaypointByName(waypointName);

            if (waypoint == null)
            {
                Debug.LogError($"{gameObject.name}: Waypoint '{waypointName}' not found. Skipping.");
            }
            else
            {
             //   Debug.Log($"{gameObject.name} is traversing to waypoint '{waypointName}'.");
                yield return StartCoroutine(MoveToPosition(waypoint.position));
              //  Debug.Log($"{gameObject.name} arrived at waypoint '{waypointName}'.");
                currentWaypointIndex = index;
            }

            // Only continue if still active or if schedule is ending and required loops aren't completed
            if (!isActive && !scheduleEnding)
                break;

            bool isEndOfPath = false;

            index += direction;

            if (index >= block.waypointNames.Length)
            {
                if (canBacktrack)
                {
                    direction = -1;
                    index = block.waypointNames.Length - 2;
                    isEndOfPath = true;
                    loopsCompleted++;
                    Debug.Log($"{gameObject.name} reached the end of the path. Backtracking.");
                }
                else
                {
                    // Stay at the last waypoint
                    index = block.waypointNames.Length - 1;
                //    Debug.Log($"{gameObject.name} reached the end of the path and will stay there as backtracking is disabled.");
                }
            }
            else if (index < 0)
            {
                if (canBacktrack)
                {
                    direction = 1;
                    index = 1;
                    isEndOfPath = true;
                    loopsCompleted++;
                    Debug.Log($"{gameObject.name} reached the start of the path. Moving forward.");
                }
                else
                {
                    // Stay at the first waypoint
                    index = 0;
                    Debug.Log($"{gameObject.name} reached the start of the path and will stay there as backtracking is disabled.");
                }
            }

            if (isEndOfPath)
            {
                // Wait for 3 seconds at the end before changing direction
                Debug.Log($"{gameObject.name} is waiting for 3 seconds before changing direction.");
                yield return new WaitForSeconds(3f);
            }

            // If schedule is ending and required loops are completed, initiate return
            if (scheduleEnding && loopsCompleted >= requiredLoops)
            {
                Debug.Log($"{gameObject.name} has completed the required loops. Initiating return to original position.");
                yield return StartCoroutine(ReturnToOriginalPosition());
                break;
            }

            yield return null;
        }

        // Coroutine ends here. Cleanup if needed.
        if (!isActive && !scheduleEnding)
        {
            Debug.Log($"{gameObject.name} has completed its active schedule.");
            currentMovementCoroutine = null;
        }
    }

    private IEnumerator ReturnToOriginalPosition()
    {
        if (npcSchedule.scheduleBlocks.Length == 0 ||
            npcSchedule.scheduleBlocks[currentScheduleIndex].waypointNames.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No waypoints defined in the schedule.");
            yield break;
        }

        string originalWaypointName = npcSchedule.scheduleBlocks[currentScheduleIndex].waypointNames[0];
        Transform originalWaypoint = WaypointManager.Instance.GetWaypointByName(originalWaypointName);

        if (originalWaypoint == null)
        {
            Debug.LogError($"{gameObject.name}: Original waypoint '{originalWaypointName}' not found.");
            yield break;
        }

        // Create a list of waypoints to traverse back to the first waypoint
        NPCSchedule.ScheduleBlock block = npcSchedule.scheduleBlocks[currentScheduleIndex];
        int targetIndex = 0;
        int step = -1; // Moving backwards

        while (currentWaypointIndex > targetIndex)
        {
            currentWaypointIndex += step;
            string waypointName = block.waypointNames[currentWaypointIndex];
            Transform waypoint = WaypointManager.Instance.GetWaypointByName(waypointName);

            if (waypoint == null)
            {
                Debug.LogError($"{gameObject.name}: Waypoint '{waypointName}' not found during backtracking. Skipping.");
                continue;
            }

            Debug.Log($"{gameObject.name} is backtracking to waypoint '{waypointName}'.");
            yield return StartCoroutine(MoveToPosition(waypoint.position));
            Debug.Log($"{gameObject.name} arrived at waypoint '{waypointName}'.");
        }

        Debug.Log($"{gameObject.name} has returned to the original position '{originalWaypointName}'.");
        currentMovementCoroutine = null;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float speed = 6f; // Adjust as needed

        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition;
    }
}