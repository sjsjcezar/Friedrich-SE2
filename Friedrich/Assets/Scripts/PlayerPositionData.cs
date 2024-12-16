using UnityEngine;

[CreateAssetMenu(fileName = "PlayerPositionData", menuName = "Game/Player Position Data")]
public class PlayerPositionData : ScriptableObject
{
    public Vector3 lastOverworldPosition;
    public bool hasStoredPosition;

    public void SavePosition(Vector3 position)
    {
        lastOverworldPosition = position;
        hasStoredPosition = true;
        Debug.Log($"Position saved to ScriptableObject: {position}");
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssets();
        #endif
    }

    public void ClearPosition()
    {
        hasStoredPosition = false;
        Debug.Log("Position data cleared from ScriptableObject");
    }

    
}