using UnityEngine;

public class RandomPropSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Array of ground tile prefabs to spawn.")]
    [SerializeField] private GameObject[] propPrefabs;
    [Tooltip("Parent object to organize spawned ground tiles.")]
    [SerializeField] private GameObject propsParent;
    [Tooltip("Number of tiles to spawn per row.")]
    [SerializeField] private int spawnCountPerRow = 0;
    [Tooltip("Total number of rows of tiles")]
    [SerializeField] private int rowCount = 0;
    [Tooltip("Starting position for spawning tiles.")]
    [SerializeField] private Vector3 initialSpawnPosition = Vector3.zero;
    [Tooltip("Distance between each spawned tile.")]
    [SerializeField] private float spawnDistance = 1.0f;

    private void Start()
    {
        SpawnObjects();
    }

    private void SpawnObjects()
    {
        Vector3 spawnPosition = initialSpawnPosition;

        for (int currentRowCount = 0; currentRowCount < rowCount; currentRowCount++)
        {
            spawnPosition = new Vector3(initialSpawnPosition.x, spawnPosition.y, spawnPosition.z);

            for (int currentSpawnCountPerRow = 0; currentSpawnCountPerRow < spawnCountPerRow; currentSpawnCountPerRow++)
            {
                int randomIndex = Random.Range(0, propPrefabs.Length);

                Instantiate(propPrefabs[randomIndex], spawnPosition, Quaternion.identity, propsParent.transform);

                spawnPosition += new Vector3(spawnDistance, 0.0f, 0.0f);
            }

            spawnPosition += new Vector3(0.0f, 0.0f, spawnDistance);
        }
    }
}
