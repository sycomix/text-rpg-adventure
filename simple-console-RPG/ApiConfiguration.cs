using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace simple_console_RPG
{
    public class ApiConfiguration
    {
        public string ApiKey { get; set; } = "";
        public string BaseUrl { get; set; } = "http://localhost:8080/v1";
        public string Model { get; set; } = "gpt-3.5-turbo";

        // Additional configuration options
        public float Temperature { get; set; } = 0.7f;
        public int MaxTokens { get; set; } = 800;
        public bool StreamResponse { get; set; } = false;
        public int TimeoutSeconds { get; set; } = 60;

        [JsonIgnore]
        public bool IsLocalModel => !BaseUrl.Contains("openai.com");

        public static ApiConfiguration LoadFromFile(string filePath = "apisettings.json")
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
                    var config = JsonSerializer.Deserialize<ApiConfiguration>(json, options);
                    return config ?? CreateDefaultConfig();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading API configuration: {ex.Message}");
                    Console.WriteLine("Using default configuration instead.");
                }
            }
            else
            {
                Console.WriteLine($"Configuration file '{filePath}' not found.");
            }

            // If file doesn't exist or there's an error, try to load from apikey.txt for backward compatibility
            if (File.Exists("apikey.txt"))
            {
                Console.WriteLine("Using legacy apikey.txt configuration.");
                return new ApiConfiguration
                {
                    ApiKey = File.ReadAllText("apikey.txt").Trim(),
                    BaseUrl = "https://api.openai.com/v1", // Default to OpenAI API
                    Model = "gpt-3.5-turbo"
                };
            }

            Console.WriteLine("No configuration found. Using default settings.");
            return CreateDefaultConfig();
        }

        public void SaveToFile(string filePath = "apisettings.json")
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, json);
                Console.WriteLine($"Configuration saved to {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving configuration: {ex.Message}");
            }
        }

        private static ApiConfiguration CreateDefaultConfig()
        {
            return new ApiConfiguration
            {
                ApiKey = "",
                BaseUrl = "http://localhost:8080/v1",
                Model = "gpt-3.5-turbo",
                Temperature = 0.7f,
                MaxTokens = 800,
                StreamResponse = false,
                TimeoutSeconds = 60
            };
        }
    }
}
