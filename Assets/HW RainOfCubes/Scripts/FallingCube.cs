using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Renderer))]
public class FallingCube : MonoBehaviour
{
    [Header("Цвета")]
    [SerializeField] private Color _defaultColor = Color.green;
    [SerializeField] private Color _collidedColor = Color.red;

    [Header("Время жизни после касания с платформой")]
    [SerializeField] private float _minLifeTime = 2f;
    [SerializeField] private float _maxLifeTime = 5f;
    [SerializeField] private float _lifeTimer;

    private bool _hasTouchedPlatform = false;
    private bool _isDying;
    
    private Rigidbody _rigidbody;
    private Renderer _renderer;

    private Action<FallingCube> _returnToPool;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (_isDying == false)
            return;

        _lifeTimer -= Time.deltaTime;

        if (_lifeTimer <= 0)
        {
            _isDying = false;
            _returnToPool?.Invoke(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasTouchedPlatform)
            return;

        if (collision.collider.TryGetComponent<Platform>(out _))
        {
            _hasTouchedPlatform = true;
            SetColor(_collidedColor);
            _lifeTimer = UnityEngine.Random.Range(_minLifeTime, _maxLifeTime);
            _isDying = true;
        }
    }

    public void Initialize(Action<FallingCube> returnToPool)
    {
        _returnToPool = returnToPool;
    }

    public void ResetState()
    {
        _hasTouchedPlatform = false;
        _isDying = false;
        _lifeTimer = 0;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        SetColor(_defaultColor);
    }

    private void SetColor(Color color)
    {
        _renderer.material.color = color;
    }
}
