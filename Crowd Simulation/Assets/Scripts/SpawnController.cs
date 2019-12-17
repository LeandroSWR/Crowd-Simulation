using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Class that spawns NPCs
/// </summary>
public class SpawnController : MonoBehaviour {

    // Pool of agents
    [SerializeField] private AgentPool pool;

    // How many agents we want to spawn
    [Range(100, 500)]
    [SerializeField]
    private float nAgentsToSpawn;

    // How long to wait between each spawn
    [SerializeField] private float spawnDelay;

    // Array with all possible spawn points
    private Transform[] spawnPoints;

    // How long to wait between each spawn
    private WaitForSeconds waitDelay;

    // Start is called before the first frame update
    void Start() {

        // Set the spawn delay to be 0.5 seconds;
        waitDelay = new WaitForSeconds(spawnDelay);

        // Get all the spawn points
        spawnPoints = GetComponentsInChildren<Transform>();

        StartCoroutine(SpawnAgents());
    }

    /// <summary>
    /// Spawn each agent with a small delay
    /// </summary>
    /// <returns>Wait a certain amount of time to spawn the next agent</returns>
    private IEnumerator SpawnAgents() {

        // Go through our pool of agents
        for (int i = 0; i < nAgentsToSpawn; i++) {

            // Select a random spawn point to spawn the agent
            pool.Agents[i].transform.position = spawnPoints[Random.Range(1, spawnPoints.Length)].position;

            // Set the agent to be active
            pool.Agents[i].gameObject.SetActive(true);

            // Wait a certain amount of time to spawn the next agent
            yield return waitDelay;
        }
    }
}
