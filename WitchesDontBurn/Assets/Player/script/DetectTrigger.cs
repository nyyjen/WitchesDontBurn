using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DetectTrigger : MonoBehaviour
{

    private CharacterController characterController;
    void Awake()
    {
        // try to get CharacterController on this object, or parent (in case this script is on a child pickup zone)
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        // diagnostic log to see what collider is hitting
        Debug.Log($"DetectTrigger: OnTriggerEnter2D with '{other.gameObject.name}' tag={other.gameObject.tag} layer={LayerMask.LayerToName(other.gameObject.layer)}");

        // If the collider belongs to a child of the water prefab, use root to find the prefab root
        GameObject hitObject = other.gameObject;
        if (other.transform.root != null && other.transform.root != other.transform)
        {
            hitObject = other.transform.root.gameObject;
        }

        // safety: ensure we have a character controller
        if (characterController == null)
        {
            Debug.LogWarning("DetectTrigger: CharacterController not found when picking up water.");
            return;
        }

        if (hitObject.CompareTag("BigWater"))
        {
            characterController.currentWater += 3;
        }
        else if (hitObject.CompareTag("SmallWater"))
        {
            characterController.currentWater += 1;
        }

        if (characterController.currentWater > 6)
        {
            characterController.currentWater = 6;
        }

        if (hitObject.CompareTag("BigWater") || hitObject.CompareTag("SmallWater"))
        {
            Destroy(hitObject);
        }
    }// 
     // 
     //Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exit Trigger"); 
    }
}
