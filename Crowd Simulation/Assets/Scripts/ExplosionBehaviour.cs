using System.Collections;
using UnityEngine;

/// <summary>
/// Class responsible for all explosion behaviours
/// </summary>
public class ExplosionBehaviour : MonoBehaviour {

    // The Explosions speeds
    private float explosionSpeed;
    private float fireSpeed;

    // All Explosions radiuses
    private float killRadius;  // The radius at which NPC's get killed
    private float stunRadius;  // The radius at which NPC's get stunned
    private float panicRadius; // The radius at which NPC's start panicking

    // The scale to be applied
    private Vector3 desiredScale;

    // The current explosion radius
    private float currentRadius;

    // The script that spawned this explosion
    private ExplosionManager explosionManager;

    /// <summary>
    /// Awake is called before the game starts
    /// </summary>
    private void Awake() {

        // Reposition the explosion prefab accordingly
        transform.position = new Vector3(transform.position.x,
                                         transform.localScale.y, 
                                         transform.position.z);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update() {

        // Spread the fire / Change scale
        SpreadFire();
    }

    /// <summary>
    /// Assign Explosion Manager variables to an Explosion
    /// </summary>
    /// <param name="explosionSpeed">The speed of the explosion</param>
    /// <param name="fireSpeed">The speed of the fire spread</param>
    /// <param name="killRadius">The radius to kill NPCs</param>
    /// <param name="stunRadius">The radius to stun NPCs</param>
    /// <param name="panicRadius">The radius for the NPCs to panic</param>
    public void AssignVariables(float explosionSpeed, float fireSpeed, 
        float killRadius, float stunRadius, float panicRadius,
        ExplosionManager explosionManager) {

        this.explosionSpeed = explosionSpeed;
        this.fireSpeed = fireSpeed;
        this.killRadius = killRadius;
        this.stunRadius = stunRadius;
        this.panicRadius = panicRadius;

        this.explosionManager = explosionManager;
    }

    /// <summary>
    /// Start the explosion "animation"
    /// </summary>
    /// <param name = "explosion" > The explosion object</param>
    /// <returns></returns>
    private void SpreadFire() {

        // Save this explosion radius
        currentRadius = desiredScale.x / 2.0f;

        // If our current radius is less than the one to kill NPCs...
        if (currentRadius < killRadius) {

            // ...Increase the radius with the appropriate speed
            desiredScale += Vector3.one * (Time.deltaTime * explosionSpeed);

        // If the current radius is bigger or equals the radius to kill NPCs...
        } else {

            // ...Increase the radius with a faster speed
            desiredScale += Vector3.one * (Time.deltaTime * fireSpeed);
        }

        // Apply the correct scale on the 'y' axis (by resetting it)
        desiredScale.y = transform.localScale.y;

        // Apply the new scale to this explosion GameObject
        transform.localScale = desiredScale;
    }

    /// <summary>
    /// Creates an OverlapSphere at the center of the Instantiated Explosion
    /// </summary>
    public void Explode() {

        // Gets all colliders around this GameObject (with a big enough radius)
        Collider[] npcs = Physics.OverlapSphere(transform.position, 500);

        // Iterates through all Colliders found...
        foreach(Collider c in npcs) {

            // ...finds the ones with the 'NPC' tag...
            if (c.CompareTag("NPC")) {

                // ...fetch its NPC Behaviour script...
                NPCBehaviour npc = c.GetComponent<NPCBehaviour>();

                // ...if the distance is smaller than the kill radius on the moment of the explosion...
                if (Vector3.Distance(transform.position, npc.transform.position) <= killRadius) {

                    // ...'kill' the npc...
                    npc.IsDead = true;

                    // ...and update the UI display...
                    explosionManager.UpdateKillCount();

                    StartCoroutine(NPCAddForce(npc, 500, 100));

                    // ...if the distance is smaller than the stun radius on the moment of the explosion...
                } else if (Vector3.Distance(transform.position, npc.transform.position) <= stunRadius) {

                    // ...set its stun time...
                    npc.StunTime = explosionManager.StunTime;

                    // ...set the npc as Stunned...
                    npc.IsStunned = true;

                    // ...and also as Panicking (although he won't be able to run)
                    npc.IsPanicking = true;

                // ...if the distance is smaller than the panic radius on the moment of the explosion...
                } else if (Vector3.Distance(transform.position, npc.transform.position) <= panicRadius) {

                    // ...set the npc as Panicking (will be able to run)
                    npc.IsPanicking = true;
                }
            }
        }
    }

    /// <summary>
    /// Gets called when the Collider 'other' enters the trigger
    /// </summary>
    /// <param name="other">The Collider we hit</param>
    private void OnTriggerEnter(Collider other) {

        // If the 'other' Collider has an 'NPC' tag...
        if (other.CompareTag("NPC")) {

            // ...fetch its NPC Behaviour script...
            NPCBehaviour npc = other.GetComponent<NPCBehaviour>();

            // If the distance between the Explosion and the NPC is smaller (or equal)
            // than that of the current explosion radius...
            if (Vector3.Distance(transform.position, npc.transform.position) <= currentRadius) {

                // Verify if the NPC is already dead
                if (!npc.IsDead) {

                    // ...'kill' the NPC...
                    npc.IsDead = true;

                    // ...and update the UI display
                    explosionManager.UpdateKillCount();
                }

                // If the distance between the Explosion and the NPC is bigger
                // than that of the current explosion radius...
            } else {

                // ...set the npc as Panicking
                npc.IsPanicking = true;
            }
        }
    }

    /// <summary>
    /// OnTriggerStay is called almost all the frames for every Collider other that is touching the trigger.
    /// </summary>
    /// <param name="other">The Collider we're hitting</param>
    private void OnTriggerStay(Collider other) {

        // If the 'other' Collider has an 'NPC' tag...
        if (other.CompareTag("NPC")) {

            // ...fetch its NPC Behaviour script...
            NPCBehaviour npc = other.GetComponent<NPCBehaviour>();

            // If the distance between the Explosion and the NPC is smaller (or equal)
            // than that of the current explosion radius...
            if (Vector3.Distance(transform.position, npc.transform.position) <= currentRadius) {

                // Verify if the NPC is already dead
                if (!npc.IsDead) {

                    // ...'kill' the NPC...
                    npc.IsDead = true;

                    // ...and update the UI display
                    explosionManager.UpdateKillCount();

                    StartCoroutine(NPCAddForce(npc, 500, 0));
                }
            }
        }
    }

    /// <summary>
    /// Applies forces to an NPC when hit by an explosion
    /// </summary>
    /// <param name="npc">The NPC we hit</param>
    /// <param name="explosionForce">The explosion force</param>
    /// <param name="verticalForce">The vertical explosion force</param>
    /// <returns>Skips 1 frame</returns>
    private IEnumerator NPCAddForce(NPCBehaviour npc, float explosionForce, float verticalForce) {

        // Waits for the next frame
        yield return 0;

        // Adds an explosion force based on the center of the explosion
        npc.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, killRadius, verticalForce, ForceMode.Acceleration);
    }

    private void OnDrawGizmos() {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stunRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, killRadius);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}
