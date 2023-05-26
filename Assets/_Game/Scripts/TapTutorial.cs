using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapTutorial : MonoBehaviour
{


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (GameController.IsOverRaycastBlockingUI()) return;
                Destroy(gameObject);
        }
    }
}
