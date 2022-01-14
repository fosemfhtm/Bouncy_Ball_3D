
using UnityEngine;
 
public class real_camera : MonoBehaviour
{
    public GameObject targetPosition;
 
    void Update()
    {
        transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition.transform.position, 0.5f);
        transform.rotation = targetPosition.transform.rotation;
    }
}
