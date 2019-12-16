using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionManager : MonoBehaviour {

    // The Keyboard's key to trigger explosions
    [SerializeField] private KeyCode explosionKey;

    // The Explosions speeds
    [SerializeField] private float explosionSpeed;
    [SerializeField] private float fireSpeed;

    // All Explosions radiuses
    [SerializeField] private float killRadius;  // The radius at which NPC's get killed
    [SerializeField] private float stunRadius;  // The radius at which NPC's get stunned
    [SerializeField] private float panicRadius; // The radius at which NPC's start panicking

    // The explosion's prefab object
    [SerializeField] private Transform explosionPrefab;

    // The agents' parent object
    [SerializeField] private Transform agentsParent;

    void Start() {


    }

    void Update() {

        CheckInput();
    }

    /// <summary>
    /// Detect the Player's Input
    /// </summary>
    private void CheckInput() {

        if (Input.GetKeyDown(explosionKey)) {

            SelectTerrorist();
        }
    }

    /// <summary>
    /// Selects which NPC will explode
    /// </summary>
    private void SelectTerrorist() {

        // Get a random index based on the number of NPCs
        int npcIndex = Random.Range(0, 500);

        ExplosionBehaviour eb = 
            Instantiate(explosionPrefab,
            agentsParent.GetChild(npcIndex).position,
            Quaternion.identity).GetComponent<ExplosionBehaviour>();

        eb.AssignVariables(explosionSpeed, fireSpeed, killRadius, stunRadius, panicRadius);
    }
}
