using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public CharacterType characterType;
    public int baseDamage;
    public int skillDamage;

    public int Vitality;
    public int Power;
    public int Fortitude;
    public int speed;

    public int VitalityValue; // For swapping values so that enemy does not have 0 vitality after a battle

    public float PowerMultiplier = 1.0f;

    // Updated to calculate damage based on player stats instead
    public int CalculateDamage(int playerBaseDamage, int playerPower, float playerPowerMultiplier, int enemyDefense)
    {
        // Calculation now uses player's base damage and stats
        int damage = playerBaseDamage + Mathf.RoundToInt(playerPower * playerPowerMultiplier) - Fortitude;
        int minimumDamage = Mathf.RoundToInt(0.1f * playerBaseDamage); // Ensures minimum damage of 10% of player baseDamage

        // Return either the calculated damage or the minimum damage, whichever is higher
        return Mathf.Max(damage, minimumDamage);
    }
}
