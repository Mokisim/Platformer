using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UserInterface : MonoBehaviour
{
    private AppleBonus _appleBonus;

    [SerializeField] private Text _scoreText;
    [SerializeField] private Image[] _hearts;
    [SerializeField] private Sprite _heartSprite;
    [SerializeField] private Sprite _emptyHeartSprite;

    private int _score = 0;
    private int _health;
    private int _currentHearts;

    [SerializeField] private Rigidbody2D _player;
    [SerializeField] private Transform _spawnPoint;

    private Transform _lastCheackPoint;

    [SerializeField] private RestartScript _restartScript;

    private bool _isImmotral;
    private bool _isPickUp;

    private void Start()
    {
        _lastCheackPoint = _spawnPoint;
        Time.timeScale = 1f;
        _health = 3;
        _currentHearts = _health;
    }

    private void Update()
    {
        HealthChecker();
    }

    public void OnTriggerEnter2D(Collider2D player)
    {
        if (player.CompareTag("AppleBonus") && _isPickUp == false)
        {
            _score += 1;
            _scoreText.text = _score.ToString();
            _isPickUp = true;
            Invoke("PickUpBonus", 0.02f);
        }

        if (player.CompareTag("Trap") && _isImmotral == false)
        {
            _health--;
            _isImmotral = true;
            Invoke("ImmortalTimeChecker", 0.5f);

            _player.position = _lastCheackPoint.position;
        }

        if (player.CompareTag("Enemy") && _isImmotral == false)
        {
            _health--;
            _isImmotral = true;
            Invoke("ImmortalTimeChecker", 0.5f);

            _player.position = _lastCheackPoint.position;
        }

        if (player.CompareTag("CheackPoint"))
        {
            _lastCheackPoint.position = _player.transform.position;
        }
    }

    private void ImmortalTimeChecker()
    {
        _isImmotral = false;
    }

    private void PickUpBonus()
    {
        _isPickUp = false;
    }

    private void HealthChecker()
    {
        if (_health > _currentHearts)
        {
            _health = _currentHearts;
        }

        for (int i = 0; i < _hearts.Length; i++)
        {
            if (i < Mathf.RoundToInt(_health))
            {
                _hearts[i].sprite = _heartSprite;
            }
            else
            {
                _hearts[i].sprite = _emptyHeartSprite;
            }

            if (i < _currentHearts)
            {
                _hearts[i].enabled = true;
            }
            else
            {
                _hearts[i].enabled = false;
            }

            if (_health < 1)
            {
                _restartScript.Setup();
            }
        }
    }
}

