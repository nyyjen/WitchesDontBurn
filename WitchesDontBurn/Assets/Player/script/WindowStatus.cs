using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class WindowStatus : MonoBehaviour
{
    [Header("Visuals / VFX")]
    [Tooltip("Optional GameObject (ParticleSystem) to enable when window is on fire")]
    public GameObject fireVFX;
    [Tooltip("Optional GameObject (child) to show when window has people inside")]
    public GameObject occupiedMarker;

    [Header("Detection")]
    [Tooltip("Tag used to identify NPCs (entering the window area counts as 'has people')")]
    public string npcTag = "NPC";

    [Header("Events")]
    public UnityEvent onFireStarted;
    public UnityEvent onFireExtinguished;
    public UnityEvent onPeopleEntered;
    public UnityEvent onPeopleLeft;

    // state
    [SerializeField]
    private bool isOnFire = false;
    private int peopleCount = 0;

    public bool IsOnFire => isOnFire;
    public bool HasPeople => peopleCount > 0;
    public int PeopleCount => peopleCount;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"WindowStatus: Collider on '{name}' is not a trigger. It is recommended to set it as trigger for detection.");
        }

        // initialize visual states
        UpdateVisuals();
    }

    /// <summary>
    /// Ignite the window (start fire).
    /// </summary>
    public void Ignite()
    {
        if (isOnFire) return;
        isOnFire = true;
        UpdateVisuals();
        onFireStarted?.Invoke();
    }

    /// <summary>
    /// Extinguish the window (stop fire).
    /// </summary>
    public void Extinguish()
    {
        if (!isOnFire) return;
        isOnFire = false;
        UpdateVisuals();
        onFireExtinguished?.Invoke();
    }

    private void UpdateVisuals()
    {
        if (fireVFX != null)
            fireVFX.SetActive(isOnFire);

        if (occupiedMarker != null)
            occupiedMarker.SetActive(HasPeople);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // check root tag first (in case collider is on child)
        GameObject hit = other.transform.root != null && other.transform.root != other.transform ? other.transform.root.gameObject : other.gameObject;
        if (hit.CompareTag(npcTag))
        {
            peopleCount++;
            UpdateVisuals();
            onPeopleEntered?.Invoke();
            Debug.Log($"WindowStatus: NPC entered {name}. PeopleCount={peopleCount}");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;
        GameObject hit = other.transform.root != null && other.transform.root != other.transform ? other.transform.root.gameObject : other.gameObject;
        if (hit.CompareTag(npcTag))
        {
            peopleCount = Mathf.Max(0, peopleCount - 1);
            UpdateVisuals();
            onPeopleLeft?.Invoke();
            Debug.Log($"WindowStatus: NPC left {name}. PeopleCount={peopleCount}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isOnFire ? Color.red : Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);

        Gizmos.color = Color.yellow;
        Gizmos.DrawIcon(transform.position + Vector3.up * 0.5f, HasPeople ? "d_icon_person.png" : "d_icon_person_grey.png", true);
    }
}
