using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Class responsible for all UI elements' actions
/// </summary>
public class UIManager : MonoBehaviour {

    // The minimum time scale
    [SerializeField] private int minTimeScale;

    // The maximum time scale
    [SerializeField] private int maxTimeScale;

    // The time scale text present on the UI
    [SerializeField] private Text timeScaleText;

    // The slider which controls the amount of agents to spawn
    [SerializeField] private Slider spawnSlider;

    // The text that gets updated with the slider's value
    [SerializeField] private Text sliderText;

    /// <summary>
    /// Awake is called before the game starts
    /// </summary>
    private void Awake() {

        // Start the time scale as the minimum one
        Time.timeScale = minTimeScale;
        timeScaleText.text = $"{Time.timeScale}x";
    }

    /// <summary>
    /// Changes the current time scale
    /// </summary>
    public void ChangeTimeScale() {

        // Loop through all possible tims scale values
        for (int i = minTimeScale; i <= maxTimeScale; i++) {

            // If the time scale equals the current iteration
            // and is different from maximum time scale...
            if (Time.timeScale == i && Time.timeScale != maxTimeScale) {

                // ...increase the Time scale and leave the 'for' loop
                Time.timeScale++;
                break;

            // If the time scale equals the maximum time scale...
            } else if (Time.timeScale == maxTimeScale) {

                // ...reset it to the minimum value
                Time.timeScale = minTimeScale;
            }
        }

        // Show the time scale on the button's text
        timeScaleText.text = $"{Time.timeScale}x";
    }

    /// <summary>
    /// Updates the slider's text
    /// </summary>
    public void UpdateSliderText() => sliderText.text = spawnSlider.value.ToString("0");

    /// <summary>
    /// Restarts the Simulation
    /// </summary>
    public void RestartScene() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
