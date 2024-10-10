using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace EndlessRunner.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float _initialPlayerSpeed = 4f;
        [SerializeField] private float _maximumPlayerSpeed = 30f;
        [SerializeField] private float _playerSpeedIncreaseRate = .1f;
        [SerializeField] private float _jumpHeight = 1.0f;
        [SerializeField] private float _initialGravityValue = -9.81f;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private LayerMask _turnLayer;
        [SerializeField] private AnimationClip _slideAnimationClip;
        [SerializeField] private Animator _animator;
        
        private float _playerSpeed;
        private float _gravity;
        private Vector3 _movementDirection = Vector3.forward;
        private Vector3 _playerVelocity;
        private int _slidingAnimationId;
        
        private PlayerInput _playerInput;
        private InputAction _turnAction;
        private InputAction _slideAction;
        private InputAction _jumpAction;

        private CharacterController _controller;
        
        [SerializeField] private UnityEvent<Vector3> _turnEvent;
        private bool _sliding;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _controller = GetComponent<CharacterController>();

            _slidingAnimationId = Animator.StringToHash("Sliding");
            
            _turnAction = _playerInput.actions["Turn"];
            _slideAction = _playerInput.actions["Slide"];
            _jumpAction = _playerInput.actions["Jump"];
        }

        private void OnEnable()
        {
            _turnAction.performed += PlayerTurn;
            _slideAction.performed += PlayerSlide;
            _jumpAction.performed += PlayerJump;
        }

        private void OnDisable()
        {
            _turnAction.performed -= PlayerTurn;
            _slideAction.performed -= PlayerSlide;
            _jumpAction.performed -= PlayerJump;
        }


        private void Start()
        {
            _playerSpeed = _initialPlayerSpeed;
            _gravity = _initialGravityValue;
        }

        private void Update()
        {
            _controller.Move(transform.forward * _playerSpeed * Time.deltaTime);
            if (IsGrounded() && _playerVelocity.y < 0)
            {
                _playerVelocity.y = 0f;
            }

            _playerVelocity.y += _gravity * Time.deltaTime;
            _controller.Move(_playerVelocity * Time.deltaTime);
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                return;
            }
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * _movementDirection;
            _turnEvent?.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition.Value);
        }

        private void Turn(float turnValue, Vector3 turnPosition)
        {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            _controller.enabled = false;
            transform.position = tempPlayerPosition;
            _controller.enabled = true;
            
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            _movementDirection = transform.forward.normalized;
        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, _turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1)
                    || (type == TileType.RIGHT && turnValue == 1)
                    || (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if (_sliding || !IsGrounded()) return;
            StartCoroutine(Slide());
        }

        private IEnumerator Slide()
        {
            _sliding = true;
            // Shrink the collider
            Vector3 originalControllerCenter = _controller.center;
            Vector3 newControllerCenter = originalControllerCenter;
            _controller.height /= 2;
            newControllerCenter.y -= _controller.height / 2;
            _controller.center = newControllerCenter;
            // Play the sliding animation
            _animator.Play(_slidingAnimationId);
            yield return new WaitForSeconds(_slideAnimationClip.length);
            // Set the character controller collider back to normal after sliding
            _controller.height *= 2;
            _controller.center = originalControllerCenter;
            _sliding = false;
        }
        
        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (IsGrounded())
            {
                _playerVelocity.y += MathF.Sqrt(_jumpHeight * _gravity * -3f);
                _controller.Move(_playerVelocity * Time.deltaTime);
            }
        }

        private bool IsGrounded(float length = .2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= _controller.height / 2f;
            raycastOriginFirst.y += .1f;
            
            Vector3 raycastOriginSecond = raycastOriginFirst;
            
            raycastOriginFirst -= transform.forward * .2f;
            raycastOriginSecond += transform.forward * .2f;
            
            // Debug.DrawLine(raycastOriginFirst, Vector3.down, Color.green, 2f);
            // Debug.DrawLine(raycastOriginSecond, Vector3.down, Color.red, 2f);
            
            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, _groundLayer)
                || Physics.Raycast(raycastOriginSecond, Vector3.down, out RaycastHit hit2, length, _groundLayer))
            {
                return true;
            }
            
            return false;
        }
    }
}