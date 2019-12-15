using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using LibGameAI.DecisionTrees;

public class NPCBehaviour : MonoBehaviour
{
    // Arrays with the location of all areas of interest
    [SerializeField] private Transform[] eatingAreas;
    [SerializeField] private TablesManager[] restingAreas;
    [SerializeField] private Transform[] stages;

    // How fast the agent moves
    [SerializeField] private float speed = 10f;

    
    private readonly float maximumExcitementLevel = 100f;   // How much excitement the agent can have
    private float excitementLevel;  // The current level of excitement
    private float excitementStep;   // How much the excitement decreases each second

    
    private readonly float maximumStaminaLevel = 100f;  // How much stamina the agent can have
    private float staminaLevel; // The current level of stamina 
    private float staminaStep;  // How much the stamina decreases each second
    private bool isResting; // If the agent is currently resting

    
    private readonly float maximumFullnessLevel = 100f; // How full the agent can be
    private float fullnessLevel;    // The current level of fullness
    private float fullnessSpet; // How much the fullness decreases each second
    private bool isEating;  // If the agent is currently eating

    // The multiplier for increasing the values when resting or eating
    private float multiplier;

    // The current stage the agent is watching
    private Transform currentStage;

    // Reference to our Nav Mesh Agent
    private NavMeshAgent agent;


    private IDecisionTreeNode isAgentHungry;
    private IDecisionTreeNode agentIsHungry;
    private IDecisionTreeNode isAgentTired;
    private IDecisionTreeNode agentIsTired;
    private IDecisionTreeNode isAgentExcited;
    private IDecisionTreeNode agentNotExcited;

    /// <summary>
    /// Awake is called when the script is loaded
    /// </summary>
    private void Awake()
    {
        agentIsHungry = new ActionNode(AgentGoEat);
        
        agentIsTired = new ActionNode(AgentGoRest);
        
        agentNotExcited = new ActionNode(AgentChangeStage);

        isAgentExcited = new DecisionNode(IsAgentExcited, new ActionNode(() => { }), agentNotExcited);
        
        isAgentTired = new DecisionNode(IsAgentTired, agentIsTired, isAgentExcited);
        
        isAgentHungry = new DecisionNode(IsAgentHungry, agentIsHungry, isAgentTired);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        // Initialize `agent` by getting the NavMeshAgent component
        agent = this.GetComponent<NavMeshAgent>();

        // Define the initial values for the excitement level
        excitementLevel = 0f;
        // Define the step amount for the excitement level
        excitementStep = Random.Range(1f, 3f);

        // Define the initial values for the stamina level
        staminaLevel = Random.Range(50f, 100f);
        // Define the step amount for the stamina level
        staminaStep = Random.Range(1f, 3f);

        // Define the initial values for the fullness level
        fullnessLevel = Random.Range(25f, 90f);
        // Define the step amount for the fullness level
        fullnessSpet = Random.Range(1f, 4f);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        (isAgentHungry.MakeDecision() as ActionNode).Execute();

        UpdateExcitementLevel();
        UpdateStaminaLevel();
        UpdateFullness();
    }

    /// <summary>
    /// Checks if the agent is currently hungry
    /// </summary>
    /// <returns>True if the agent is hungry or eating, false otherwise</returns>
    private bool IsAgentHungry() => fullnessLevel == 0f || isEating;

    /// <summary>
    /// Makes the agent go eat till he's full again
    /// </summary>
    private void AgentGoEat() {
        
    }

    /// <summary>
    /// Checks if the agent is currently tired
    /// </summary>
    /// <returns>True if the agent is tired or resting, false otherwise</returns>
    private bool IsAgentTired() => staminaLevel == 0f || isResting;

    /// <summary>
    /// Makes the agent go rest till he's got stamina again
    /// </summary>
    private void AgentGoRest()
    {

    }

    /// <summary>
    /// Checks if the agent is currently excited
    /// </summary>
    /// <returns>True if the agent is excited, false otherwise</returns>
    private bool IsAgentExcited() => excitementLevel > 0f;

    /// <summary>
    /// Makes the agent go to another stage
    /// </summary>
    private void AgentChangeStage()
    {
        float stageSelect = Random.Range(1, 100);

        Vector3 currentStagePosition;

        if (currentStage == null)
        {
            currentStage = stageSelect > 60 ? stages[0] : stageSelect > 28 ? stages[1] : stages[2];
        } else
        {
            currentStagePosition = currentStage.position;

            while (currentStage.position == currentStagePosition)
            {
                currentStage = stageSelect > 60 ? stages[0] : stageSelect > 28 ? stages[1] : stages[2];
            }
        }

        // Increasse the excitement level when we switch stage
        excitementLevel = Random.Range(80f, 90f);

        StartCoroutine(MoveToCurrentStage());
    }

    private IEnumerator MoveToCurrentStage()
    {
        do
        {
            agent.destination = currentStage.position;
            yield return null;
        } while (true);
    }

    /// <summary>
    /// Decrease the current excitement level by `excitementStep` each second
    /// </summary>
    private void UpdateExcitementLevel()
    {
        excitementLevel = Mathf.Clamp(
            excitementLevel - (excitementStep * Time.deltaTime),
            0f, maximumExcitementLevel);
    }

    /// <summary>
    /// Decrease the current stamina level by `staminaStep` each second
    /// </summary>
    private void UpdateStaminaLevel()
    {
        staminaLevel = Mathf.Clamp(
            staminaLevel - (staminaStep * Time.deltaTime),
            0f, maximumStaminaLevel);
    }

    /// <summary>
    /// Decrease the current fullness level by `fullnessStep` each second
    /// </summary>
    private void UpdateFullness()
    {
        fullnessLevel = Mathf.Clamp(
            fullnessLevel - (fullnessSpet * Time.deltaTime),
            0f, maximumFullnessLevel);
    }
}
