using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Transform))]
public class AppleAnimation : MonoBehaviour
{
    private Transform _transform;
    private float _directionY = 0.1f;
    private int _animationDuration = 1;
    private int _loops = -1;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _transform.DOMove(new Vector2(_transform.position.x, _transform.position.y + _directionY), _animationDuration).SetLoops(_loops, LoopType.Yoyo);
    }
}
