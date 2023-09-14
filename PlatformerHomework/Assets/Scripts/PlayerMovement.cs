using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;

    private float _movementAcceleration = 65;
    private float _maxMovementSpeed = 12;
    private float _groundLinearDrag = 10;
    private float _horizontalDirection;

    private float _jumpForce = 20f;
    [SerializeField] private LayerMask _groundLayer;
    private float _groundRaycastLength = 0.65f;
    private bool _onGround;
    [SerializeField] private Vector3 _groundRaycastOffset;
    private bool _canJump => _jumpBufferCounter > 0f && (_hangTimeCounter > 0f || _extraJumpsValue > 0);
    private float _airLinearDrag = 2.5f;
    private float _fallMultiplier = 8f;
    private float _lowJumpFallMultiplier = 3f;
    private int _extraJumps = 0;
    private int _extraJumpsValue;
    private float _hangTime = 0.2f;
    private float _hangTimeCounter;
    private float _jumpBufferLength = 0.1f;
    private float _jumpBufferCounter;

    [SerializeField]private Vector3 _edgeRaycastOffset;
    [SerializeField]private Vector3 _innerRaycastOffset;
    private float _topRaycastLength = 0.85f;
    private bool _canCornerCorrect;

    private bool _changingDirection =>
        (_rigidbody2D.velocity.x > 0f && _horizontalDirection < 0f) || (_rigidbody2D.velocity.x < 0f && _horizontalDirection > 0f);

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        _horizontalDirection = GetInput().x;

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferLength;
        }
        else
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        if (_canJump)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        MoveCharacter();

        if (_onGround)
        {
            ApplyGroundLinearDrag();
            _extraJumpsValue = _extraJumps;
            _hangTimeCounter = _hangTime;
        }
        else
        {
            ApplyAirLinearDrag();
            FallMultiplier();
            _hangTimeCounter -= Time.deltaTime;
        }

        if (_canCornerCorrect)
        {
            CornerCorrect(_rigidbody2D.velocity.y); 
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
    }

    private void Jump()
    {
        _hangTimeCounter = 0f;
        _jumpBufferCounter = 0f;

        if (!_onGround)
        {
            _extraJumpsValue--;
        }

        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0f);
        _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
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

        if(hit.collider != null)
        {
            float newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength, 
                transform.position - _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x + newPosition, transform.position.y, transform.position.z);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, yVelocity);
            return;
        }

        hit = Physics2D.Raycast(transform.position + _innerRaycastOffset + Vector3.up * _topRaycastLength, Vector3.right, _topRaycastLength, _groundLayer );

        if(hit.collider != null)
        {
            float newPosition = Vector3.Distance(new Vector3(hit.point.x, transform.position.y, 0f) + Vector3.up * _topRaycastLength,
                transform.position + _edgeRaycastOffset + Vector3.up * _topRaycastLength);
            transform.position = new Vector3(transform.position.x - newPosition, transform.position.y, transform.position.z);
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, yVelocity);
        }
    }
}
