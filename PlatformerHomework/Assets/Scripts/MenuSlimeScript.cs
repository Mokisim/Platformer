using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MenuSlimeScript : MonoBehaviour
{
    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        _animator.Play("MenuSlimeAnimation");
    }
}
