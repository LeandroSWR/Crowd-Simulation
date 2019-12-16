using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake() {

        // Reposition the explosion prefab accordingly
        transform.position = new Vector3(transform.position.x,
                                         transform.localScale.y, 
                                         transform.position.z);
    }

    void Update() {

        // Change scale every frame
        Explode();
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
        float killRadius, float stunRadius, float panicRadius) {

        this.explosionSpeed = explosionSpeed;
        this.fireSpeed = fireSpeed;
        this.killRadius = killRadius;
        this.stunRadius = stunRadius;
        this.panicRadius = panicRadius;
    }

    /// <summary>
    /// Start the explosion "animation"
    /// </summary>
    /// <param name="explosion">The explosion object</param>
    /// <returns></returns>
    private void Explode() {

        if (desiredScale.x >= killRadius) return;

        desiredScale += Vector3.one * (Time.deltaTime * explosionSpeed);
        desiredScale.y = transform.localScale.y;

        transform.localScale = desiredScale;
    }
}
