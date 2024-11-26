using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolcanoShake : MonoBehaviour
{

    

    public float MaxShakeAmount;
    public float MinShakeAmount;
    float shakeAmount;
    public GameObject CenterPivot;
    bool ShakeOn = false;

    void Start(){
        VolcanoShakeOnF();
    }

    void Update()
    {
         if(ShakeOn == true){
            transform.localPosition = CenterPivot.transform.localPosition + Random.insideUnitSphere * shakeAmount;
        }


    }
    void VolcanoShakeOnF()
    {
        StartCoroutine("VolcanoShakeOnE");
    }
    IEnumerator VolcanoShakeOnE(){
        ShakeOn = true;
        for (int i = 0; i < 5; i++){
            shakeAmount = Random.Range(MinShakeAmount,MaxShakeAmount);
            yield return new WaitForSeconds(Random.Range(1,3));
        }
        ShakeOn = false;
        yield return new WaitForSeconds(Random.Range(1, 3));
        VolcanoShakeOnF();
    }



}
