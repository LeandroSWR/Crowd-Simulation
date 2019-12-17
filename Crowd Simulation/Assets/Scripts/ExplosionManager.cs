using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExplosionManager : MonoBehaviour {

    [Header("Key")]
    // The Keyboard's key to trigger explosions
    [SerializeField] private KeyCode explosionKey;

    [Header("Speeds")]
    // The Explosions speeds
    [SerializeField] private float explosionSpeed;
    [SerializeField] private float fireSpeed;

    [Header("Radiuses")]
    // All Explosions radiuses
    [SerializeField] private float killRadius;  // The radius at which NPC's get killed
    [SerializeField] private float stunRadius;  // The radius at which NPC's get stunned
    [SerializeField] private float panicRadius; // The radius at which NPC's start panicking
    [SerializeField] private float maxRadius;   // The radius at which the Explosions stop

    [Header("Timer")]
    // The Stun time to be applied if necessary
    [SerializeField] private float stunTime;
    public float StunTime { get => stunTime; }

    [Header("Explosion Prefab")]
    // The explosion's prefab object
    [SerializeField] private Transform explosionPrefab;

    [Header("Explosion Spawn Points")]
    // The parent of the explosions spawners
    [SerializeField] private Transform spawnsParent;

    [Header("UI Elements")]
    // The Text element which will contain the total kill count
    [SerializeField] private Text count;

    // The spawns for all explosions
    private List<Transform> explosionSpawns;

    // The amount of kills so far
    private int killCount;

    /// <summary>
    /// Awake is called before the game starts
    /// </summary>
    private void Awake() {

        // Get all spawns whilst loading
        GetSpanws();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update() {

        // Checks for User input every frame
        CheckInput();
    }

    /// <summary>
    /// Sets all spawns for the explosions
    /// </summary>
    private void GetSpanws() {

        // Initiate the List of spawns
        explosionSpawns = new List<Transform>();

        // Fill the spawns with each child of their parent
        foreach(Transform child in spawnsParent) {

            explosionSpawns.Add(child);
        }
    }

    /// <summary>
    /// Detect the Player's Input
    /// </summary>
    private void CheckInput() {

        // Detect the wanted button press
        if (Input.GetKeyDown(explosionKey)) {

            // Select a random NPC to Instantiate an explosion
            SelectExplosionLocation();
        }
    }

    /// <summary>
    /// Selects which NPC will explode
    /// </summary>
    private void SelectExplosionLocation() {

        // Get a random index based on the number of NPCs
        int npcIndex = Random.Range(0, explosionSpawns.Count);

        // Get the 'ExplosionBehaviour' script attached to the newly Instantiated explosion GameObject
        ExplosionBehaviour eb = 
            Instantiate(explosionPrefab,
            spawnsParent.GetChild(npcIndex).position,
            Quaternion.identity).GetComponent<ExplosionBehaviour>();

        // Pass variables from this script towards the last explosion
        eb.AssignVariables(explosionSpeed, fireSpeed, killRadius, stunRadius, panicRadius, maxRadius, this);

        // Activate the explosion
        eb.Explode();
    }

    /// <summary>
    /// Updates the Kill Count and its UI display
    /// </summary>
    public void UpdateKillCount() {

        // Increments the Kill Count by 1
        killCount++;

        // Updates the display
        count.text = killCount.ToString();
    }
}
