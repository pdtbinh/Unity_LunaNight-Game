using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageGenerator : MonoBehaviour
{
    public GameObject villagePrefab;
    public GameObject latestVillage;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void GenerateVillage()
    {
        Vector3 newPosition = new Vector3(latestVillage.transform.position.x + 25f, -1.5f, 0);
        GameObject village = Instantiate(villagePrefab, newPosition, Quaternion.identity, transform);
        latestVillage = village;
    }
}
