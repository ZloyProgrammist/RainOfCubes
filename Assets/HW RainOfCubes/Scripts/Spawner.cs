using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [Header("Префаб объекта")]
    [SerializeField] private GameObject _prefab;

    [Header("Точки спавна (пустышки над платформами)")]
    [SerializeField] private List<GameObject> _spawnPoints;

    [Header("Интервал спавна (сек)")]
    [SerializeField] private float _firstSpawnDelay = 0f;
    [SerializeField] private float _repeatRate = 5f;

    [Header("Настройки пула объектов")]
    [SerializeField] private int _poolCapacity = 5;
    [SerializeField] private int _poolMaxSize = 10;

    private ObjectPool<GameObject> _pool;
    private bool _isPendingSpawn;
    private bool _isDisposed;

    private void Awake()
    {
        _pool = new ObjectPool<GameObject> (
        createFunc: () =>
        {
            var obj = Instantiate(_prefab, transform);

            if (obj.TryGetComponent(out FallingCube component))
                component.Initialize(OnObjectReturned);

            return obj;
        },
        actionOnGet: (obj) => ActionOnGet(obj),
        actionOnRelease: (obj) => obj.SetActive(false),
        actionOnDestroy: (obj) => Destroy(obj),
        collectionCheck: true,
        defaultCapacity: _poolCapacity,
        maxSize: _poolMaxSize);
    }

    private void Start()
    {
        InvokeRepeating(nameof(TrySpawnObject), _firstSpawnDelay, _repeatRate);
    }

    private void TrySpawnObject()
    {
        if (_pool.CountActive >= _poolMaxSize)
        {
            _isPendingSpawn = true;
            return;
        }

        _pool.Get();
    }

    private void ActionOnGet(GameObject obj)
    {
        obj.transform.position = GetRandomSpawnPointPosition();

        if (obj.TryGetComponent<FallingCube>(out var cube))
            cube.ResetState();

        obj.SetActive(true);
    }

    private void OnObjectReturned(FallingCube cube)
    {
        if (_isDisposed || cube == null) 
            return;

        _pool.Release(cube.gameObject);

        if (_isPendingSpawn)
        {
            _isPendingSpawn = false;
            TrySpawnObject();
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(TrySpawnObject));
        _isDisposed = true;
        _pool?.Dispose();
    }

    private Vector3 GetRandomSpawnPointPosition()
    {
        int randomIndex = Random.Range(0, _spawnPoints.Count);

        return _spawnPoints[randomIndex].transform.position;
    }
}
