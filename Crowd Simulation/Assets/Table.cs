using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    // Array with all the table seats
    [SerializeField] private Transform[] tableSeats;

    // List of the current available seats
    public List<Transform> AvailableSeats { get; set; }

    // Array of all the taken seats
    public Transform[] TakenSeats { get; set; }

    // The maximum number of agents that can be eating at this table
    private float maxAgentsSupported;

    // The current number of agents eating at this table
    private float currentNumberOfAgents;

    // If this table is available for other agents
    public bool IsAvailable { get; private set; }

    private void Start()
    {
        // At the start all the seats in a table will be available
        AvailableSeats = new List<Transform>(tableSeats);

        // Start the array of taken seat with the size of the total seats
        TakenSeats = new Transform[tableSeats.Length];

        // The number of agents a table supports is equal to the number of seats it has
        maxAgentsSupported = tableSeats.Length;

        // We know that at the start all tables will be empty
        currentNumberOfAgents = 0f;

        // We know that at the start all tables are available
        IsAvailable = true;
    }
}
