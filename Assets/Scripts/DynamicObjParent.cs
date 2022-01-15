using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicObjParent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateAllChildren() {
        foreach (Transform child in transform) {
            child.gameObject.SetActive(true);
        }
    }
}
