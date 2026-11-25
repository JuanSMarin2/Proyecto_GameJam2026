using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance { get; private set; }

    [field: Header("Masks / Collectibles")]
    [field: SerializeField] public EventReference FirstMaskCollectedSoundEvent { get; private set; }
    [field: SerializeField] public EventReference SecondMaskCollectedSoundEvent { get; private set; }
    [field: SerializeField] public EventReference ThirdMaskCollectedSoundEvent { get; private set; }
    [field: SerializeField] public EventReference FourthMaskCollectedSoundEvent { get; private set; }
    [field: SerializeField] public EventReference MaskProximitySoundEvent { get; private set; }
    
    [field: Header("Player")]
    [field: SerializeField] public EventReference FootstepsSoundEvent { get; private set; }
    [field: SerializeField] public EventReference EquipSoundEvent { get; private set; }

    [field: SerializeField] public EventReference DeathSoundEvent { get; private set; }
    [field: SerializeField] public EventReference ReviveSoundEvent { get; private set; }
    //[field: SerializeField] public EventReference VictorySoundEvent { get; private set; }
    //[field: SerializeField] public EventReference PuzzleCompletedSoundEvent { get; private set; }

    [field: Header("Environment & Obstacles")]
    [field: SerializeField] public EventReference AmbienceSoundEvent { get; private set; }
    [field: SerializeField] public EventReference DartsSoundEvent { get; private set; }
    [field: SerializeField] public EventReference BoxMovingSoundEvent { get; private set; }
    [field: SerializeField] public EventReference LavaSoundEvent { get; private set; }
    [field: SerializeField] public EventReference LavaMovingSoundEvent { get; private set; }
    [field: SerializeField] public EventReference WindSound { get; private set; }
    [field: SerializeField] public EventReference WaterIdleSound { get; private set; }
    [field: SerializeField] public EventReference WaterPushingSound { get; private set; }
    [field: SerializeField] public EventReference WaterExit { get; private set; }
    [field: SerializeField] public EventReference FireEnemy { get; private set; }
    [field: SerializeField] public EventReference FireTorch { get; private set; }
    

    [field: Header("UI")]
    [field: SerializeField] public EventReference UIButtonSoundEvent { get; private set; }
    [field: SerializeField] public EventReference ScreenChangeSoundEvent { get; private set; }
    [field: SerializeField] public EventReference LayerChangeSoundEvent { get; private set; }
    [field: SerializeField] public EventReference UIPauseSoundEvent { get; private set; }
    [field: SerializeField] public EventReference UIErrorSoundEvent { get; private set; }
    

    [field: Header("Witch / Boss")]
    [field: SerializeField] public EventReference WitchLaughSoundEvent { get; private set; }
    [field: SerializeField] public EventReference WitchGroanSoundEvent { get; private set; }
    [field: SerializeField] public EventReference WitchMovementSoundEvent { get; private set; }
    [field: SerializeField] public EventReference WitchClapSoundEvent { get; private set; }
    [field: SerializeField] public EventReference DialogueStingerEvent { get; private set; }
    

    [field: Header("Music / Layers")]
    [field: SerializeField] public EventReference LayerMusicSoundEvent { get; private set; }
    [field: SerializeField] public EventReference BossMusicSoundEvent { get; private set; }
    [field: SerializeField] public EventReference MenuMusicSoundEvent { get; private set; }

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

    /// <summary>
    /// Mapea de forma conveniente y centralizada cualquier SoundType al EventReference de FMOD correspondiente.
    /// </summary>
    /* public EventReference GetEventByType(SoundType soundType)
    {
        switch (soundType)
        {
          /*   case SoundType.Pisadas: return FootstepsSoundEvent;
            case SoundType.Equipar: return EquipSoundEvent;
            case SoundType.Muerte: return DeathSoundEvent;
            case SoundType.Revivir: return ReviveSoundEvent;

            case SoundType.RecoleccionAfricana: return FirstMaskCollectedSoundEvent;
            case SoundType.RecoleccionEuropea: return SecondMaskCollectedSoundEvent;
            case SoundType.RecoleccionJaponesa: return ThirdMaskCollectedSoundEvent;
            case SoundType.RecoleccionLatina: return FourthMaskCollectedSoundEvent;

            case SoundType.Dardos: return DartsSoundEvent;
            case SoundType.BloqueMoviendose: return BoxMovingSoundEvent;
            case SoundType.Lava: return LavaSoundEvent;
            case SoundType.LavaMoving: return LavaMovingSoundEvent;
            case SoundType.Viento: return WindSound;
            case SoundType.FuegoFatuo: return FireEnemy;
            case SoundType.UIError: return ErrorSoundEvent;
            

            case SoundType.UIButtonSound: return UIButtonSoundEvent;
            case SoundType.CambioDePantalla: return ScreenChangeSoundEvent;
            case SoundType.CambioDeCapa: return LayerChangeSoundEvent;
            case SoundType.Leitmotif2: return PauseSoundEvent;

            case SoundType.RisaBruja: return WitchLaughSoundEvent;
            case SoundType.QuejidoBruja: return WitchGroanSoundEvent;
            case SoundType.MovimiendoBruja: return WitchMovementSoundEvent;
            case SoundType.AplausoBruja: return WitchClapSoundEvent;
            case SoundType.DialogueStinger: return DialogueStingerEvent;
            case SoundType.MusicaBoss: return BossMusicSoundEvent;

            case SoundType.MusicaBase: return MenuMusicSoundEvent;
            case SoundType.CapaBase: return LayerMusicSoundEvent; 
            
            

            default:
                return default;
        }
    } */
}
