using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private float waitTime = 2f;

    private float timer;
    private bool loading;

    private void OnTriggerStay(Collider other)
    {
        if (loading || !other.CompareTag("Player"))
            return;

        timer += Time.fixedDeltaTime;

        if (timer >= waitTime)
        {
            loading = true;
            SceneManager.LoadScene(sceneName);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            timer = 0f;
    }
}