using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowingObjectScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;
    
    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipYRotationTime = 0.5f;

    private bool _isFacingRight;

    private Coroutine _turnCoroutine;

    private void Awake()
    {
        if(_playerTransform.rotation.y > 0)
        {
            _isFacingRight = true;
        }
        else if(_playerTransform.rotation.y < 0)
        {
            _isFacingRight = false;
        }
    }

    private void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void CallTurn()
    {
        _turnCoroutine = StartCoroutine(FlipYLerp());
    }

    private IEnumerator FlipYLerp()
    {
        float startRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;

        while(elapsedTime < _flipYRotationTime)
        {
            elapsedTime += Time.deltaTime;

            yRotation = Mathf.Lerp(startRotation, endRotationAmount, (elapsedTime / _flipYRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null; 
        }
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if(_isFacingRight )
        {
            return 180f;
        }
        else
        {
            return 0f;
        }
    }
}
