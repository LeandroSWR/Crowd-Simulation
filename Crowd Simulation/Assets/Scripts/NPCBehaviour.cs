using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using LibGameAI.DecisionTrees;

public class NPCBehaviour : MonoBehaviour
{
    // The Exit's location
    [SerializeField] private Transform exit;

    // Arrays with the location of all areas of interest
    [SerializeField] private TablesManager[] eatingAreas;
    [SerializeField] private Transform[] restingAreas;
    [SerializeField] private Transform[] stages;

    // How fast the agent moves
    [SerializeField] private float speed = 10f;
    
    // Boredom behaviour variables
    private readonly float maximumExcitementLevel = 100f;   // How much excitement the agent can have
    private float excitementLevel;  // The current level of excitement
    private float excitementStep;   // How much the excitement decreases each second

    // Tiredness behaviour variables
    private readonly float maximumStaminaLevel = 100f;  // How much stamina the agent can have
    private float staminaLevel; // The current level of stamina 
    private float staminaStep;  // How much the stamina decreases each second
    private bool isResting; // If the agent is currently resting

    // Hungriness behaviour variables
    private readonly float maximumFullnessLevel = 100f; // How full the agent can be
    private float fullnessLevel;    // The current level of fullness
    private float fullnessSpet; // How much the fullness decreases each second
    private bool isEating;  // If the agent is currently eating

    // The multiplier for increasing the values when resting or eating
    private float multiplier;

    // The current stage the agent is watching
    private Transform currentStage;

    // If the agent has reach the stage he wants to go to
    public bool HasReachedStage { get; private set; }

    // How long will the agent try to push forwards
    private WaitForSeconds pushFor;

    // The current resting area the agent is going to
    private Vector3? currentRestingArea;

    // The curret eating area the agent is going to
    private Transform currentEatingArea;

    // The table chosen by the agent when he goes to eat
    private Table chosenTable;

    // Property to set this agent as panicking
    public bool IsPanicking { get; set; }

    // Property to set this agent as stunned
    public bool IsStunned { get; set; }

    // Property to pronounce this agent as dead
    public bool IsDead { get; set; }

    // The timer for the Stun
    private float stunTime;
    public float StunTime { set => stunTime = value; }

    // Reference to our Nav Mesh Agent
    private NavMeshAgent agent;

    // Action and Decision Nodes \\

    // Decision node if the agent is hungry
    private IDecisionTreeNode isAgentHungry;

    // Action node in case the agent is hungry
    private IDecisionTreeNode agentIsHungry;

    // Decision node if the agent is tired
    private IDecisionTreeNode isAgentTired;

    // Action node in case the agent is tired
    private IDecisionTreeNode agentIsTired;

    // Decision node if the agent is excited
    private IDecisionTreeNode isAgentExcited;

    // Action node in case the agent is not excited
    private IDecisionTreeNode agentNotExcited;

    // Decision node if the agent is panicking
    private IDecisionTreeNode isAgentPanicking;

    // Action node in case the agent is not panicking
    private IDecisionTreeNode agentIsPanicking;

    // Decision node if the agent is stunned
    private IDecisionTreeNode isAgentStunned;

    // Action node in case the agent is not stunned
    private IDecisionTreeNode agentIsStunned;

    // Decision node if the agent is dead
    private IDecisionTreeNode isAgentDead;

    // Action node in case the agent is not stunned
    private IDecisionTreeNode agentIsDead;

    /// <summary>
    /// Awake is called when the script is loaded
    /// </summary>
    private void Awake() {

        // Create a new action node for the agent death behaviour
        agentIsDead = new ActionNode(Die);

        // Create a new action node for the agent stun behaviour
        agentIsStunned = new ActionNode(Stun);

        // Create a new action node for the agent panicking behaviour
        agentIsPanicking = new ActionNode(RunToExit);

        // Create a new action node for the agent eating behaviour
        agentIsHungry = new ActionNode(AgentGoEat);

        // Create a new action node for the agent resting behaviour
        agentIsTired = new ActionNode(AgentGoRest);

        // Create a new action node for the agent change stage behaviour
        agentNotExcited = new ActionNode(AgentChangeStage);

        // Create a new decision node to know if the agent is excited
        isAgentExcited = new DecisionNode(IsAgentExcited, new ActionNode(() => { }), agentNotExcited);

        // Create a new decision node to know if the agent is tired
        isAgentTired = new DecisionNode(IsAgentTired, agentIsTired, isAgentExcited);

        // Create a new decision node to know if the agent is hungry
        isAgentHungry = new DecisionNode(IsAgentHungry, agentIsHungry, isAgentTired);

        // Create a new decision node to know if the agent is panicking
        isAgentPanicking = new DecisionNode(IsAgentPanicking, agentIsPanicking, isAgentHungry);

        // Create a new decision node to know if the agent is stunned
        isAgentStunned = new DecisionNode(IsAgentStunned, agentIsStunned, isAgentPanicking);

        // Create a new decision node to know if the agent is dead
        isAgentDead = new DecisionNode(IsAgentDead, agentIsDead, isAgentStunned);
    }

    /// <summary>
    /// Start is called before the first frame update
    /// </summary>
    void Start()
    {
        // Initialize `agent` by getting the NavMeshAgent component
        agent = GetComponent<NavMeshAgent>();

        // Assign this speed as the agent's speed
        agent.speed = speed;

        // After colliding with someone the agent will keep pushing forward for 2 seconds
        pushFor = new WaitForSeconds(0.3f);

        // Initialize the chosen table as null
        chosenTable = null;

        // The agent starts with no resting area selected
        currentRestingArea = null;

        // Initialize the `HasReachedStage` as false
        HasReachedStage = false;

        // Set the agent as not dead, in case it had already died
        IsDead = false;

        // Define the initial values for the excitement level
        excitementLevel = 0f;
        // Define the step amount for the excitement level
        excitementStep = Random.Range(1f, 2.5f);

        // Define the initial values for the stamina level
        staminaLevel = Random.Range(0f, 100f);
        // Define the step amount for the stamina level
        staminaStep = Random.Range(1f, 3f);

        // Define the initial values for the fullness level
        fullnessLevel = Random.Range(0f, 100f);
        // Define the step amount for the fullness level
        fullnessSpet = Random.Range(1f, 3.5f);

        // Define the multiplier for resting and eating
        multiplier = Random.Range(8f, 10f);
    }

    /// <summary>
    /// Frame-rate independent message for physics calculations
    /// </summary>
    void FixedUpdate()
    {
        // Call the root node of the decision tree
        (isAgentDead.MakeDecision() as ActionNode).Execute();

        // Update the level of excitement the agent has
        UpdateExcitementLevel();
        
        // If the agent is not resting we update the stamina level
        if (!isResting)
            UpdateStaminaLevel();

        // If the agent is not eating we update the fullness level
        if (!isEating)
            UpdateFullness();
    }

    /// <summary>
    /// Checks if an agent is currently dead
    /// </summary>
    /// <returns>True if the agent is dead, false otherwise</returns>
    private bool IsAgentDead() => IsDead;

    /// <summary>
    /// 'Kills' this agent
    /// </summary>
    private void Die() {

        // Disables the agent's GameObject
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks if an agent is currently stunned
    /// </summary>
    /// <returns>True if the agent is stunned, false otherwise</returns>
    private bool IsAgentStunned() => IsStunned;

    /// <summary>
    /// Makes the agent become stunned
    /// </summary>
    private void Stun() {

        // Verify if stun time can still be reduced
        if (stunTime > 0.0f) {

            // Reduce stun time
            stunTime -= Time.fixedDeltaTime;

        } else {

            // Reduce his speed by half
            agent.speed = speed / 2.0f;

            // Reset stun time
            stunTime = 1.5f;

            // The agent stops being stunned
            IsStunned = false;
        }
    }

    /// <summary>
    /// Checks if the agent is currently panicking
    /// </summary>
    /// <returns>True if the agent is panicking, false otherwise</returns>
    private bool IsAgentPanicking() => IsPanicking;

    /// <summary>
    /// Makes the agent run towards an exit
    /// </summary>
    private void RunToExit() {
        
        // Prevent the agent from getting stuck
        // (e.g. if he was waiting for a table)
        agent.isStopped = false;

        // Set the exit as the agent's destination
        agent.SetDestination(exit.position);

        // Double his speed only if he wasn't stunned
        if (agent.speed == speed) agent.speed = speed * 2.0f;
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
        // If the agent hasn't selected an eating area yet
        if (currentEatingArea == null)
        {
            // The agent is stoped while he chooses a table
            agent.isStopped = true;

            // Select a random eating area from the two available
            TablesManager eatingArea = 
                Random.Range(1f, 100f) > 50 ? eatingAreas[0] : eatingAreas[1];

            // Set the table to the first in the list
            chosenTable = eatingArea.Tables[0];

            // Logic to choose the table with less people on it
            foreach (Table t in eatingArea.Tables)
            {
                // If the number of people sitting at the current table is greater than at table `t`
                if (chosenTable.TakenSeats.Count > t.TakenSeats.Count)
                {
                    // Set the chosen table to be the table `t`
                    chosenTable = t;
                }
            }
            
            // If the choosen table had no seats available
            if (chosenTable.AvailableSeats.Count == 0)
            {
                // If there is no table available continue walking to the stage
                AgentChangeStage();

                // Return to choose another table
                return;
            }

            // Set the current eating area to be the seat on the table we want
            currentEatingArea = chosenTable.AvailableSeats[0];

            // Remove the chosen seat from the available list
            chosenTable.AvailableSeats.Remove(currentEatingArea);

            // Add it to the taken list
            chosenTable.TakenSeats.Add(currentEatingArea);

            // Move the agent to the seat
            agent.SetDestination(currentEatingArea.position);

            // We're not close to the stage
            HasReachedStage = false;

            // The agent will move after choosing a table
            agent.isStopped = false;

        } // If the agent has't reached the objective yet
        else if (Vector3.Distance(transform.position, currentEatingArea.position) >= 1.8f)
        {
            // The agent will move after choosing a table
            agent.isStopped = false;

            // Move the agent to the seat
            agent.SetDestination(currentEatingArea.position);

            // The agent can't be resting if he's going to eat
            isResting = false;

        } // If we've reached the destination and our `fullnessLevel` is less than the maximum
        else if (Vector3.Distance(transform.position, currentEatingArea.position) <= 1.8f && 
            fullnessLevel < maximumFullnessLevel)
        {
            // The agent is eating so he doesn't move
            agent.isStopped = true;

            // We're currently eating
            isEating = true;

            // Increase the `fullnessLevel` each second based on the `(fullnessStep * multiplier)`
            fullnessLevel = Mathf.Min(
                fullnessLevel + ((fullnessSpet * multiplier) * Time.deltaTime),
                maximumFullnessLevel);

        }   // If the `fullnessLevel` is at maximum
        else if (fullnessLevel >= maximumFullnessLevel)
        {
            // The agent is not eating so he can move
            agent.isStopped = false;

            // We finished eating so the seat will now be available
            // Add it to the available seats list
            chosenTable.AvailableSeats.Add(currentEatingArea);
            // Remove it from the taken seats list
            chosenTable.TakenSeats.Remove(currentEatingArea);

            // Set the current eating area to null
            currentEatingArea = null;

            // We're no longer eating
            isEating = false;

            // Start moving the agent to the stage
            excitementLevel = 0;
        }
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
        // If we don't currently have a resting area assigned
        if (currentRestingArea == null)
        {

            // Choses randomly between the two available resting areas
            // And selects a random position on it's collider
            currentRestingArea = GetRandomPointInBounds(
                (Random.Range(1, 100) > 50 ?
                restingAreas[0] : restingAreas[1]).GetComponent<BoxCollider>().bounds);

            // Sets the agent destination to be that resting area
            agent.SetDestination(currentRestingArea.Value);

        } 
        else if (Vector3.Distance(transform.position, currentRestingArea.Value) > 2f) 
        {
            // Sets the agent destination to be that resting area
            agent.SetDestination(currentRestingArea.Value);

        }// If we've reached the destination and our `staminaLevel` is less than the maximum
        else if (Vector3.Distance(transform.position, currentRestingArea.Value) < 2f &&
            staminaLevel < maximumStaminaLevel)
        {
            // We're currently resting
            isResting = true;

            // Increase the `staminaLevel` each second based on the `(staminaStep * multiplier)`
            staminaLevel = Mathf.Min(
                staminaLevel + ((staminaStep * multiplier) * Time.deltaTime),
                maximumStaminaLevel);

        }   // If the `staminaLevel` is at maximum
        else if (staminaLevel >= maximumStaminaLevel)
        {
            // Set the `currentRestingArea` to null
            currentRestingArea = null;

            // The agent is no longer resting
            isResting = false;

            // The agent has not yet reached the stage
            HasReachedStage = false;

            // Set the excitement level to 0
            excitementLevel = 0;
        }
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
        // Get a new random float between 1 and 100
        float stageSelect = Random.Range(1, 100);

        // If the current stage is null
        if (currentStage == null)
        {
            // Select one of the three stages, with the bigger ones having a higher chance
            currentStage = stageSelect > 60 ? stages[0] : stageSelect > 28 ? stages[1] : stages[2];

            // Move the agent to the current stage
            agent.SetDestination(currentStage.position);

        } // If the agent has't reached the objective yet
        else if (Vector3.Distance(transform.position, currentStage.position) > 2.5f)
        {
            // If the agent has not reached the stage he will move
            agent.isStopped = false;

            // Move the agent to the current stage
            agent.SetDestination(currentStage.position);

        } // If the distance from the agent to the target position is less than 3
        else if (Vector3.Distance(transform.position, currentStage.position) < 2.5f)
        {
            // The agent has reached the stage
            HasReachedStage = true;

            // Increasse the excitement level when we switch stage
            excitementLevel = Random.Range(90f, 100f);

            // Set the current stage to null
            currentStage = null;

            // If the agent has reached the stage he will stop moving
            agent.isStopped = true;
        }
        else if (HasReachedStage)
        {
            // If the agent has reached the stage he will stop moving
            agent.isStopped = true;

            // Increasse the excitement level when we switch stage
            excitementLevel = Random.Range(90f, 100f);

            // Set the current stage to null
            currentStage = null;
        }
    }

    /// <summary>
    /// Decrease the current excitement level by `excitementStep` each second
    /// </summary>
    private void UpdateExcitementLevel()
    {
        // Clamp the excitement level between 0 and `maximumExcitementLevel`
        excitementLevel = Mathf.Clamp(
            excitementLevel - (excitementStep * Time.deltaTime),
            0f, maximumExcitementLevel);
    }

    /// <summary>
    /// Decrease the current stamina level by `staminaStep` each second
    /// </summary>
    private void UpdateStaminaLevel()
    {
        // Clamp the stamina level between 0 and `maximumStaminaLevel`
        staminaLevel = Mathf.Clamp(
            staminaLevel - (staminaStep * Time.deltaTime),
            0f, maximumStaminaLevel);
    }

    /// <summary>
    /// Decrease the current fullness level by `fullnessStep` each second
    /// </summary>
    private void UpdateFullness()
    {
        // Clamp the fullness level between 0 and `maximumFullnessLevel`
        fullnessLevel = Mathf.Clamp(
            fullnessLevel - (fullnessSpet * Time.deltaTime),
            0f, maximumFullnessLevel);
    }

    /// <summary>
    /// Called when the agent collides with another agent
    /// </summary>
    /// <param name="other">The collider of the other agent</param>
    private void OnCollisionEnter(Collision other)
    {
        // If the agent collided with another npc
        if (other.transform.CompareTag("NPC"))
        {
            // And that npc is on a stage
            if (other.transform.GetComponent<NPCBehaviour>().HasReachedStage)
            {
                // If the agent is not yet on a stage
                if (!HasReachedStage)
                {
                    // Start a corroutine
                    StartCoroutine(StopPushing());
                }
            }
        }
    }

    /// <summary>
    /// Gets called when the Collider 'other' enters the trigger
    /// </summary>
    /// <param name="other">The Collider we hit</param>
    private void OnTriggerEnter(Collider other) {
        
        // If the Collider the agent hit is another NPC and this agent is panicking
        if (other.CompareTag("NPC") && IsPanicking) {

            // Set the other NPC as panicking aswell
            other.GetComponent<NPCBehaviour>().IsPanicking = true;
        }
    }

    /// <summary>
    /// Makes the agent stop pushing forawrd after some time
    /// </summary>
    /// <returns>Waits for some seconds</returns>
    private IEnumerator StopPushing()
    {
        // Waits for some seconds
        yield return pushFor;

        // If the agent is currently moving towards a stage
        if (currentStage != null && agent.destination == currentStage.position)
        {
            // The agent has reached the stage
            HasReachedStage = true;
        }
    }

    /// <summary>
    /// Returns a random position inside a collider
    /// </summary>
    /// <param name="bounds">The bounds of the collider</param>
    /// <returns>A Vector3 with a random position</returns>
    public Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            0f,
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
