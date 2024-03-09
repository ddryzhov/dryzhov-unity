using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public GameObject destroyedVersion; // Assign a prefab of the destroyed barrel

    public void Destroy()
    {
        // Instantiate the destroyed version of the barrel
        if (destroyedVersion != null)
        {
            GameObject destroyedInstance = Instantiate(destroyedVersion, transform.position, transform.rotation);
            destroyedInstance.transform.localScale = Vector3.one * 13;
        }

        // Disable the original barrel
        gameObject.SetActive(false);
    }
}
