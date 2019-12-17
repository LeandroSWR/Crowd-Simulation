using UnityEngine;

/// <summary>
/// Class that gets all tables present on the Scene
/// </summary>
public class TablesManager : MonoBehaviour
{
    // Array of all existing tables in this resting area
    [SerializeField] private Table[] tables;
    public Table[] Tables { get => tables; }    // Let the agent see all the tables
}
