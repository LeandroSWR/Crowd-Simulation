using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TablesManager : MonoBehaviour
{
    // Array of all existing tables in this resting area
    [SerializeField] private Table[] tables;
    public Table[] Tables { get => tables; }    // Let the agent see all the tables
}
