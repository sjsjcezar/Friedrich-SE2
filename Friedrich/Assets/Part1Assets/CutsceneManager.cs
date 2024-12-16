using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public PlayableDirector timelineDirector;
    public bool IsCutsceneFinished { get; private set; }

    private void Start()
    {
        IsCutsceneFinished = false;
        if (timelineDirector != null)
        {
            timelineDirector.stopped += OnTimelineFinished;
        }
    }

    public void StartCutscene()
    {
        // Simply play the Timeline without binding a specific character
        timelineDirector.Play();
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        IsCutsceneFinished = true;
    }
}
