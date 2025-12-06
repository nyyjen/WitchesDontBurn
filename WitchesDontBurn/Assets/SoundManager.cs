using UnityEngine;

public class SoundManager : MonoBehaviour
{

     [SerializeField] private AK.Wwise.Event _EventName = null;
    //private GameObject sound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

      _EventName.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
