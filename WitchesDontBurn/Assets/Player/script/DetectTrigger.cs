using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DetectTrigger : MonoBehaviour
{

    private CharacterController characterController;
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("BigWater"))
        {
            characterController.currentWater+=3;
        }
        else if (other.CompareTag("SmallWater"))
        {
            characterController.currentWater+=1;
        }
        if (characterController.currentWater > 6)
        {
            characterController.currentWater = 6;
        }
        if (other.CompareTag("BigWater") || other.CompareTag("SmallWater"))
        {
            Destroy(other.gameObject);
        }
    }// 
     // 
     //Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("Exit Trigger"); 
    }
}
