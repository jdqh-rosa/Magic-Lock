using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Lock testLock = new Lock();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow)) testLock.TurnDialLeft();
        if(Input.GetKeyDown(KeyCode.RightArrow)) testLock.TurnDialRight();
    }
}
