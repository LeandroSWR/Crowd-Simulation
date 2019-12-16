using UnityEngine;

public class AgentPool : MonoBehaviour
{
    // Array with 500 agents
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
