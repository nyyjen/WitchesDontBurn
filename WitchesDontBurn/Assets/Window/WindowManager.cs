using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;


public class WindowManager : MonoBehaviour
{
 
    [SerializeField] private float timeToNextEvent = 1.0f;
    [SerializeField] private GameObject Human = null;
    [SerializeField] private GameObject Pet = null;
    private float eventTimer = 0.0f;

    private GameObject[] WindowList;
    private List<GameObject> NPCs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WindowList = GameObject.FindGameObjectsWithTag("Window");
        NPCs = new List<GameObject>();

    }

    // Update is called once per frame
    void Update()
    {
        eventTimer += Time.deltaTime;

        if(eventTimer >= timeToNextEvent)
        {
            createFireEvent();
            
        }
    }

    void createFireEvent()
    {
        eventTimer = 0.0f;

        // find available window
        foreach( GameObject obj in WindowList )
        {
            WindowBehaviour w = obj.GetComponent<WindowBehaviour>();

            if(w != null)
            {
                if( w.IsOnFire() == false )
                {
                   GameObject eventHuman = Instantiate(Human, obj.transform.position, obj.transform.rotation);
                   Debug.Log(w.transform.parent.position);    

                   if(eventHuman)
                   {
                        NPCs.Add( eventHuman );
                        w.StartFire();
                        Debug.Log("Created human fire event");
                        break; //end loop when event has been created
                   }
                }
            }
        }  
    
    }
}
