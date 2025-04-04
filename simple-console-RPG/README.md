# Text RPG Adventure

A procedurally generated text-based RPG using OpenAI-compatible APIs. It features a character creator and a custom language model designed for D&D environments that works in tandem with the API to generate a random adventure depending on the player's choices and actions. Each adventure is unique.

## API Configuration

This application now supports any OpenAI-compatible API, including local models through services like OpenWebUI, LM Studio, Ollama, etc.

### Configuration Steps

1. Edit the `apisettings.json` file in the application directory with your preferred API settings:

```json
{
  "ApiKey": "your-api-key-here",
  "BaseUrl": "http://localhost:8080/v1",
  "Model": "gpt-3.5-turbo",
  "Temperature": 0.7,
  "MaxTokens": 800,
  "StreamResponse": false,
  "TimeoutSeconds": 60
}
```

### Configuration Options

#### API Connection Settings

- **ApiKey**: Your API key (can be empty for some local services)
- **BaseUrl**: The base URL of the API service
  - For OpenAI: `https://api.openai.com/v1`
  - For OpenWebUI: `http://localhost:8080/v1` (default)
  - For other services: Check their documentation
- **Model**: The model to use (e.g., `gpt-3.5-turbo`, `llama3`, etc.)

#### Model Parameters

- **Temperature**: Controls randomness (0.0 to 1.0)
  - Lower values (e.g., 0.2) make responses more focused and deterministic
  - Higher values (e.g., 0.8) make responses more creative and varied
- **MaxTokens**: Maximum tokens to generate in responses
  - Higher values allow for longer responses but may be slower
- **StreamResponse**: Whether to stream the response (true/false)
  - Currently not fully implemented in the UI
- **TimeoutSeconds**: Timeout in seconds for API requests
  - Increase this value if you experience timeout errors with slower models

### Using with OpenWebUI

1. Install and run [OpenWebUI](https://docs.openwebui.com/)
2. Configure your models in OpenWebUI
3. Set the BaseUrl in `apisettings.json` to `http://localhost:8080/v1` (or your custom port)
4. Set the Model to match one of your configured models in OpenWebUI
5. The ApiKey can be left empty or set to your OpenWebUI API key if you've configured one

### Error Handling

The application now includes improved error handling for API calls:

- Connection errors are caught and displayed with helpful messages
- Timeouts are handled gracefully
- If an API call fails, the application will continue running with fallback responses
- Detailed error messages are shown in the console

### Fallback to Original Configuration

If `apisettings.json` is not found, the application will look for the original `apikey.txt` file and use the OpenAI API with the key found there. If neither is found, default settings will be used.

## How to Play

1. Run the application
2. Create your character by following the prompts
3. Roll the dice to determine your adventure parameters
4. Follow the story and make choices to progress through the adventure

Enjoy your unique adventure!
