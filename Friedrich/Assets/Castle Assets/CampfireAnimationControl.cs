using System.Collections;
using UnityEngine;

public class CampfireAnimationControl : MonoBehaviour
{
    // Reference to the Animator component
    private Animator campfireAnimator;

    // Variable to control the speed of the animation
    private float animationSpeed;

    // The minimum and maximum variation of the animation speed for randomness
    public float minSpeed = 0.8f;
    public float maxSpeed = 1.2f;

    // Unique seed to make each campfire start at a different time
    private float startDelay;

    void Start()
    {
        // Get the Animator component from the campfire GameObject
        campfireAnimator = GetComponent<Animator>();

        // Set a random start delay for each campfire to make them sync differently
        startDelay = Random.Range(0f, 2f);

        // Set a random animation speed for each campfire within the given range
        animationSpeed = Random.Range(minSpeed, maxSpeed);

        // Set the animation speed and start the delayed animation
        campfireAnimator.speed = animationSpeed;

        // Start the animation with a delay
        StartCoroutine(DelayedAnimationStart());
    }

    // Coroutine to delay the start of the animation for randomness
    private IEnumerator DelayedAnimationStart()
    {
        yield return new WaitForSeconds(startDelay);

        // Play the animation (if it's not playing yet)
        campfireAnimator.SetTrigger("StartFire");
    }
}
