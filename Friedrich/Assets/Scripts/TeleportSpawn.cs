using UnityEngine;

public class TeleportSpawnPoint : MonoBehaviour
{
    public string spawnPointID;

    void Start()
    {
        if (GlobalVariables.lastTeleportSpawnID == spawnPointID)
        {
            if (PlayerMovement.instance != null)
            {
                PlayerMovement.instance.transform.position = transform.position;
                PlayerMovement.instance.transform.eulerAngles = transform.eulerAngles;

                // Clear the spawn ID after using it
                GlobalVariables.lastTeleportSpawnID = "";

                // Clear LastExitName to prevent SceneEntrance from overriding
                PlayerPrefs.SetString("LastExitName", "");
            }
            else
            {
                Debug.LogError("PlayerMovement.instance is null!");
            }
            Debug.Log("Teleported to " + spawnPointID);
        }
    }
}