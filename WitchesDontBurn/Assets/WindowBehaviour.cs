using UnityEngine;
using UnityEngine.Rendering;



public class WindowBehaviour : MonoBehaviour
{
    [SerializeField] private bool onFire;
    [SerializeField] private float timeOnFire;
    [SerializeField] public bool isPet;
    [SerializeField] private bool isHuman;

    [SerializeField] private float maxTimeOnFire = 5.0f;
    [SerializeField] private float timeToFireIntensityChange = 2.5f;  
    [SerializeField] private float humanJumps = 4.5f;  

    [SerializeField] private Sprite vfxSmallFire = null;
    [SerializeField] private Sprite vfxMediumFire = null;
    [SerializeField] private Sprite vfxBigFire = null;


    private bool intensityLevelUp = false;
    private bool isBurnt = false; 
    private SpriteRenderer sr = null; 
    public Collider2D targetCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sr = gameObject.GetComponent<SpriteRenderer>();
        targetCollider = gameObject.GetComponent<BoxCollider2D>();
    }

    void Align()
    {
        Bounds spriteBounds = sr.bounds;
        Bounds colliderBounds = targetCollider.bounds;

        float spriteBottom = spriteBounds.min.y;
        float colliderBottom = colliderBounds.min.y;

        // Move sprite so its bottom aligns with collider bottom
        float offset = colliderBottom - spriteBottom;

        sr.transform.position += new Vector3(0, offset, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if( onFire && !HasBurntDown() )
        {
            timeOnFire += Time.deltaTime;

            if( timeOnFire >= humanJumps )
            {
                // human will jump
                // pet will die

            }
            else if(!intensityLevelUp && timeOnFire >= timeToFireIntensityChange)
            {
                intensityLevelUp = true;

                if ( sr && vfxMediumFire)
                {
                      sr.sprite = vfxMediumFire;
                      //float Xwidth = .75f / vfxMediumFire.bounds.size.x;
                      //float Ywidth = .75f / vfxMediumFire.bounds.size.y;
                      //sr.transform.localScale = new Vector3(Xwidth, Ywidth, 1);
                }     
            }
        }

    }

    public void StartFire()
    {
        onFire = true;
        timeOnFire = 0.0f;
        intensityLevelUp = false;

        if( sr && vfxSmallFire )
        {
            sr.sprite = vfxSmallFire;
            Align();
        }
    }

    public void ResetWindow()
    {
        onFire = false;

        if( sr )
        {
            sr.sprite = null;
        }
        
    }

    public void SetBurntWindow()
    {
        if( sr && vfxBigFire && !isBurnt)
        {
            sr.sprite = vfxBigFire;
            isBurnt = true;
        }

        Debug.Log("Set burnt window");
    }

   public bool IsOnFire()
    {
        return onFire;
    }

    public bool HasBurntDown()
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
