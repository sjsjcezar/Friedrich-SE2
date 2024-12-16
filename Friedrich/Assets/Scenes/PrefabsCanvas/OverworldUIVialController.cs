using UnityEngine;

public class OverworldUIVialController : MonoBehaviour
{
    private static OverworldUIVialController instance;

    private void Awake()
    {
        // Check if an instance already exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // Destroy duplicate instance
            return;
        }

        // Assign the instance and prevent destruction on load
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
