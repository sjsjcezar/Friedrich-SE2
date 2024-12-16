using UnityEngine;

public class PauseController : MonoBehaviour
{
    private static PauseController instance;

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
