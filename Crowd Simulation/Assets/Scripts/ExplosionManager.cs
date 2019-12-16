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
    [SerializeField] private float maxRadius;   // The radius at which the Explosions stop

    // The explosion's prefab object
    [SerializeField] private Transform explosionPrefab;

    // The agents' parent object
    [SerializeField] private Transform agentsParent;

    void Update() {

        CheckInput();
    }

    /// <summary>
    /// Detect the Player's Input
    /// </summary>
    private void CheckInput() {

        // Detect the wanted button press
        if (Input.GetKeyDown(explosionKey)) {

            // Select a random NPC to Instantiate an explosion
            SelectTerrorist();
        }
    }

    /// <summary>
    /// Selects which NPC will explode
    /// </summary>
    private void SelectTerrorist() {

        // Get a random index based on the number of NPCs
        int npcIndex = Random.Range(0, 500);

        // Get the 'ExplosionBehaviour' script attached to the newly Instantiated explosion GameObject
        ExplosionBehaviour eb = 
            Instantiate(explosionPrefab,
            agentsParent.GetChild(npcIndex).position,
            Quaternion.identity).GetComponent<ExplosionBehaviour>();

        // Pass variables from this script towards the last explosion
        eb.AssignVariables(explosionSpeed, fireSpeed, killRadius, stunRadius, panicRadius, maxRadius);

        // Activate the explosion
        eb.Explode();
    }
}
