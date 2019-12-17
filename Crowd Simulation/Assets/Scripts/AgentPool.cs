using UnityEngine;

/// <summary>
/// Class that saves all agents in this GameObject
/// </summary>
public class AgentPool : MonoBehaviour
{
    // Array with all agents
    public Transform[] Agents { get; private set; }

    /// <summary>
    /// When the script is loaded
    /// </summary>
    private void Awake()
    {
        // Get all the agents to the array
        Agents = GetComponentsInChildren<Transform>(true);
    }
}
