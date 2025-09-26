# Unity NPC Dialogue Debugging Tools

Tools to fix common problems when adding ChatGPT, Claude, or Gemini dialogue to your Unity NPCs. Keep your characters speaking naturally and staying in character.

![Unity Version](https://img.shields.io/badge/Unity-2022.3%2B-blue)
![License](https://img.shields.io/badge/License-MIT-green)
![C#](https://img.shields.io/badge/C%23-9.0-orange)

## Overview

When adding dialogue systems to NPCs, you might run into these problems:
- NPCs saying things like "As an AI language model..." instead of staying in character
- Medieval characters talking about smartphones or the internet
- Characters forgetting previous conversations
- Inappropriate responses that don't fit your game
- Slow response times or errors

This toolkit helps you catch and fix these issues before players see them.

## Features

### Core Tools
- **Response Checker** - Catches when NPCs say the wrong things
- **Character Profiles** - ScriptableObject-based personality system (no coding required!)
- **Character Consistency** - Keeps NPCs talking like themselves
- **Content Filter** - Removes inappropriate content with regex word boundaries
- **Response Memory** - Saves common responses to keep NPCs consistent
- **Performance Monitor** - Tracks response times and errors
- **Automatic Scene Setup** - Complete test scene with one click
- **Professional UI** - Bottom-half canvas with scrollable dialogue history
- **Async/Await Architecture** - Modern error handling throughout

### Works With
- **ChatGPT** - OpenAI's dialogue service
- **Claude** - Anthropic's dialogue service
- **Gemini** - Google's dialogue service

### Extra Features
- See debug info right in Unity
- Track how long responses take
- Remember previous conversations
- Backup responses when things go wrong
- Export logs to check later

## Quick Start

### Step 1: Installation

**Option A: Download and Copy (Easiest for Beginners)**
1. Download this repository as a ZIP file
2. Open your Unity project
3. Copy the `Scripts` folder into your project's `Assets` folder

**Option B: Using Git (If you know Git)**
```
git clone https://github.com/yourusername/Unity-AI-NPC-Debugging.git
```

### Step 2: Set Up Your API Key

**What's an API Key?**
An API key is like a password that lets your game talk to ChatGPT, Claude, or Gemini. You need one to make NPCs speak.

1. **Find the example file:**
   - Look for `api-config.example.json` in the downloaded folder
   - Copy it and rename to `api-config.json`
   - Put this file next to your Unity project's Assets folder (NOT inside it)

2. **Add your API key:**
   Open `api-config.json` in any text editor and replace `YOUR_OPENAI_API_KEY_HERE` with your actual key:
   ```json
   {
     "activeProvider": "openai",
     "providers": {
       "openai": {
         "apiKey": "sk-abc123...",  // <- Your real key goes here
       }
     }
   }
   ```

3. **Where to get API keys (all require sign-up):**
   - **ChatGPT**: https://platform.openai.com/api-keys
   - **Claude**: https://console.anthropic.com/
   - **Gemini**: https://makersuite.google.com/app/apikey

**Important**: Keep your API key secret! Never share it or upload it online.

### Step 3: Import the Namespace

All scripts use the `AngrySharkStudio.LLM` namespace. Add this to your scripts:

```csharp
using AngrySharkStudio.LLM.Core;
using AngrySharkStudio.LLM.ScriptableObjects;
using AngrySharkStudio.LLM.Examples;
```

### Step 4: Create Your NPC Character

1. **Create a Character Profile:**
   - Right-click in Project window → Create → AI NPC → Character Profile
   - Or use Tools menu → AngrySharkStudio → AI NPC → Character Profiles → Choose a preset:
     - **Gruff Merchant**: Suspicious trader with medieval speech
     - **Wise Scholar**: Knowledgeable librarian
     - **Suspicious Guard**: Vigilant city guard

2. **Configure your character in the Inspector (no coding!):**
   - **Character Identity**: Name, personality, backstory
   - **Speech Patterns**: Common phrases, vocabulary style
   - **Knowledge**: Topics they know or avoid
   - **Behavior**: Mood, helpfulness, verbosity

3. **Add to your NPC GameObject:**
   ```csharp
   // Add the dialogue component
   gameObject.AddComponent<SmartNpcExample>();
   ```
   Then drag your Character Profile asset to the component in Inspector

4. **Test it out:**
   ```csharp
   // When player talks to NPC:
   await npc.ProcessPlayerInput("Hello, do you have any healing potions?");
   
   // NPC will respond using the character profile settings
   ```

### Step 5: Quick Setup - Complete Test Scene

The fastest way to get started:

1. **Create Complete Test Scene:**
   - Tools → AngrySharkStudio → AI NPC → Setup → Create Complete Test Scene
   - This automatically:
     - Creates LlmManager singleton for API communication
     - Places NPC at y=4 for visibility  
     - Creates UI on bottom half of screen with:
       - Professional layout using Unity's built-in sprites
       - Permanent scrollbar for dialogue history
       - Full-width text display with proper wrapping
       - Responsive input field with Enter key support
     - Creates and assigns a default CharacterProfile (Gruff Merchant)
     - Connects all components properly
     - Sets up materials for URP rendering

2. **Alternative Individual Setup:**
   - Tools → AngrySharkStudio → AI NPC → Setup → Create NPC GameObject
   - Tools → AngrySharkStudio → AI NPC → Setup → Create Test UI

3. **Important Notes:**
   - NPC visual is a blue capsule (URP material in Art/Materials folder)
   - UI only covers bottom half of screen so NPC remains visible
   - SimpleConsoleTest component adds context menu options for testing

## How It Works

### The Response Checker
Stops NPCs from saying things that break your game, configured through Character Profiles:

```csharp
// Manual configuration (if needed for runtime changes):
var debugger = GetComponent<AiResponseDebugger>();
debugger.bannedPhrases.Add("as an AI");

// Recommended approach - Configure in Character Profile asset:
// All banned phrases, response length, and validation rules
// are set in the Character Profile Inspector - no code needed!
```

### Character Personality (ScriptableObject System)
Characters are configured through reusable Character Profile assets:

```csharp
// No more hardcoding! Create a CharacterProfile asset instead:
// 1. Right-click in Project → Create → AI NPC → Character Profile
// 2. Configure all settings in the Inspector
// 3. Assign to your NPC's SmartNpcExample component

// The system automatically loads all settings from the profile:
namespace AngrySharkStudio.LLM.Examples
{
    public class SmartNpcExample : MonoBehaviour
    {
        [SerializeField] private CharacterProfile characterProfile;
        // All personality, vocabulary, era settings come from the profile!
    }
}
```

**Benefits of the ScriptableObject system:**
- Create NPCs without coding
- Reuse character profiles across multiple NPCs
- Test different personalities by swapping profiles
- Non-programmers can create NPCs

### Content Filter
Keep your game appropriate for your audience:

```csharp
var filter = GetComponent<AiContentFilter>();
filter.targetSafety = SafetyLevel.FamilyFriendly;  // No bad language

// If NPC tries to say something inappropriate:
var filterResult = filter.FilterContent(npcResponse);
if (!filterResult.passed) {
    // Use a safe backup response instead
    npcResponse = filter.GetSafeFallbackResponse();
}
```

## Making NPCs Remember Previous Conversations

The toolkit saves responses so NPCs stay consistent:

```csharp
// If player asks "What do you sell?" multiple times,
// NPC will give the same answer each time
var cache = GetComponent<AiResponseCache>();
cache.enableCaching = true;
cache.maxCacheSize = 100;  // Remember 100 different conversations
```

## Example: Complete NPC Setup with Character Profiles

Here's how to set up a merchant NPC using the ScriptableObject system:

```csharp
using UnityEngine;
using AngrySharkStudio.LLM.Examples;
using AngrySharkStudio.LLM.ScriptableObjects;

public class MerchantNPC : MonoBehaviour
{
    [SerializeField] private CharacterProfile merchantProfile;
    private SmartNpcExample npcDialogue;
    
    void Start()
    {
        // Add the dialogue system
        npcDialogue = gameObject.AddComponent<SmartNpcExample>();
        
        // The character profile is assigned in Inspector
        // All NPC settings come from the ScriptableObject
    }
    
    async void OnPlayerInteract(string playerMessage)
    {
        // NPC responds based on character profile settings
        await npcDialogue.ProcessPlayerInput(playerMessage);
    }
}
```

**Quick Setup Without Code:**
1. GameObject → AI NPC Debugging → Create Complete Test Scene
2. This creates everything you need with example character profiles!

## Performance Tips

- **Response Time**: NPCs usually respond in 1-3 seconds
- **Cost**: Each response costs a tiny amount (usually less than $0.01)
- **Caching**: Saves money by reusing common responses
- **Fallbacks**: Always have backup dialogue ready
- **UI Optimized**: Touch-friendly scrolling for mobile devices
- **Async Operations**: Non-blocking AI calls keep your game responsive

## Troubleshooting

### If This Is Your First Time

**"I don't know where to put the files"**
- The `Scripts` folder goes inside your Unity project's `Assets` folder
- The `api-config.json` goes OUTSIDE the Assets folder (next to it)

**"What's an API key?"**
- It's like a password that lets your game talk to ChatGPT/Claude/Gemini
- You need to sign up on their websites to get one
- Each service charges a small amount per response (usually pennies)

**"How do I create different NPC personalities?"**
- Use the Character Profile system!
- Right-click in Project → Create → AI NPC → Character Profile
- Configure everything in Inspector (no coding required)
- Assign the profile to your NPC's SmartNpcExample component

**"My NPC isn't responding"**
1. Check the Unity Console for red error messages
2. Make sure your API key is correct in `api-config.json`
3. Make sure you have internet connection
4. Try the test button in the Inspector

### Common Problems and Solutions

**"No API key configured"**
- Your `api-config.json` file is missing or in the wrong place
- Should be next to your Assets folder, not inside it
- Make sure it's named exactly `api-config.json`

**"NPC says 'As an AI language model...'"**
- Add more banned phrases in the Inspector
- Make the personality description stronger
- Add example phrases your NPC should use

**"Responses are too slow"**
- Enable caching to reuse common responses
- Reduce max response length
- Use simpler prompts

**"NPC forgets previous conversations"**
- Make sure the conversation history is enabled
- Check that the cache isn't full
- Increase cache size if needed

**"Text appears too narrow in dialogue window"**
- This is a Unity UI issue - the VerticalLayoutGroup needs childControlWidth enabled
- Use the automatic scene setup which configures this correctly
- If setting up manually, ensure Content GameObject has proper LayoutGroup settings

**"Can't scroll the dialogue history"**
- Make sure the scrollbar visibility is set to "Permanent" not "Auto Hide"
- Check that the scroll rect's content is properly sized
- The automatic setup handles this configuration

**"UI components showing errors about missing RectTransform"**
- Always use the Tools menu setup options instead of manual creation
- UI elements must be created with `typeof(RectTransform)` in code
- The provided setup scripts handle this automatically

## Contributing

Want to help improve these tools? Great!

1. Fork this repository
2. Make your changes
3. Test them in Unity
4. Submit a pull request

## License

MIT License - Use these tools in any project, commercial or personal.

## Learn More

- [Full Tutorial](https://www.angry-shark-studio.com/blog/debugging-ai-unity-npcs-weird-things)

---

Made by [Angry Shark Studio](https://www.angry-shark-studio.com)