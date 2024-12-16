using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEntrance : MonoBehaviour
{
    public string lastExitName;

    void Start()
    {
        // Check if a teleport spawn is set
        if (!string.IsNullOrEmpty(GlobalVariables.lastTeleportSpawnID))
        {
            // Teleport spawn is active; do not override player position
            return;
        }

        // Proceed with default entrance logic
        if (PlayerPrefs.GetString("LastExitName") == lastExitName)
        {
            PlayerMovement.instance.transform.position = transform.position;
            PlayerMovement.instance.transform.eulerAngles = transform.eulerAngles;

            // Optionally, clear the LastExitName to prevent repeated positioning
            PlayerPrefs.SetString("LastExitName", "");
        }
    }
}