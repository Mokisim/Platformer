using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainsawRotation : MonoBehaviour
{
    private void Update()
    {
        transform.Rotate(0f, 0f, transform.rotation.z + 20);
    }
}
