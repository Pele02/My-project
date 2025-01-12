using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public int lives = 3; // Starting lives
    public Text livesText; // Reference to UI Text to display lives

    void Start()
    {
        UpdateLivesUI();
    }

    public void LoseLife()
    {
        lives--;
        UpdateLivesUI();

        if (lives <= 0)
        {
            GameOver();
        }
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives;
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        // Add Game Over logic, like reloading the scene
    }
}
