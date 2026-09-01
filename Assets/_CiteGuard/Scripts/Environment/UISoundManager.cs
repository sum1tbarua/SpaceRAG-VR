using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance { get; private set; }

    [Header("Optional Custom Audio")]
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.18f;

    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.28f;

    private AudioSource audioSource;
    private AudioClip generatedHoverClip;
    private AudioClip generatedClickClip;

    private void Awake()
    {
        Instance = this;

        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;

        if (hoverClip == null)
        {
            generatedHoverClip = CreateChirp(
                "Generated UI Hover",
                650f,
                900f,
                0.055f,
                0.35f
            );

            hoverClip = generatedHoverClip;
        }

        if (clickClip == null)
        {
            generatedClickClip = CreateChirp(
                "Generated UI Click",
                950f,
                520f,
                0.09f,
                0.4f
            );

            clickClip = generatedClickClip;
        }
    }

    public void PlayHover()
    {
        if (hoverClip != null)
        {
            audioSource.PlayOneShot(
                hoverClip,
                hoverVolume
            );
        }
    }

    public void PlayClick()
    {
        if (clickClip != null)
        {
            audioSource.PlayOneShot(
                clickClip,
                clickVolume
            );
        }
    }

    private AudioClip CreateChirp(
        string clipName,
        float startingFrequency,
        float endingFrequency,
        float duration,
        float strength
    )
    {
        const int sampleRate = 44100;

        int sampleCount =
            Mathf.CeilToInt(sampleRate * duration);

        float[] samples = new float[sampleCount];
        float phase = 0f;

        for (int sampleIndex = 0;
             sampleIndex < sampleCount;
             sampleIndex++)
        {
            float progress =
                (float)sampleIndex /
                Mathf.Max(1, sampleCount - 1);

            float frequency = Mathf.Lerp(
                startingFrequency,
                endingFrequency,
                progress
            );

            phase +=
                2f * Mathf.PI * frequency /
                sampleRate;

            float attack =
                Mathf.Clamp01(progress / 0.08f);

            float decay =
                Mathf.Pow(1f - progress, 2f);

            float envelope = attack * decay;

            float fundamental = Mathf.Sin(phase);
            float harmonic =
                0.18f * Mathf.Sin(phase * 2f);

            samples[sampleIndex] =
                (fundamental + harmonic) *
                envelope *
                strength;
        }

        AudioClip generatedClip = AudioClip.Create(
            clipName,
            sampleCount,
            1,
            sampleRate,
            false
        );

        generatedClip.SetData(samples, 0);

        return generatedClip;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (generatedHoverClip != null)
        {
            Destroy(generatedHoverClip);
        }

        if (generatedClickClip != null)
        {
            Destroy(generatedClickClip);
        }
    }
}