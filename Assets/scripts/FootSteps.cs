using UnityEngine;
using FMODUnity;


using UnityEngine.InputSystem; //start imput system

public class FootSteps : MonoBehaviour
{

            [SerializeField] private EventReference soundEvent;
        [SerializeField, Min(0f)] private float startTime = 15f;
        [SerializeField, Min(0f)] private float endTime = 40f;
        [SerializeField] private bool playOnlyOnce = true;

        private float enabledAt;
        private bool hasPlayed;

        private void OnEnable()
        {
            enabledAt = Time.time;
            hasPlayed = false;
        }

        private void Update()
        {
            float elapsed = Time.time - enabledAt;

            if (elapsed < startTime || elapsed > endTime)
                return;

            if (soundEvent.IsNull || (playOnlyOnce && hasPlayed))
                return;

            if (MovementKeyPressed())
            {
                RuntimeManager.PlayOneShot(soundEvent);
                hasPlayed = true;
            }
        }

        private static bool MovementKeyPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            return keyboard != null &&
                   (keyboard.wKey.wasPressedThisFrame ||
                    keyboard.aKey.wasPressedThisFrame ||
                    keyboard.sKey.wasPressedThisFrame ||
                    keyboard.dKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.W) ||
               Input.GetKeyDown(KeyCode.A) ||
               Input.GetKeyDown(KeyCode.S) ||
               Input.GetKeyDown(KeyCode.D);
#endif
        }
    }