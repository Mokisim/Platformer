using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PhysicsJump : MonoBehaviour
{
    [SerializeField] private float _length;
    [SerializeField] private float _duration;
    [SerializeField] private JumpFX _jumpFX;
    [SerializeField] private Rigidbody2D _rb;
    
    public void Jump2(Vector3 direction)
    {
        Vector3 target = transform.position + (direction * _length);
        _jumpFX.PlayAnimations(transform, _duration);
        StartCoroutine(JumpByTime(target));
    }
    
    private IEnumerator JumpByTime(Vector3 target)
    {
        float expiredSeconds = 0f;
        float progress = 0f;

        Vector2 startPosition = transform.position;

        while (progress < 1)
        {
            expiredSeconds += 1;
            progress = expiredSeconds / _duration;

            transform.position = Vector3.Lerp(startPosition, target, progress);

            yield return null;
        }
    }
}
