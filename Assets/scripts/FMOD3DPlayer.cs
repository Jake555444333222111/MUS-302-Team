using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMOD3DPlayer : MonoBehaviour
{
    public EventReference soundEvent;
    public bool playOnEnable = true;

    private EventInstance instance;

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    public void Play()
    {
        if (soundEvent.IsNull || instance.isValid())
            return;

        instance = RuntimeManager.CreateInstance(soundEvent);
        instance.set3DAttributes(
            RuntimeUtils.To3DAttributes(gameObject)
        );
        instance.start();
    }

    private void LateUpdate()
    {
        if (instance.isValid())
        {
            instance.set3DAttributes(
                RuntimeUtils.To3DAttributes(gameObject)
            );

            instance.getPlaybackState(out PLAYBACK_STATE state);

            if (state == PLAYBACK_STATE.STOPPED)
            {
                instance.release();
                instance.clearHandle();
            }
        }
    }

    public void Stop()
    {
        if (!instance.isValid())
            return;

        instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        instance.clearHandle();
    }

    private void OnDisable()
    {
        Stop();
    }
}
