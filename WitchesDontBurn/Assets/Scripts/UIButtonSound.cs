using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event _uiWwiseEvent = null;

    public void Play()
    {
        _uiWwiseEvent.Post(gameObject);
    }
}
