using System.Collections;
using UnityEngine;

public class PlayerTransparencyFader : MonoBehaviour
{
    private MeshRenderer[] meshRenderers;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;

    [Header("Material Settings")]
    [Tooltip("Reference to the original material.")]
    [SerializeField] private Material originalMaterial;
    [Tooltip("Reference to the material used during fading.")]
    [SerializeField] private Material transparentMaterial;

    [Header("Fade Settings")]
    [Tooltip("Duration of the fade in seconds.")]
    [SerializeField] private float fadeDuration = 0.0f;

    private Coroutine fadeAlphaRoutine;

    private void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    public void FadeAlphaToZero()
    {
        if (fadeAlphaRoutine != null)
        {
            StopCoroutine(fadeAlphaRoutine);
            fadeAlphaRoutine = null;
        }

        fadeAlphaRoutine = StartCoroutine(FadeAlphaToZeroRoutine());
    }

    public void FadeAlphaToOne()
    {
        if (fadeAlphaRoutine != null)
        {
            StopCoroutine(fadeAlphaRoutine);
            fadeAlphaRoutine = null;
        }

        fadeAlphaRoutine = StartCoroutine(FadeAlphaToOneRoutine());
    }

    private IEnumerator FadeAlphaToZeroRoutine()
    {
        SetToTransparentMaterial();

        float elapsed = 0.0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                Color color = meshRenderer.material.GetColor("_BaseColor");
                color.a = Mathf.Lerp(1.0f, 0.0f, t);
                meshRenderer.material.SetColor("_BaseColor", color);
            }
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                Color color = skinnedMeshRenderer.material.GetColor("_BaseColor");
                color.a = Mathf.Lerp(1.0f, 0.0f, t);
                skinnedMeshRenderer.material.SetColor("_BaseColor", color);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Color finalColor = meshRenderer.material.GetColor("_BaseColor");
            finalColor.a = 0.0f;
            meshRenderer.material.SetColor("_BaseColor", finalColor);
        }
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            Color finalColor = skinnedMeshRenderer.material.GetColor("_BaseColor");
            finalColor.a = 0.0f;
            skinnedMeshRenderer.material.SetColor("_BaseColor", finalColor);
        }
    }

    private IEnumerator FadeAlphaToOneRoutine()
    {
        float elapsed = 0.0f;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;

            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                Color color = meshRenderer.material.GetColor("_BaseColor");
                color.a = Mathf.Lerp(0.0f, 1.0f, t);
                meshRenderer.material.SetColor("_BaseColor", color);
            }
            foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
            {
                Color color = skinnedMeshRenderer.material.GetColor("_BaseColor");
                color.a = Mathf.Lerp(0.0f, 1.0f, t);
                skinnedMeshRenderer.material.SetColor("_BaseColor", color);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            Color finalColor = meshRenderer.material.GetColor("_BaseColor");
            finalColor.a = 1.0f;
            meshRenderer.material.SetColor("_BaseColor", finalColor);
        }
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            Color finalColor = skinnedMeshRenderer.material.GetColor("_BaseColor");
            finalColor.a = 1.0f;
            skinnedMeshRenderer.material.SetColor("_BaseColor", finalColor);
        }

        SetToOriginalMaterial();
    }

    private void SetToOriginalMaterial()
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = originalMaterial;
        }
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.material = originalMaterial;
        }
    }

    private void SetToTransparentMaterial()
    {
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            meshRenderer.material = transparentMaterial;
        }
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            skinnedMeshRenderer.material = transparentMaterial;
        }
    }
}
