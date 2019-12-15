using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using LibGameAI.DecisionTrees;

public class NPCBehaviour : MonoBehaviour
{
    // Arrays with the location of all areas of interest
    [SerializeField] private TablesManager[] eatingAreas;
    [SerializeField] private Transform[] restingAreas;
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

    // The current resting area the agent is going to
    private Transform currentRestingArea;

    // The curret eating area the agent is going to
    private Transform currentEatingArea;

    // The table chosen by the agent when he goes to eat
    private Table chosenTable;

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

        // Initialize the chosen table as null
        chosenTable = null;

        // Define the initial values for the excitement level
        excitementLevel = 0f;
        // Define the step amount for the excitement level
        excitementStep = Random.Range(1f, 3f);

        // Define the initial values for the stamina level
        staminaLevel = 30f; // Random.Range(50f, 100f);
        // Define the step amount for the stamina level
        staminaStep = 0; // Random.Range(1f, 3f);

        // Define the initial values for the fullness level
        fullnessLevel = Random.Range(25f, 90f);
        // Define the step amount for the fullness level
        fullnessSpet = Random.Range(1f, 3f);

        // Define the multiplier for resting and eating
        multiplier = Random.Range(3f, 6f);
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        (isAgentHungry.MakeDecision() as ActionNode).Execute();

        UpdateExcitementLevel();
        
        if (!isResting)
            UpdateStaminaLevel();
        if (!isEating)
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
        // If the agent hasn't selected an eating area yet
        if (currentEatingArea == null)
        {
            // Select a random eating area from the two available
            TablesManager eatingArea = 
                Random.Range(1f, 100f) > 50 ? eatingAreas[0] : eatingAreas[1];

            // Create a new byte to know the number of people sitting at the currently chosen table
            byte numPeopleSitting = 10;

            // Logic to chose the table with less people on it
            foreach (Table t in eatingArea.Tables)
            {
                // If the agent hasn't choosen a table yet
                if (chosenTable == null)
                {
                    // Set the table to the first in the list
                    chosenTable = t;
                }

                // If the number of people sitting at the current table is greater than at table `t`
                if (numPeopleSitting > chosenTable.TakenSeats.Count)
                {
                    // Set the `numPeopleSitting` to be the table `t` number of taken seats
                    numPeopleSitting = (byte)chosenTable.TakenSeats.Count;

                    // Set the chose table to be the table `t`
                    chosenTable = t;
                }
            }

            // If the chosen table has not seats left return to try again
            if (chosenTable.AvailableSeats.Count == 0)
            {
                return;
            }

            // Set the current eating area to be the seat on the table we want
            currentEatingArea = chosenTable.AvailableSeats[0];

            // Remove the chosen seat from the available list
            chosenTable.AvailableSeats.Remove(currentEatingArea);

            // Add it to the taken list
            chosenTable.TakenSeats.Add(currentEatingArea);

            // Move the agent to the seat
            agent.destination = currentEatingArea.position;
        }

        // If we've reached the destination and our `fullnessLevel` is less than the maximum
        if (agent.remainingDistance < 1 && fullnessLevel < maximumFullnessLevel)
        {
            // We're currently eating
            isEating = true;

            // Increase the `fullnessLevel` each second based on the `(fullnessStep * multiplier)`
            fullnessLevel = Mathf.Min(
                fullnessLevel + ((fullnessSpet * multiplier) * Time.deltaTime),
                maximumFullnessLevel);

        }   // If the `fullnessLevel` is at maximum
        else if (fullnessLevel >= maximumFullnessLevel)
        {
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
            StartCoroutine(MoveToCurrentStage());
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
            currentRestingArea = Random.Range(1, 100) > 50 ? restingAreas[0] : restingAreas[1];
            
            // Sets the agent destination to be that resting area
            agent.destination = currentRestingArea.position;
        }

        // If we've reached the destination and our `staminaLevel` is less than the maximum
        if (agent.remainingDistance < 1 && staminaLevel < maximumStaminaLevel)
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
            currentRestingArea = null;
            isResting = false;

            StartCoroutine(MoveToCurrentStage());
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
        
        } // If the current stage is not null
        else
        {
            // Check what stage the agent is currently on to go to another
            if (currentStage == stages[0])
            {
                // If it's the stage 0 we can go to the stage 1 or 2
                currentStage = stageSelect > 40 ? stages[1] : stages[2];
            }
            else if (currentStage == stages[1])
            {
                // If it's the stage 1 we can go to the stage 0 or 2
                currentStage = stageSelect > 40 ? stages[0] : stages[2];
            }
            else
            {
                // If it's the stage 2 we can go to the stage 0 or 1
                currentStage = stageSelect > 40 ? stages[0] : stages[1];
            }
        }

        // Increasse the excitement level when we switch stage
        excitementLevel = Random.Range(90f, 100f);

        StartCoroutine(MoveToCurrentStage());
    }

    private IEnumerator MoveToCurrentStage()
    {
        do
        {
            // Move the aget to the current stage
            agent.destination = currentStage.position;

            // Wait a frame
            yield return null;
        } while (fullnessLevel != 0f && !isEating && staminaLevel != 0f && !isResting);
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
