using UnityEngine;

public class ScenceManager : MonoBehaviour
{
    public YourAction1SO ya1config;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject gameObject = GameObject.Find("Cube");
        ya1config.Cube=gameObject;
        ya1config.BoxCollider=gameObject.GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
