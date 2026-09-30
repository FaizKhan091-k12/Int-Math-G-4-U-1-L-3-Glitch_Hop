using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using TMPro;
using DG.Tweening;

public class GlitchHopGameManager : MonoBehaviour
{
    public static GlitchHopGameManager instance;

    [System.Serializable]
    public class ActivityItem
    {
        public string stageTitle;
        public string equipmentTag;
        [TextArea(3, 5)]
        public string clueText;
        public string equationDisplay;
        public string[] options;
        public string[] optionSubtitles;
        public int correctOptionIndex;
        [TextArea(2, 4)]
        public string voiceoverCorrect;
        [TextArea(2, 4)]
        public string voiceoverWrong;
        public Sprite itemVisualSprite;
    }

    [Header("Randomization Settings")]
    [Tooltip("If enabled, questions will be shuffled in random order.")]
    public bool randomizeQuestions = false;
    [Tooltip("If enabled, option order will be shuffled for each question.")]
    public bool randomizeOptions = true;

    [Header("Screens")]
    [SerializeField] private GameObject screenInstructions;
    [SerializeField] private GameObject screenActivity;
    [SerializeField] private GameObject screenVictory;

    [Header("Instruction Screen UI")]
    [SerializeField] private Button startSoundcheckButton;
    [SerializeField] private RectTransform instructionCard;

    [Header("Activity Screen UI")]
    [SerializeField] private TextMeshProUGUI stageIndicatorText;
    [SerializeField] private TextMeshProUGUI comboStreakText;
    [SerializeField] private TextMeshProUGUI clueTMP;
    [SerializeField] private TextMeshProUGUI equationTMP;
    [SerializeField] private Image itemVisualImage;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionBadgeTexts;
    [SerializeField] private TextMeshProUGUI[] optionTexts;
    [SerializeField] private TextMeshProUGUI[] optionSubtitles;
    [SerializeField] private Image[] stageDots;

    [Header("Interactive Equalizer & Rig")]
    [SerializeField] private RectTransform[] equalizerBars;
    [SerializeField] private TextMeshProUGUI rigStatusText;
    [SerializeField] private Image rigStatusIcon;
    [SerializeField] private Image rigGlowBorder;

    [Header("Engineer Communicator Panel")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackStatusTMP;
    [SerializeField] private TextMeshProUGUI feedbackVoiceoverTMP;
    [SerializeField] private Image feedbackStatusBg;
    [SerializeField] private Button nextStageButton;

    [Header("Victory Screen UI")]
    [SerializeField] private RectTransform victoryCard;
    [SerializeField] private Button playAgainButton;

    [Header("Audio")]
    [SerializeField] private Button muteButton;

    [Header("Items Data")]
    [SerializeField] private List<ActivityItem> activityItems = new List<ActivityItem>();

    private List<ActivityItem> activePlayItems = new List<ActivityItem>();
    private int currentItemIndex = 0;
    private int activeCorrectOptionIndex = 0;
    private bool isAnsweringLocked = false;
    private bool isCurrentTrackSynced = false;
    private int currentStreak = 0;

    private readonly Color colorGlitchRed = new Color(1f, 0.22f, 0.32f, 1f);
    private readonly Color colorSyncedGreen = new Color(0.2f, 0.95f, 0.45f, 1f);
    private readonly Color colorCyanDefault = new Color(0f, 0.88f, 1f, 1f);
    private readonly Color colorWarningOrange = new Color(1f, 0.65f, 0.15f, 1f);

    // High-contrast, crystal-clear readability colors
    private readonly Color colorCardCorrectBg = new Color(0.06f, 0.28f, 0.16f, 1f); // Deep rich cyber emerald
    private readonly Color colorCardWrongBg = new Color(0.32f, 0.08f, 0.12f, 1f);   // Deep rich cyber crimson
    private readonly Color colorCardDefaultBg = new Color(0.11f, 0.16f, 0.28f, 1f); // Deep cyber navy

    [Header("Juice & Visual FX")]
    [SerializeField] private Image screenFlashOverlay;
    [SerializeField] private Transform floatingJuiceSpawnParent;
    [SerializeField] private TMP_FontAsset mainFontAsset;

    private float[] eqTargetHeights;
    private float[] eqCurrentHeights;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SetupButtonListeners();
        EnsureRuntimeInteractiveComponents();
        InitEqualizer();
        ShowInstructionsScreen();
    }

    private void EnsureRuntimeInteractiveComponents()
    {
        PopulateDefaultItemData();

        // Ensure Canvas has required shader channels for ProceduralImage
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
        }

        // Apply ProceduralImage with rounded corners across panels & buttons
        if (instructionCard != null)
        {
            ApplyProceduralImage(instructionCard.gameObject, 20f);
            Transform storyTr = instructionCard.Find("StoryBox");
            if (storyTr != null) ApplyProceduralImage(storyTr.gameObject, 14f);
            Transform missionTr = instructionCard.Find("MissionBox");
            if (missionTr != null) ApplyProceduralImage(missionTr.gameObject, 14f);
        }

        if (victoryCard != null)
        {
            ApplyProceduralImage(victoryCard.gameObject, 20f);
            Transform badgeTr = victoryCard.Find("MasterBadge");
            if (badgeTr != null) ApplyProceduralImage(badgeTr.gameObject, 14f);
        }

        if (itemVisualImage != null && itemVisualImage.transform.parent != null)
        {
            ApplyProceduralImage(itemVisualImage.transform.parent.gameObject, 18f);
        }

        if (startSoundcheckButton != null)
            ApplyProceduralImage(startSoundcheckButton.gameObject, 14f);

        if (nextStageButton != null)
            ApplyProceduralImage(nextStageButton.gameObject, 12f);

        if (playAgainButton != null)
            ApplyProceduralImage(playAgainButton.gameObject, 14f);

        if (feedbackPanel != null)
            ApplyProceduralImage(feedbackPanel, 14f);

        if (clueTMP != null && clueTMP.transform.parent != null)
            ApplyProceduralImage(clueTMP.transform.parent.gameObject, 14f);

        if (equationTMP != null && equationTMP.transform.parent != null)
            ApplyProceduralImage(equationTMP.transform.parent.gameObject, 14f);

        if (rigStatusText != null && rigStatusText.transform.parent != null)
            ApplyProceduralImage(rigStatusText.transform.parent.gameObject, 12f);

        if (stageDots != null)
        {
            for (int i = 0; i < stageDots.Length; i++)
            {
                if (stageDots[i] != null)
                    ApplyProceduralImage(stageDots[i].gameObject, 14f);
            }
        }

        // Enforce perfect right-side console layout anchors to eliminate any empty void
        if (optionButtons != null && optionButtons.Length > 0 && optionButtons[0] != null)
        {
            Transform optionsContainerTr = optionButtons[0].transform.parent;
            if (optionsContainerTr != null)
            {
                RectTransform optContainerRT = optionsContainerTr.GetComponent<RectTransform>();
                if (optContainerRT != null)
                {
                    optContainerRT.anchorMin = new Vector2(0.03f, 0.23f);
                    optContainerRT.anchorMax = new Vector2(0.97f, 0.51f);
                    optContainerRT.anchoredPosition = Vector2.zero;
                    optContainerRT.sizeDelta = Vector2.zero;
                }

                HorizontalLayoutGroup optLayout = optionsContainerTr.GetComponent<HorizontalLayoutGroup>();
                if (optLayout != null)
                {
                    optLayout.spacing = 20;
                    optLayout.childAlignment = TextAnchor.MiddleCenter;
                    optLayout.childForceExpandWidth = true;
                    optLayout.childForceExpandHeight = true;
                }

                Transform rightTerminalTr = optionsContainerTr.parent;
                if (rightTerminalTr != null)
                {
                    ApplyProceduralImage(rightTerminalTr.gameObject, 16f);

                    Transform clueTr = rightTerminalTr.Find("ClueBox");
                    if (clueTr != null)
                    {
                        ApplyProceduralImage(clueTr.gameObject, 14f);
                        RectTransform cRT = clueTr.GetComponent<RectTransform>();
                        if (cRT != null)
                        {
                            cRT.anchorMin = new Vector2(0.03f, 0.71f);
                            cRT.anchorMax = new Vector2(0.97f, 0.98f);
                            cRT.anchoredPosition = Vector2.zero;
                            cRT.sizeDelta = Vector2.zero;
                        }
                    }

                    Transform eqTr = rightTerminalTr.Find("EquationHologramBox");
                    if (eqTr != null)
                    {
                        ApplyProceduralImage(eqTr.gameObject, 14f);
                        RectTransform eRT = eqTr.GetComponent<RectTransform>();
                        if (eRT != null)
                        {
                            eRT.anchorMin = new Vector2(0.03f, 0.53f);
                            eRT.anchorMax = new Vector2(0.97f, 0.69f);
                            eRT.anchoredPosition = Vector2.zero;
                            eRT.sizeDelta = Vector2.zero;
                        }
                    }
                }
            }
        }

        if (feedbackPanel != null)
        {
            ApplyProceduralImage(feedbackPanel, 14f);
            RectTransform fbRT = feedbackPanel.GetComponent<RectTransform>();
            if (fbRT != null)
            {
                fbRT.anchorMin = new Vector2(0.03f, 0.025f);
                fbRT.anchorMax = new Vector2(0.97f, 0.21f);
                fbRT.anchoredPosition = Vector2.zero;
                fbRT.sizeDelta = Vector2.zero;
            }
        }

        // Setup Screen Flash Overlay for Juicy Blooms
        if (screenFlashOverlay == null && screenActivity != null)
        {
            Transform existingFlash = screenActivity.transform.Find("ScreenFlashOverlay");
            if (existingFlash != null)
            {
                screenFlashOverlay = existingFlash.GetComponent<Image>();
            }
            else
            {
                GameObject flashObj = new GameObject("ScreenFlashOverlay", typeof(RectTransform));
                flashObj.transform.SetParent(screenActivity.transform, false);
                RectTransform flashRT = flashObj.GetComponent<RectTransform>();
                flashRT.anchorMin = Vector2.zero;
                flashRT.anchorMax = Vector2.one;
                flashRT.sizeDelta = Vector2.zero;
                screenFlashOverlay = flashObj.AddComponent<Image>();
                screenFlashOverlay.color = Color.clear;
                screenFlashOverlay.raycastTarget = false;
                flashObj.transform.SetAsLastSibling();
                flashObj.SetActive(false);
            }
        }

        if (floatingJuiceSpawnParent == null && screenActivity != null)
        {
            floatingJuiceSpawnParent = screenActivity.transform;
        }

        // Setup Option Cards (Badges, Primary Text, Subtitles, ProceduralImage rounding)
        if (optionButtons != null && optionButtons.Length > 0)
        {
            if (optionSubtitles == null || optionSubtitles.Length != optionButtons.Length)
                optionSubtitles = new TextMeshProUGUI[optionButtons.Length];
            if (optionBadgeTexts == null || optionBadgeTexts.Length != optionButtons.Length)
                optionBadgeTexts = new TextMeshProUGUI[optionButtons.Length];

            string[] defaultBadges = { "PATCH CH 1", "PATCH CH 2", "PATCH CH 3" };

            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (optionButtons[i] == null) continue;

                ApplyProceduralImage(optionButtons[i].gameObject, 16f);

                RectTransform btnRT = optionButtons[i].GetComponent<RectTransform>();
                if (btnRT != null)
                {
                    btnRT.sizeDelta = new Vector2(275, 230);
                }

                // Badge (Top chip)
                Transform badgeTr = optionButtons[i].transform.Find("Badge");
                if (badgeTr == null)
                {
                    GameObject badgeObj = new GameObject("Badge", typeof(RectTransform));
                    badgeObj.transform.SetParent(optionButtons[i].transform, false);
                    RectTransform bRT = badgeObj.GetComponent<RectTransform>();
                    bRT.anchorMin = new Vector2(0.5f, 1f);
                    bRT.anchorMax = new Vector2(0.5f, 1f);
                    bRT.pivot = new Vector2(0.5f, 1f);
                    bRT.anchoredPosition = new Vector2(0, -8);
                    bRT.sizeDelta = new Vector2(170, 28);

                    ApplyProceduralImage(badgeObj, 10f);
                    ProceduralImage bPImg = badgeObj.GetComponent<ProceduralImage>();
                    if (bPImg != null) bPImg.color = new Color(0f, 0.5f, 0.7f, 0.35f);

                    GameObject bTextObj = new GameObject("Text", typeof(RectTransform));
                    bTextObj.transform.SetParent(badgeObj.transform, false);
                    RectTransform btRT = bTextObj.GetComponent<RectTransform>();
                    btRT.anchorMin = Vector2.zero;
                    btRT.anchorMax = Vector2.one;
                    btRT.sizeDelta = Vector2.zero;

                    TextMeshProUGUI bTMP = bTextObj.AddComponent<TextMeshProUGUI>();
                    if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                        bTMP.font = optionTexts[i].font;
                    bTMP.text = (i < defaultBadges.Length) ? defaultBadges[i] : $"SETTING {i + 1}";
                    bTMP.fontSize = 12;
                    bTMP.fontStyle = FontStyles.Bold;
                    bTMP.color = new Color(0f, 0.95f, 1f, 1f);
                    bTMP.alignment = TextAlignmentOptions.Center;
                    optionBadgeTexts[i] = bTMP;
                }
                else
                {
                    ApplyProceduralImage(badgeTr.gameObject, 10f);
                    optionBadgeTexts[i] = badgeTr.GetComponentInChildren<TextMeshProUGUI>();
                }

                // Primary Text (Center)
                Transform mainTextTr = optionButtons[i].transform.Find("Text");
                if (mainTextTr != null)
                {
                    RectTransform mainRT = mainTextTr.GetComponent<RectTransform>();
                    if (mainRT != null)
                    {
                        // mainRT.anchorMin = new Vector2(0.04f, 0.38f);
                        // mainRT.anchorMax = new Vector2(0.96f, 0.84f);
                        // mainRT.anchoredPosition = Vector2.zero;
                        // mainRT.sizeDelta = Vector2.zero;
                    }
                    TextMeshProUGUI optTMP = mainTextTr.GetComponent<TextMeshProUGUI>();
                    if (optTMP != null)
                    {
                        optTMP.fontSize = 90;
                        optTMP.fontStyle = FontStyles.Bold;
                    }
                }

                // Subtitle (Bottom description)
                Transform subTr = optionButtons[i].transform.Find("Subtitle");
                TextMeshProUGUI subTMP = null;
                if (subTr == null)
                {
                    GameObject subObj = new GameObject("Subtitle", typeof(RectTransform));
                    subObj.transform.SetParent(optionButtons[i].transform, false);
                    RectTransform subRT = subObj.GetComponent<RectTransform>();
                    subRT.anchorMin = new Vector2(0.04f, 0.05f);
                    subRT.anchorMax = new Vector2(0.96f, 0.36f);
                    subRT.anchoredPosition = Vector2.zero;
                    subRT.sizeDelta = Vector2.zero;

                    subTMP = subObj.AddComponent<TextMeshProUGUI>();
                    if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                        subTMP.font = optionTexts[i].font;
                    subTMP.fontSize = 12;
                    subTMP.fontStyle = FontStyles.Normal;
                    subTMP.color = new Color(0f, 0.85f, 1f, 0.9f);
                    subTMP.alignment = TextAlignmentOptions.Center;
                    subTMP.enableWordWrapping = true;
                }
                else
                {
                    subTMP = subTr.GetComponent<TextMeshProUGUI>();
                }

                optionSubtitles[i] = subTMP;
            }
        }

        // Setup Equalizer Bars on Left Rig if missing
        if ((equalizerBars == null || equalizerBars.Length == 0) && itemVisualImage != null)
        {
            Transform leftRig = itemVisualImage.transform.parent;
            if (leftRig != null)
            {
                RectTransform itemRT = itemVisualImage.GetComponent<RectTransform>();
                itemRT.anchorMin = new Vector2(0.04f, 0.16f);
                itemRT.anchorMax = new Vector2(0.96f, 0.91f);

                Transform existingPill = leftRig.Find("RigStatusPill");
                if (existingPill == null)
                {
                    GameObject pillObj = new GameObject("RigStatusPill", typeof(RectTransform));
                    pillObj.transform.SetParent(leftRig, false);
                    RectTransform pillRT = pillObj.GetComponent<RectTransform>();
                    pillRT.anchorMin = new Vector2(0.04f, 1f);
                    pillRT.anchorMax = new Vector2(0.96f, 1f);
                    pillRT.pivot = new Vector2(0.5f, 1f);
                    pillRT.anchoredPosition = new Vector2(0, -15);
                    pillRT.sizeDelta = new Vector2(0, 40);

                    Image pillBg = pillObj.AddComponent<Image>();
                    pillBg.color = new Color(0.08f, 0.12f, 0.2f, 0.9f);

                    GameObject iconObj = new GameObject("StatusIcon", typeof(RectTransform));
                    iconObj.transform.SetParent(pillObj.transform, false);
                    RectTransform iconRT = iconObj.GetComponent<RectTransform>();
                    iconRT.anchorMin = new Vector2(0f, 0.5f);
                    iconRT.anchorMax = new Vector2(0f, 0.5f);
                    iconRT.pivot = new Vector2(0f, 0.5f);
                    iconRT.anchoredPosition = new Vector2(15, 0);
                    iconRT.sizeDelta = new Vector2(16, 16);
                    rigStatusIcon = iconObj.AddComponent<Image>();
                    rigStatusIcon.color = colorWarningOrange;

                    GameObject textObj = new GameObject("StatusText", typeof(RectTransform));
                    textObj.transform.SetParent(pillObj.transform, false);
                    RectTransform textRT = textObj.GetComponent<RectTransform>();
                    textRT.anchorMin = new Vector2(0f, 0f);
                    textRT.anchorMax = new Vector2(1f, 1f);
                    textRT.anchoredPosition = Vector2.zero;
                    textRT.sizeDelta = new Vector2(-80, 0);

                    rigStatusText = textObj.AddComponent<TextMeshProUGUI>();
                    if (clueTMP != null) rigStatusText.font = clueTMP.font;
                    rigStatusText.fontSize = 17;
                    rigStatusText.fontStyle = FontStyles.Bold;
                    rigStatusText.color = colorWarningOrange;
                    rigStatusText.alignment = TextAlignmentOptions.MidlineLeft;
                }

                Transform existingSpectrum = leftRig.Find("EqualizerSpectrum");
                if (existingSpectrum == null)
                {
                    GameObject specObj = new GameObject("EqualizerSpectrum", typeof(RectTransform));
                    specObj.transform.SetParent(leftRig, false);
                    RectTransform specRT = specObj.GetComponent<RectTransform>();
                    specRT.anchorMin = new Vector2(0.04f, 0.02f);
                    specRT.anchorMax = new Vector2(0.96f, 0.14f);
                    specRT.anchoredPosition = Vector2.zero;
                    specRT.sizeDelta = Vector2.zero;

                    Image specBg = specObj.AddComponent<Image>();
                    specBg.color = new Color(0.03f, 0.05f, 0.1f, 0.85f);

                    HorizontalLayoutGroup layout = specObj.AddComponent<HorizontalLayoutGroup>();
                    layout.spacing = 6;
                    layout.childAlignment = TextAnchor.LowerCenter;
                    layout.childForceExpandWidth = true;
                    layout.childForceExpandHeight = false;

                    equalizerBars = new RectTransform[16];
                    for (int i = 0; i < 16; i++)
                    {
                        GameObject barObj = new GameObject($"Bar_{i}", typeof(RectTransform));
                        barObj.transform.SetParent(specObj.transform, false);
                        RectTransform barRT = barObj.GetComponent<RectTransform>();
                        barRT.sizeDelta = new Vector2(22, 30);
                        Image barImg = barObj.AddComponent<Image>();
                        barImg.color = colorWarningOrange;
                        equalizerBars[i] = barRT;
                    }
                }
            }
        }
    }

    private void PopulateDefaultItemData()
    {
        if (activityItems == null || activityItems.Count == 0) return;

        string[][] defaultSubs = new string[][]
        {
            new string[] { "[REVERSE FACTOR • BALANCES 4 × 15 = 60]", "[DUPLICATE FACTOR 4]", "[OVERLOAD 4 × 64 ≠ 60]" },
            new string[] { "[POWER OF 10 • 5 × 2 = 10 (FAST CALC)]", "[UNGROUPED • 18 × 5 WITHOUT REGROUP]", "[INVALID FACTOR]" },
            new string[] { "[REMAINING 4 BEATS • 14 - 10 = 4]", "[UNSLICED 14 BEATS]", "[DOUBLE LOOP 28 BEATS]" },
            new string[] { "[HIGHEST PRIORITY • ÷ COMES BEFORE + / -]", "[LOWER PRIORITY • CANNOT ADD FIRST]", "[LOWER PRIORITY • CANNOT SUBTRACT FIRST]" },
            new string[] { "[DROP BEAT: 3 × 4 = 12, THEN 20 - 12 = 8 dB]", "[ORDER ERROR • (20 - 3) × 4]", "[INCOMPLETE • 3 × 4 ONLY]" }
        };

        string[] tags = new string[]
        {
            "STEREO STAGE CHANNELS",
            "16-STEP BASS SEQUENCER",
            "14-BEAT DIGITAL SAMPLER",
            "EFFECTS PEDALBOARD CHAIN",
            "MASTER VOLUME DJ CONSOLE"
        };

        for (int i = 0; i < activityItems.Count; i++)
        {
            if (string.IsNullOrEmpty(activityItems[i].equipmentTag) && i < tags.Length)
            {
                activityItems[i].equipmentTag = tags[i];
            }
            if ((activityItems[i].optionSubtitles == null || activityItems[i].optionSubtitles.Length == 0) && i < defaultSubs.Length)
            {
                activityItems[i].optionSubtitles = defaultSubs[i];
            }
        }
    }

    private void InitEqualizer()
    {
        if (equalizerBars != null && equalizerBars.Length > 0)
        {
            eqTargetHeights = new float[equalizerBars.Length];
            eqCurrentHeights = new float[equalizerBars.Length];
            for (int i = 0; i < equalizerBars.Length; i++)
            {
                if (equalizerBars[i] != null)
                {
                    ApplyProceduralImage(equalizerBars[i].gameObject, 4f);
                }
                eqCurrentHeights[i] = 20f;
                eqTargetHeights[i] = 40f;
            }
        }
    }

    private void Update()
    {
        UpdateEqualizerVisualizer();

        // Subtle beat pulse on visual image
        if (itemVisualImage != null && isCurrentTrackSynced)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 8f) * 0.02f;
            itemVisualImage.transform.localScale = new Vector3(pulse, pulse, 1f);
        }
    }

    private void UpdateEqualizerVisualizer()
    {
        if (equalizerBars == null || equalizerBars.Length == 0) return;

        for (int i = 0; i < equalizerBars.Length; i++)
        {
            if (equalizerBars[i] == null) continue;

            if (Mathf.Abs(eqCurrentHeights[i] - eqTargetHeights[i]) < 6f || Random.value < 0.15f)
            {
                if (isCurrentTrackSynced)
                {
                    float wave = Mathf.Sin(Time.time * 7f + (i * 0.38f)) * 0.5f + 0.5f;
                    eqTargetHeights[i] = Mathf.Lerp(25f, 95f, wave);
                }
                else
                {
                    eqTargetHeights[i] = Random.Range(10f, 98f);
                }
            }

            eqCurrentHeights[i] = Mathf.Lerp(eqCurrentHeights[i], eqTargetHeights[i], Time.deltaTime * 16f);
            equalizerBars[i].sizeDelta = new Vector2(equalizerBars[i].sizeDelta.x, eqCurrentHeights[i]);

            Image barImg = equalizerBars[i].GetComponent<Image>();
            if (barImg != null)
            {
                if (isCurrentTrackSynced)
                {
                    barImg.color = Color.Lerp(colorSyncedGreen, colorCyanDefault, (float)i / equalizerBars.Length);
                }
                else
                {
                    barImg.color = (Random.value < 0.08f) ? colorGlitchRed : colorWarningOrange;
                }
            }
        }
    }

    private void SetupButtonListeners()
    {
        if (startSoundcheckButton != null)
        {
            startSoundcheckButton.onClick.AddListener(OnStartSoundcheckClicked);
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index));
        }

        if (nextStageButton != null)
        {
            nextStageButton.onClick.AddListener(OnNextStageClicked);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(OnPlayAgainClicked);
        }

        if (muteButton != null && AudioManager.instance != null)
        {
            muteButton.onClick.AddListener(AudioManager.instance.ToggleMute);
        }
    }

    public void ShowInstructionsScreen()
    {
        screenInstructions.SetActive(true);
        screenActivity.SetActive(false);
        screenVictory.SetActive(false);

        if (instructionCard != null)
        {
            instructionCard.localScale = Vector3.zero;
            instructionCard.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void OnStartSoundcheckClicked()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClick();

        StartCoroutine(StartSoundcheckRoutine());
    }

    private IEnumerator StartSoundcheckRoutine()
    {
        yield return null;
        currentItemIndex = 0;
        currentStreak = 0;

        // Build active playlist (with optional question randomization)
        BuildActivePlaylist();

        screenInstructions.SetActive(false);
        screenActivity.SetActive(true);
        screenVictory.SetActive(false);

        LoadItem(currentItemIndex);
    }

    private void BuildActivePlaylist()
    {
        activePlayItems.Clear();
        activePlayItems.AddRange(activityItems);

        if (randomizeQuestions)
        {
            // Fisher-Yates shuffle questions
            for (int i = activePlayItems.Count - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                ActivityItem temp = activePlayItems[i];
                activePlayItems[i] = activePlayItems[rnd];
                activePlayItems[rnd] = temp;
            }
        }
    }

    public void LoadItem(int index)
    {
        if (index < 0 || index >= activePlayItems.Count)
        {
            ShowVictoryScreen();
            return;
        }

        currentItemIndex = index;
        isAnsweringLocked = false;
        isCurrentTrackSynced = false;
        ActivityItem item = activePlayItems[index];

        // Track header & indicator
        if (stageIndicatorText != null)
        {
            stageIndicatorText.text = $"TRACK {index + 1} / {activePlayItems.Count}  •  {item.stageTitle.ToUpper()}";
        }

        // Combo streak text
        if (comboStreakText != null)
        {
            comboStreakText.text = (currentStreak > 1) ? $"SYNC COMBO: x{currentStreak}" : "SYNC READY";
            comboStreakText.color = (currentStreak > 1) ? colorSyncedGreen : colorCyanDefault;
        }

        // Rig status text
        if (rigStatusText != null)
        {
            string tag = string.IsNullOrEmpty(item.equipmentTag) ? "DIAGNOSTIC STATUS" : item.equipmentTag.ToUpper();
            rigStatusText.text = $"DIAGNOSTIC STATUS: {tag} [GLITCH DETECTED]";
            rigStatusText.color = colorWarningOrange;
        }

        if (rigStatusIcon != null)
        {
            rigStatusIcon.color = colorWarningOrange;
        }

        if (rigGlowBorder != null)
        {
            rigGlowBorder.color = new Color(colorWarningOrange.r, colorWarningOrange.g, colorWarningOrange.b, 0.4f);
        }

        // Stage dots
        for (int i = 0; i < stageDots.Length; i++)
        {
            if (stageDots[i] != null)
            {
                if (i < index)
                {
                    stageDots[i].color = colorSyncedGreen;
                }
                else if (i == index)
                {
                    stageDots[i].color = colorCyanDefault;
                }
                else
                {
                    stageDots[i].color = new Color(0.25f, 0.3f, 0.45f, 0.8f);
                }
            }
        }

        // Clue and Equation
        if (clueTMP != null)
        {
            clueTMP.text = item.clueText;
        }

        if (equationTMP != null)
        {
            equationTMP.text = item.equationDisplay;
            equationTMP.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 5, 1);
        }

        // Visual Sprite & Rig Pulse
        if (itemVisualImage != null && item.itemVisualSprite != null)
        {
            itemVisualImage.sprite = item.itemVisualSprite;
            itemVisualImage.transform.localScale = Vector3.one * 0.95f;
            itemVisualImage.transform.DOScale(1f, 0.45f).SetEase(Ease.OutSine);
        }

        // Prepare option order (Randomized or Canonical)
        int numOptions = item.options.Length;
        int[] displayIndices = new int[numOptions];
        for (int i = 0; i < numOptions; i++) displayIndices[i] = i;

        if (randomizeOptions)
        {
            // Shuffle indices
            for (int i = numOptions - 1; i > 0; i--)
            {
                int rnd = Random.Range(0, i + 1);
                int temp = displayIndices[i];
                displayIndices[i] = displayIndices[rnd];
                displayIndices[rnd] = temp;
            }
        }

        // Find which display position contains the correct option
        for (int i = 0; i < numOptions; i++)
        {
            if (displayIndices[i] == item.correctOptionIndex)
            {
                activeCorrectOptionIndex = i;
                break;
            }
        }

        // Render Option Cards
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < numOptions)
            {
                int sourceIdx = displayIndices[i];
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].interactable = true;

                if (optionTexts[i] != null)
                {
                    optionTexts[i].text = item.options[sourceIdx];
                    // Font size stays 90 for all questions, except 26 for the operations question (Item 4)
                    bool isLongOptionQuestion = item.options[sourceIdx].Length > 5;
                    optionTexts[i].fontSize = isLongOptionQuestion ? 26f : 90f;
                }

                if (optionSubtitles != null && i < optionSubtitles.Length && optionSubtitles[i] != null)
                {
                    if (item.optionSubtitles != null && sourceIdx < item.optionSubtitles.Length)
                    {
                        optionSubtitles[i].gameObject.SetActive(true);
                        optionSubtitles[i].text = item.optionSubtitles[sourceIdx];
                    }
                    else
                    {
                        optionSubtitles[i].gameObject.SetActive(false);
                    }
                }

                Image btnImg = optionButtons[i].GetComponent<Image>();
                if (btnImg != null)
                {
                    btnImg.color = colorCardDefaultBg;
                }

                Transform badgeTr = optionButtons[i].transform.Find("Badge");
                if (badgeTr != null)
                {
                    Image bImg = badgeTr.GetComponent<Image>();
                    if (bImg != null) bImg.color = new Color(0f, 0.5f, 0.7f, 0.35f);
                }
                if (optionBadgeTexts != null && i < optionBadgeTexts.Length && optionBadgeTexts[i] != null)
                {
                    optionBadgeTexts[i].color = new Color(0f, 0.95f, 1f, 1f);
                }
                if (optionTexts != null && i < optionTexts.Length && optionTexts[i] != null)
                {
                    optionTexts[i].color = Color.white;
                }
                if (optionSubtitles != null && i < optionSubtitles.Length && optionSubtitles[i] != null)
                {
                    optionSubtitles[i].color = new Color(0f, 0.85f, 1f, 0.9f);
                }

                optionButtons[i].transform.localScale = Vector3.zero;
                optionButtons[i].transform.DOScale(1f, 0.35f + (i * 0.08f)).SetEase(Ease.OutBack);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }

        // Reset communicator panel to listening state
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true); // Always visible as sound engineer communicator!
            if (feedbackStatusTMP != null)
            {
                feedbackStatusTMP.text = "[ COMMS: LISTENING TO FREQUENCY BUS... ]";
                feedbackStatusTMP.color = colorCyanDefault;
            }
            if (feedbackVoiceoverTMP != null)
            {
                feedbackVoiceoverTMP.text = "\"Awaiting patch command. Select the matching frequency setting above to realign the audio track.\"";
            }
            if (feedbackStatusBg != null)
            {
                feedbackStatusBg.color = new Color(0.07f, 0.11f, 0.2f, 0.95f);
            }
        }

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(false);
        }
    }

    private void OnOptionSelected(int optionIndex)
    {
        if (isAnsweringLocked) return;

        bool isCorrect = (optionIndex == activeCorrectOptionIndex);
        ActivityItem item = activePlayItems[currentItemIndex];

        if (isCorrect)
        {
            isAnsweringLocked = true;
            isCurrentTrackSynced = true;
            currentStreak++;

            if (AudioManager.instance != null)
                AudioManager.instance.PlayCorrect();

            TriggerScreenFlash(true);

            // Correct button juicy punch & high-contrast cyber emerald glow
            Button selectedBtn = optionButtons[optionIndex];
            selectedBtn.transform.DOPunchScale(Vector3.one * 0.24f, 0.45f, 10, 1);
            Image btnImg = selectedBtn.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.color = colorCardCorrectBg; // Rich dark cyber emerald (#062816)
            }

            // Update button texts for 100% crystal-clear readability
            Transform badgeTr = selectedBtn.transform.Find("Badge");
            if (badgeTr != null)
            {
                Image bImg = badgeTr.GetComponent<Image>();
                if (bImg != null) bImg.color = new Color(0.1f, 0.45f, 0.24f, 0.95f);
            }
            if (optionBadgeTexts != null && optionIndex < optionBadgeTexts.Length && optionBadgeTexts[optionIndex] != null)
            {
                optionBadgeTexts[optionIndex].color = new Color(0.35f, 1f, 0.65f, 1f); // Neon green
            }
            if (optionTexts != null && optionIndex < optionTexts.Length && optionTexts[optionIndex] != null)
            {
                optionTexts[optionIndex].color = Color.white; // Pure white bold number (14:1 contrast!)
            }
            if (optionSubtitles != null && optionIndex < optionSubtitles.Length && optionSubtitles[optionIndex] != null)
            {
                optionSubtitles[optionIndex].color = new Color(0.85f, 1f, 0.92f, 1f); // Crisp mint-white subtitle (12:1 contrast!)
            }

            // Spawn floating juice popup
            string popupTxt = currentStreak > 1 ? $"+100 SYNC! COMBO x{currentStreak}!" : "+100 FREQ SYNC!";
            SpawnJuiceFloatingText(selectedBtn.transform.position, popupTxt, colorSyncedGreen);

            // Punch the hardware visual image & glow border
            if (itemVisualImage != null)
            {
                itemVisualImage.transform.DOPunchScale(Vector3.one * 0.12f, 0.5f, 8, 1);
            }
            if (rigGlowBorder != null)
            {
                rigGlowBorder.color = colorSyncedGreen;
                rigGlowBorder.transform.DOPunchScale(Vector3.one * 0.08f, 0.5f, 5, 1);
            }

            // Update combo streak text
            if (comboStreakText != null)
            {
                comboStreakText.text = $"SYNC COMBO: x{currentStreak} [PERFECT!]";
                comboStreakText.color = colorSyncedGreen;
                comboStreakText.transform.DOPunchScale(Vector3.one * 0.35f, 0.4f, 8, 1);
            }

            // Update rig status
            if (rigStatusText != null)
            {
                string tag = string.IsNullOrEmpty(item.equipmentTag) ? "FREQUENCY" : item.equipmentTag.ToUpper();
                rigStatusText.text = $"DIAGNOSTIC STATUS: {tag} [SYNCED & ONLINE]";
                rigStatusText.color = colorSyncedGreen;
            }
            if (rigStatusIcon != null)
            {
                rigStatusIcon.color = colorSyncedGreen;
            }

            // Disable other buttons
            for (int i = 0; i < optionButtons.Length; i++)
            {
                if (i != optionIndex)
                {
                    optionButtons[i].interactable = false;
                }
            }

            // Update stage dot
            if (currentItemIndex < stageDots.Length && stageDots[currentItemIndex] != null)
            {
                stageDots[currentItemIndex].color = colorSyncedGreen;
                stageDots[currentItemIndex].transform.DOPunchScale(Vector3.one * 0.35f, 0.4f, 5, 1);
            }

            ShowFeedback(true, item.voiceoverCorrect);
        }
        else
        {
            currentStreak = 0; // Reset streak
            if (comboStreakText != null)
            {
                comboStreakText.text = "SYNC READY";
                comboStreakText.color = colorCyanDefault;
            }

            if (AudioManager.instance != null)
                AudioManager.instance.PlayWrong();

            TriggerScreenFlash(false);

            // Wrong button wiggle
            Button wrongBtn = optionButtons[optionIndex];
            wrongBtn.transform.DOShakePosition(0.45f, new Vector3(18f, 0f, 0f), 22, 90f);
            Image btnImg = wrongBtn.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.color = colorCardWrongBg;
                btnImg.DOColor(colorCardDefaultBg, 0.85f);
            }
            if (optionSubtitles != null && optionIndex < optionSubtitles.Length && optionSubtitles[optionIndex] != null)
            {
                optionSubtitles[optionIndex].color = new Color(1f, 0.85f, 0.88f, 1f);
            }

            // Spawn floating glitch popup
            SpawnJuiceFloatingText(wrongBtn.transform.position, "[!] GLITCH DETECTED", colorGlitchRed);

            // Screen & Rig glitch shake
            if (screenActivity != null)
            {
                screenActivity.transform.DOShakePosition(0.35f, new Vector3(10f, 6f, 0f), 20, 90f);
            }
            if (itemVisualImage != null)
            {
                itemVisualImage.transform.DOShakePosition(0.35f, new Vector3(12f, 8f, 0f), 18, 90f);
            }
            if (rigGlowBorder != null)
            {
                rigGlowBorder.color = colorGlitchRed;
            }

            if (rigStatusText != null)
            {
                string tag = string.IsNullOrEmpty(item.equipmentTag) ? "FREQUENCY" : item.equipmentTag.ToUpper();
                rigStatusText.text = $"DIAGNOSTIC STATUS: {tag} [STATIC DISTORTION]";
                rigStatusText.color = colorGlitchRed;
            }

            ShowFeedback(false, item.voiceoverWrong);
        }
    }

    private void TriggerScreenFlash(bool correct)
    {
        if (screenFlashOverlay == null) return;

        screenFlashOverlay.gameObject.SetActive(true);
        Color flashColor = correct ? new Color(0.2f, 1f, 0.45f, 0.25f) : new Color(1f, 0.18f, 0.25f, 0.28f);
        screenFlashOverlay.color = flashColor;
        screenFlashOverlay.DOFade(0f, 0.45f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (screenFlashOverlay != null)
                screenFlashOverlay.gameObject.SetActive(false);
        });
    }

    private void SpawnJuiceFloatingText(Vector3 worldPos, string text, Color color)
    {
        Transform parentTr = floatingJuiceSpawnParent != null ? floatingJuiceSpawnParent : transform;
        GameObject popupObj = new GameObject("JuicePopup", typeof(RectTransform));
        popupObj.transform.SetParent(parentTr, false);
        popupObj.transform.position = worldPos + new Vector3(0, 40f, 0);

        TextMeshProUGUI tmp = popupObj.AddComponent<TextMeshProUGUI>();
        if (mainFontAsset != null) tmp.font = mainFontAsset;
        else if (clueTMP != null) tmp.font = clueTMP.font;

        tmp.text = text;
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;

        RectTransform rt = popupObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(350, 60);

        popupObj.transform.localScale = Vector3.zero;
        popupObj.transform.DOScale(1.2f, 0.25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            popupObj.transform.DOScale(1f, 0.15f);
        });

        rt.DOMoveY(worldPos.y + 110f, 0.75f).SetEase(Ease.OutCubic);
        tmp.DOFade(0f, 0.75f).SetEase(Ease.InQuad).OnComplete(() =>
        {
            Destroy(popupObj);
        });
    }

    private void ShowFeedback(bool correct, string voiceover)
    {
        if (feedbackPanel == null) return;

        feedbackPanel.SetActive(true);
        feedbackPanel.transform.DOPunchScale(Vector3.one * 0.04f, 0.3f, 5, 1);

        if (feedbackStatusTMP != null)
        {
            feedbackStatusTMP.text = correct ? "[OK] FREQUENCY PATCH APPLIED! [SYNCED]" : "[!] AUDIO GLITCH DETECTED! [RESYNC REQUIRED]";
            feedbackStatusTMP.color = correct ? new Color(0.3f, 1f, 0.6f, 1f) : new Color(1f, 0.35f, 0.45f, 1f);
        }

        if (feedbackVoiceoverTMP != null)
        {
            feedbackVoiceoverTMP.text = $"\"{voiceover}\"";
            feedbackVoiceoverTMP.color = new Color(0.94f, 0.97f, 1f, 1f);
        }

        if (feedbackStatusBg != null)
        {
            // Deep sleek console slate so text is 100% legible
            feedbackStatusBg.color = correct ? new Color(0.05f, 0.16f, 0.11f, 0.97f) : new Color(0.20f, 0.05f, 0.08f, 0.97f);
        }

        if (nextStageButton != null)
        {
            nextStageButton.gameObject.SetActive(correct);
            if (correct)
            {
                nextStageButton.transform.localScale = Vector3.zero;
                nextStageButton.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);

                TextMeshProUGUI nextBtnTMP = nextStageButton.GetComponentInChildren<TextMeshProUGUI>();
                if (nextBtnTMP != null)
                {
                    nextBtnTMP.text = (currentItemIndex == activePlayItems.Count - 1) ? "FINALIZE MIX >" : "NEXT TRACK >";
                }
            }
        }
    }

    public void OnNextStageClicked()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClick();

        StartCoroutine(NextStageRoutine());
    }

    private IEnumerator NextStageRoutine()
    {
        yield return null;
        int nextIndex = currentItemIndex + 1;
        if (nextIndex < activePlayItems.Count)
        {
            LoadItem(nextIndex);
        }
        else
        {
            ShowVictoryScreen();
        }
    }

    public void ShowVictoryScreen()
    {
        screenInstructions.SetActive(false);
        screenActivity.SetActive(false);
        screenVictory.SetActive(true);

        if (AudioManager.instance != null)
            AudioManager.instance.PlayCorrect();

        if (victoryCard != null)
        {
            victoryCard.localScale = Vector3.zero;
            victoryCard.DOScale(1f, 0.6f).SetEase(Ease.OutBack);
        }
    }

    public void OnPlayAgainClicked()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlayClick();

        StartCoroutine(PlayAgainRoutine());
    }

    private IEnumerator PlayAgainRoutine()
    {
        yield return null;
        ShowInstructionsScreen();
    }

    private void ApplyProceduralImage(GameObject go, float radius, float borderWidth = 0f)
    {
        if (go == null) return;
        ProceduralImage pImg = go.GetComponent<ProceduralImage>();
        if (pImg == null)
        {
            Image oldImg = go.GetComponent<Image>();
            Color c = oldImg != null ? oldImg.color : Color.white;
            Sprite s = (oldImg != null && oldImg.sprite != null && oldImg.sprite.name != "UISprite") ? oldImg.sprite : null;
            Button btn = go.GetComponent<Button>();

            if (oldImg != null)
            {
                DestroyImmediate(oldImg);
            }

            pImg = go.AddComponent<ProceduralImage>();
            pImg.color = c;
            pImg.BorderWidth = borderWidth;
            pImg.FalloffDistance = 1f;
            if (s != null) pImg.sprite = s;
            if (btn != null) btn.targetGraphic = pImg;
        }

        UniformModifier mod = go.GetComponent<UniformModifier>();
        if (mod == null)
        {
            mod = go.AddComponent<UniformModifier>();
        }
        mod.Radius = radius;
    }
}
