# AI NPC Debug Toolkit

Debug and visualize AI-powered NPC dialogue systems for ChatGPT, Claude, Gemini, and other LLM integrations in Unity.

## Features

- **Real-time Response Visualization**: See AI responses in-scene with customizable debug UI
- **Character Consistency Validation**: Detect when NPCs break character or provide inconsistent responses
- **API Error Detection**: Catch and log API communication issues before they affect players
- **Response Caching**: Improve performance with intelligent conversation caching
- **Content Filtering**: Validate AI responses against content policies
- **Multiple LLM Support**: Works with ChatGPT, Claude, Gemini, and custom endpoints

## Installation

### Via Unity Package Manager

1. Open Unity Package Manager (Window → Package Manager)
2. Click the **+** dropdown in the top-left corner
3. Select **"Add package from git URL..."**
4. Paste this URL:
```https://github.com/angrysharkstudio/unity-ai-npc-debugging.git?path=Assets/com.angryshark.aidebugger```
5. Click **Add**

### Specific Version Installation

For production projects, lock to a specific version:
```https://github.com/angrysharkstudio/unity-ai-npc-debugging.git?path=Assets/com.angryshark.aidebugger#v1.0.0```

## Quick Start

### 1. Create a Character Profile

1. In Unity Project window, right-click
2. Select **Create → AI NPC → Character Profile**
3. Configure NPC personality, background, and behavior

### 2. Add Debug Components

Add `AiResponseDebugger` component to your NPC GameObject to visualize AI responses in real-time.

### 3. Configure API Settings

Copy `api-config.example.json` from the samples folder and rename to `api-config.json`. Add your API keys:

```json
{
  "provider": "openai",
  "apiKey": "your-api-key-here",
  "model": "gpt-4"
}

### 4. Test with Console Example

Import the **Basic Console Example** sample from Package Manager to see a working implementation.

## Components

### AiResponseDebugger

Visualizes AI responses in the scene view with customizable UI.

```csharp
using AngrySharkStudio.LLM;

public class MyNpc : MonoBehaviour
{
    [SerializeField] private AiResponseDebugger debugger;

    void Start()
    {
        debugger.ShowResponse("Hello, traveler!");
    }
}
```

### AiContentFilter

Validates AI responses against content policies and character consistency rules.

### AiResponseCache

Caches conversation history to reduce API calls and improve performance.

### CharacterProfile ScriptableObject

Stores NPC personality, background, and dialogue rules. Create via:
**Create → AI NPC → Character Profile**

## Requirements

- Unity 2020.3 or newer
- TextMeshPro (usually included by default)
- Active internet connection for LLM API calls
- API key for OpenAI, Anthropic Claude, or Google Gemini

## Documentation

Full documentation available at:
[https://github.com/angrysharkstudio/unity-ai-npc-debugging](https://github.com/angrysharkstudio/unity-ai-npc-debugging)

## Support

- **Issues**: [GitHub Issues](https://github.com/angrysharkstudio/unity-ai-npc-debugging/issues)
- **Email**: studio.angry.shark@gmail.com
- **Website**: [angry-shark-studio.com](https://angry-shark-studio.com)

## License

MIT License - see [LICENSE](https://github.com/angrysharkstudio/unity-ai-npc-debugging/blob/main/LICENSE) for details.

## Credits

Developed by [Angry Shark Studio](https://angry-shark-studio.com)
Unity Certified Expert team specializing in AR/VR and AI integration.