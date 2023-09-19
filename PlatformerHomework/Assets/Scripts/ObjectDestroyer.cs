using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField] private GameObject _apple;
    
    public void DestroyApple()
    {
        Destroy(_apple);
    }
}
