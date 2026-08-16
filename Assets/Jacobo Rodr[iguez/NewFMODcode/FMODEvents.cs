using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    [field: Header("Sound Events")]
    [field: SerializeField] public EventReference FirstMaskCollectedSoundEvent { get; private set; }

  public static FMODEvents instance { get; private set; }

  private void Awake()
  {
      if (instance == null)
      {
          instance = this;
          DontDestroyOnLoad(gameObject);
      }
      else
      {
          Debug.Log("Mas de un FMODEvents en escena, destruyendo el más nuevo.");
          Destroy(gameObject);
      }
  }
}
