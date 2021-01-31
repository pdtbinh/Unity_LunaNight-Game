using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricShow : MonoBehaviour
{
    public Sprite[] elecs;

    public Material[] elecMats;

    private float elecSize;

    public float upperPosY;

    public float lowerPosY;

    private bool charged;

    // Start is called before the first frame update
    void Start()
    {
        // elecIndex is initially at 0
        elecSize = 0f;

        // whether shape is charged
        charged = true;  
    }

    public void ChargeLighting()
    {
        StartCoroutine("ElectricCharge");
    }

    public void StopLighting()
    {
        StopCoroutine("ElectricCharge");
    }

    IEnumerator ElectricCharge()
    {
        //elecSize = 0f;

        while (charged)
        {
            if (elecSize >= 1f)
            {
                elecSize = 0f;

                float newPosY = Random.Range(lowerPosY, upperPosY);
                transform.localPosition = new Vector3(0f, newPosY, 0f);

                int newElecIndex = Random.Range(0, elecs.Length);
                GetComponent<SpriteRenderer>().sprite = elecs[newElecIndex];
                GetComponent<SpriteRenderer>().material = elecMats[newElecIndex];
            }

            else
            {
                transform.localScale = new Vector3(elecSize, 1f, 1f);
                elecSize += Time.deltaTime * Random.Range(1, 5);
            }

            yield return new WaitForSeconds(0f);
        }
    }
}
