using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreenManager : MonoBehaviour
{
    public static string targetScene;     // Store the scene to load after the loading screen

    public GameObject loadingScreen;      // Optional: Assign loading screen UI (if you want it visible)
    public Slider loadingBar;             // Optional: Slider for loading progress bar

    // Call this to start loading a scene (static method for global access)
    public static void LoadScene(string sceneToLoad)
    {
        targetScene = sceneToLoad;
        SceneManager.LoadScene("LoadingScene"); // Switch to the loading screen scene
    }

    private void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        // Start loading the target scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        
        // Prevent the scene from being activated immediately
        operation.allowSceneActivation = false;

        // Optional: Display loading screen UI if you have one
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // Update loading progress (if you use a loading bar)
        while (!operation.isDone)
        {
            // Calculate progress (0.0 to 0.9 means loading, 1.0 means it's ready)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // If you have a loading bar, update it
            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            // Debugging: Log loading progress (optional)
            Debug.Log($"Loading progress: {progress * 100}%");

            // If loading is done, allow scene activation
            if (operation.progress >= 0.9f)
            {
                // Optional: Add a small delay before activating the scene
                yield return new WaitForSeconds(1f);

                // Activate the loaded scene
                operation.allowSceneActivation = true;
            }

            yield return null; // Wait for the next frame
        }
    }
}
