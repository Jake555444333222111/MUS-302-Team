using FMODUnity;
using UnityEngine;

public class randomeeventtimeplay : MonoBehaviour
{
    [Header("Select your FMOD event")]
    public EventReference soundEvent;

    [Header("Delay between plays (seconds)")]
    [Min(0.1f)] public float minimumDelay = 15f;
    [Min(0.1f)] public float maximumDelay = 40f;

    private float timer;

    private void OnEnable()
    {
        ResetTimer();
    }

    private void Update()
    {
        if (soundEvent.IsNull)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            RuntimeManager.PlayOneShot(soundEvent, transform.position);
            ResetTimer();
        }
    }

    private void ResetTimer()
    {
        timer = Random.Range(minimumDelay, maximumDelay);
    }

    private void OnValidate()
    {
        minimumDelay = Mathf.Max(0.1f, minimumDelay);
        maximumDelay = Mathf.Max(minimumDelay, maximumDelay);
    }
}
