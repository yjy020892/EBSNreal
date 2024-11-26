using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoLight : MonoBehaviour
{
    // Start is called before the first frame update

    Light _light;
    public float MinLightintensity;
    public float MaxLightintensity;
    void Start()
    {
        _light = GetComponent<Light>();
        StartCoroutine("VolcanoLightE");
    }
    IEnumerator VolcanoLightE(){

        while(true){
            yield return new WaitForSeconds(Random.Range(0.1f, 0.5f));
            _light.intensity = Random.Range(MinLightintensity, MaxLightintensity);


        }

    }

}
