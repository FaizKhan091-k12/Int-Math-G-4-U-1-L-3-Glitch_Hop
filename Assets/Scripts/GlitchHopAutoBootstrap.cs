using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using TMPro;
using DG.Tweening;

public class GlitchHopAutoBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnSceneLoaded()
    {
        if (FindFirstObjectByType<GlitchHopGameManager>() == null)
        {
            BuildRuntimeSetup();
        }
    }

    public static void BuildRuntimeSetup()
    {
        Debug.Log("[GlitchHop] Bootstrapping UI Hierarchy at Runtime...");

        // Ensure EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Camera setup
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.09f, 1f);
        }

        // Load Sprites
        Sprite bgSprite = Resources.Load<Sprite>("GlitchHop/glitch_hop_lab_bg");
        Sprite item1Sprite = Resources.Load<Sprite>("GlitchHop/item1_speakers");
        Sprite item2Sprite = Resources.Load<Sprite>("GlitchHop/item2_sequencer");
        Sprite item3Sprite = Resources.Load<Sprite>("GlitchHop/item3_sampler");
        Sprite item4Sprite = Resources.Load<Sprite>("GlitchHop/item4_pedalboard");
        Sprite item5Sprite = Resources.Load<Sprite>("GlitchHop/item5_volume_slider");
        Sprite victorySprite = Resources.Load<Sprite>("GlitchHop/final_victory_screen");
        Sprite titleBadgeSprite = Resources.Load<Sprite>("GlitchHop/title_badge_lab");
        Sprite muteSprite = Resources.Load<Sprite>("GlitchHop/Mute");
        Sprite unmuteSprite = Resources.Load<Sprite>("GlitchHop/UnMute");

        // Load Audio Clips - Strictly never use Dial.mp3
        AudioClip bgmClip = Resources.Load<AudioClip>("GlitchHop/Audio/Up-on-a-Housetop-chosic.com_");
        AudioClip correctClip = Resources.Load<AudioClip>("GlitchHop/Audio/Correct Sound");
        AudioClip wrongClip = Resources.Load<AudioClip>("GlitchHop/Audio/Wrong");
        AudioClip clickClip = null; // Procedural soft cyber click will be used by AudioManager

        // Load Font
        TMP_FontAsset fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bgObj = CreateUIElement("GlobalBackground", canvasObj.transform);
        StretchFull(bgObj.GetComponent<RectTransform>());
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = new Color(0.7f, 0.75f, 0.9f, 1f);

        // Overlay
        GameObject overlayObj = CreateUIElement("DarkOverlay", canvasObj.transform);
        StretchFull(overlayObj.GetComponent<RectTransform>());
        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0.04f, 0.05f, 0.1f, 0.55f);

        // Top Bar
        GameObject topBar = CreateUIElement("TopBar", canvasObj.transform);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.anchoredPosition = Vector2.zero;
        topBarRT.sizeDelta = new Vector2(0, 80);
        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.05f, 0.07f, 0.14f, 0.85f);

        // Top Title
        GameObject titleTextObj = CreateUIElement("TopTitleText", topBar.transform);
        RectTransform titleRT = titleTextObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.5f);
        titleRT.anchorMax = new Vector2(0, 0.5f);
        titleRT.pivot = new Vector2(0, 0.5f);
        titleRT.anchoredPosition = new Vector2(30, 0);
        titleRT.sizeDelta = new Vector2(400, 60);
        TextMeshProUGUI topTitleTMP = titleTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) topTitleTMP.font = fontAsset;
        topTitleTMP.text = "GLITCH-HOP AUDIO LAB";
        topTitleTMP.fontSize = 24;
        topTitleTMP.fontStyle = FontStyles.Bold;
        topTitleTMP.color = new Color(0f, 0.9f, 1f, 1f);
        topTitleTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Stage Dots
        GameObject stageDotsContainer = CreateUIElement("StageDotsContainer", topBar.transform);
        RectTransform dotsContainerRT = stageDotsContainer.GetComponent<RectTransform>();
        dotsContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
        dotsContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        dotsContainerRT.pivot = new Vector2(0.5f, 0.5f);
        dotsContainerRT.anchoredPosition = Vector2.zero;
        dotsContainerRT.sizeDelta = new Vector2(300, 40);

        HorizontalLayoutGroup dotsLayout = stageDotsContainer.AddComponent<HorizontalLayoutGroup>();
        dotsLayout.spacing = 15;
        dotsLayout.childAlignment = TextAnchor.MiddleCenter;
        dotsLayout.childForceExpandWidth = false;
        dotsLayout.childForceExpandHeight = false;

        Image[] stageDots = new Image[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject dotObj = CreateUIElement($"Dot_{i + 1}", stageDotsContainer.transform);
            RectTransform dotRT = dotObj.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(28, 28);
            Image dotImg = dotObj.AddComponent<Image>();
            dotImg.color = (i == 0) ? new Color(0f, 0.85f, 1f, 1f) : new Color(0.25f, 0.3f, 0.45f, 0.7f);
            stageDots[i] = dotImg;

            GameObject dotNumObj = CreateUIElement("Num", dotObj.transform);
            StretchFull(dotNumObj.GetComponent<RectTransform>());
            TextMeshProUGUI numTMP = dotNumObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) numTMP.font = fontAsset;
            numTMP.text = (i + 1).ToString();
            numTMP.fontSize = 14;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.color = Color.white;
            numTMP.alignment = TextAlignmentOptions.Center;
        }

        // Mute Button
        GameObject muteBtnObj = CreateUIElement("MuteButton", topBar.transform);
        RectTransform muteRT = muteBtnObj.GetComponent<RectTransform>();
        muteRT.anchorMin = new Vector2(1, 0.5f);
        muteRT.anchorMax = new Vector2(1, 0.5f);
        muteRT.pivot = new Vector2(1, 0.5f);
        muteRT.anchoredPosition = new Vector2(-150, 0);
        muteRT.sizeDelta = new Vector2(50, 50);
        Image muteImg = muteBtnObj.AddComponent<Image>();
        muteImg.sprite = unmuteSprite != null ? unmuteSprite : muteSprite;
        Button muteBtn = muteBtnObj.AddComponent<Button>();
        muteBtnObj.AddComponent<UIHoverClickEffect>();

        // Audio Manager
        GameObject audioMgrObj = new GameObject("AudioManager");
        AudioManager audioMgr = audioMgrObj.AddComponent<AudioManager>();
        AudioSource musicSrc = audioMgrObj.AddComponent<AudioSource>();
        AudioSource sfxSrc = audioMgrObj.AddComponent<AudioSource>();

        // Setup audio fields using reflection for runtime
        SetField(audioMgr, "musicSource", musicSrc);
        SetField(audioMgr, "sfxSource", sfxSrc);
        SetField(audioMgr, "bgmClip", bgmClip);
        SetField(audioMgr, "correctClip", correctClip);
        SetField(audioMgr, "wrongClip", wrongClip);
        SetField(audioMgr, "clickClip", clickClip);
        SetField(audioMgr, "muteButtonImage", muteImg);
        SetField(audioMgr, "muteSprite", muteSprite);
        SetField(audioMgr, "unmuteSprite", unmuteSprite);

        // SCREEN 1: Instructions
        GameObject screenInstructions = CreateUIElement("Screen_Instructions", canvasObj.transform);
        StretchFull(screenInstructions.GetComponent<RectTransform>());

        GameObject instrCard = CreateUIElement("InstructionCard", screenInstructions.transform);
        RectTransform instrCardRT = instrCard.GetComponent<RectTransform>();
        instrCardRT.anchorMin = new Vector2(0.5f, 0.5f);
        instrCardRT.anchorMax = new Vector2(0.5f, 0.5f);
        instrCardRT.pivot = new Vector2(0.5f, 0.5f);
        instrCardRT.sizeDelta = new Vector2(1150, 780);
        instrCardRT.anchoredPosition = new Vector2(0, -25);
        Image instrCardImg = instrCard.AddComponent<Image>();
        instrCardImg.color = new Color(0.06f, 0.08f, 0.16f, 0.94f);

        GameObject titleBannerObj = CreateUIElement("TitleBanner", instrCard.transform);
        RectTransform titleBannerRT = titleBannerObj.GetComponent<RectTransform>();
        titleBannerRT.anchorMin = new Vector2(0.5f, 1f);
        titleBannerRT.anchorMax = new Vector2(0.5f, 1f);
        titleBannerRT.pivot = new Vector2(0.5f, 1f);
        titleBannerRT.anchoredPosition = new Vector2(0, -25);
        titleBannerRT.sizeDelta = new Vector2(500, 200);
        Image bannerImg = titleBannerObj.AddComponent<Image>();
        bannerImg.sprite = titleBadgeSprite;
        bannerImg.preserveAspect = true;

        GameObject subtitleObj = CreateUIElement("Subtitle", instrCard.transform);
        RectTransform subRT = subtitleObj.GetComponent<RectTransform>();
        subRT.anchorMin = new Vector2(0.5f, 1f);
        subRT.anchorMax = new Vector2(0.5f, 1f);
        subRT.pivot = new Vector2(0.5f, 1f);
        subRT.anchoredPosition = new Vector2(0, -235);
        subRT.sizeDelta = new Vector2(1000, 40);
        TextMeshProUGUI subTMP = subtitleObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) subTMP.font = fontAsset;
        subTMP.text = "SCREEN 1 — INSTRUCTIONS";
        subTMP.fontSize = 22;
        subTMP.fontStyle = FontStyles.Bold;
        subTMP.color = new Color(1f, 0.8f, 0.2f, 1f);
        subTMP.alignment = TextAlignmentOptions.Center;

        GameObject storyBoxObj = CreateUIElement("StoryBox", instrCard.transform);
        RectTransform storyRT = storyBoxObj.GetComponent<RectTransform>();
        storyRT.anchorMin = new Vector2(0.5f, 1f);
        storyRT.anchorMax = new Vector2(0.5f, 1f);
        storyRT.pivot = new Vector2(0.5f, 1f);
        storyRT.anchoredPosition = new Vector2(0, -280);
        storyRT.sizeDelta = new Vector2(1020, 150);
        Image storyBg = storyBoxObj.AddComponent<Image>();
        storyBg.color = new Color(0.1f, 0.13f, 0.24f, 0.85f);

        GameObject storyTextObj = CreateUIElement("StoryText", storyBoxObj.transform);
        StretchFull(storyTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI storyTMP = storyTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) storyTMP.font = fontAsset;
        storyTMP.text = "Welcome to the Glitch-Hop Audio Lab! You are the head sound engineer for the galaxy's biggest music festival, but a rogue cyber-virus has scrambled the synthesizer's audio tracks right before showtime!";
        storyTMP.fontSize = 21;
        storyTMP.color = new Color(0.92f, 0.95f, 1f, 1f);
        storyTMP.alignment = TextAlignmentOptions.Center;
        storyTMP.enableWordWrapping = true;

        GameObject missionBoxObj = CreateUIElement("MissionBox", instrCard.transform);
        RectTransform missionRT = missionBoxObj.GetComponent<RectTransform>();
        missionRT.anchorMin = new Vector2(0.5f, 1f);
        missionRT.anchorMax = new Vector2(0.5f, 1f);
        missionRT.pivot = new Vector2(0.5f, 1f);
        missionRT.anchoredPosition = new Vector2(0, -445);
        missionRT.sizeDelta = new Vector2(1020, 180);
        Image missionBg = missionBoxObj.AddComponent<Image>();
        missionBg.color = new Color(0.08f, 0.18f, 0.32f, 0.85f);

        GameObject missionTextObj = CreateUIElement("MissionText", missionBoxObj.transform);
        StretchFull(missionTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI missionTMP = missionTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) missionTMP.font = fontAsset;
        missionTMP.text = "<color=#00FFFF><b>YOUR MISSION:</b></color> Read the sonic clues to fix the broken sound waves. Use your knowledge of the <b>commutative</b>, <b>associative</b>, and <b>distributive laws</b>, along with the correct <b>order of operations</b>, to realign the music frequencies. Click the correct setting to patch the audio and get the crowd dancing!";
        missionTMP.fontSize = 20;
        missionTMP.color = new Color(0.88f, 0.96f, 1f, 1f);
        missionTMP.alignment = TextAlignmentOptions.Center;
        missionTMP.enableWordWrapping = true;

        GameObject startBtnObj = CreateUIElement("StartSoundcheckButton", instrCard.transform);
        RectTransform startBtnRT = startBtnObj.GetComponent<RectTransform>();
        startBtnRT.anchorMin = new Vector2(0.5f, 0f);
        startBtnRT.anchorMax = new Vector2(0.5f, 0f);
        startBtnRT.pivot = new Vector2(0.5f, 0f);
        startBtnRT.anchoredPosition = new Vector2(0, 40);
        startBtnRT.sizeDelta = new Vector2(420, 75);
        Image startBtnImg = startBtnObj.AddComponent<Image>();
        startBtnImg.color = new Color(0f, 0.75f, 0.95f, 1f);
        Button startBtn = startBtnObj.AddComponent<Button>();
        startBtnObj.AddComponent<UIHoverClickEffect>();

        GameObject startBtnTextObj = CreateUIElement("Text", startBtnObj.transform);
        StretchFull(startBtnTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI startBtnTMP = startBtnTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) startBtnTMP.font = fontAsset;
        startBtnTMP.text = "BEGIN SOUNDCHECK >";
        startBtnTMP.fontSize = 24;
        startBtnTMP.fontStyle = FontStyles.Bold;
        startBtnTMP.color = new Color(0.04f, 0.08f, 0.16f, 1f);
        startBtnTMP.alignment = TextAlignmentOptions.Center;

        // SCREEN 2: Activity
        GameObject screenActivity = CreateUIElement("Screen_Activity", canvasObj.transform);
        StretchFull(screenActivity.GetComponent<RectTransform>());
        screenActivity.SetActive(false);

        GameObject stageBannerObj = CreateUIElement("StageHeaderBanner", screenActivity.transform);
        RectTransform stageBannerRT = stageBannerObj.GetComponent<RectTransform>();
        stageBannerRT.anchorMin = new Vector2(0.5f, 1f);
        stageBannerRT.anchorMax = new Vector2(0.5f, 1f);
        stageBannerRT.pivot = new Vector2(0.5f, 1f);
        stageBannerRT.anchoredPosition = new Vector2(0, -90);
        stageBannerRT.sizeDelta = new Vector2(1820, 48);
        Image stageBannerImg = stageBannerObj.AddComponent<Image>();
        stageBannerImg.color = new Color(0.08f, 0.12f, 0.22f, 0.85f);

        GameObject stageTextObj = CreateUIElement("StageIndicatorText", stageBannerObj.transform);
        StretchFull(stageTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI stageIndicatorTMP = stageTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) stageIndicatorTMP.font = fontAsset;
        stageIndicatorTMP.text = "TRACK 1 / 5 • COMMUTATIVE LAW";
        stageIndicatorTMP.fontSize = 20;
        stageIndicatorTMP.fontStyle = FontStyles.Bold;
        stageIndicatorTMP.color = new Color(0f, 0.95f, 0.95f, 1f);
        stageIndicatorTMP.alignment = TextAlignmentOptions.Center;

        GameObject contentArea = CreateUIElement("ContentArea", screenActivity.transform);
        RectTransform contentAreaRT = contentArea.GetComponent<RectTransform>();
        contentAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentAreaRT.pivot = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchoredPosition = new Vector2(0, -45);
        contentAreaRT.sizeDelta = new Vector2(1820, 840);

        // Left Visual Rig
        GameObject leftVisualRig = CreateUIElement("LeftVisualRig", contentArea.transform);
        RectTransform leftRigRT = leftVisualRig.GetComponent<RectTransform>();
        leftRigRT.anchorMin = new Vector2(0f, 0f);
        leftRigRT.anchorMax = new Vector2(0.48f, 1f);
        leftRigRT.anchoredPosition = Vector2.zero;
        leftRigRT.sizeDelta = Vector2.zero;
        Image leftRigBg = leftVisualRig.AddComponent<Image>();
        leftRigBg.color = new Color(0.06f, 0.08f, 0.15f, 0.9f);

        GameObject itemImgObj = CreateUIElement("ItemVisualImage", leftVisualRig.transform);
        RectTransform itemImgRT = itemImgObj.GetComponent<RectTransform>();
        itemImgRT.anchorMin = new Vector2(0.04f, 0.05f);
        itemImgRT.anchorMax = new Vector2(0.96f, 0.95f);
        itemImgRT.anchoredPosition = Vector2.zero;
        itemImgRT.sizeDelta = Vector2.zero;
        Image itemVisualImg = itemImgObj.AddComponent<Image>();
        itemVisualImg.sprite = item1Sprite;
        itemVisualImg.preserveAspect = true;

        // Right Terminal
        GameObject rightTerminal = CreateUIElement("RightTerminal", contentArea.transform);
        RectTransform rightTermRT = rightTerminal.GetComponent<RectTransform>();
        rightTermRT.anchorMin = new Vector2(0.5f, 0f);
        rightTermRT.anchorMax = new Vector2(1f, 1f);
        rightTermRT.anchoredPosition = Vector2.zero;
        rightTermRT.sizeDelta = Vector2.zero;
        Image rightTermBg = rightTerminal.AddComponent<Image>();
        rightTermBg.color = new Color(0.07f, 0.1f, 0.18f, 0.92f);

        // Clue Box
        GameObject clueBoxObj = CreateUIElement("ClueBox", rightTerminal.transform);
        RectTransform clueBoxRT = clueBoxObj.GetComponent<RectTransform>();
        clueBoxRT.anchorMin = new Vector2(0.03f, 0.71f);
        clueBoxRT.anchorMax = new Vector2(0.97f, 0.98f);
        clueBoxRT.anchoredPosition = Vector2.zero;
        clueBoxRT.sizeDelta = Vector2.zero;
        Image clueBoxBg = clueBoxObj.AddComponent<Image>();
        clueBoxBg.color = new Color(0.11f, 0.16f, 0.28f, 0.9f);

        GameObject clueTextObj = CreateUIElement("ClueTMP", clueBoxObj.transform);
        StretchFull(clueTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI clueTMP = clueTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) clueTMP.font = fontAsset;
        clueTMP.text = "Clue text goes here...";
        clueTMP.fontSize = 21;
        clueTMP.color = new Color(0.9f, 0.95f, 1f, 1f);
        clueTMP.alignment = TextAlignmentOptions.TopLeft;
        clueTMP.enableWordWrapping = true;

        // Equation Hologram
        GameObject eqBoxObj = CreateUIElement("EquationHologramBox", rightTerminal.transform);
        RectTransform eqBoxRT = eqBoxObj.GetComponent<RectTransform>();
        eqBoxRT.anchorMin = new Vector2(0.03f, 0.53f);
        eqBoxRT.anchorMax = new Vector2(0.97f, 0.69f);
        eqBoxRT.anchoredPosition = Vector2.zero;
        eqBoxRT.sizeDelta = Vector2.zero;
        Image eqBoxBg = eqBoxObj.AddComponent<Image>();
        eqBoxBg.color = new Color(0.04f, 0.22f, 0.32f, 0.95f);

        GameObject eqTextObj = CreateUIElement("EquationTMP", eqBoxObj.transform);
        StretchFull(eqTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI equationTMP = eqTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) equationTMP.font = fontAsset;
        equationTMP.text = "4 × [ ? ] = 60";
        equationTMP.fontSize = 32;
        equationTMP.fontStyle = FontStyles.Bold;
        equationTMP.color = new Color(0.2f, 1f, 0.6f, 1f);
        equationTMP.alignment = TextAlignmentOptions.Center;

        // Options
        GameObject optionsContainer = CreateUIElement("OptionsContainer", rightTerminal.transform);
        RectTransform optionsRT = optionsContainer.GetComponent<RectTransform>();
        optionsRT.anchorMin = new Vector2(0.03f, 0.23f);
        optionsRT.anchorMax = new Vector2(0.97f, 0.51f);
        optionsRT.anchoredPosition = Vector2.zero;
        optionsRT.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup optLayout = optionsContainer.AddComponent<HorizontalLayoutGroup>();
        optLayout.spacing = 20;
        optLayout.childAlignment = TextAnchor.MiddleCenter;
        optLayout.childForceExpandWidth = true;
        optLayout.childForceExpandHeight = true;

        Button[] optionButtons = new Button[3];
        TextMeshProUGUI[] optionTexts = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = CreateUIElement($"OptionBtn_{i}", optionsContainer.transform);
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.15f, 0.22f, 0.38f, 1f);
            Button btn = btnObj.AddComponent<Button>();
            btnObj.AddComponent<UIHoverClickEffect>();
            optionButtons[i] = btn;

            GameObject textObj = CreateUIElement("Text", btnObj.transform);
            StretchFull(textObj.GetComponent<RectTransform>());
            TextMeshProUGUI optTMP = textObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) optTMP.font = fontAsset;
            optTMP.text = $"Option {i + 1}";
            optTMP.fontSize = 24;
            optTMP.fontStyle = FontStyles.Bold;
            optTMP.color = Color.white;
            optTMP.alignment = TextAlignmentOptions.Center;
            optTMP.enableWordWrapping = true;
            optionTexts[i] = optTMP;
        }

        // Feedback Panel
        GameObject feedbackPanelObj = CreateUIElement("FeedbackPanel", rightTerminal.transform);
        RectTransform fbRT = feedbackPanelObj.GetComponent<RectTransform>();
        fbRT.anchorMin = new Vector2(0.03f, 0.025f);
        fbRT.anchorMax = new Vector2(0.97f, 0.21f);
        fbRT.anchoredPosition = Vector2.zero;
        fbRT.sizeDelta = Vector2.zero;
        Image fbBg = feedbackPanelObj.AddComponent<Image>();
        fbBg.color = new Color(0.08f, 0.14f, 0.25f, 0.95f);

        GameObject fbStatusObj = CreateUIElement("FeedbackStatusTMP", feedbackPanelObj.transform);
        RectTransform fbStatusRT = fbStatusObj.GetComponent<RectTransform>();
        fbStatusRT.anchorMin = new Vector2(0f, 1f);
        fbStatusRT.anchorMax = new Vector2(1f, 1f);
        fbStatusRT.pivot = new Vector2(0.5f, 1f);
        fbStatusRT.anchoredPosition = new Vector2(0, -12);
        fbStatusRT.sizeDelta = new Vector2(-40, 36);
        TextMeshProUGUI fbStatusTMP = fbStatusObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) fbStatusTMP.font = fontAsset;
        fbStatusTMP.text = "[OK] FREQUENCY PATCH APPLIED! [SYNCED]";
        fbStatusTMP.fontSize = 20;
        fbStatusTMP.fontStyle = FontStyles.Bold;
        fbStatusTMP.color = new Color(0.2f, 0.95f, 0.45f, 1f);
        fbStatusTMP.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject fbVoObj = CreateUIElement("FeedbackVoiceoverTMP", feedbackPanelObj.transform);
        RectTransform fbVoRT = fbVoObj.GetComponent<RectTransform>();
        fbVoRT.anchorMin = new Vector2(0f, 0.32f);
        fbVoRT.anchorMax = new Vector2(1f, 0.82f);
        fbVoRT.anchoredPosition = Vector2.zero;
        fbVoRT.sizeDelta = new Vector2(-40, 0);
        TextMeshProUGUI fbVoTMP = fbVoObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) fbVoTMP.font = fontAsset;
        fbVoTMP.text = "\"Voiceover message goes here...\"";
        fbVoTMP.fontSize = 19;
        fbVoTMP.color = new Color(0.92f, 0.95f, 1f, 1f);
        fbVoTMP.alignment = TextAlignmentOptions.TopLeft;
        fbVoTMP.enableWordWrapping = true;

        GameObject nextBtnObj = CreateUIElement("NextStageButton", feedbackPanelObj.transform);
        RectTransform nextBtnRT = nextBtnObj.GetComponent<RectTransform>();
        nextBtnRT.anchorMin = new Vector2(1f, 0f);
        nextBtnRT.anchorMax = new Vector2(1f, 0f);
        nextBtnRT.pivot = new Vector2(1f, 0f);
        nextBtnRT.anchoredPosition = new Vector2(-20, 15);
        nextBtnRT.sizeDelta = new Vector2(250, 52);
        Image nextBtnImg = nextBtnObj.AddComponent<Image>();
        nextBtnImg.color = new Color(0f, 0.85f, 0.5f, 1f);
        Button nextBtn = nextBtnObj.AddComponent<Button>();
        nextBtnObj.AddComponent<UIHoverClickEffect>();

        GameObject nextBtnTextObj = CreateUIElement("Text", nextBtnObj.transform);
        StretchFull(nextBtnTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI nextBtnTMP = nextBtnTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) nextBtnTMP.font = fontAsset;
        nextBtnTMP.text = "NEXT TRACK >";
        nextBtnTMP.fontSize = 20;
        nextBtnTMP.fontStyle = FontStyles.Bold;
        nextBtnTMP.color = new Color(0.04f, 0.1f, 0.14f, 1f);
        nextBtnTMP.alignment = TextAlignmentOptions.Center;

        // SCREEN 3: Victory
        GameObject screenVictory = CreateUIElement("Screen_Victory", canvasObj.transform);
        StretchFull(screenVictory.GetComponent<RectTransform>());
        screenVictory.SetActive(false);

        GameObject vicBgObj = CreateUIElement("VictoryBg", screenVictory.transform);
        StretchFull(vicBgObj.GetComponent<RectTransform>());
        Image vicBgImg = vicBgObj.AddComponent<Image>();
        vicBgImg.sprite = victorySprite;
        vicBgImg.color = new Color(0.95f, 0.95f, 1f, 1f);

        GameObject vicCard = CreateUIElement("VictoryCard", screenVictory.transform);
        RectTransform vicCardRT = vicCard.GetComponent<RectTransform>();
        vicCardRT.anchorMin = new Vector2(0.5f, 0.5f);
        vicCardRT.anchorMax = new Vector2(0.5f, 0.5f);
        vicCardRT.pivot = new Vector2(0.5f, 0.5f);
        vicCardRT.sizeDelta = new Vector2(1100, 700);
        vicCardRT.anchoredPosition = Vector2.zero;
        Image vicCardImg = vicCard.AddComponent<Image>();
        vicCardImg.color = new Color(0.04f, 0.06f, 0.15f, 0.92f);

        GameObject vicHeadObj = CreateUIElement("Headline", vicCard.transform);
        RectTransform vicHeadRT = vicHeadObj.GetComponent<RectTransform>();
        vicHeadRT.anchorMin = new Vector2(0.5f, 1f);
        vicHeadRT.anchorMax = new Vector2(0.5f, 1f);
        vicHeadRT.pivot = new Vector2(0.5f, 1f);
        vicHeadRT.anchoredPosition = new Vector2(0, -60);
        vicHeadRT.sizeDelta = new Vector2(1000, 60);
        TextMeshProUGUI vicHeadTMP = vicHeadObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) vicHeadTMP.font = fontAsset;
        vicHeadTMP.text = "AUDIO LAB FULLY RESTORED!";
        vicHeadTMP.fontSize = 44;
        vicHeadTMP.fontStyle = FontStyles.Bold;
        vicHeadTMP.color = new Color(0f, 0.95f, 1f, 1f);
        vicHeadTMP.alignment = TextAlignmentOptions.Center;

        GameObject vicSubObj = CreateUIElement("Subtitle", vicCard.transform);
        RectTransform vicSubRT = vicSubObj.GetComponent<RectTransform>();
        vicSubRT.anchorMin = new Vector2(0.5f, 1f);
        vicSubRT.anchorMax = new Vector2(0.5f, 1f);
        vicSubRT.pivot = new Vector2(0.5f, 1f);
        vicSubRT.anchoredPosition = new Vector2(0, -135);
        vicSubRT.sizeDelta = new Vector2(1000, 50);
        TextMeshProUGUI vicSubTMP = vicSubObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) vicSubTMP.font = fontAsset;
        vicSubTMP.text = "You patched every single glitch in the system!";
        vicSubTMP.fontSize = 26;
        vicSubTMP.fontStyle = FontStyles.Bold;
        vicSubTMP.color = new Color(1f, 0.85f, 0.25f, 1f);
        vicSubTMP.alignment = TextAlignmentOptions.Center;

        GameObject vicBodyObj = CreateUIElement("BodyText", vicCard.transform);
        RectTransform vicBodyRT = vicBodyObj.GetComponent<RectTransform>();
        vicBodyRT.anchorMin = new Vector2(0.5f, 1f);
        vicBodyRT.anchorMax = new Vector2(0.5f, 1f);
        vicBodyRT.pivot = new Vector2(0.5f, 1f);
        vicBodyRT.anchoredPosition = new Vector2(0, -210);
        vicBodyRT.sizeDelta = new Vector2(950, 160);
        TextMeshProUGUI vicBodyTMP = vicBodyObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) vicBodyTMP.font = fontAsset;
        vicBodyTMP.text = "By applying the <b>Laws of Arithmetic</b> and routing the proper <b>Order of Operations</b>, you saved the concert and mixed a multi-platinum track. The crowd is going wild!";
        vicBodyTMP.fontSize = 24;
        vicBodyTMP.color = new Color(0.9f, 0.95f, 1f, 1f);
        vicBodyTMP.alignment = TextAlignmentOptions.Center;
        vicBodyTMP.enableWordWrapping = true;

        GameObject masterBadgeObj = CreateUIElement("MasterBadge", vicCard.transform);
        RectTransform masterBadgeRT = masterBadgeObj.GetComponent<RectTransform>();
        masterBadgeRT.anchorMin = new Vector2(0.5f, 0.5f);
        masterBadgeRT.anchorMax = new Vector2(0.5f, 0.5f);
        masterBadgeRT.pivot = new Vector2(0.5f, 0.5f);
        masterBadgeRT.anchoredPosition = new Vector2(0, -70);
        masterBadgeRT.sizeDelta = new Vector2(850, 90);
        Image badgeBg = masterBadgeObj.AddComponent<Image>();
        badgeBg.color = new Color(0.15f, 0.12f, 0.35f, 0.95f);

        GameObject badgeTextObj = CreateUIElement("Text", masterBadgeObj.transform);
        StretchFull(badgeTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI badgeTMP = badgeTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) badgeTMP.font = fontAsset;
        badgeTMP.text = "* YOU ARE A MASTER AUDIO ENGINEER! *";
        badgeTMP.fontSize = 32;
        badgeTMP.fontStyle = FontStyles.Bold;
        badgeTMP.color = new Color(0.3f, 1f, 0.65f, 1f);
        badgeTMP.alignment = TextAlignmentOptions.Center;

        GameObject playAgainBtnObj = CreateUIElement("PlayAgainButton", vicCard.transform);
        RectTransform playAgainRT = playAgainBtnObj.GetComponent<RectTransform>();
        playAgainRT.anchorMin = new Vector2(0.5f, 0f);
        playAgainRT.anchorMax = new Vector2(0.5f, 0f);
        playAgainRT.pivot = new Vector2(0.5f, 0f);
        playAgainRT.anchoredPosition = new Vector2(0, 50);
        playAgainRT.sizeDelta = new Vector2(380, 75);
        Image playAgainImg = playAgainBtnObj.AddComponent<Image>();
        playAgainImg.color = new Color(0f, 0.75f, 0.95f, 1f);
        Button playAgainBtn = playAgainBtnObj.AddComponent<Button>();
        playAgainBtnObj.AddComponent<UIHoverClickEffect>();

        GameObject playAgainTextObj = CreateUIElement("Text", playAgainBtnObj.transform);
        StretchFull(playAgainTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI playAgainTMP = playAgainTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) playAgainTMP.font = fontAsset;
        playAgainTMP.text = "REPLAY SOUNDCHECK (RESTART)";
        playAgainTMP.fontSize = 24;
        playAgainTMP.fontStyle = FontStyles.Bold;
        playAgainTMP.color = new Color(0.04f, 0.08f, 0.16f, 1f);
        playAgainTMP.alignment = TextAlignmentOptions.Center;

        // GlitchHopGameManager
        GameObject gmObj = new GameObject("GlitchHopGameManager");
        GlitchHopGameManager gm = gmObj.AddComponent<GlitchHopGameManager>();

        SetField(gm, "screenInstructions", screenInstructions);
        SetField(gm, "screenActivity", screenActivity);
        SetField(gm, "screenVictory", screenVictory);
        SetField(gm, "startSoundcheckButton", startBtn);
        SetField(gm, "instructionCard", instrCardRT);
        SetField(gm, "stageIndicatorText", stageIndicatorTMP);
        SetField(gm, "clueTMP", clueTMP);
        SetField(gm, "equationTMP", equationTMP);
        SetField(gm, "itemVisualImage", itemVisualImg);
        SetField(gm, "optionButtons", optionButtons);
        SetField(gm, "optionTexts", optionTexts);
        SetField(gm, "stageDots", stageDots);
        SetField(gm, "feedbackPanel", feedbackPanelObj);
        SetField(gm, "feedbackStatusTMP", fbStatusTMP);
        SetField(gm, "feedbackVoiceoverTMP", fbVoTMP);
        SetField(gm, "feedbackStatusBg", fbBg);
        SetField(gm, "nextStageButton", nextBtn);
        SetField(gm, "victoryCard", vicCardRT);
        SetField(gm, "playAgainButton", playAgainBtn);
        SetField(gm, "muteButton", muteBtn);

        // Activity Items List
        List<GlitchHopGameManager.ActivityItem> items = new List<GlitchHopGameManager.ActivityItem>
        {
            new GlitchHopGameManager.ActivityItem
            {
                stageTitle = "Commutative Law • Stage Speakers",
                clueText = "* To balance the left and right stage speakers, the audio volume must match perfectly. Reversing the channels using the commutative law will fix the sound. If the left channel is programmed as 15 × 4 = 60, what missing volume level will balance the right channel: 4 × __ = 60?",
                equationDisplay = "Left: 15 × 4 = 60   ->   Right: 4 × [ ? ] = 60",
                options = new string[] { "15", "4", "64" },
                correctOptionIndex = 0,
                voiceoverCorrect = "Channels synced! The commutative law proves that swapping the order of our factors leaves the total track volume exactly the same.",
                voiceoverWrong = "The sound is totally lopsided! Look at the left channel's numbers. We just need to flip their order to keep the output identical.",
                itemVisualSprite = item1Sprite
            },
            new GlitchHopGameManager.ActivityItem
            {
                stageTitle = "Associative Law • Bass Sequencer",
                clueText = "* The bass sequencer is lagging because the code is too heavy. Use the associative law to regroup these sound waves into a quick, friendly power of 10 first: (18 × 5) × 2 = 18 × (5 × 2). What simplified value goes into the master loop? 18 × __",
                equationDisplay = "(18 × 5) × 2 = 18 × (5 × 2) = 18 × [ ? ]",
                options = new string[] { "10", "90", "25" },
                correctOptionIndex = 0,
                voiceoverCorrect = "Smooth bassline! Grouping five and two creates an instant ten, turning a complicated sound mix into a super easy mental calculation.",
                voiceoverWrong = "Too much audio static! Look inside the second pair of parentheses. Multiply those two isolated numbers together first to clear the lag.",
                itemVisualSprite = item2Sprite
            },
            new GlitchHopGameManager.ActivityItem
            {
                stageTitle = "Distributive Law • Beat Slicer",
                clueText = "* A massive 14-beat sound sample is completely overloading the digital track. Chop it down using the distributive law! Let's break 7 × 14 into two easier sub-loops: (7 × 10) + (7 × __). What is the missing slice?",
                equationDisplay = "7 × 14 = (7 × 10) + (7 × [ ? ])",
                options = new string[] { "4", "14", "28" },
                correctOptionIndex = 0,
                voiceoverCorrect = "Remix achieved! Decomposing fourteen into ten and four allows you to distribute the multiplication into two effortless steps.",
                voiceoverWrong = "The sampler crashed! You already split a block of ten away from the original fourteen-beat clip. How many beats are left over to distribute?",
                itemVisualSprite = item3Sprite
            },
            new GlitchHopGameManager.ActivityItem
            {
                stageTitle = "Order of Operations • FX Pedal Chain",
                clueText = "* The synthesizer’s special effects pedal chain is glitching out. To prevent a massive sound pop that could blow the speakers, which operation must you process first in this command string? 45 + 12 ÷ 3 - 6",
                equationDisplay = "45 + 12 ÷ 3 - 6   ->   Which operation FIRST?",
                options = new string[] { "Division (12 ÷ 3)", "Addition (45 + 12)", "Subtraction (3 - 6)" },
                correctOptionIndex = 0,
                voiceoverCorrect = "Perfect frequency! According to the strict rules of operation order, division always takes absolute priority over addition and subtraction.",
                voiceoverWrong = "Ouch, that scratched the record! Don't just read the audio line from left to right. You must locate the highest-priority math operation first.",
                itemVisualSprite = item4Sprite
            },
            new GlitchHopGameManager.ActivityItem
            {
                stageTitle = "Master Calculation • Volume Slider",
                clueText = "* The master volume slider requires the absolute correct final value of this multi-operation audio sequence. Calculate carefully to drop the beat: 20 - 3 × 4",
                equationDisplay = "20 - 3 × 4 = [ ? ]",
                options = new string[] { "8", "68", "12" },
                correctOptionIndex = 0,
                voiceoverCorrect = "The beat just dropped perfectly! Multiplying three by four gives twelve, and subtracting twelve from twenty leaves a flawless volume level of eight.",
                voiceoverWrong = "Total sonic distortion! You subtracted three from twenty first. Remember, the multiplication effect must always fire before the subtraction step.",
                itemVisualSprite = item5Sprite
            }
        };

        SetField(gm, "activityItems", items);

        Debug.Log("[GlitchHop] Bootstrapping Complete! Game Ready.");
    }

    private static void SetField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }
}
