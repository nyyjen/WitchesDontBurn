using UnityEngine;



public class WindowBehaviour : MonoBehaviour
{
    [SerializeField] private bool onFire;
    [SerializeField] private float timeOnFire;
    [SerializeField] public bool isPet;
    [SerializeField] private bool isHuman;

    [SerializeField] private static float maxTimeOnFire = 5.0f;
    [SerializeField] private static float timeToFireIntensityChange = 2.5f;  

    private bool intensityLevelUp = false;   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if( onFire )
        {
            timeOnFire += Time.deltaTime;

            if( timeOnFire >= maxTimeOnFire )
            {
                // human will jump
                // pet will die

                //onFire = false;

            }
            else if(!intensityLevelUp && timeOnFire >= timeToFireIntensityChange)
            {
                intensityLevelUp = true;
            }
        }

    }

    public void StartFire()
    {
        onFire = true;
        timeOnFire = 0.0f;
        intensityLevelUp = false;
    }

    public void ResetWindow()
    {
        onFire = false;
    }

   public bool IsOnFire()
    {
        return onFire;
    }

    public bool BurntDown()
    {
        return (timeOnFire >= maxTimeOnFire);
    }

    public void OnTriggerEnter2D(Collider2D other)
    {

    }

    public void OnTriggerStay2D(Collider2D other)
    {

    }
    public void OnTriggerExit2D(Collider2D other)
    {

    }
}
