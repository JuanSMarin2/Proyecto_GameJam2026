using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using System.Drawing;
public class postprocessConfig : MonoBehaviour
{

    [SerializeField] private Volume volume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private Vignette vignette;
    private UnityEngine.Color colorMaskMagenta = new UnityEngine.Color(0.3314534f, 0f, 0.3960784f, 1f);
    private UnityEngine.Color colorMaskCyan = new UnityEngine.Color(0f, 0.07384165f, 0.3960784f, 1f);
    private UnityEngine.Color colorMaskGreen = new UnityEngine.Color(0f, 0.3960784f, 0.05349636f, 1f);
    private UnityEngine.Color colorMaskYellow = new UnityEngine.Color(0.3918388f, 0.3960784f, 0f, 1f);

    void Start()
    {
        if (volume != null)
        {
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);
            volume.profile.TryGet(out vignette);
        }
    }

    public void ActivaMaskMagenta()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.458f;
            vignette.color.value = colorMaskMagenta;
        }

        lensDistortion.intensity.value = -0.22f;
        chromaticAberration.intensity.value = 0.198f;

    }

    public void ActivaMaskCyan()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.458f;
            vignette.color.value = colorMaskCyan;
        }

        lensDistortion.intensity.value = -0.22f;
        chromaticAberration.intensity.value = 0.198f;

    }

    public void ActivaMaskGreen()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.458f;
            vignette.color.value = colorMaskGreen;
        }

        lensDistortion.intensity.value = -0.22f;
        chromaticAberration.intensity.value = 0.198f;

    }

    public void ActivaMaskYellow()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0.458f;
            vignette.color.value = colorMaskYellow;
        }

        lensDistortion.intensity.value = -0.22f;
        chromaticAberration.intensity.value = 0.198f;

    }

    public void DesactivaMask()
    {
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
        }

        lensDistortion.intensity.value = 0f;
        chromaticAberration.intensity.value = 0f;

    }

}
