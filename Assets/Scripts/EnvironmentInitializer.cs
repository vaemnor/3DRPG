using System.Collections;
using UnityEngine;

public class EnvironmentInitializer : MonoBehaviour
{
    private GameObject environment;

    [SerializeField] private GameObject[] objectsToSpawn;
    [SerializeField] private int spawnCountPerRow = 0;
    [SerializeField] private int rowCount = 0;
    [SerializeField] private Vector3 initialSpawnLocation = Vector3.zero;
    [SerializeField] private float spawnDistance = 1f;

    private Vector3 currentSpawnLocation = Vector3.zero;
    private int randomIndex = 0;

    private void Awake()
    {
        environment = GameObject.Find("Environment");
    }

    private void Start()
    {
        currentSpawnLocation = initialSpawnLocation;
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        for (int currentRowCount = 0; currentRowCount < rowCount; currentRowCount++)
        {
            currentSpawnLocation = new Vector3(initialSpawnLocation.x, currentSpawnLocation.y, currentSpawnLocation.z);

            for (int currentSpawnCountPerRow = 0; currentSpawnCountPerRow < spawnCountPerRow; currentSpawnCountPerRow++)
            {
                randomIndex = Random.Range(0, objectsToSpawn.Length);

                GameObject objectToSpawn = Instantiate(objectsToSpawn[randomIndex], currentSpawnLocation, Quaternion.identity, environment.transform);

                currentSpawnLocation += new Vector3(spawnDistance, 0f, 0f);
            }

            currentSpawnLocation += new Vector3(0f, 0f, spawnDistance);
        }

        yield return null;
    }
}
