using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpBonusScript : MonoBehaviour
{
    private Rigidbody2D _appleRigidbody2D;

    private void Awake()
    {
        _appleRigidbody2D = GetComponent<Rigidbody2D>();
    }

}
