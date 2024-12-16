using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemySpawnPoint", menuName = "SpawnPoints/EnemySpawnPoint")]
public class EnemySpawnPoint : ScriptableObject
{
    public string enemyID;
    public CharacterType enemyType;
    public Vector3 spawnPosition;
}