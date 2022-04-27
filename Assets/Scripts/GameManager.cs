using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public GameObject GameOverScreen;
    public GameObject GameWinScreen;

    private bool GameOver = false;
    // Start is called before the first frame update
    void Start()
    {
        Agent.OnGuardHasSpottedPlayer += EndGame;
        Player.ReachedFinish += WonGame;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameOver)
            if (Input.GetKeyDown(KeyCode.Space))
                SceneManager.LoadScene(0);
    }

    void EndGame()
    {
        ShowEndScreen(GameOverScreen);
    }    
    void WonGame()
    {
        ShowEndScreen(GameWinScreen);
    }

    void ShowEndScreen(GameObject EndScreen)
    {
        GameOver = true;
        EndScreen.SetActive(true);
        Agent.OnGuardHasSpottedPlayer -= EndGame;
        Player.ReachedFinish -= WonGame;
    }
}
