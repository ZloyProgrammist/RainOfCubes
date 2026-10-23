using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Spawner : MonoBehaviour
{
    [Header("Префаб объекта")]
    [SerializeField] private FallingCube _prefab;

    [Header("Точки спавна (пустышки над платформами)")]
    [SerializeField] private List<GameObject> _spawnPoints;

    [Header("Интервал спавна (сек)")]
    [SerializeField] private float _repeatRate = 5f;

    [Header("Настройки пула объектов")]
    [SerializeField] private int _poolCapacity = 5;
    [SerializeField] private int _poolMaxSize = 10;

    private ObjectPool<FallingCube> _pool;
    private bool _isPendingSpawn;
    private bool _isDisposed;
    private bool _isSpawning = false;
    private Coroutine _spawnRoutine;

    private void Awake()
    {
        _pool = new ObjectPool<FallingCube> (
        createFunc: () =>
        {
            var obj = Instantiate(_prefab, transform);
            obj.Initialize(OnObjectReturned);

            return obj;
        },
        actionOnGet: (obj) => ActionOnGet(obj),
        actionOnRelease: (obj) => obj.gameObject.SetActive(false),
        actionOnDestroy: (obj) => Destroy(obj),
        collectionCheck: true,
        defaultCapacity: _poolCapacity,
        maxSize: _poolMaxSize);
    }

    private void Start()
    {
        _isSpawning = true;
        _spawnRoutine = StartCoroutine(SpawnLoop());
    }

    private void OnDestroy()
    {
        _isSpawning = false;

        if (_spawnRoutine != null)
            StopCoroutine(_spawnRoutine);

        _isDisposed = true;
        _pool?.Dispose();
    }

    private IEnumerator SpawnLoop()
    {
        while (_isSpawning)
        {
            TrySpawnObject();
            yield return new WaitForSeconds(_repeatRate);
        }
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

    private void ActionOnGet(FallingCube cube)
    {
        cube.transform.position = GetRandomSpawnPointPosition();
        cube.ResetState();
        cube.gameObject.SetActive(true);
    }

    private void OnObjectReturned(FallingCube cube)
    {
        if (_isDisposed || cube == null) 
            return;

        _pool.Release(cube);

        if (_isPendingSpawn)
        {
            _isPendingSpawn = false;
            TrySpawnObject();
        }
    }

    private Vector3 GetRandomSpawnPointPosition()
    {
        int randomIndex = Random.Range(0, _spawnPoints.Count);

        return _spawnPoints[randomIndex].transform.position;
    }
}
