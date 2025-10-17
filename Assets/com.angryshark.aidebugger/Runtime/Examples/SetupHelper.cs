using System.IO;
using AngrySharkStudio.LLM.API;
using AngrySharkStudio.LLM.Core;
using AngrySharkStudio.LLM.Models.Character;
using AngrySharkStudio.LLM.PlatformDebuggers;
using AngrySharkStudio.LLM.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AngrySharkStudio.LLM.Examples {
    /// <summary>
    /// Helper script to automatically create test UI setup with TextMeshPro
    /// Makes it super easy for beginners to get started
    /// </summary>
    public class SetupHelper : MonoBehaviour {

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int Color = Shader.PropertyToID("_Color");

        #if UNITY_EDITOR

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Setup/Create Complete Test Scene", false, 0)]
        public static void CreateCompleteTestScene() {
            // Ensure LlmManager exists
            EnsureLlmManagerExists();

            // Create default CharacterProfile
            var profile = CreateDefaultCharacterProfile();

            // Create NPC with all components
            var npc = CreateNpcGameObjectInternal();

            // Assign a CharacterProfile and configure all components
            ConfigureNpcComponents(npc, profile);

            // Create UI
            var ui = CreateTestUIInternal();

            // Connect NPC to UI
            ConnectNpcToUI(ui, npc);

            // Initialize UI after NPC is connected
            var dialogueUI = ui.GetComponent<DialogueTestUI>();

            if (dialogueUI != null) {
                dialogueUI.InitializeUI();
            }

            // Select the NPC so the user can see it
            Selection.activeGameObject = npc;

            Debug.Log("Complete test scene created! Ready to test AI NPC dialogue.");
            Debug.Log("Make sure to add your API key to api-config.json file in project root.");
            Debug.Log("CharacterProfile created and assigned automatically!");
            Debug.Log("Click the input field, type a message and press Send or Enter.");
        }

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Setup/Create NPC GameObject", false, 1)]
        public static void CreateNpcGameObject() {
            CreateNpcGameObjectInternal();
        }

        private static GameObject CreateNpcGameObjectInternal() {
            // Create GameObject
            var npc = new GameObject("AI_NPC_TestCharacter") {
                transform = {
                    // Position at y=4 for visibility
                    position = new Vector3(0, 4, 0)
                }
            };

            // Add components in the correct order
            npc.AddComponent<SmartNpcExample>();
            npc.AddComponent<AiResponseDebugger>();
            npc.AddComponent<NpcCharacterConsistency>();
            npc.AddComponent<AiContentFilter>();
            npc.AddComponent<AiResponseCache>();
            npc.AddComponent<AiRequestQueue>();

            // Add a platform debugger (default to ChatGPT)
            npc.AddComponent<ChatGptDebugger>();

            // Also add a simple console test for easy testing
            npc.AddComponent<SimpleConsoleTest>();

            // Create visual representation (optional)
            var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(npc.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.name = "Visual";

            // Create URP material
            var npcMaterial = CreateUrpMaterial("NPC_Material", new Color(0.5f, 0.8f, 1f));
            var renderer = visual.GetComponent<Renderer>();

            if (renderer != null && npcMaterial != null) {
                renderer.material = npcMaterial;
            }

            Debug.Log("NPC created! Components added: SmartNpcExample, debuggers, and filters.");
            Debug.Log("IMPORTANT: Assign a CharacterProfile ScriptableObject to the SmartNpcExample component!");

            return npc;
        }

        [MenuItem("Tools/AngrySharkStudio/AI NPC/Setup/Create Test UI", false, 2)]
        public static void CreateTestUI() {
            CreateTestUIInternal();
        }

        private static GameObject CreateTestUIInternal() {
            // Create Canvas if needed
            var canvas = FindObjectOfType<Canvas>();

            if (canvas == null) {
                var canvasGo = new GameObject("Canvas");
                canvas = canvasGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGo.AddComponent<CanvasScaler>();
                canvasGo.AddComponent<GraphicRaycaster>();

                // Configure CanvasScaler for responsive UI
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Create an EventSystem if needed
            if (FindObjectOfType<EventSystem>() == null) {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            // Create UI holder
            var uiHolder = new GameObject("NPC_Dialogue_UI");
            uiHolder.transform.SetParent(canvas.transform, false);

            // Configure as bottom half of screen only
            var holderRect = uiHolder.AddComponent<RectTransform>();
            holderRect.anchorMin = new Vector2(0, 0);
            holderRect.anchorMax = new Vector2(1, 0.5f);
            holderRect.offsetMin = Vector2.zero;
            holderRect.offsetMax = Vector2.zero;

            // Create an input field
            var inputField = CreateTMPInputField(uiHolder.transform);
            ConfigureInputField(inputField);

            // Create the sendButton
            var sendButton = CreateTMPButton(uiHolder.transform, "SendButton", "Send");
            ConfigureSendButton(sendButton);

            // Create a response area
            var responseArea = CreateResponseArea(uiHolder.transform);

            // Create loading indicator
            var loadingText = CreateLoadingIndicator(uiHolder.transform);

            // Add DialogueTestUI component and configure
            var dialogueUI = uiHolder.AddComponent<DialogueTestUI>();

            // Use SerializedObject since fields are now [SerializeField] private
            var serializedUI = new SerializedObject(dialogueUI);
            serializedUI.FindProperty("playerInput").objectReferenceValue = inputField.GetComponent<TMP_InputField>();
            serializedUI.FindProperty("sendButton").objectReferenceValue = sendButton.GetComponent<Button>();

            serializedUI.FindProperty("npcResponseText").objectReferenceValue =
                responseArea.GetComponentInChildren<TextMeshProUGUI>();
            serializedUI.FindProperty("loadingIndicator").objectReferenceValue = loadingText;
            serializedUI.ApplyModifiedProperties();

            EditorUtility.SetDirty(dialogueUI);

            // Don't initialize here - will be done after NPC is connected
            Debug.Log("TextMeshPro UI created successfully!");

            return uiHolder;
        }

        private static GameObject CreateTMPInputField(Transform parent) {
            // Create an input field using TMP
            var inputGo = new GameObject("PlayerInput", typeof(RectTransform));
            inputGo.transform.SetParent(parent, false);

            var inputField = inputGo.AddComponent<TMP_InputField>();

            // Create a text area
            var textArea = new GameObject("Text Area", typeof(RectTransform));
            textArea.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -7);

            // Create placeholder
            var placeholder = new GameObject("Placeholder", typeof(RectTransform));
            placeholder.transform.SetParent(textArea.transform, false);
            var placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Type your message here...";
            placeholderText.fontSize = 16;
            placeholderText.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

            var placeholderRect = placeholder.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            // Create text
            var text = new GameObject("Text", typeof(RectTransform));
            text.transform.SetParent(textArea.transform, false);
            var inputText = text.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 16;
            inputText.color = UnityEngine.Color.black;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;

            var textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Configure input field references
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;

            // Add background
            var bg = inputGo.AddComponent<Image>();
            bg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            bg.type = Image.Type.Sliced;
            bg.color = UnityEngine.Color.white;

            return inputGo;
        }

        private static void ConfigureInputField(GameObject inputField) {
            var rect = inputField.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.1f);
            rect.anchorMax = new Vector2(0.75f, 0.1f); // Leave space for a button
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 40);
        }

        private static GameObject CreateTMPButton(Transform parent, string name, string buttonText) {
            var buttonGo = new GameObject(name, typeof(RectTransform));
            buttonGo.transform.SetParent(parent, false);

            // Add button components
            var buttonImage = buttonGo.AddComponent<Image>();
            buttonImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            buttonImage.type = Image.Type.Sliced;
            buttonGo.AddComponent<Button>();
            buttonImage.color = new Color(0.2f, 0.6f, 1f);

            // Create text
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(buttonGo.transform, false);
            var text = textGo.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.fontSize = 18;
            text.color = UnityEngine.Color.white;
            text.alignment = TextAlignmentOptions.Center;

            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return buttonGo;
        }

        private static void ConfigureSendButton(GameObject button) {
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.8f, 0.1f);
            rect.anchorMax = new Vector2(0.9f, 0.1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 40);
        }

        private static GameObject CreateResponseArea(Transform parent) {
            // Create a scroll view
            var scrollView = new GameObject("ResponseScrollView", typeof(RectTransform));
            scrollView.transform.SetParent(parent, false);

            var scrollRect = scrollView.AddComponent<ScrollRect>();
            var scrollBg = scrollView.AddComponent<Image>();
            scrollBg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            scrollBg.type = Image.Type.Sliced;
            scrollBg.color = new Color(0.95f, 0.95f, 0.95f);

            // Configure scroll view position - use full height
            var scrollRectTransform = scrollView.GetComponent<RectTransform>();
            scrollRectTransform.anchorMin = new Vector2(0.05f, 0.05f);
            scrollRectTransform.anchorMax = new Vector2(0.95f, 0.95f);
            scrollRectTransform.offsetMin = Vector2.zero;
            scrollRectTransform.offsetMax = Vector2.zero;

            // Create viewport
            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
            viewportImage.type = Image.Type.Sliced;
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10, 10);
            viewportRect.offsetMax = new Vector2(-10, -10);

            // Create content
            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 0); // Width handled by anchors

            // Add ContentSizeFitter to content for proper resizing
            var contentFitter = content.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add VerticalLayoutGroup for a proper message layout
            var layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.childControlWidth = true; // Control child width
            layoutGroup.childControlHeight = true; // Control child height
            layoutGroup.childForceExpandWidth = true; // Force expand width
            layoutGroup.childForceExpandHeight = false; // Do not force to expand height
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);

            // Create text
            var responseText = new GameObject("ResponseText", typeof(RectTransform));
            responseText.transform.SetParent(content.transform, false);
            var text = responseText.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 16;
            text.color = UnityEngine.Color.black;
            text.alignment = TextAlignmentOptions.TopLeft;

            // No ContentSizeFitter or LayoutElement needed - parent's LayoutGroup controls sizing


            // Configure scroll rect
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = 20;
            scrollRect.movementType = ScrollRect.MovementType.Clamped; // Prevent elastic bouncing

            // Create scrollbar
            var scrollbarGo = new GameObject("Scrollbar Vertical");
            scrollbarGo.transform.SetParent(scrollView.transform, false);
            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            var scrollbarBg = scrollbarGo.AddComponent<Image>();
            scrollbarBg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            scrollbarBg.type = Image.Type.Sliced;
            scrollbarBg.color = new Color(0.96f, 0.96f, 0.96f, 1f);

            // Configure the scrollbar position (right side)
            var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1, 0);
            scrollbarRect.anchorMax = new Vector2(1, 1);
            scrollbarRect.pivot = new Vector2(1, 0.5f);
            scrollbarRect.anchoredPosition = new Vector2(-10, 0);
            scrollbarRect.sizeDelta = new Vector2(20, -20); // Full height with padding

            // Create a scrollbar handle area
            var handleArea = new GameObject("Sliding Area", typeof(RectTransform));
            handleArea.transform.SetParent(scrollbarGo.transform, false);
            var handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(0, 4); // Padding at top/bottom
            handleAreaRect.offsetMax = new Vector2(0, -4);

            // Create a scrollbar handle
            var handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);
            var handleImg = handle.AddComponent<Image>();
            handleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            handleImg.type = Image.Type.Sliced;
            handleImg.color = new Color(0.7f, 0.7f, 0.7f, 1f);

            var handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(10, 10);

            // Configure scrollbar
            scrollbar.targetGraphic = handleImg;
            scrollbar.handleRect = handleRect;
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.size = 0.3f; // Default handle size

            // Set scrollbar colors for better interactivity
            var colors = scrollbar.colors;
            colors.normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
            colors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            colors.selectedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            colors.disabledColor = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            scrollbar.colors = colors;

            // Connect scrollbar to scroll rect
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            scrollRect.verticalScrollbarSpacing = 0;

            return scrollView;
        }

        private static GameObject CreateLoadingIndicator(Transform parent) {
            var loadingGo = new GameObject("LoadingIndicator", typeof(RectTransform));
            loadingGo.transform.SetParent(parent, false);

            // Add background for better visibility
            var background = loadingGo.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.8f); // Dark semi-transparent background

            // Create text as a child for padding
            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(loadingGo.transform, false);

            var loadingText = textGo.AddComponent<TextMeshProUGUI>();
            loadingText.text = "AI is thinking...";
            loadingText.fontSize = 24; // Bigger font
            loadingText.fontStyle = FontStyles.Bold;
            loadingText.color = new Color(0.9f, 0.9f, 0.9f); // White text on a dark background
            loadingText.alignment = TextAlignmentOptions.Center;

            // Position text to fill parent
            var textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Position above scroll view - much more visible
            var rect = loadingGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.65f); // Higher position
            rect.anchorMax = new Vector2(0.5f, 0.65f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(300, 60); // Bigger size

            // Add rounded corners effect (optional)
            background.sprite = null; // Could add rounded sprite here

            // Start hidden
            loadingGo.SetActive(false);

            // Ensure the loading indicator is on top
            loadingGo.transform.SetAsLastSibling();

            return loadingGo;
        }

        private static void EnsureLlmManagerExists() {
            var existingManager = FindObjectOfType<LlmManager>();

            if (existingManager != null) {
                return;
            }

            var managerGo = new GameObject("LlmManager");
            managerGo.AddComponent<LlmManager>();
            Debug.Log("Created LlmManager singleton in scene");
        }

        private static Material CreateUrpMaterial(string materialName, Color color) {
            // Ensure Art/Materials folder exists
            const string materialPath = "Assets/Art/Materials";

            if (!AssetDatabase.IsValidFolder("Assets/Art")) {
                AssetDatabase.CreateFolder("Assets", "Art");
            }

            if (!AssetDatabase.IsValidFolder(materialPath)) {
                AssetDatabase.CreateFolder("Assets/Art", "Materials");
            }

            // Try to find URP Lit shader
            var urpLitShader = Shader.Find("Universal Render Pipeline/Lit");

            if (urpLitShader == null) {
                // Fallback to standard shader if URP not available
                urpLitShader = Shader.Find("Standard");
            }

            // Create material
            var newMaterial = new Material(urpLitShader) {
                name = materialName
            };
            newMaterial.SetColor(BaseColor, color); // URP property
            newMaterial.SetColor(Color, color); // Standard shader fallback

            // Save material as an asset
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{materialPath}/{materialName}.mat");
            AssetDatabase.CreateAsset(newMaterial, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created material at: {assetPath}");

            return newMaterial;
        }

        /// <summary>
        /// Creates a default CharacterProfile with merchant settings
        /// </summary>
        private static CharacterProfile CreateDefaultCharacterProfile() {
            // Ensure directory exists
            const string profilePath = "Assets/AI_NPC_Profiles";

            if (!Directory.Exists(profilePath)) {
                Directory.CreateDirectory(profilePath);
            }

            // Create a CharacterProfile instance
            var profile = ScriptableObject.CreateInstance<CharacterProfile>();

            // Configure with merchant defaults using reflection to set private fields
            var serializedProfile = new SerializedObject(profile);

            serializedProfile.FindProperty("npcName").stringValue = "Gareth the Merchant";

            serializedProfile.FindProperty("personality").stringValue =
                "A gruff but fair medieval merchant who sells weapons and armor. " +
                "Suspicious of strangers but helpful to paying customers. Speaks in short, direct sentences.";

            serializedProfile.FindProperty("backstory").stringValue =
                "Former soldier turned merchant after a battle injury. Knows quality gear when he sees it. " +
                "Has seen enough adventurers to be both helpful and cautious.";

            // Common phrases
            var commonPhrases = serializedProfile.FindProperty("commonPhrases");
            commonPhrases.ClearArray();
            commonPhrases.arraySize = 5;
            commonPhrases.GetArrayElementAtIndex(0).stringValue = "What do ye need?";
            commonPhrases.GetArrayElementAtIndex(1).stringValue = "That'll cost ye extra.";
            commonPhrases.GetArrayElementAtIndex(2).stringValue = "Aye, good choice.";
            commonPhrases.GetArrayElementAtIndex(3).stringValue = "Coin first, then we talk.";
            commonPhrases.GetArrayElementAtIndex(4).stringValue = "Safe travels, adventurer.";

            // Set enums
            serializedProfile.FindProperty("vocabularyLevel").enumValueIndex = (int)VocabularyLevel.Medieval;
            serializedProfile.FindProperty("characterEra").enumValueIndex = (int)Era.Fantasy;
            serializedProfile.FindProperty("defaultMood").enumValueIndex = (int)EmotionalState.Neutral;

            // Behavioral traits
            serializedProfile.FindProperty("formalityLevel").floatValue = 0.3f; // Casual
            serializedProfile.FindProperty("helpfulness").floatValue = 0.6f;
            serializedProfile.FindProperty("verbosity").floatValue = 0.4f; // Concise
            serializedProfile.FindProperty("maxResponseLength").intValue = 200;

            serializedProfile.ApplyModifiedProperties();

            // Save as an asset
            var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{profilePath}/Gareth_Merchant.asset");
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Created CharacterProfile at: {assetPath}");

            return profile;
        }

        /// <summary>
        /// Configures all NPC components and assigns the CharacterProfile
        /// </summary>
        private static void ConfigureNpcComponents(GameObject npc, CharacterProfile profile) {
            var smartNpc = npc.GetComponent<SmartNpcExample>();

            if (smartNpc == null) return;

            // Assign CharacterProfile using SerializedObject
            var serializedNpc = new SerializedObject(smartNpc);
            serializedNpc.FindProperty("characterProfile").objectReferenceValue = profile;
            serializedNpc.ApplyModifiedProperties();

            // Get all the components
            var debugger = npc.GetComponent<AiResponseDebugger>();
            var consistency = npc.GetComponent<NpcCharacterConsistency>();
            var contentFilter = npc.GetComponent<AiContentFilter>();
            var responseCache = npc.GetComponent<AiResponseCache>();
            var platformDebugger = npc.GetComponent<ChatGptDebugger>(); // Get the platform debugger

            // Assign components to SmartNpcExample using SerializedObject
            serializedNpc.FindProperty("debugger").objectReferenceValue = debugger;
            serializedNpc.FindProperty("characterConsistency").objectReferenceValue = consistency;
            serializedNpc.FindProperty("contentFilter").objectReferenceValue = contentFilter;
            serializedNpc.FindProperty("responseCache").objectReferenceValue = responseCache;
            serializedNpc.FindProperty("platformDebugger").objectReferenceValue = platformDebugger;
            serializedNpc.ApplyModifiedProperties();

            // Configure NpcCharacterConsistency with the profile
            if (consistency != null) {
                var serializedConsistency = new SerializedObject(consistency);
                serializedConsistency.FindProperty("characterProfile").objectReferenceValue = profile;
                serializedConsistency.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(smartNpc);
            EditorUtility.SetDirty(consistency);

            Debug.Log("✅ All NPC components configured and CharacterProfile assigned!");
        }

        /// <summary>
        /// Connects the NPC to the DialogueTestUI using SerializedObject
        /// </summary>
        private static void ConnectNpcToUI(GameObject ui, GameObject npc) {
            var dialogueUI = ui.GetComponent<DialogueTestUI>();
            var smartNpc = npc.GetComponent<SmartNpcExample>();

            if (dialogueUI == null || smartNpc == null) {
                return;
            }

            // Use SerializedObject since the field is now [SerializeField] private
            var serializedUI = new SerializedObject(dialogueUI);
            serializedUI.FindProperty("npc").objectReferenceValue = smartNpc;
            serializedUI.ApplyModifiedProperties();

            EditorUtility.SetDirty(dialogueUI);

            Debug.Log("NPC connected to UI successfully!");
        }

        #endif

    }
}