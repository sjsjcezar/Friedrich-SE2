using UnityEngine;

public class InventoryUICanvas : MonoBehaviour
{
    private static InventoryUICanvas instance;

    private void Awake()
    {
        Debug.Log("InventoryUICanvas Awake called.");
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        // Assign the instance and prevent destruction on load
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure the Canvas is enabled
        Canvas canvas = GetComponent<Canvas>();
        canvas.enabled = true; // Enable the Canvas
    }

    public void InitializeCanvas()
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }
    }
}