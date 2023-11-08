using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private UserInterface _userInterface;

    [Header("Movement")]
    private bool _facingRight = true;
    private float _movementAcceleration = 65;
    private float _maxMovementSpeed = 12;
    private float _groundLinearDrag = 10;
    private float _horizontalDirection;
    private float _jumpForce = 20f;
    [SerializeField] private LayerMask _groundLayer;
    private bool _onGround;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _canJump => _jumpBufferCounter > 0f && _aviableJumps > 0;
    private float _airLinearDrag = 5f;
    private float _fallMultiplier = 7f;
    private float _lowJumpFallMultiplier = 2f;
    [SerializeField] private int _jumpsValue = 1;
    private int _aviableJumps;
    private float _hangTime = 0.2f;
    private float _hangTimeCounter;
    private float _jumpBufferLength = 0.1f;
    private float _jumpBufferCounter;
    private int _jumpInputChecker = 0;

    [Header("Dash")]
    private float _dashSpeed = 10;
    private float _dashTime;
    private float _startDashTime = 0.15f;
    private int _direction;
    private bool _lockDash = false;

    [Header("CheckCollisions")]
    [SerializeField] private float _groundRaycastLength = 0.65f;
    [SerializeField] private Vector3 _edgeRaycastOffset;
    [SerializeField] private Vector3 _innerRaycastOffset;
    private float _topRaycastLength = 0.85f;
    private bool _canCornerCorrect;

    private bool _changingDirection =>
       (_rigidbody2D.velocity.x > 0f && _horizontalDirection < 0f) || (_rigidbody2D.velocity.x < 0f && _horizontalDirection > 0f);

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _userInterface = GetComponent<UserInterface>();

        _dashTime = _startDashTime;
    }

    private void Update()
    {
        _horizontalDirection = GetInput().x;

        Dash();

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferLength;
            Debug.Log($"prigok: {_jumpBufferCounter}");
        }

        if (_canJump && Input.GetButtonDown("Jump"))
        {
            if ((_hangTimeCounter <= 0) && (_aviableJumps == _jumpsValue))
            {
                _aviableJumps--;
            }

            if (_aviableJumps > 0)
            {
                Jump();
                _jumpInputChecker = 0;
                _aviableJumps--;
                _hangTimeCounter = 0f;
            }
        }

        if (_aviableJumps > 0 && _hangTimeCounter > 0)
        {
            _hangTimeCounter -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        MoveCharacter();

        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _animator.SetBool("OnGround", true);

            if (_jumpInputChecker > 0)
            {
                _hangTimeCounter = _hangTime;
                _aviableJumps = _jumpsValue;
            }
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _animator.SetBool("OnGround", false);
            _jumpBufferCounter -= Time.deltaTime;
            _jumpInputChecker = 1;
            Debug.Log($"polet: {_jumpBufferCounter}");
        }

        if (_canCornerCorrect)
        {
            CornerCorrect(_rigidbody2D.velocity.y);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 2 && _lockDash)
        {
            _rigidbody2D.velocity = Vector2.zero;
        }
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void MoveCharacter()
    {
        _rigidbody2D.AddForce(new Vector2(_horizontalDirection, 0f) * _movementAcceleration);

        if (Mathf.Abs(_rigidbody2D.velocity.x) > _maxMovementSpeed)
        {
            _rigidbody2D.velocity = new Vector2(Mathf.Sign(_rigidbody2D.velocity.x) * _maxMovementSpeed, _rigidbody2D.velocity.y);
        }

        float currentSpeed = Mathf.Abs(Input.GetAxisRaw("Horizontal") * _maxMovementSpeed);
        _animator.SetFloat("Speed", currentSpeed);

        if (Input.GetAxisRaw("Horizontal") > 0 && !_facingRight)
        {
            Flip();
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && _facingRight)
        {
            Flip();
        }
    }

    private void Jump()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0f);
        _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }

    private void Dash()
    {
        if (_direction == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                _direction = 1;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                _direction = 2;
            }
        }
        else
        {
            if (_dashTime <= 0)
            {
                _direction = 0;
                _dashTime = _startDashTime;
                _rigidbody2D.velocity = Vector2.zero;
            }
            else
            {
                _dashTime -= Time.deltaTime;
                Vector2 dashPosition = _rigidbody2D.position;
                float dashDistance = 0.3f;

                if (_direction == 1)
                {
                    dashPosition.x -= dashDistance;
                }
                else if (_direction == 2)
                {
                    dashPosition.x += dashDistance;
                }

                _rigidbody2D.MovePosition(dashPosition);
            }
        }
    }

    private void DashLock()
    {
        _lockDash = false;
    }

    private void FallMultiplier()
    {
        if (_rigidbody2D.velocity.y < 0)
        {
            _rigidbody2D.gravityScale = _fallMultiplier;
        }
        else if (_rigidbody2D.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            _rigidbody2D.gravityScale = _lowJumpFallMultiplier;
        }
        else
        {
            _rigidbody2D.gravityScale = 1f;
        }
    }

    private void CheckCollisions()
    {
        _onGround = Physics2D.Raycast(transform.position + _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer) ||
        Physics2D.Raycast(transform.position - _groundRaycastOffset, Vector2.down, _groundRaycastLength, _groundLayer);

        _canCornerCorrect = Physics2D.Raycast(transform.position + _edgeRaycastOffset, Vector2.up, _topRaycastLength, _groundLayer) &&
            !Physics2D.Raycast(transform.position + _innerRaycastOffset, Vector2.up, _topRaycastLength, _groundLayer) ||
            Physics2D.Raycast(transform.position - _edgeRaycastOffset, Vector2.up, _topRaycastLength, _groundLayer) &&
            !Physics2D.Raycast(transform.position - _innerRaycastOffset, Vector2.up, _topRaycastLength, _groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position + _groundRaycastOffset, transform.position + _groundRaycastOffset + Vector3.down * _groundRaycastLength);
        Gizmos.DrawLine(transform.position - _groundRaycastOffset, transform.position - _groundRaycastOffset + Vector3.down * _groundRaycastLength);

        Gizmos.DrawLine(transform.position + _edgeRaycastOffset, transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _edgeRaycastOffset, transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset, transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength);
        Gizmos.DrawLine(transform.position - _innerRaycastOffset, transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength);

        Gizmos.DrawLine(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.left * _topRaycastLength);
        Gizmos.DrawLine(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength,
                        transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength + Vector3.right * _topRaycastLength);
    }

    private void ApplyGroundLinearDrag()
    {
        if (Mathf.Abs(_horizontalDirection) < 0.4f || _changingDirection)
        {
            _rigidbody2D.drag = _groundLinearDrag;
        }
        else
        {
            _rigidbody2D.drag = 0f;
        }
    }

    private void ApplyAirLinearDrag()
    {
        _rigidbody2D.drag = _airLinearDrag;
    }

    private void CornerCorrect(float yVelocity)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.left, _topRaycastLength, _groundLayer);

        if (hit.collider != null)
        {
            float newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x + newPosition, transform.position.y, transform.position.z);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, yVelocity);
            return;
        }

        hit = Physics2D.Raycast(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.right, _topRaycastLength, _groundLayer);

        if (hit.collider != null)
        {
            float newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x - newPosition, transform.position.y, transform.position.z);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, yVelocity);
        }
    }

    private void Flip()
    {
        _facingRight = !_facingRight;

        transform.Rotate(0f, 180f, 0f);
    }
}
