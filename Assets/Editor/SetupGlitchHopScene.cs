using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

[InitializeOnLoad]
public class SetupGlitchHopScene
{
    static SetupGlitchHopScene()
    {
        EditorApplication.delayCall += () =>
        {
            if (!SessionState.GetBool("GlitchHopSceneBuilt_v7", false))
            {
                SessionState.SetBool("GlitchHopSceneBuilt_v7", true);
                BuildScene();
            }
        };
    }

    [MenuItem("GlitchHop/Build Complete Visuals and Scene")]
    public static void BuildScene()
    {
        ConfigureAllSprites();

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (var obj in rootObjects)
        {
            if (obj.name != "Main Camera" && obj.name != "Global Light 2D")
            {
                Object.DestroyImmediate(obj);
            }
            else if (obj.name == "Main Camera")
            {
                Camera cam = obj.GetComponent<Camera>();
                if (cam != null)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0.04f, 0.05f, 0.09f, 1f);
                }
            }
        }

        // Load Sprites
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/glitch_hop_lab_bg.jpg");
        Sprite item1Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item1_speakers.jpg");
        Sprite item2Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item2_sequencer.jpg");
        Sprite item3Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item3_sampler.jpg");
        Sprite item4Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item4_pedalboard.jpg");
        Sprite item5Sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/item5_volume_slider.jpg");
        Sprite victorySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/final_victory_screen.jpg");
        Sprite titleBadgeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/title_badge_lab.jpg");
        Sprite muteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Mute.png");
        Sprite unmuteSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UnMute.png");

        // Load Audio Clips
        AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Up-on-a-Housetop-chosic.com_.mp3");
        AudioClip correctClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Correct Sound.mp3");
        AudioClip wrongClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Wrong.mp3");

        // Load TMP Font
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // 1. Create EventSystem
        CreateEventSystem();

        // 2. Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Global Background
        GameObject bgObj = CreateUIElement("GlobalBackground", canvasObj.transform);
        StretchFull(bgObj.GetComponent<RectTransform>());
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = bgSprite;
        bgImg.color = new Color(0.7f, 0.75f, 0.9f, 1f);

        // Dark Vignette / Gradient Overlay
        GameObject overlayObj = CreateUIElement("DarkOverlay", canvasObj.transform);
        StretchFull(overlayObj.GetComponent<RectTransform>());
        Image overlayImg = overlayObj.AddComponent<Image>();
        overlayImg.color = new Color(0.04f, 0.05f, 0.1f, 0.55f);

        // 4. Top Navigation Bar
        GameObject topBar = CreateUIElement("TopBar", canvasObj.transform);
        RectTransform topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.pivot = new Vector2(0.5f, 1);
        topBarRT.anchoredPosition = Vector2.zero;
        topBarRT.sizeDelta = new Vector2(0, 80);

        Image topBarImg = topBar.AddComponent<Image>();
        topBarImg.color = new Color(0.05f, 0.07f, 0.14f, 0.85f);

        // Top Title / Logo
        GameObject titleTextObj = CreateUIElement("TopTitleText", topBar.transform);
        RectTransform titleRT = titleTextObj.GetComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0, 0.5f);
        titleRT.anchorMax = new Vector2(0, 0.5f);
        titleRT.pivot = new Vector2(0, 0.5f);
        titleRT.anchoredPosition = new Vector2(30, 0);
        titleRT.sizeDelta = new Vector2(350, 60);
        TextMeshProUGUI topTitleTMP = titleTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) topTitleTMP.font = fontAsset;
        topTitleTMP.text = "GLITCH-HOP AUDIO LAB";
        topTitleTMP.fontSize = 22;
        topTitleTMP.fontStyle = FontStyles.Bold;
        topTitleTMP.color = new Color(0f, 0.9f, 1f, 1f);
        topTitleTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Stage Indicator Tracker (5 dots in top bar)
        GameObject stageDotsContainer = CreateUIElement("StageDotsContainer", topBar.transform);
        RectTransform dotsContainerRT = stageDotsContainer.GetComponent<RectTransform>();
        dotsContainerRT.anchorMin = new Vector2(0.5f, 0.5f);
        dotsContainerRT.anchorMax = new Vector2(0.5f, 0.5f);
        dotsContainerRT.pivot = new Vector2(0.5f, 0.5f);
        dotsContainerRT.anchoredPosition = new Vector2(-60, 0);
        dotsContainerRT.sizeDelta = new Vector2(280, 40);

        HorizontalLayoutGroup dotsLayout = stageDotsContainer.AddComponent<HorizontalLayoutGroup>();
        dotsLayout.spacing = 15;
        dotsLayout.childAlignment = TextAnchor.MiddleCenter;
        dotsLayout.childForceExpandWidth = false;
        dotsLayout.childForceExpandHeight = false;

        ProceduralImage[] stageDots = new ProceduralImage[5];
        for (int i = 0; i < 5; i++)
        {
            ProceduralImage dotImg = CreateProceduralUI($"Dot_{i + 1}", stageDotsContainer.transform, (i == 0) ? new Color(0f, 0.85f, 1f, 1f) : new Color(0.25f, 0.3f, 0.45f, 0.7f), 14f);
            RectTransform dotRT = dotImg.rectTransform;
            dotRT.sizeDelta = new Vector2(28, 28);
            stageDots[i] = dotImg;

            GameObject dotNumObj = CreateUIElement("Num", dotImg.transform);
            StretchFull(dotNumObj.GetComponent<RectTransform>());
            TextMeshProUGUI numTMP = dotNumObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) numTMP.font = fontAsset;
            numTMP.text = (i + 1).ToString();
            numTMP.fontSize = 14;
            numTMP.fontStyle = FontStyles.Bold;
            numTMP.color = Color.white;
            numTMP.alignment = TextAlignmentOptions.Center;
        }

        // Combo Streak Indicator on Top Bar
        GameObject comboObj = CreateUIElement("ComboStreakText", topBar.transform);
        RectTransform comboRT = comboObj.GetComponent<RectTransform>();
        comboRT.anchorMin = new Vector2(0.5f, 0.5f);
        comboRT.anchorMax = new Vector2(0.5f, 0.5f);
        comboRT.pivot = new Vector2(0f, 0.5f);
        comboRT.anchoredPosition = new Vector2(110, 0);
        comboRT.sizeDelta = new Vector2(260, 40);
        TextMeshProUGUI comboTMP = comboObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) comboTMP.font = fontAsset;
        comboTMP.text = "SYNC READY";
        comboTMP.fontSize = 17;
        comboTMP.fontStyle = FontStyles.Bold;
        comboTMP.color = new Color(0f, 0.88f, 1f, 1f);
        comboTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Top Right Controls (Mute Button & Edovu Logo)
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

        GameObject logoPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Edovu Logo .prefab");
        if (logoPrefab != null)
        {
            GameObject logoInst = (GameObject)PrefabUtility.InstantiatePrefab(logoPrefab, topBar.transform);
            RectTransform logoRT = logoInst.GetComponent<RectTransform>();
            logoRT.anchorMin = new Vector2(1, 0.5f);
            logoRT.anchorMax = new Vector2(1, 0.5f);
            logoRT.pivot = new Vector2(1, 0.5f);
            logoRT.anchoredPosition = new Vector2(-20, 0);
            logoRT.sizeDelta = new Vector2(90, 55);
        }

        // 5. AudioManager GameObject
        GameObject audioMgrObj = new GameObject("AudioManager");
        AudioManager audioMgr = audioMgrObj.AddComponent<AudioManager>();
        AudioSource musicSrc = audioMgrObj.AddComponent<AudioSource>();
        AudioSource sfxSrc = audioMgrObj.AddComponent<AudioSource>();

        SerializedObject audioSO = new SerializedObject(audioMgr);
        audioSO.FindProperty("musicSource").objectReferenceValue = musicSrc;
        audioSO.FindProperty("sfxSource").objectReferenceValue = sfxSrc;
        audioSO.FindProperty("bgmClip").objectReferenceValue = bgmClip;
        audioSO.FindProperty("correctClip").objectReferenceValue = correctClip;
        audioSO.FindProperty("wrongClip").objectReferenceValue = wrongClip;
        audioSO.FindProperty("clickClip").objectReferenceValue = null; // No dial audio!
        audioSO.FindProperty("muteButtonImage").objectReferenceValue = muteImg;
        audioSO.FindProperty("muteSprite").objectReferenceValue = muteSprite;
        audioSO.FindProperty("unmuteSprite").objectReferenceValue = unmuteSprite;
        audioSO.ApplyModifiedProperties();

        // 6. SCREEN 1: Instructions
        GameObject screenInstructions = CreateUIElement("Screen_Instructions", canvasObj.transform);
        StretchFull(screenInstructions.GetComponent<RectTransform>());

        ProceduralImage instrCardImg = CreateProceduralUI("InstructionCard", screenInstructions.transform, new Color(0.06f, 0.08f, 0.16f, 0.94f), 20f);
        GameObject instrCard = instrCardImg.gameObject;
        RectTransform instrCardRT = instrCard.GetComponent<RectTransform>();
        instrCardRT.anchorMin = new Vector2(0.5f, 0.5f);
        instrCardRT.anchorMax = new Vector2(0.5f, 0.5f);
        instrCardRT.pivot = new Vector2(0.5f, 0.5f);
        instrCardRT.sizeDelta = new Vector2(1150, 780);
        instrCardRT.anchoredPosition = new Vector2(0, -25);

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

        ProceduralImage storyBg = CreateProceduralUI("StoryBox", instrCard.transform, new Color(0.1f, 0.13f, 0.24f, 0.85f), 14f);
        GameObject storyBoxObj = storyBg.gameObject;
        RectTransform storyRT = storyBoxObj.GetComponent<RectTransform>();
        storyRT.anchorMin = new Vector2(0.5f, 1f);
        storyRT.anchorMax = new Vector2(0.5f, 1f);
        storyRT.pivot = new Vector2(0.5f, 1f);
        storyRT.anchoredPosition = new Vector2(0, -280);
        storyRT.sizeDelta = new Vector2(1020, 150);

        GameObject storyTextObj = CreateUIElement("StoryText", storyBoxObj.transform);
        StretchFull(storyTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI storyTMP = storyTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) storyTMP.font = fontAsset;
        storyTMP.text = "Welcome to the Glitch-Hop Audio Lab! You are the head sound engineer for the galaxy's biggest music festival, but a rogue cyber-virus has scrambled the synthesizer's audio tracks right before showtime!";
        storyTMP.fontSize = 21;
        storyTMP.color = new Color(0.92f, 0.95f, 1f, 1f);
        storyTMP.alignment = TextAlignmentOptions.Center;
        storyTMP.enableWordWrapping = true;

        ProceduralImage missionBg = CreateProceduralUI("MissionBox", instrCard.transform, new Color(0.08f, 0.18f, 0.32f, 0.85f), 14f);
        GameObject missionBoxObj = missionBg.gameObject;
        RectTransform missionRT = missionBoxObj.GetComponent<RectTransform>();
        missionRT.anchorMin = new Vector2(0.5f, 1f);
        missionRT.anchorMax = new Vector2(0.5f, 1f);
        missionRT.pivot = new Vector2(0.5f, 1f);
        missionRT.anchoredPosition = new Vector2(0, -445);
        missionRT.sizeDelta = new Vector2(1020, 180);

        GameObject missionTextObj = CreateUIElement("MissionText", missionBoxObj.transform);
        StretchFull(missionTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI missionTMP = missionTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) missionTMP.font = fontAsset;
        missionTMP.text = "<color=#00FFFF><b>YOUR MISSION:</b></color> Read the sonic clues to fix the broken sound waves. Use your knowledge of the <b>commutative</b>, <b>associative</b>, and <b>distributive laws</b>, along with the correct <b>order of operations</b>, to realign the music frequencies. Click the correct setting to patch the audio and get the crowd dancing!";
        missionTMP.fontSize = 20;
        missionTMP.color = new Color(0.88f, 0.96f, 1f, 1f);
        missionTMP.alignment = TextAlignmentOptions.Center;
        missionTMP.enableWordWrapping = true;

        ProceduralImage startBtnImg = CreateProceduralUI("StartSoundcheckButton", instrCard.transform, new Color(0f, 0.75f, 0.95f, 1f), 14f);
        GameObject startBtnObj = startBtnImg.gameObject;
        RectTransform startBtnRT = startBtnObj.GetComponent<RectTransform>();
        startBtnRT.anchorMin = new Vector2(0.5f, 0f);
        startBtnRT.anchorMax = new Vector2(0.5f, 0f);
        startBtnRT.pivot = new Vector2(0.5f, 0f);
        startBtnRT.anchoredPosition = new Vector2(0, 40);
        startBtnRT.sizeDelta = new Vector2(420, 75);
        Button startBtn = startBtnObj.AddComponent<Button>();
        startBtn.targetGraphic = startBtnImg;
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

        // 7. SCREEN 2: Activity Screen
        GameObject screenActivity = CreateUIElement("Screen_Activity", canvasObj.transform);
        StretchFull(screenActivity.GetComponent<RectTransform>());
        screenActivity.SetActive(false);

        ProceduralImage stageBannerImg = CreateProceduralUI("StageHeaderBanner", screenActivity.transform, new Color(0.08f, 0.12f, 0.22f, 0.85f), 12f);
        GameObject stageBannerObj = stageBannerImg.gameObject;
        RectTransform stageBannerRT = stageBannerObj.GetComponent<RectTransform>();
        stageBannerRT.anchorMin = new Vector2(0.5f, 1f);
        stageBannerRT.anchorMax = new Vector2(0.5f, 1f);
        stageBannerRT.pivot = new Vector2(0.5f, 1f);
        stageBannerRT.anchoredPosition = new Vector2(0, -90);
        stageBannerRT.sizeDelta = new Vector2(1820, 46);

        GameObject stageTextObj = CreateUIElement("StageIndicatorText", stageBannerObj.transform);
        StretchFull(stageTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI stageIndicatorTMP = stageTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) stageIndicatorTMP.font = fontAsset;
        stageIndicatorTMP.text = "TRACK 1 / 5 • COMMUTATIVE LAW";
        stageIndicatorTMP.fontSize = 20;
        stageIndicatorTMP.fontStyle = FontStyles.Bold;
        stageIndicatorTMP.color = new Color(0f, 0.95f, 0.95f, 1f);
        stageIndicatorTMP.alignment = TextAlignmentOptions.Center;

        // Content Area Container (Split into Left Visual & Right Console)
        GameObject contentArea = CreateUIElement("ContentArea", screenActivity.transform);
        RectTransform contentAreaRT = contentArea.GetComponent<RectTransform>();
        contentAreaRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentAreaRT.pivot = new Vector2(0.5f, 0.5f);
        contentAreaRT.anchoredPosition = new Vector2(0, -45);
        contentAreaRT.sizeDelta = new Vector2(1820, 840);

        // LEFT: Visual Equipment Rig (Width 47%, ~855px)
        ProceduralImage leftRigBg = CreateProceduralUI("LeftVisualRig", contentArea.transform, new Color(0.06f, 0.08f, 0.15f, 0.9f), 18f);
        GameObject leftVisualRig = leftRigBg.gameObject;
        RectTransform leftRigRT = leftVisualRig.GetComponent<RectTransform>();
        leftRigRT.anchorMin = new Vector2(0f, 0f);
        leftRigRT.anchorMax = new Vector2(0.47f, 1f);
        leftRigRT.anchoredPosition = Vector2.zero;
        leftRigRT.sizeDelta = Vector2.zero;

        // Rig Status Pill on top of hardware
        ProceduralImage statusPillBg = CreateProceduralUI("RigStatusPill", leftVisualRig.transform, new Color(0.08f, 0.12f, 0.2f, 0.9f), 12f);
        GameObject rigStatusPill = statusPillBg.gameObject;
        RectTransform statusPillRT = rigStatusPill.GetComponent<RectTransform>();
        statusPillRT.anchorMin = new Vector2(0.03f, 1f);
        statusPillRT.anchorMax = new Vector2(0.97f, 1f);
        statusPillRT.pivot = new Vector2(0.5f, 1f);
        statusPillRT.anchoredPosition = new Vector2(0, -12);
        statusPillRT.sizeDelta = new Vector2(0, 42);

        ProceduralImage statusIconImg = CreateProceduralUI("StatusIcon", rigStatusPill.transform, new Color(1f, 0.65f, 0.15f, 1f), 8f);
        GameObject statusIconObj = statusIconImg.gameObject;
        RectTransform statusIconRT = statusIconObj.GetComponent<RectTransform>();
        statusIconRT.anchorMin = new Vector2(0f, 0.5f);
        statusIconRT.anchorMax = new Vector2(0f, 0.5f);
        statusIconRT.pivot = new Vector2(0f, 0.5f);
        statusIconRT.anchoredPosition = new Vector2(15, 0);
        statusIconRT.sizeDelta = new Vector2(16, 16);

        GameObject statusTextObj = CreateUIElement("StatusText", rigStatusPill.transform);
        RectTransform statusTextRT = statusTextObj.GetComponent<RectTransform>();
        statusTextRT.anchorMin = new Vector2(0f, 0f);
        statusTextRT.anchorMax = new Vector2(1f, 1f);
        statusTextRT.anchoredPosition = Vector2.zero;
        statusTextRT.sizeDelta = new Vector2(-80, 0);
        statusTextRT.pivot = new Vector2(0.5f, 0.5f);
        TextMeshProUGUI rigStatusTMP = statusTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) rigStatusTMP.font = fontAsset;
        rigStatusTMP.text = "DIAGNOSTIC STATUS: STEREO STAGE CHANNELS [GLITCH DETECTED]";
        rigStatusTMP.fontSize = 16;
        rigStatusTMP.fontStyle = FontStyles.Bold;
        rigStatusTMP.color = new Color(1f, 0.65f, 0.15f, 1f);
        rigStatusTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Visual Display Frame
        GameObject itemImgObj = CreateUIElement("ItemVisualImage", leftVisualRig.transform);
        RectTransform itemImgRT = itemImgObj.GetComponent<RectTransform>();
        itemImgRT.anchorMin = new Vector2(0.03f, 0.20f);
        itemImgRT.anchorMax = new Vector2(0.97f, 0.91f);
        itemImgRT.anchoredPosition = Vector2.zero;
        itemImgRT.sizeDelta = Vector2.zero;
        Image itemVisualImg = itemImgObj.AddComponent<Image>();
        itemVisualImg.sprite = item1Sprite;
        itemVisualImg.preserveAspect = true;

        // Animated Equalizer Spectrum Container at bottom of Left Rig
        ProceduralImage eqSpectrumBg = CreateProceduralUI("EqualizerSpectrum", leftVisualRig.transform, new Color(0.03f, 0.05f, 0.1f, 0.85f), 12f);
        GameObject eqSpectrumObj = eqSpectrumBg.gameObject;
        RectTransform eqSpectrumRT = eqSpectrumObj.GetComponent<RectTransform>();
        eqSpectrumRT.anchorMin = new Vector2(0.03f, 0.025f);
        eqSpectrumRT.anchorMax = new Vector2(0.97f, 0.18f);
        eqSpectrumRT.anchoredPosition = Vector2.zero;
        eqSpectrumRT.sizeDelta = Vector2.zero;

        HorizontalLayoutGroup eqLayout = eqSpectrumObj.AddComponent<HorizontalLayoutGroup>();
        eqLayout.spacing = 5;
        eqLayout.childAlignment = TextAnchor.LowerCenter;
        eqLayout.childForceExpandWidth = true;
        eqLayout.childForceExpandHeight = false;

        RectTransform[] eqBars = new RectTransform[20];
        for (int i = 0; i < 20; i++)
        {
            ProceduralImage barImg = CreateProceduralUI($"EqBar_{i}", eqSpectrumObj.transform, new Color(1f, 0.6f, 0.2f, 0.9f), 4f);
            RectTransform barRT = barImg.rectTransform;
            barRT.sizeDelta = new Vector2(18, 35);
            eqBars[i] = barRT;
        }

        // RIGHT: Sound Terminal & Controls (Width 51%, ~930px)
        ProceduralImage rightTermBg = CreateProceduralUI("RightTerminal", contentArea.transform, new Color(0.07f, 0.1f, 0.18f, 0.92f), 18f);
        GameObject rightTerminal = rightTermBg.gameObject;
        RectTransform rightTermRT = rightTerminal.GetComponent<RectTransform>();
        rightTermRT.anchorMin = new Vector2(0.49f, 0f);
        rightTermRT.anchorMax = new Vector2(1f, 1f);
        rightTermRT.anchoredPosition = Vector2.zero;
        rightTermRT.sizeDelta = Vector2.zero;

        // Clue Display Box (Top: 0.71 to 0.98)
        ProceduralImage clueBoxBg = CreateProceduralUI("ClueBox", rightTerminal.transform, new Color(0.11f, 0.16f, 0.28f, 0.9f), 14f);
        GameObject clueBoxObj = clueBoxBg.gameObject;
        RectTransform clueBoxRT = clueBoxObj.GetComponent<RectTransform>();
        clueBoxRT.anchorMin = new Vector2(0.03f, 0.71f);
        clueBoxRT.anchorMax = new Vector2(0.97f, 0.98f);
        clueBoxRT.anchoredPosition = Vector2.zero;
        clueBoxRT.sizeDelta = Vector2.zero;

        GameObject clueTextObj = CreateUIElement("ClueTMP", clueBoxObj.transform);
        RectTransform clueTextRT = clueTextObj.GetComponent<RectTransform>();
        clueTextRT.anchorMin = Vector2.zero;
        clueTextRT.anchorMax = Vector2.one;
        clueTextRT.sizeDelta = new Vector2(-40, -20);
        clueTextRT.anchoredPosition = Vector2.zero;
        TextMeshProUGUI clueTMP = clueTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) clueTMP.font = fontAsset;
        clueTMP.text = "Clue text goes here...";
        clueTMP.fontSize = 21;
        clueTMP.color = new Color(0.9f, 0.95f, 1f, 1f);
        clueTMP.alignment = TextAlignmentOptions.TopLeft;
        clueTMP.enableWordWrapping = true;

        // Equation Display Hologram Box (Middle: 0.53 to 0.69)
        ProceduralImage eqBoxBg = CreateProceduralUI("EquationHologramBox", rightTerminal.transform, new Color(0.04f, 0.22f, 0.32f, 0.95f), 14f);
        GameObject eqBoxObj = eqBoxBg.gameObject;
        RectTransform eqBoxRT = eqBoxObj.GetComponent<RectTransform>();
        eqBoxRT.anchorMin = new Vector2(0.03f, 0.53f);
        eqBoxRT.anchorMax = new Vector2(0.97f, 0.69f);
        eqBoxRT.anchoredPosition = Vector2.zero;
        eqBoxRT.sizeDelta = Vector2.zero;

        GameObject eqTextObj = CreateUIElement("EquationTMP", eqBoxObj.transform);
        StretchFull(eqTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI equationTMP = eqTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) equationTMP.font = fontAsset;
        equationTMP.text = "Left: 15 × 4 = 60   ->   Right: 4 × [ ? ] = 60";
        equationTMP.fontSize = 32;
        equationTMP.fontStyle = FontStyles.Bold;
        equationTMP.color = new Color(0.2f, 1f, 0.6f, 1f);
        equationTMP.alignment = TextAlignmentOptions.Center;

        // Options Grid (0.23 to 0.51, height ~240px!)
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
        TextMeshProUGUI[] optionSubtitles = new TextMeshProUGUI[3];
        TextMeshProUGUI[] optionBadges = new TextMeshProUGUI[3];

        string[] defaultBadges = { "PATCH A", "PATCH B", "PATCH C" };

        for (int i = 0; i < 3; i++)
        {
            ProceduralImage btnImg = CreateProceduralUI($"OptionBtn_{i}", optionsContainer.transform, new Color(0.11f, 0.16f, 0.28f, 1f), 16f);
            GameObject btnObj = btnImg.gameObject;
            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btnObj.AddComponent<UIHoverClickEffect>();
            optionButtons[i] = btn;

            // Top Badge Pill
            ProceduralImage bImg = CreateProceduralUI("Badge", btnObj.transform, new Color(0f, 0.5f, 0.7f, 0.35f), 10f);
            GameObject badgeObj = bImg.gameObject;
            RectTransform bRT = badgeObj.GetComponent<RectTransform>();
            bRT.anchorMin = new Vector2(0.5f, 1f);
            bRT.anchorMax = new Vector2(0.5f, 1f);
            bRT.pivot = new Vector2(0.5f, 1f);
            bRT.anchoredPosition = new Vector2(0, -8);
            bRT.sizeDelta = new Vector2(170, 28);

            GameObject bTextObj = CreateUIElement("Text", badgeObj.transform);
            StretchFull(bTextObj.GetComponent<RectTransform>());
            TextMeshProUGUI bTMP = bTextObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) bTMP.font = fontAsset;
            bTMP.text = defaultBadges[i];
            bTMP.fontSize = 12;
            bTMP.fontStyle = FontStyles.Bold;
            bTMP.color = new Color(0f, 0.95f, 1f, 1f);
            bTMP.alignment = TextAlignmentOptions.Center;
            optionBadges[i] = bTMP;

            // Primary Text (Center)
            GameObject textObj = CreateUIElement("Text", btnObj.transform);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.04f, 0.38f);
            textRT.anchorMax = new Vector2(0.96f, 0.84f);
            textRT.anchoredPosition = Vector2.zero;
            textRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI optTMP = textObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) optTMP.font = fontAsset;
            optTMP.text = $"Option {i + 1}";
            optTMP.fontSize = 32;
            optTMP.fontStyle = FontStyles.Bold;
            optTMP.color = Color.white;
            optTMP.alignment = TextAlignmentOptions.Center;
            optTMP.enableWordWrapping = true;
            optionTexts[i] = optTMP;

            // Subtitle Description Text (Bottom)
            GameObject subObj = CreateUIElement("Subtitle", btnObj.transform);
            RectTransform subTextRT = subObj.GetComponent<RectTransform>();
            subTextRT.anchorMin = new Vector2(0.04f, 0.05f);
            subTextRT.anchorMax = new Vector2(0.96f, 0.36f);
            subTextRT.anchoredPosition = Vector2.zero;
            subTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI subOptTMP = subObj.AddComponent<TextMeshProUGUI>();
            if (fontAsset != null) subOptTMP.font = fontAsset;
            subOptTMP.text = "[ ACTION EXPLANATION ]";
            subOptTMP.fontSize = 12;
            subOptTMP.fontStyle = FontStyles.Normal;
            subOptTMP.color = new Color(0f, 0.85f, 1f, 0.9f);
            subOptTMP.alignment = TextAlignmentOptions.Center;
            subOptTMP.enableWordWrapping = true;
            optionSubtitles[i] = subOptTMP;
        }

        // Voiceover / Sound Engineer Communicator Panel (0.025 to 0.21, height ~160px)
        ProceduralImage fbBg = CreateProceduralUI("FeedbackPanel", rightTerminal.transform, new Color(0.07f, 0.11f, 0.2f, 0.95f), 14f);
        GameObject feedbackPanelObj = fbBg.gameObject;
        RectTransform fbRT = feedbackPanelObj.GetComponent<RectTransform>();
        fbRT.anchorMin = new Vector2(0.03f, 0.025f);
        fbRT.anchorMax = new Vector2(0.97f, 0.21f);
        fbRT.anchoredPosition = Vector2.zero;
        fbRT.sizeDelta = Vector2.zero;

        GameObject fbStatusObj = CreateUIElement("FeedbackStatusTMP", feedbackPanelObj.transform);
        RectTransform fbStatusRT = fbStatusObj.GetComponent<RectTransform>();
        fbStatusRT.anchorMin = new Vector2(0f, 1f);
        fbStatusRT.anchorMax = new Vector2(1f, 1f);
        fbStatusRT.pivot = new Vector2(0.5f, 1f);
        fbStatusRT.anchoredPosition = new Vector2(0, -10);
        fbStatusRT.sizeDelta = new Vector2(-35, 32);
        TextMeshProUGUI fbStatusTMP = fbStatusObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) fbStatusTMP.font = fontAsset;
        fbStatusTMP.text = "[ COMMS: LISTENING TO FREQUENCY BUS... ]";
        fbStatusTMP.fontSize = 17;
        fbStatusTMP.fontStyle = FontStyles.Bold;
        fbStatusTMP.color = new Color(0f, 0.88f, 1f, 1f);
        fbStatusTMP.alignment = TextAlignmentOptions.MidlineLeft;

        GameObject fbVoObj = CreateUIElement("FeedbackVoiceoverTMP", feedbackPanelObj.transform);
        RectTransform fbVoRT = fbVoObj.GetComponent<RectTransform>();
        fbVoRT.anchorMin = new Vector2(0f, 0.32f);
        fbVoRT.anchorMax = new Vector2(1f, 0.82f);
        fbVoRT.anchoredPosition = Vector2.zero;
        fbVoRT.sizeDelta = new Vector2(-35, 0);
        TextMeshProUGUI fbVoTMP = fbVoObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) fbVoTMP.font = fontAsset;
        fbVoTMP.text = "\"Awaiting patch command. Select the matching frequency setting above to realign the audio track.\"";
        fbVoTMP.fontSize = 18;
        fbVoTMP.color = new Color(0.92f, 0.95f, 1f, 1f);
        fbVoTMP.alignment = TextAlignmentOptions.TopLeft;
        fbVoTMP.enableWordWrapping = true;

        ProceduralImage nextBtnImg = CreateProceduralUI("NextStageButton", feedbackPanelObj.transform, new Color(0f, 0.85f, 0.5f, 1f), 12f);
        GameObject nextBtnObj = nextBtnImg.gameObject;
        RectTransform nextBtnRT = nextBtnObj.GetComponent<RectTransform>();
        nextBtnRT.anchorMin = new Vector2(1f, 0f);
        nextBtnRT.anchorMax = new Vector2(1f, 0f);
        nextBtnRT.pivot = new Vector2(1f, 0f);
        nextBtnRT.anchoredPosition = new Vector2(-15, 12);
        nextBtnRT.sizeDelta = new Vector2(240, 50);
        Button nextBtn = nextBtnObj.AddComponent<Button>();
        nextBtn.targetGraphic = nextBtnImg;
        nextBtnObj.AddComponent<UIHoverClickEffect>();
        nextBtnObj.SetActive(false);

        GameObject nextBtnTextObj = CreateUIElement("Text", nextBtnObj.transform);
        StretchFull(nextBtnTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI nextBtnTMP = nextBtnTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) nextBtnTMP.font = fontAsset;
        nextBtnTMP.text = "NEXT TRACK >";
        nextBtnTMP.fontSize = 20;
        nextBtnTMP.fontStyle = FontStyles.Bold;
        nextBtnTMP.color = new Color(0.04f, 0.1f, 0.14f, 1f);
        nextBtnTMP.alignment = TextAlignmentOptions.Center;

        // Screen Flash Overlay (Juicy Screen FX)
        GameObject flashOverlayObj = CreateUIElement("ScreenFlashOverlay", screenActivity.transform);
        StretchFull(flashOverlayObj.GetComponent<RectTransform>());
        Image flashImg = flashOverlayObj.AddComponent<Image>();
        flashImg.color = Color.clear;
        flashImg.raycastTarget = false;
        flashOverlayObj.SetActive(false);

        // 8. SCREEN 3: Final Victory Screen
        GameObject screenVictory = CreateUIElement("Screen_Victory", canvasObj.transform);
        StretchFull(screenVictory.GetComponent<RectTransform>());
        screenVictory.SetActive(false);

        GameObject vicBgObj = CreateUIElement("VictoryBg", screenVictory.transform);
        StretchFull(vicBgObj.GetComponent<RectTransform>());
        Image vicBgImg = vicBgObj.AddComponent<Image>();
        vicBgImg.sprite = victorySprite;
        vicBgImg.color = new Color(0.95f, 0.95f, 1f, 1f);

        ProceduralImage vicCardImg = CreateProceduralUI("VictoryCard", screenVictory.transform, new Color(0.04f, 0.06f, 0.15f, 0.92f), 20f);
        GameObject vicCard = vicCardImg.gameObject;
        RectTransform vicCardRT = vicCard.GetComponent<RectTransform>();
        vicCardRT.anchorMin = new Vector2(0.5f, 0.5f);
        vicCardRT.anchorMax = new Vector2(0.5f, 0.5f);
        vicCardRT.pivot = new Vector2(0.5f, 0.5f);
        vicCardRT.sizeDelta = new Vector2(1100, 700);
        vicCardRT.anchoredPosition = Vector2.zero;

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

        ProceduralImage badgeBg = CreateProceduralUI("MasterBadge", vicCard.transform, new Color(0.15f, 0.12f, 0.35f, 0.95f), 14f);
        GameObject masterBadgeObj = badgeBg.gameObject;
        RectTransform masterBadgeRT = masterBadgeObj.GetComponent<RectTransform>();
        masterBadgeRT.anchorMin = new Vector2(0.5f, 0.5f);
        masterBadgeRT.anchorMax = new Vector2(0.5f, 0.5f);
        masterBadgeRT.pivot = new Vector2(0.5f, 0.5f);
        masterBadgeRT.anchoredPosition = new Vector2(0, -70);
        masterBadgeRT.sizeDelta = new Vector2(850, 90);

        GameObject badgeTextObj = CreateUIElement("Text", masterBadgeObj.transform);
        StretchFull(badgeTextObj.GetComponent<RectTransform>());
        TextMeshProUGUI badgeTMP = badgeTextObj.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null) badgeTMP.font = fontAsset;
        badgeTMP.text = "* YOU ARE A MASTER AUDIO ENGINEER! *";
        badgeTMP.fontSize = 32;
        badgeTMP.fontStyle = FontStyles.Bold;
        badgeTMP.color = new Color(0.3f, 1f, 0.65f, 1f);
        badgeTMP.alignment = TextAlignmentOptions.Center;

        ProceduralImage playAgainImg = CreateProceduralUI("PlayAgainButton", vicCard.transform, new Color(0f, 0.75f, 0.95f, 1f), 14f);
        GameObject playAgainBtnObj = playAgainImg.gameObject;
        RectTransform playAgainRT = playAgainBtnObj.GetComponent<RectTransform>();
        playAgainRT.anchorMin = new Vector2(0.5f, 0f);
        playAgainRT.anchorMax = new Vector2(0.5f, 0f);
        playAgainRT.pivot = new Vector2(0.5f, 0f);
        playAgainRT.anchoredPosition = new Vector2(0, 50);
        playAgainRT.sizeDelta = new Vector2(380, 75);
        Button playAgainBtn = playAgainBtnObj.AddComponent<Button>();
        playAgainBtn.targetGraphic = playAgainImg;
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

        // 9. GlitchHopGameManager Setup & Linking
        GameObject gmObj = new GameObject("GlitchHopGameManager");
        GlitchHopGameManager gm = gmObj.AddComponent<GlitchHopGameManager>();

        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("randomizeQuestions").boolValue = false;
        gmSO.FindProperty("randomizeOptions").boolValue = true;

        gmSO.FindProperty("screenInstructions").objectReferenceValue = screenInstructions;
        gmSO.FindProperty("screenActivity").objectReferenceValue = screenActivity;
        gmSO.FindProperty("screenVictory").objectReferenceValue = screenVictory;

        gmSO.FindProperty("startSoundcheckButton").objectReferenceValue = startBtn;
        gmSO.FindProperty("instructionCard").objectReferenceValue = instrCardRT;

        gmSO.FindProperty("stageIndicatorText").objectReferenceValue = stageIndicatorTMP;
        gmSO.FindProperty("comboStreakText").objectReferenceValue = comboTMP;
        gmSO.FindProperty("clueTMP").objectReferenceValue = clueTMP;
        gmSO.FindProperty("equationTMP").objectReferenceValue = equationTMP;
        gmSO.FindProperty("itemVisualImage").objectReferenceValue = itemVisualImg;

        SerializedProperty optBtnsProp = gmSO.FindProperty("optionButtons");
        optBtnsProp.arraySize = optionButtons.Length;
        for (int i = 0; i < optionButtons.Length; i++)
            optBtnsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionButtons[i];

        SerializedProperty optTextsProp = gmSO.FindProperty("optionTexts");
        optTextsProp.arraySize = optionTexts.Length;
        for (int i = 0; i < optionTexts.Length; i++)
            optTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionTexts[i];

        SerializedProperty optSubsProp = gmSO.FindProperty("optionSubtitles");
        optSubsProp.arraySize = optionSubtitles.Length;
        for (int i = 0; i < optionSubtitles.Length; i++)
            optSubsProp.GetArrayElementAtIndex(i).objectReferenceValue = optionSubtitles[i];

        SerializedProperty optBadgesProp = gmSO.FindProperty("optionBadgeTexts");
        optBadgesProp.arraySize = optionBadges.Length;
        for (int i = 0; i < optionBadges.Length; i++)
            optBadgesProp.GetArrayElementAtIndex(i).objectReferenceValue = optionBadges[i];

        SerializedProperty stageDotsProp = gmSO.FindProperty("stageDots");
        stageDotsProp.arraySize = stageDots.Length;
        for (int i = 0; i < stageDots.Length; i++)
            stageDotsProp.GetArrayElementAtIndex(i).objectReferenceValue = stageDots[i];

        SerializedProperty eqBarsProp = gmSO.FindProperty("equalizerBars");
        eqBarsProp.arraySize = eqBars.Length;
        for (int i = 0; i < eqBars.Length; i++)
            eqBarsProp.GetArrayElementAtIndex(i).objectReferenceValue = eqBars[i];

        gmSO.FindProperty("rigStatusText").objectReferenceValue = rigStatusTMP;
        gmSO.FindProperty("rigStatusIcon").objectReferenceValue = statusIconImg;

        gmSO.FindProperty("feedbackPanel").objectReferenceValue = feedbackPanelObj;
        gmSO.FindProperty("feedbackStatusTMP").objectReferenceValue = fbStatusTMP;
        gmSO.FindProperty("feedbackVoiceoverTMP").objectReferenceValue = fbVoTMP;
        gmSO.FindProperty("feedbackStatusBg").objectReferenceValue = fbBg;
        gmSO.FindProperty("nextStageButton").objectReferenceValue = nextBtn;

        gmSO.FindProperty("victoryCard").objectReferenceValue = vicCardRT;
        gmSO.FindProperty("playAgainButton").objectReferenceValue = playAgainBtn;
        gmSO.FindProperty("muteButton").objectReferenceValue = muteBtn;

        gmSO.FindProperty("screenFlashOverlay").objectReferenceValue = flashImg;
        gmSO.FindProperty("floatingJuiceSpawnParent").objectReferenceValue = screenActivity.transform;
        gmSO.FindProperty("mainFontAsset").objectReferenceValue = fontAsset;

        // 10. Populate Activity Items
        SerializedProperty itemsProp = gmSO.FindProperty("activityItems");
        itemsProp.arraySize = 5;

        // Item 1
        SerializedProperty it1 = itemsProp.GetArrayElementAtIndex(0);
        it1.FindPropertyRelative("stageTitle").stringValue = "Commutative Law • Stereo Balance";
        it1.FindPropertyRelative("equipmentTag").stringValue = "STEREO STAGE CHANNELS";
        it1.FindPropertyRelative("clueText").stringValue = "* To balance the left and right stage speakers, the audio volume must match perfectly. Reversing the channels using the commutative law will fix the sound. If the left channel is programmed as 15 × 4 = 60, what missing volume level will balance the right channel: 4 × __ = 60?";
        it1.FindPropertyRelative("equationDisplay").stringValue = "Left: 15 × 4 = 60   ->   Right: 4 × [ ? ] = 60";
        SerializedProperty it1Opts = it1.FindPropertyRelative("options");
        it1Opts.arraySize = 3;
        it1Opts.GetArrayElementAtIndex(0).stringValue = "15";
        it1Opts.GetArrayElementAtIndex(1).stringValue = "4";
        it1Opts.GetArrayElementAtIndex(2).stringValue = "64";
        SerializedProperty it1Subs = it1.FindPropertyRelative("optionSubtitles");
        it1Subs.arraySize = 3;
        it1Subs.GetArrayElementAtIndex(0).stringValue = "[REVERSE FACTOR • BALANCES 4 × 15 = 60]";
        it1Subs.GetArrayElementAtIndex(1).stringValue = "[UNBALANCED • DUPLICATE FACTOR]";
        it1Subs.GetArrayElementAtIndex(2).stringValue = "[OVERLOAD • 4 × 64 ≠ 60]";
        it1.FindPropertyRelative("correctOptionIndex").intValue = 0;
        it1.FindPropertyRelative("voiceoverCorrect").stringValue = "Channels synced! The commutative law proves that swapping the order of our factors leaves the total track volume exactly the same.";
        it1.FindPropertyRelative("voiceoverWrong").stringValue = "The sound is totally lopsided! Look at the left channel's numbers. We just need to flip their order to keep the output identical.";
        it1.FindPropertyRelative("itemVisualSprite").objectReferenceValue = item1Sprite;

        // Item 2
        SerializedProperty it2 = itemsProp.GetArrayElementAtIndex(1);
        it2.FindPropertyRelative("stageTitle").stringValue = "Associative Law • Bass Sequencer";
        it2.FindPropertyRelative("equipmentTag").stringValue = "16-STEP BASS SEQUENCER";
        it2.FindPropertyRelative("clueText").stringValue = "* The bass sequencer is lagging because the code is too heavy. Use the associative law to regroup these sound waves into a quick, friendly power of 10 first: (18 × 5) × 2 = 18 × (5 × 2). What simplified value goes into the master loop? 18 × __";
        it2.FindPropertyRelative("equationDisplay").stringValue = "(18 × 5) × 2 = 18 × (5 × 2) = 18 × [ ? ]";
        SerializedProperty it2Opts = it2.FindPropertyRelative("options");
        it2Opts.arraySize = 3;
        it2Opts.GetArrayElementAtIndex(0).stringValue = "10";
        it2Opts.GetArrayElementAtIndex(1).stringValue = "90";
        it2Opts.GetArrayElementAtIndex(2).stringValue = "25";
        SerializedProperty it2Subs = it2.FindPropertyRelative("optionSubtitles");
        it2Subs.arraySize = 3;
        it2Subs.GetArrayElementAtIndex(0).stringValue = "[POWER OF 10 • 5 × 2 = 10 (FAST CALC)]";
        it2Subs.GetArrayElementAtIndex(1).stringValue = "[UNGROUPED • 18 × 5 WITHOUT REGROUP]";
        it2Subs.GetArrayElementAtIndex(2).stringValue = "[INVALID FACTOR]";
        it2.FindPropertyRelative("correctOptionIndex").intValue = 0;
        it2.FindPropertyRelative("voiceoverCorrect").stringValue = "Smooth bassline! Grouping five and two creates an instant ten, turning a complicated sound mix into a super easy mental calculation.";
        it2.FindPropertyRelative("voiceoverWrong").stringValue = "Too much audio static! Look inside the second pair of parentheses. Multiply those two isolated numbers together first to clear the lag.";
        it2.FindPropertyRelative("itemVisualSprite").objectReferenceValue = item2Sprite;

        // Item 3
        SerializedProperty it3 = itemsProp.GetArrayElementAtIndex(2);
        it3.FindPropertyRelative("stageTitle").stringValue = "Distributive Law • Beat Slicer";
        it3.FindPropertyRelative("equipmentTag").stringValue = "14-BEAT DIGITAL SAMPLER";
        it3.FindPropertyRelative("clueText").stringValue = "* A massive 14-beat sound sample is completely overloading the digital track. Chop it down using the distributive law! Let's break 7 × 14 into two easier sub-loops: (7 × 10) + (7 × __). What is the missing slice?";
        it3.FindPropertyRelative("equationDisplay").stringValue = "7 × 14 = (7 × 10) + (7 × [ ? ])";
        SerializedProperty it3Opts = it3.FindPropertyRelative("options");
        it3Opts.arraySize = 3;
        it3Opts.GetArrayElementAtIndex(0).stringValue = "4";
        it3Opts.GetArrayElementAtIndex(1).stringValue = "14";
        it3Opts.GetArrayElementAtIndex(2).stringValue = "28";
        SerializedProperty it3Subs = it3.FindPropertyRelative("optionSubtitles");
        it3Subs.arraySize = 3;
        it3Subs.GetArrayElementAtIndex(0).stringValue = "[REMAINING SLICE • 14 - 10 = 4 BEATS]";
        it3Subs.GetArrayElementAtIndex(1).stringValue = "[UNDECOMPOSED • FULL 14 BEATS]";
        it3Subs.GetArrayElementAtIndex(2).stringValue = "[OVERLOADED DOUBLE SLICE]";
        it3.FindPropertyRelative("correctOptionIndex").intValue = 0;
        it3.FindPropertyRelative("voiceoverCorrect").stringValue = "Remix achieved! Decomposing fourteen into ten and four allows you to distribute the multiplication into two effortless steps.";
        it3.FindPropertyRelative("voiceoverWrong").stringValue = "The sampler crashed! You already split a block of ten away from the original fourteen-beat clip. How many beats are left over to distribute?";
        it3.FindPropertyRelative("itemVisualSprite").objectReferenceValue = item3Sprite;

        // Item 4
        SerializedProperty it4 = itemsProp.GetArrayElementAtIndex(3);
        it4.FindPropertyRelative("stageTitle").stringValue = "Order of Operations • FX Pedal Chain";
        it4.FindPropertyRelative("equipmentTag").stringValue = "EFFECTS PEDALBOARD CHAIN";
        it4.FindPropertyRelative("clueText").stringValue = "* The synthesizer’s special effects pedal chain is glitching out. To prevent a massive sound pop that could blow the speakers, which operation must you process first in this command string? 45 + 12 ÷ 3 - 6";
        it4.FindPropertyRelative("equationDisplay").stringValue = "45 + 12 ÷ 3 - 6   ->   Which operation FIRST?";
        SerializedProperty it4Opts = it4.FindPropertyRelative("options");
        it4Opts.arraySize = 3;
        it4Opts.GetArrayElementAtIndex(0).stringValue = "Division (12 ÷ 3)";
        it4Opts.GetArrayElementAtIndex(1).stringValue = "Addition (45 + 12)";
        it4Opts.GetArrayElementAtIndex(2).stringValue = "Subtraction (3 - 6)";
        SerializedProperty it4Subs = it4.FindPropertyRelative("optionSubtitles");
        it4Subs.arraySize = 3;
        it4Subs.GetArrayElementAtIndex(0).stringValue = "[HIGHEST PRIORITY • ÷ COMES BEFORE + / -]";
        it4Subs.GetArrayElementAtIndex(1).stringValue = "[LOWER PRIORITY • CANNOT ADD FIRST]";
        it4Subs.GetArrayElementAtIndex(2).stringValue = "[LOWER PRIORITY • CANNOT SUBTRACT FIRST]";
        it4.FindPropertyRelative("correctOptionIndex").intValue = 0;
        it4.FindPropertyRelative("voiceoverCorrect").stringValue = "Perfect frequency! According to the strict rules of operation order, division always takes absolute priority over addition and subtraction.";
        it4.FindPropertyRelative("voiceoverWrong").stringValue = "Ouch, that scratched the record! Don't just read the audio line from left to right. You must locate the highest-priority math operation first.";
        it4.FindPropertyRelative("itemVisualSprite").objectReferenceValue = item4Sprite;

        // Item 5
        SerializedProperty it5 = itemsProp.GetArrayElementAtIndex(4);
        it5.FindPropertyRelative("stageTitle").stringValue = "Master Calculation • Volume Slider";
        it5.FindPropertyRelative("equipmentTag").stringValue = "MASTER VOLUME DJ CONSOLE";
        it5.FindPropertyRelative("clueText").stringValue = "* The master volume slider requires the absolute correct final value of this multi-operation audio sequence. Calculate carefully to drop the beat: 20 - 3 × 4";
        it5.FindPropertyRelative("equationDisplay").stringValue = "20 - 3 × 4 = [ ? ]";
        SerializedProperty it5Opts = it5.FindPropertyRelative("options");
        it5Opts.arraySize = 3;
        it5Opts.GetArrayElementAtIndex(0).stringValue = "8";
        it5Opts.GetArrayElementAtIndex(1).stringValue = "68";
        it5Opts.GetArrayElementAtIndex(2).stringValue = "12";
        SerializedProperty it5Subs = it5.FindPropertyRelative("optionSubtitles");
        it5Subs.arraySize = 3;
        it5Subs.GetArrayElementAtIndex(0).stringValue = "[DROP BEAT: 3 × 4 = 12, THEN 20 - 12 = 8 dB]";
        it5Subs.GetArrayElementAtIndex(1).stringValue = "[ORDER ERROR: (20 - 3) × 4]";
        it5Subs.GetArrayElementAtIndex(2).stringValue = "[INCOMPLETE: 3 × 4 ONLY]";
        it5.FindPropertyRelative("correctOptionIndex").intValue = 0;
        it5.FindPropertyRelative("voiceoverCorrect").stringValue = "The beat just dropped perfectly! Multiplying three by four gives twelve, and subtracting twelve from twenty leaves a flawless volume level of eight.";
        it5.FindPropertyRelative("voiceoverWrong").stringValue = "Total sonic distortion! You subtracted three from twenty first. Remember, the multiplication effect must always fire before the subtraction step.";
        it5.FindPropertyRelative("itemVisualSprite").objectReferenceValue = item5Sprite;

        gmSO.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[GlitchHop] Scene v4 rebuilt with spacious option cards, communicator integration, and randomization booleans!");
    }

    private static void ConfigureAllSprites()
    {
        string[] spritePaths = {
            "Assets/Sprites/glitch_hop_lab_bg.jpg",
            "Assets/Sprites/item1_speakers.jpg",
            "Assets/Sprites/item2_sequencer.jpg",
            "Assets/Sprites/item3_sampler.jpg",
            "Assets/Sprites/item4_pedalboard.jpg",
            "Assets/Sprites/item5_volume_slider.jpg",
            "Assets/Sprites/final_victory_screen.jpg",
            "Assets/Sprites/title_badge_lab.jpg",
            "Assets/Sprites/Mute.png",
            "Assets/Sprites/UnMute.png",
            "Assets/Sprites/play again.png"
        };

        foreach (string path in spritePaths)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool modified = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    modified = true;
                }
                if (importer.spriteImportMode != SpriteImportMode.Single)
                {
                    importer.spriteImportMode = SpriteImportMode.Single;
                    modified = true;
                }
                if (importer.maxTextureSize < 2048)
                {
                    importer.maxTextureSize = 2048;
                    modified = true;
                }
                if (modified)
                {
                    importer.SaveAndReimport();
                }
            }
        }
        AssetDatabase.Refresh();
    }

    private static GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static ProceduralImage CreateProceduralUI(string name, Transform parent, Color color, float cornerRadius, float borderWidth = 0f)
    {
        GameObject go = CreateUIElement(name, parent);
        ProceduralImage pImg = go.AddComponent<ProceduralImage>();
        pImg.color = color;
        pImg.BorderWidth = borderWidth;
        pImg.FalloffDistance = 1f;
        UniformModifier mod = go.AddComponent<UniformModifier>();
        mod.Radius = cornerRadius;
        return pImg;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}
