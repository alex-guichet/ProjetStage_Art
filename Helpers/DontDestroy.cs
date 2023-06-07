using UnityEngine;
public class DontDestroy : MonoBehaviour
{
    private void Awake()
    {
        var sound_manager_array = FindObjectsOfType<DontDestroy>();

        if (sound_manager_array.Length > 1)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
}