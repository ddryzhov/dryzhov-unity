using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public GameObject destroyedVersion; // Assign a prefab of the destroyed barrel
    private bool hasBeenDestroyed = false;

    public void Destroy()
    {
        // Instantiate the destroyed version of the barrel
        if (!hasBeenDestroyed && destroyedVersion != null)
        {
            GameObject destroyedInstance = Instantiate(destroyedVersion, transform.position, transform.rotation);
            destroyedInstance.transform.localScale = Vector3.one * 13;
            hasBeenDestroyed = true;
        }

        // Disable the original barrel
        gameObject.SetActive(false);
    }
}
