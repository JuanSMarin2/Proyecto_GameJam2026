using System;
using FMODUnity;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class FMODParameterResetter : MonoBehaviour
{
    [Serializable]
    private class ParameterReset
    {
        [SerializeField] private string parameterName;
        [SerializeField] private float value;

        public string ParameterName { get { return parameterName; } }
        public float Value { get { return value; } }
    }

    [Header("Global FMOD Parameters")]
    [Tooltip("Restablece los parámetros al cargar la escena.")]
    [SerializeField] private bool resetOnAwake = true;
    [Tooltip("Parámetros globales y valores que se aplicarán. También puedes llamar ResetParameters antes de iniciar una EventInstance.")]
    [SerializeField] private ParameterReset[] parameters = new ParameterReset[0];

    private void Awake()
    {
        if (resetOnAwake)
            ResetParameters();
    }

    public void ResetParameters()
    {
        if (parameters == null)
            return;

        foreach (ParameterReset parameter in parameters)
        {
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.ParameterName))
            {
                Debug.LogWarning("[FMODParameterResetter] Hay un parámetro sin nombre configurado.", this);
                continue;
            }

            if (!RuntimeManager.IsInitialized)
            {
                Debug.LogError("[FMODParameterResetter] FMOD no está inicializado; no se pudo restablecer el parámetro " + parameter.ParameterName + ".", this);
                return;
            }

            FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByName(parameter.ParameterName, parameter.Value, true);
            if (result != FMOD.RESULT.OK)
                Debug.LogError("[FMODParameterResetter] No se pudo restablecer el parámetro " + parameter.ParameterName + ": " + result + ".", this);
        }
    }
}
