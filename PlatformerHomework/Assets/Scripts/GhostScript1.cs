using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostScript1 : MonoBehaviour
{
    [SerializeField] private Transform _path;
    [SerializeField] private float _speed;

    private Transform[] _points;
    private int _currentPoint;

    private bool _facingRight = true;

    private void Start()
    {
        _points = new Transform[_path.childCount];

        for (int i = 0; i < _path.childCount; i++)
        {
            _points[i] = _path.GetChild(i);
        }
    }

    private void Update()
    {
        Transform target = _points[_currentPoint];

        transform.position = Vector3.MoveTowards(transform.position, target.position, _speed * Time.deltaTime);

        if (transform.position == target.position)
        {
            _currentPoint++;

            if (_currentPoint >= _points.Length)
            {
                _currentPoint = 0;
            }
        }

        if(transform.position.x < target.position.x && !_facingRight)
        {
            Flip();
        }
        else if(transform.position.x > target.position.x && _facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        _facingRight = !_facingRight;

        transform.Rotate(0f, 180f, 0f);
    }
}
