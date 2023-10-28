using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpFX : MonoBehaviour
{
    [SerializeField] private AnimationCurve _yAnimation;
    [SerializeField] private AnimationCurve _scaleAnimation;

    public void PlayAnimations(Transform jumper, float duration)
    {
        StartCoroutine(AnimationByTime(jumper, duration));
    }

    private IEnumerator AnimationByTime(Transform jumper, float duration)
    {
        float expiredSeconds = 0f;
        float progress = 0f;

        Vector2 startPosition = jumper.position;

        while (progress < 1)
        {
            expiredSeconds += Time.deltaTime;
            progress = expiredSeconds / duration;

            jumper.position = startPosition + new Vector2(0, _yAnimation.Evaluate(progress));

            float scale = _scaleAnimation.Evaluate(progress);
            jumper.localScale = Vector2.one * _scaleAnimation.Evaluate(progress);

            yield return null;
        }
    }
}
