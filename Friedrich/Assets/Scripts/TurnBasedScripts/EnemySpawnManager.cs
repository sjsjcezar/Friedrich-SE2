using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    public List<EnemySpawnPoint> spawnPoints;

    void Start()
    {
        // Only initialize the spawn points and enemy statuses
        foreach (var spawnPoint in spawnPoints)
        {
            // Initialize enemy status if not already set
            if (!GlobalVariables.enemyStatus.ContainsKey(spawnPoint.enemyID))
            {
                GlobalVariables.enemyStatus.Add(spawnPoint.enemyID, false);
            }

            // Add spawn point to GlobalVariables if not already present
            if (!GlobalVariables.enemySpawnPoints.Contains(spawnPoint))
            {
                GlobalVariables.enemySpawnPoints.Add(spawnPoint);
            }
        }
    }
}