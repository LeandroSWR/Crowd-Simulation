using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds a method responsible for restarting the simulation
/// </summary>
public class SceneRestarter : MonoBehaviour {

    /// <summary>
    /// Restarts the Simulation
    /// </summary>
    public void RestartScene() => 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
