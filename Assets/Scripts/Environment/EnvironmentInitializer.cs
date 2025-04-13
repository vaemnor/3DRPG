using UnityEngine;

public class EnvironmentInitializer : MonoBehaviour
{
    [Header("General Spawn Settings")]
    [Tooltip("Number of tiles to spawn per row.")]
    [SerializeField] private int spawnCountPerRow = 0;
    [Tooltip("Total number of rows of tiles")]
    [SerializeField] private int rowCount = 0;
    [Tooltip("Starting position for spawning tiles.")]
    [SerializeField] private Vector3 initialSpawnPosition = Vector3.zero;
    [Tooltip("Distance between each spawned tile.")]
    [SerializeField] private float spawnDistance = 1.0f;
    
    [Header("Ground Spawn Settings")]
    [Tooltip("Array of ground tile prefabs to spawn.")]
    [SerializeField] private GameObject[] groundPrefabs;
    [Tooltip("Parent object to organize spawned ground tiles.")]
    [SerializeField] private GameObject groundParent;

    [Header("Wall Spawn Settings")]
    [Tooltip("Array of wall tile prefabs to spawn.")]
    [SerializeField] private GameObject[] wallPrefabs;
    [Tooltip("Prefab used for wall corners.")]
    [SerializeField] private GameObject wallCornerPrefab;
    [Tooltip("Parent object to organize spawned wall tiles.")]
    [SerializeField] private GameObject wallsParent;

    private void Start()
    {
        SpawnGround();
        SpawnWalls();
    }

    private void SpawnGround()
    {
        Vector3 spawnPosition = initialSpawnPosition;

        for (int currentRowCount = 0; currentRowCount < rowCount; currentRowCount++)
        {
            spawnPosition = new Vector3(initialSpawnPosition.x, spawnPosition.y, spawnPosition.z);

            for (int currentSpawnCountPerRow = 0; currentSpawnCountPerRow < spawnCountPerRow; currentSpawnCountPerRow++)
            {
                int randomIndex = Random.Range(0, groundPrefabs.Length);

                Instantiate(groundPrefabs[randomIndex], spawnPosition, Quaternion.identity, groundParent.transform);

                spawnPosition += new Vector3(spawnDistance, 0.0f, 0.0f);
            }

            spawnPosition += new Vector3(0.0f, 0.0f, spawnDistance);
        }
    }

    private void SpawnWalls()
    {
        SpawnWallRows();
        SpawnWallCorners();
    }

    private void SpawnWallRows()
    {
        // bottom
        for (int i = 0; i < spawnCountPerRow - 1; i++)
        {
            int randomIndex = Random.Range(0, wallPrefabs.Length);
            Vector3 spawnPosition = initialSpawnPosition + new Vector3((i + 0.5f) * spawnDistance, initialSpawnPosition.y, -0.5f * spawnDistance);
            Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
            Instantiate(wallPrefabs[randomIndex], spawnPosition, rotation, wallsParent.transform);
        }

        // left
        for (int i = 0; i < rowCount - 1; i++)
        {
            int randomIndex = Random.Range(0, wallPrefabs.Length);
            Vector3 spawnPosition = initialSpawnPosition + new Vector3(-0.5f * spawnDistance, initialSpawnPosition.y, (i + 0.5f) * spawnDistance);
            Quaternion rotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);
            Instantiate(wallPrefabs[randomIndex], spawnPosition, rotation, wallsParent.transform);
        }

        // top
        for (int i = 0; i < spawnCountPerRow - 1; i++)
        {
            int randomIndex = Random.Range(0, wallPrefabs.Length);
            Vector3 spawnPosition = initialSpawnPosition + new Vector3((i + 0.5f) * spawnDistance, initialSpawnPosition.y, (rowCount - 1 + 0.5f) * spawnDistance);
            Quaternion rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
            Instantiate(wallPrefabs[randomIndex], spawnPosition, rotation, wallsParent.transform);
        }

        // right
        for (int i = 0; i < rowCount - 1; i++)
        {
            int randomIndex = Random.Range(0, wallPrefabs.Length);
            Vector3 spawnPosition = initialSpawnPosition + new Vector3((spawnCountPerRow - 1 + 0.5f) * spawnDistance, initialSpawnPosition.y, (i + 0.5f) * spawnDistance);
            Quaternion rotation = Quaternion.Euler(0.0f, 270.0f, 0.0f);
            Instantiate(wallPrefabs[randomIndex], spawnPosition, rotation, wallsParent.transform);
        }
    }

    private void SpawnWallCorners()
    {
        Vector3 bottomLeft = initialSpawnPosition + new Vector3(-0.5f * spawnDistance, initialSpawnPosition.y, -0.5f * spawnDistance);
        Vector3 topLeft = initialSpawnPosition + new Vector3(-0.5f * spawnDistance, initialSpawnPosition.y, (rowCount - 1 + 0.5f) * spawnDistance);
        Vector3 topRight = initialSpawnPosition + new Vector3((spawnCountPerRow - 1 + 0.5f) * spawnDistance, initialSpawnPosition.y, (rowCount - 1 + 0.5f) * spawnDistance);
        Vector3 bottomRight = initialSpawnPosition + new Vector3((spawnCountPerRow - 1 + 0.5f) * spawnDistance, initialSpawnPosition.y, -0.5f * spawnDistance);

        Instantiate(wallCornerPrefab, bottomLeft, Quaternion.Euler(0.0f, 0.0f, 0.0f), wallsParent.transform);
        Instantiate(wallCornerPrefab, topLeft, Quaternion.Euler(0.0f, 90.0f, 0.0f), wallsParent.transform);
        Instantiate(wallCornerPrefab, topRight, Quaternion.Euler(0.0f, 180.0f, 0.0f), wallsParent.transform);
        Instantiate(wallCornerPrefab, bottomRight, Quaternion.Euler(0.0f, 270.0f, 0.0f), wallsParent.transform);
    }

}
