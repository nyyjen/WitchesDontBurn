using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DetectTrigger : MonoBehaviour
{

    private CharacterController characterController;
    public bool inWindowRange = false;
    public bool inGroundRange = false;
    public GameObject npcOnWindow = null;
    void Awake()
    {
        // try to get CharacterController on this object, or parent (in case this script is on a child pickup zone)
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        // If the collider belongs to a child of the water prefab, use root to find the prefab root
        GameObject hitObject = other.gameObject;
        if (other.transform.root != null && other.transform.root != other.transform)
        {
            hitObject = other.transform.root.gameObject;
        }

        if (hitObject.CompareTag("Window"))
        {
            inWindowRange = true;
        }

        if (hitObject.CompareTag("ground"))
        {
            inGroundRange = true;
        }

        if (hitObject.CompareTag("NPC"))
        {
            npcOnWindow = hitObject;
        }

        if (hitObject.CompareTag("BigWater"))
        {
            characterController.currentWater += 3;
            AkSoundEngine.PostEvent("SFX_PickupWater_Big", gameObject);
        }
        else if (hitObject.CompareTag("SmallWater"))
        {
            characterController.currentWater += 1;
            AkSoundEngine.PostEvent("SFX_PickupWater_Small", gameObject);
        }

        if (characterController.currentWater > 6)
        {
            characterController.currentWater = 6;
        }

        if (hitObject.CompareTag("BigWater") || hitObject.CompareTag("SmallWater"))
        {
            Destroy(hitObject);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        GameObject hitObject = collision.transform.root.gameObject;

        if (hitObject.CompareTag("Window"))
        {
            inWindowRange = false;
            npcOnWindow = null;
        }

        if (hitObject.CompareTag("ground"))
        {
            inGroundRange = false;
        }

        if (hitObject.CompareTag("NPC"))
        {
            npcOnWindow = null;
        }
    }
}