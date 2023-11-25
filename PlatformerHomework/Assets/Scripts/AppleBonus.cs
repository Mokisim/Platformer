using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Transform))]
public class AppleBonus : MonoBehaviour
{
    [SerializeField] public AudioClip _eatingClip;
    [SerializeField] private Collider2D _apple;

    private Transform _transform;
    private float _directionY = 0.1f;
    private int _animationDuration = 1;
    private int _loops = -1;

    private void Start()
    {
        _transform = GetComponent<Transform>();
        _transform.DOMove(new Vector2(_transform.position.x, _transform.position.y + _directionY), _animationDuration).SetLoops(_loops, LoopType.Yoyo);
    }

    private void OnTriggerStay2D(Collider2D apple)
    {
        if (apple.CompareTag("Player"))
        {
            PlaySound();
            DestroyApple();
            Debug.Log("Trigger");
        }
    }

    private void PlaySound()
    {
        AudioSource.PlayClipAtPoint(_eatingClip, transform.position);
    }

    public void DestroyApple()
    {
        Destroy(_apple.gameObject);
    }
}
