using UnityEngine;
public class DestroyOutOfBounds : MonoBehaviour
{
    public float destroyYPosition = -10f;
    
    void Update()
    {
        if (transform.position.y < destroyYPosition)
            Destroy(gameObject);
    }
}