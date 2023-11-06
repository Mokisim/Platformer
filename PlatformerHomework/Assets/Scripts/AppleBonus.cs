using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class AppleBonus : MonoBehaviour
{
    [SerializeField] public AudioSource _eatingSound;
    [SerializeField] public AudioClip _eatingClip;
    [SerializeField] private Collider2D _apple;

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
        _eatingSound.PlayOneShot(_eatingClip);
    }

    public void DestroyApple()
    {
        Destroy(_apple.gameObject);
    }
}
