using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;
using OpenAI_API;
using OpenAI_API.Chat;

namespace simple_console_RPG
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string ResponseBody { get; }

        public ApiException(string message, int statusCode, string responseBody) : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }
    }

    public class OpenAiCompatibleService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _config;

        public OpenAiCompatibleService(ApiConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Create HttpClient with handler that allows for HTTP if needed
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

            // Add authorization header if API key is provided
            if (!string.IsNullOrEmpty(_config.ApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);
            }
        }

        public async Task<ChatResult> CreateChatCompletionAsync(ChatRequest request)
        {
            try
            {
                // Convert OpenAI_API ChatRequest to our own format
                var messages = new List<Dictionary<string, string>>();

                foreach (var message in request.Messages)
                {
                    messages.Add(new Dictionary<string, string>
                    {
                        { "role", message.Role.ToString().ToLower() },
                        { "content", message.Content }
                    });
                }

                // Use configuration values but allow request to override them
                var requestData = new Dictionary<string, object>
                {
                    { "model", _config.Model },
                    { "messages", messages },
                    { "temperature", request.Temperature > 0 ? request.Temperature : _config.Temperature },
                    { "max_tokens", request.MaxTokens > 0 ? request.MaxTokens : _config.MaxTokens },
                    { "stream", _config.StreamResponse }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestData),
                    Encoding.UTF8,
                    "application/json");

                Console.WriteLine($"Sending request to {_config.BaseUrl}/chat/completions");
                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    var errorMessage = $"API request failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}";

                    Console.WriteLine(errorMessage);
                    Console.WriteLine($"Response: {errorBody}");

                    throw new ApiException(errorMessage, (int)response.StatusCode, errorBody);
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseBody);

                // Extract the response content
                string responseContent = "";
                if (responseData != null && responseData.TryGetValue("choices", out var choices) &&
                    choices.GetArrayLength() > 0)
                {
                    if (choices[0].TryGetProperty("message", out var message) &&
                        message.TryGetProperty("content", out var contentElement))
                    {
                        responseContent = contentElement.GetString() ?? "";
                    }
                    else if (choices[0].TryGetProperty("text", out var textElement))
                    {
                        // Handle different API response formats
                        responseContent = textElement.GetString() ?? "";
                    }
                }

                if (string.IsNullOrEmpty(responseContent))
                {
                    Console.WriteLine("Warning: Received empty response from API");
                    Console.WriteLine($"Full response: {responseBody}");
                    responseContent = "[No response generated]";
                }

                // Create a ChatResult object
                var result = new ChatResult
                {
                    Choices = new List<ChatChoice>
                    {
                        new ChatChoice
                        {
                            Message = new ChatMessage(ChatMessageRole.Assistant, responseContent)
                        }
                    }
                };

                return result;
            }
            catch (ApiException)
            {
                // Let ApiException propagate as it's already formatted
                throw;
            }
            catch (HttpRequestException ex)
            {
                var errorMessage = $"Network error when calling API: {ex.Message}";
                Console.WriteLine(errorMessage);

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                throw new ApiException(errorMessage, 0, "");
            }
            catch (TaskCanceledException)
            {
                var errorMessage = $"API request timed out after {_config.TimeoutSeconds} seconds";
                Console.WriteLine(errorMessage);
                throw new ApiException(errorMessage, 0, "");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Unexpected error calling API: {ex.Message}";
                Console.WriteLine(errorMessage);
                throw new ApiException(errorMessage, 0, ex.ToString());
            }
        }

        public ChatConversation CreateConversation()
        {
            try
            {
                // Create a conversation object that will use our service
                var conversation = new ChatConversation(null);

                // Set up the conversation with configuration properties
                conversation.Model = _config.Model;
                conversation.RequestParameters.Temperature = _config.Temperature;
                conversation.RequestParameters.MaxTokens = _config.MaxTokens;

                // Inject our custom API handling
                conversation.ApiCall = async (request) => await CreateChatCompletionAsync(request);

                return conversation;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating conversation: {ex.Message}");
                throw;
            }
        }
    }
}
