using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    #region Singleton
    static public SpawnManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    static public InputManager thePlayer;
    public Transform editorAssignedPlayer;

    [System.Serializable]
    public class EnemySpawner
    {
        public GameObject enemyPrefab;
        public Transform spawnPoint;
        //public Transform moveToPoint;

        public void Activate()
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint);
            enemy.GetComponent<EnemyInput>().hostileTarget = thePlayer;
        }
    }

    [System.Serializable]
    public class EnemyGroupSpawner
    {
        public List<EnemySpawner> enemySpawners = new List<EnemySpawner>();

        public void SpawnEnemies()
        {
            for (int i = 0; i < enemySpawners.Count; i++)
            {
                enemySpawners[i].Activate();
            }
        }
    }

    public List<EnemyGroupSpawner> enemyGroupSpawners = new List<EnemyGroupSpawner>();

    public void Trigger(int whichEnemyGroup)
    {
        if (whichEnemyGroup >= enemyGroupSpawners.Count) return;

        enemyGroupSpawners[whichEnemyGroup].SpawnEnemies();
    }

    private void Start()
    {
        thePlayer = editorAssignedPlayer.GetComponentInChildren<InputManager>();
    }
}
