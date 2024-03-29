﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour {
	
	private int turn;
	private int round;
	private bool displayInfo;
	private int frameCounter;
    public int timer;
    public int numberRoundMaximum = 2;
	
	private GameObject squad;

    public enum GameState { START, LOST, WIN, LOSTROUND };
    static public GameState gameState = GameState.START;
    static public int NbCopsAlive;
    static public int NbEnemyAlive;
    private EnemyControllerScript[] enemies;
	
	// Use this for initialization
	public void Start () {
		turn = 1;
		round = 1;

        GameManagerScript.gameState = GameState.START;

		displayInfo = true;
		frameCounter = 0;
		
		// Hide the messages
		this.transform.GetChild(0).GetComponent<TextMesh>().text = "";
		this.transform.GetChild(1).GetComponent<TextMesh>().text = "";

        this.squad = GameObject.FindGameObjectWithTag("Squad");
        GameManagerScript.NbCopsAlive = this.squad.GetComponent<SquadManagerScript>().listOfCops.Length;
        this.squad.GetComponent<SquadManagerScript>().deactivateCopsFieldOfView();

        this.enemies = GameObject.FindObjectsOfType<EnemyControllerScript>();
        GameManagerScript.NbEnemyAlive = this.enemies.Length;
        foreach (EnemyControllerScript enemy in this.enemies)
        {
            enemy.setExecuteActions(false);
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (GameManagerScript.gameState == GameState.START)
        {
            // If the user press "ENTER" and the current turn is 1, then the turn 2 starts
            if (Input.GetKeyDown(KeyCode.Return))
            {
                this.LaunchTurn();
            }

            // If the units have finished their paths
            bool hasTheEnnemiesFinishedToMove = true;
            for (int i = 0; i < this.enemies.Length; ++i)
            {
                if (this.enemies[i].Pv > 0 && this.enemies[i].GetComponent<NavMeshEnemyScript>().hasFinishedItsPath() == false)
                    hasTheEnnemiesFinishedToMove = false;
            }
            if (squad.GetComponent<SquadManagerScript>().hasTheTeamFinishedToMove() && hasTheEnnemiesFinishedToMove && turn == 2)
            {
                turn = 1;
                ++round;

                //displayInfo = true;
                frameCounter = 0;

                /* During the turn 1:
                    - The units can't move
                    - The user can select units
                    - The counters of actions per units are reset
                    - Cops field of view are deactivated, so the player can click through them
                */
                squad.GetComponent<SquadManagerScript>().makeUnitsStopMoving();
                squad.GetComponent<SquadManagerScript>().allowToSelectUnit(true);
                squad.GetComponent<SquadManagerScript>().resetListOfActionsCounter();
                squad.GetComponent<SquadManagerScript>().deactivateCopsFieldOfView();
                squad.GetComponent<SquadManagerScript>().SetExecuteActions(false);

                // Deactivate the actions of all enemies
                foreach (EnemyControllerScript enemy in this.enemies)
                {
                    enemy.setExecuteActions(false);
                }
            }

            // Display messages
            if (displayInfo)
            {
                // Until the the frameCounter is lower than timer, the messages will be displayed
                //this.transform.GetChild(0).GetComponent<TextMesh>().text = "Round " + round;
                //this.transform.GetChild(1).GetComponent<TextMesh>().text = "Turn " + turn;
                squad.GetComponent<SquadManagerScript>().allowToSelectUnit(false);

                ++frameCounter;

                if (frameCounter >= timer)
                {
                    displayInfo = false;
                    this.transform.GetChild(0).GetComponent<TextMesh>().text = "";
                    this.transform.GetChild(1).GetComponent<TextMesh>().text = "";

                    if (turn == 1)
                        squad.GetComponent<SquadManagerScript>().allowToSelectUnit(true);
                }
            }

            if (GameManagerScript.NbCopsAlive <= 0)
            {
                GameManagerScript.gameState = GameState.LOST;
            } else if (GameManagerScript.NbEnemyAlive <= 0) {
                GameManagerScript.gameState = GameState.WIN;
            } else if (this.round > this.numberRoundMaximum) {
                GameManagerScript.gameState = GameState.LOSTROUND;
            }
        }
	}

    public void LaunchTurn()
    {
        if (turn == 1)
        {
            frameCounter = 0;
            ++turn;

            /* During the turn 2:
                - The units are allowed to move
                - None of the units are selected
                - It's impossible to select an unit
                - The cops field of view are activated so they can see enemies
            */
            squad.GetComponent<SquadManagerScript>().makeUnitsMove();
            squad.GetComponent<SquadManagerScript>().unselectCop();
            squad.GetComponent<SquadManagerScript>().allowToSelectUnit(false);
            squad.GetComponent<SquadManagerScript>().activateCopsFieldOfView();
            squad.GetComponent<SquadManagerScript>().SetExecuteActions(true);

            // Activate the actions of all enemies
            foreach (EnemyControllerScript enemy in this.enemies)
            {
                enemy.setExecuteActions(true);
            }

            displayInfo = true;
        }
    }

    public int GetTurn()
    {
        return this.turn;
    }

    public int GetRound()
    {
        return this.round;
    }
}
