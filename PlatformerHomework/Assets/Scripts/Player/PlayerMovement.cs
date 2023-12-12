using System;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Transform _playerTransform;

    [Header("Movement")]
    private float _horizontal;
    private float _speed = 8f;
    private bool _facingRight = true;
    private float _groundLinearDrag = 5f;
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

    [Header("CheckCollisions")]
    [SerializeField] private float _groundRaycastLength = 0.65f;
    [SerializeField] private Vector3 _edgeRaycastOffset;
    [SerializeField] private Vector3 _innerRaycastOffset;
    private float _topRaycastLength = 0.85f;
    private bool _canCornerCorrect;

    [Header("Dashing")]
    private bool _canDash = true;
    private bool _isDashing;
    private float _dashingPower = 30f;
    private float _dashingTime = 0.3f;
    private float _dashingCooldown = 0.5f;

    [Header("Wall slide")]
    [SerializeField] private Transform _wallCheck;
    [SerializeField] private LayerMask _wallLayer;

    private bool _isWallSliding;
    private float _wallSlidingSpeed = 2f;
    private bool _isWallJumping;
    private float _wallJumpingTime = 0.2f;
    private float _wallJumpingCounter = 1f;
    private float _wallJumpingDuration = 0.4f;
    private Vector2 _wallJumpingPower = new Vector2(100f, 16f);

    [Header("Camera")]
    [SerializeField] private GameObject _cameraFollowGo;
    private CameraFollowingObjectScript _cameraFollowObject;
    private float _fallSpeedYDampingChangeTreshold;

    private bool _changingDirection =>
       (_rigidbody2D.velocity.x > 0f && _horizontalDirection < 0f) || (_rigidbody2D.velocity.x < 0f && _horizontalDirection > 0f);

    private void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _playerTransform = GetComponent<Transform>();

        _cameraFollowObject = _cameraFollowGo.GetComponent<CameraFollowingObjectScript>();

        _fallSpeedYDampingChangeTreshold = CameraManager.instance._fallSpeedYDampingChangeTheshold;
    }

    private void Update()
    {
        _horizontalDirection = GetInput().x;

        if (_isDashing)
        {
            return;
        }

        _horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetAxisRaw("Horizontal") > 0 && !_facingRight)
        {
            Flip();
            _cameraFollowObject.CallTurn();
        }
        else if (Input.GetAxisRaw("Horizontal") < 0 && _facingRight)
        {
            Flip();
            _cameraFollowObject.CallTurn();
        }

        if (Input.GetButtonDown("Jump"))
        {
            _jumpBufferCounter = _jumpBufferLength;
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

        if (Input.GetKeyDown(KeyCode.Mouse1) && _canDash)
        {
            StartCoroutine(Dash());
        }

        if (_onGround && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpYDamping(true);
        }

        if (!_onGround && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
            CameraManager.instance.LerpedFromPlayerFalling = false;

            CameraManager.instance.LerpYDamping(false);
        }
    }

    private void FixedUpdate()
    {
        WallSlide();
        WallJump();
        CheckCollisions();

        if (_isDashing)
        {
            return;
        }

        _rigidbody2D.velocity = new Vector2(_horizontal * _speed, _rigidbody2D.velocity.y);

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

    private void Jump()
    {
        _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, 0f);
        _rigidbody2D.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
    }

    private IEnumerator Dash()
    {
        int facingLeftNumber = -1;
        _canDash = false;
        _isDashing = true;
        float originalGravity = _rigidbody2D.gravityScale;
        _rigidbody2D.gravityScale = 0f;
        _animator.StopPlayback();
        _animator.Play("DashAnimation");

        if (_facingRight)
        {
            _rigidbody2D.velocity = new Vector2(transform.localScale.x * _dashingPower, 0f);
        }
        else if (!_facingRight)
        {
            _rigidbody2D.velocity = new Vector2(transform.localScale.x * _dashingPower * facingLeftNumber, 0f);
        }

        yield return new WaitForSeconds(_dashingTime);
        _rigidbody2D.gravityScale = originalGravity;
        _isDashing = false;
        _animator.StopPlayback();
        yield return new WaitForSeconds(_dashingCooldown);
        _canDash = true;
    }

    private bool IsWalled()
    {
        float circleRadius = 0.2f;

        return Physics2D.OverlapCircle(_wallCheck.position, circleRadius, _wallLayer);
    }

    private void WallSlide()
    {
        if (IsWalled() && !_onGround && Input.GetAxisRaw("Horizontal") != 0)
        {
            _isWallSliding = true;
            _rigidbody2D.velocity = new Vector2(_rigidbody2D.velocity.x, Mathf.Clamp(_rigidbody2D.velocity.y, -_wallSlidingSpeed, float.MaxValue));
            _spriteRenderer.flipX = true;
        }
        else
        {
            _isWallSliding = false;
            _spriteRenderer.flipX = false;
        }
    }

    private void WallJump()
    {
        if (_isWallSliding)
        {
            _isWallJumping = false;
            _wallJumpingCounter = _wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            _wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && _wallJumpingCounter > 0)
        {
            _isWallJumping = true;

            _rigidbody2D.velocity = new Vector2(-Input.GetAxisRaw("Horizontal") * _wallJumpingPower.x, _wallJumpingPower.y);
            
            _wallJumpingCounter -= 1;

            Invoke(nameof(StopWallJumping), _wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        _isWallJumping = false;
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
        _rigidbody2D.drag = _groundLinearDrag;
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
