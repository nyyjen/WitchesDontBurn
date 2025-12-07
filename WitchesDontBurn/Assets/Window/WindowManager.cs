using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager.UI;


public class WindowManager : MonoBehaviour
{
 
    [SerializeField] private float timeToNextEvent = 1.0f;
    [SerializeField] private GameObject[] CharacterList;
    [SerializeField] private GameObject[] AnimalList;
    private float eventTimer = 0.0f;


    private GameObject[] WindowList;
    private List<GameObject> NPCs;

    private int numBurntWindows = 0;
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
            CreateFireEvent();   
        }

        foreach(GameObject obj in WindowList)
        {
            WindowBehaviour w = obj.GetComponent<WindowBehaviour>();

            if( w )
            {
                if( w.HasBurntDown() )
                {
                    w.SetBurntWindow();
                    //NPCs.Remove(obj);
                    numBurntWindows++;
                    //decrease player points
                    Debug.Log(numBurntWindows);
                }
            }
        }
    }

    public bool IsGameOver()
    {
        return WindowList.Length == numBurntWindows;
    }

    void CreateFireEvent()
    {
        eventTimer = 0.0f;

        int nextWindowIndex = Random.Range(0, WindowList.GetLength(0));

        WindowBehaviour w = WindowList[nextWindowIndex].GetComponent<WindowBehaviour>();

        // find available window
            if(w != null)
            {
                if( w.IsOnFire() == false && w.HasBurntDown() == false )
                {

                    if( Random.Range(0, 2) == 1)
                    {
                        SpawnPet(w.gameObject);
                        
                    }
                    else
                    {
                        SpawnHuman(w.gameObject);
                    }

                    w.StartFire();
             
                }
            }
            else
            {
                if( numBurntWindows != WindowList.Length)
                {
                    CreateFireEvent();
                }    
            }
            
        }  


        public void SpawnHuman(GameObject obj)
    {
          int index = Random.Range(0, CharacterList.GetLength(0));
          GameObject eventHuman = Instantiate(CharacterList[index], obj.transform.position, obj.transform.rotation);
          NPCs.Add( eventHuman ); 
          Debug.Log(CharacterList.Length);
    }   

    public void SpawnPet(GameObject obj)
    {
          int index = Random.Range(0, AnimalList.GetLength(0));
          GameObject eventPet = Instantiate(AnimalList[index], obj.transform.position, obj.transform.rotation);
          NPCs.Add( eventPet ); 
          Debug.Log(AnimalList.Length);
    }   
}






