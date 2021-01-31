using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlowObstacle : MonoBehaviour
{
    private void OnEnable()
    {
        // Delay blowing by 0.5 second
        Invoke("BlowAwayObstacle", 0.5f);
    }

    // Blow away nearby obstacles
    private void BlowAwayObstacle()
    {
        if (PlayerCtrl.revived)
        {
            // Find all obstacles
            GameObject[] redTr = GameObject.FindGameObjectsWithTag("RedTriangle");
            GameObject[] redSq = GameObject.FindGameObjectsWithTag("RedSquare");
            GameObject[] redPo = GameObject.FindGameObjectsWithTag("RedPolygon");
            GameObject[] yellowTr = GameObject.FindGameObjectsWithTag("YellowTriangle");
            GameObject[] yellowSq = GameObject.FindGameObjectsWithTag("YellowSquare");
            GameObject[] yellowPo = GameObject.FindGameObjectsWithTag("YellowPolygon");

            // Store all obstacle categories in a collection
            GameObject[][] collection = { redTr, redSq, redPo, yellowTr, yellowSq, yellowPo };

            // Loop through all obstacle to see which one will be blown away
            for (int groupIndex = 0; groupIndex < collection.Length; groupIndex++)
            {
                GameObject[] obs = collection[groupIndex];

                for (int index = 0; index < obs.Length; index++)
                {
                    obs[index].GetComponent<RedCtrl>().BlownByFusionExp();
                }
            }
        }
    }
}
