using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameStates GameState { get; set; }

    void Start()
    {
        ChangeState(GameStates.Start);
    }

    static public void ChangeState(GameStates state)
    {
        GameState = state;

        switch (GameState)
        {
            case GameStates.Start:
                StartGame();
                break;
            case GameStates.Runing:
                break;
            case GameStates.End:
                EndGame();
                break;
            default:
                break;
        }
    }

    static private void StartGame()
    {
        ChangeState(GameStates.Runing);
    }

    static private void EndGame()
    {
        Debug.Log("Game Over!");
    }

    static private void Restart()
    {
        SceneManager.LoadScene("Game");
    }
}

public enum GameStates { Start, Runing, End }
