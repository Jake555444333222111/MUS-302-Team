
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODRandomPlayer : MonoBehaviour
{
    [Header("Select your FMOD event")]
    public EventReference soundEvent;

    [Header("Delay between plays (seconds)")]
    [Min(0.1f)] public float minimumDelay = 15f;
    [Min(0.1f)] public float maximumDelay = 40f;

    [Header("Override Attenuation")]
    public bool overrideAttenuation = true;

    [Min(0f)]
    public float overrideMinDistance = 1f;

    [Min(0.1f)]
    public float overrideMaxDistance = 100f;

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
            PlaySound();
            ResetTimer();
        }
    }

    private void PlaySound()
    {
        EventInstance sound = RuntimeManager.CreateInstance(soundEvent);

        sound.set3DAttributes(
            RuntimeUtils.To3DAttributes(transform.position)
        );

        // Apply FMOD attenuation override
        if (overrideAttenuation)
        {
            sound.setProperty(
                EVENT_PROPERTY.MINIMUM_DISTANCE,
                overrideMinDistance
            );

            sound.setProperty(
                EVENT_PROPERTY.MAXIMUM_DISTANCE,
                overrideMaxDistance
            );
        }

        sound.start();
        sound.release();
    }

    private void ResetTimer()
    {
        timer = Random.Range(minimumDelay, maximumDelay);
    }

    private void OnValidate()
    {
        minimumDelay = Mathf.Max(0.1f, minimumDelay);
        maximumDelay = Mathf.Max(minimumDelay, maximumDelay);

        overrideMinDistance = Mathf.Max(0f, overrideMinDistance);
        overrideMaxDistance = Mathf.Max(
            overrideMinDistance + 0.1f,
            overrideMaxDistance
        );
    }
}
