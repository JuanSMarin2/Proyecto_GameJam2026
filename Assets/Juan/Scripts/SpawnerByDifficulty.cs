using System.Collections.Generic;
using UnityEngine;

public class SpawnerByDifficulty : MonoBehaviour
{
    [SerializeField] private List<GameObject> ObjectsInMediumOrHard = new List<GameObject>();
    [SerializeField] private List<GameObject> ObjectsInHard = new List<GameObject>();
    void Start()
    {
        foreach (GameObject obj in ObjectsInMediumOrHard)        {
            if (DifficultySelectManager.Instance != null)
            {
                if (DifficultySelectManager.Instance.CurrentMode == DifficultySelectManager.DifficultyMode.Easy)
                {
                    obj.SetActive(false);
                }
            }
        }

        foreach (GameObject obj in ObjectsInHard) {
            if (DifficultySelectManager.Instance != null)
            {
                if (DifficultySelectManager.Instance.CurrentMode != DifficultySelectManager.DifficultyMode.Hard)
                {
                    obj.SetActive(false);
                }
            }
        }



    }

 


}
