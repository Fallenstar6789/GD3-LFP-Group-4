using UnityEngine;
using System.Collections.Generic;

public class Door_Unlocker : MonoBehaviour//vuyi edit
{
    [Header("Stage 1: first enemies")]
    // The first 2 enemies. Door 1 opens when these are dead
    public List<GameObject> enemiesToKill = new List<GameObject>();

    public GameObject zombies;

    [Tooltip("Door 1: destroyed when the first enemies are dead")]
    public List<GameObject> destroyOnClear = new List<GameObject>();

    [Header("Stage 2: all enemies")]
    // every enemy is dead
    public List<GameObject> allEnemies = new List<GameObject>();

    [Tooltip("Door 2 changes colour when ALL enemies are dead")]
    public GameObject doorToTurnGreen;
    public Color completeColor = Color.green;

    private bool stage1Done = false;
    private bool stage2Done = false;


    public bool IsComplete => stage2Done;

    private void Update()
    {
        if (!stage1Done)
        {
            CheckStage1();
        }
        else if (!stage2Done)
        {
            CheckStage2();
        }
    }

    private void CheckStage1()
    {
       
        if (enemiesToKill.Count == 0) return;

        if (AnyAlive(enemiesToKill)) return;

        stage1Done = true;

         zombies.SetActive(true);

      
        foreach (GameObject go in destroyOnClear)//destroy door when enemies dead
        {
            if (go != null) Destroy(go);
        }
    }

    private void CheckStage2()
    {
       
        if (allEnemies.Count == 0) return;

        if (AnyAlive(allEnemies)) return;

        stage2Done = true;

        // Turn door 2 green
        if (doorToTurnGreen != null)
        {
            SpriteRenderer[] renderers = doorToTurnGreen.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sr in renderers)
            {
                sr.color = completeColor;
            }
        }
    }

    private bool AnyAlive(List<GameObject> list)//checks if any enemies are still alive
    {
        foreach (GameObject enemy in list)
        {
            if (enemy != null) return true;
        }

        return false;
    }
}