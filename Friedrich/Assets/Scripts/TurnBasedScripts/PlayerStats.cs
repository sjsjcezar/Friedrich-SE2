   using UnityEngine;

   [CreateAssetMenu(fileName = "NewPlayerStats", menuName = "Player Stats")]
   public class PlayerStats : ScriptableObject
   {
    public CharacterType characterType;
    public int baseDamage;
    public int currentDamage;
    public int Vitality; // Current vitality value HP
    public int maxVitality; // New maximum health value MAX HP
    public int vitalityStat; // New stat for scaling Max Vitality
    public int Power;
    public int basePower;
    public int Fortitude;
    public int Intelligence; // Current intelligence value
    public int maxIntelligence; // Maximum intelligence value
    public int ArcaneStat; // New stat for scaling Intelligence
    public float PowerMultiplier = 1.0f;

    public int Level = 1;
    public int currentCrystalsHeld = 0;
    public int crystalsNeededForNextLevel = 400; // Initial requirement



    private int initialBaseDamage;
    private int initialVitalityStat;
    private int initialPower;
    private int initialFortitude;
    private int initialArcaneStat;

    public WeaponSO equippedWeapon; 

    public void EquipWeapon(WeaponSO weapon)
    {
        if (equippedWeapon != null)
        {
            UnequipWeapon();
        }
        equippedWeapon = weapon;
        currentDamage = baseDamage + weapon.damage;
        Power += weapon.powerBonus;  // Simply add the bonus
        Debug.Log($"Equipped {weapon.weaponName}. Current Damage: {currentDamage}, Power: {Power}");
    }

    public void UnequipWeapon()
    {
        if (equippedWeapon != null)
        {
            Debug.Log($"Unequipped {equippedWeapon.weaponName}");
            Power -= equippedWeapon.powerBonus;  // Remove the bonus
            equippedWeapon = null;
            currentDamage = baseDamage;
        }
    }


    public void InitializeVitals(int initialVitality, int initialArcaneStat)
    {
        Vitality = initialVitality;
        maxVitality = vitalityStat * 3;
        ArcaneStat = initialArcaneStat;
        maxIntelligence = CalculateMaxIntelligence();

        currentDamage = baseDamage;
    }

        /// <summary>
        /// Adds Fate Crystals to the player.
        /// </summary>
        /// <param name="amount">Number of crystals to add.</param>
        public void AddFateCrystals(int amount)
        {
            currentCrystalsHeld += amount;
            Debug.Log($"Added {amount} Fate Crystals. Total Crystals: {currentCrystalsHeld}");
        }

        /// <summary>
        /// Attempts to level up the player if enough crystals are held.
        /// </summary>
        public bool TryLevelUp()
        {
            if (currentCrystalsHeld >= crystalsNeededForNextLevel)
            {
                LevelUp();
                return true;
            }
            Debug.Log("Not enough Fate Crystals to level up.");
            return false;
        }

    private void LevelUp()
    {
        Level++;
        currentCrystalsHeld -= crystalsNeededForNextLevel;
        crystalsNeededForNextLevel += Mathf.RoundToInt(crystalsNeededForNextLevel * 0.3f);
        
        // Store weapon bonus if equipped
        int weaponBonus = equippedWeapon != null ? equippedWeapon.powerBonus : 0;
        
        // Remove weapon bonus before adding level stats
        if (equippedWeapon != null)
        {
            Power -= equippedWeapon.powerBonus;
        }
        
        // Increase base stats
        vitalityStat += 5;
        Power += 5;
        Fortitude += 5;
        ArcaneStat += 5;
        
        // Recalculate dependent stats
        maxVitality = vitalityStat * 3;
        maxIntelligence = CalculateMaxIntelligence();
        
        // Re-add weapon bonus if equipped
        if (equippedWeapon != null)
        {
            Power += weaponBonus;
        }

        Debug.Log($"Player leveled up to Level {Level}! New stats - VitalityStat: {vitalityStat}, Power: {Power} (Base: {Power - weaponBonus} + Weapon: {weaponBonus}), Fortitude: {Fortitude}, ArcaneStat: {ArcaneStat}");
    }
    
       public bool HasEnoughIntelligence(int cost)
       {
           return Intelligence >= cost;
       }

       public void UseIntelligence(int amount)
       {
           Intelligence = Mathf.Max(0, Intelligence - amount);
           // Update MP/Intelligence UI here
           FindObjectOfType<HealthManager>()?.UpdatePlayerIntelligence(Intelligence, maxIntelligence);
           Debug.Log($"Used {amount} Intelligence. Current Intelligence: {Intelligence}/{maxIntelligence}");
       }

       public void RestoreIntelligence(int amount)
       {
           Intelligence = Mathf.Min(maxIntelligence, Intelligence + amount);
           FindObjectOfType<HealthManager>()?.UpdatePlayerIntelligence(Intelligence, maxIntelligence);
           Debug.Log($"Restored {amount} Intelligence. Current Intelligence: {Intelligence}/{maxIntelligence}");
       }

       public int CalculateDamage(int baseDamage, int power, float powerMultiplier, int enemyDefense)
       {
           int damage = baseDamage + Mathf.RoundToInt(power * powerMultiplier) - enemyDefense;
           int minimumDamage = Mathf.RoundToInt(0.1f * baseDamage);
           return Mathf.Max(damage, minimumDamage);
       }

       public void TakeDamage(int damage)
       {
           Vitality -= damage;
           if (Vitality < 0) Vitality = 0;
           FindObjectOfType<HealthManager>()?.UpdatePlayerHealth(Vitality, maxVitality); // Update slider
           Debug.Log($"Player took {damage} damage. Current Vitality: {Vitality}/{maxVitality}");
       }

       /// <summary>
       /// Calculates damage mitigation based on Fortitude for Physical skills.
       /// </summary>
       /// <param name="incomingDamage">The damage to be mitigated.</param>
       /// <returns>The mitigated damage.</returns>
       public int MitigatePhysicalDamage(int incomingDamage)
       {
           // Example formula: Damage reduction = Fortitude * 0.5%
           float mitigationRate = Fortitude * 0.005f;
           float mitigatedDamage = incomingDamage * (1 - mitigationRate);
           return Mathf.Max(Mathf.RoundToInt(mitigatedDamage), 0); // Ensure damage doesn't go negative
       }

       /// <summary>
       /// Calculates damage mitigation based on ArcaneStat for Magical skills.
       /// </summary>
       /// <param name="incomingDamage">The damage to be mitigated.</param>
       /// <returns>The mitigated damage.</returns>
       public int MitigateMagicalDamage(int incomingDamage)
       {
           // Example formula: Damage reduction = ArcaneStat * 0.5%
           float mitigationRate = ArcaneStat * 0.005f;
           float mitigatedDamage = incomingDamage * (1 - mitigationRate);
           return Mathf.Max(Mathf.RoundToInt(mitigatedDamage), 0); // Ensure damage doesn't go negative
       }

       /// <summary>
       /// Calculates the maximum Intelligence based on ArcaneStat.
       /// </summary>
       /// <returns>Calculated maxIntelligence.</returns>
       public int CalculateMaxIntelligence()
       {
           return ArcaneStat * 3;
       }

       /// <summary>
       /// Updates the ArcaneStat and recalculates maxIntelligence accordingly.
       /// </summary>
       /// <param name="newArcaneStat">The new ArcaneStat value.</param>
       public void UpdateArcaneStat(int newArcaneStat)
       {
           ArcaneStat = newArcaneStat;
           maxIntelligence = CalculateMaxIntelligence();
           Intelligence = Mathf.Min(Intelligence, maxIntelligence);
           FindObjectOfType<HealthManager>()?.UpdatePlayerIntelligence(Intelligence, maxIntelligence);
           Debug.Log($"ArcaneStat updated to {ArcaneStat}. Max Intelligence is now {maxIntelligence}.");
       }

    private void OnValidate()
    {
        // Initialize base stats based on CharacterType at Level 1
        if (Level == 1)
        {
            switch (characterType)
            {
                case CharacterType.Warrior:
                    baseDamage = 20;
                    vitalityStat = 75;
                    Power = 40;
                    Fortitude = 45;
                    ArcaneStat = 35;
                    break;
                case CharacterType.Mage:
                    baseDamage = 15; // Assuming Fire Staff Base Damage is treated as baseDamage
                    vitalityStat = 60;
                    Power = 30;
                    Fortitude = 35;
                    ArcaneStat = 70;
                    break;
                default:
                    Debug.LogWarning("Unknown CharacterType. Please set to Warrior or Mage.");
                    break;
            }

            // Recalculate dependent stats
            maxVitality = vitalityStat * 3;
         //   Intelligence = CalculateMaxIntelligence();
          //  maxIntelligence = Intelligence;

            // Initialize crystals needed for next level
            crystalsNeededForNextLevel = Mathf.RoundToInt(400 * Mathf.Pow(1.3f, Level - 1));
        }
        else
        {
            // If Level has been changed manually, scale stats accordingly
            int levelsGained = Level - 1;

            // Ensure stats scale correctly only when levels increase
            if (levelsGained > 0)
            {
                vitalityStat = GetBaseVitality() + (4 * levelsGained);
                Power = GetBasePower() + (4 * levelsGained);
                Fortitude = GetBaseFortitude() + (4 * levelsGained);
                ArcaneStat = GetBaseArcaneStat() + (4 * levelsGained);

                // Recalculate dependent stats
                maxVitality = vitalityStat * 3;
               // Intelligence = CalculateMaxIntelligence();
              //  maxIntelligence = Intelligence;

                // Recalculate crystals needed for next level
                crystalsNeededForNextLevel = Mathf.RoundToInt(crystalsNeededForNextLevel * Mathf.Pow(1.3f, levelsGained));
            }
        }
            equippedWeapon = null;
            currentDamage = baseDamage;
            Power = GetBasePower() + (5 * (Level - 1)); // Reset Power to base value without weapon bonus

            int basePowerForLevel = GetBasePower() + (5 * (Level - 1));
            Power = basePowerForLevel;
    
        if (equippedWeapon == null)
        {
            currentDamage = baseDamage;
        }
        else
        {
            currentDamage = baseDamage + equippedWeapon.damage;
        }

        if (equippedWeapon != null)
        {
            Power += equippedWeapon.powerBonus;
        }
        currentDamage = baseDamage + (equippedWeapon != null ? equippedWeapon.damage : 0);
    
    }

    private int GetBaseVitality()
    {
        return characterType == CharacterType.Warrior ? 75 : 60;
    }

    private int GetBasePower()
    {
        return characterType == CharacterType.Warrior ? 40 : 30;
    }

    private int GetBaseFortitude()
    {
        return characterType == CharacterType.Warrior ? 45 : 35;
    }

    private int GetBaseArcaneStat()
    {
        return characterType == CharacterType.Warrior ? 35 : 70;
    }
    
}