using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.AI;
using PawTrack.Api.Models;
using System.Text;
using System.Text.Json;

namespace PawTrack.Api.Services
{
    public class AiDescriptionService : IAiDescriptionService
    {
        private readonly PawTrackDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AiDescriptionService(PawTrackDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public AiDescriptionService(PawTrackDbContext context)
            : this(context, new HttpClient(), new ConfigurationBuilder().AddEnvironmentVariables().Build())
        {
        }

        public async Task<AIGeneratedDescriptionDto> GenerateAsync(GenerateDescriptionDto dto)
        {
            var animal = await _context.Animals
                .Include(a => a.Category)
                .FirstOrDefaultAsync(a => a.Id == dto.AnimalId);

            if (animal is null)
                throw new KeyNotFoundException($"Animal with Id {dto.AnimalId} was not found.");

            var behavior = await _context.BehaviorRecords
                .Where(b => b.AnimalId == dto.AnimalId)
                .OrderByDescending(b => b.AssessedAt)
                .FirstOrDefaultAsync();

            var generatedText = await CallGeminiApiAsync(animal, behavior);

            var description = new AIGeneratedDescription
            {
                AnimalId = animal.Id,
                GeneratedText = generatedText,
                ModelVersion = "gemini-2.5-flash",
                IsApproved = false
            };

            _context.AIGeneratedDescriptions.Add(description);
            await _context.SaveChangesAsync();

            return ToDto(description, animal.Name);
        }

        public async Task<List<AIGeneratedDescriptionDto>> GetPendingAsync()
        {
            var list = await _context.AIGeneratedDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => !d.IsApproved)
                .OrderByDescending(d => d.GeneratedAt)
                .ToListAsync();

            return list.Select(d => ToDto(d, null)).ToList();
        }

        public async Task<AIGeneratedDescriptionDto?> GetForAnimalAsync(int animalId)
        {
            var description = await _context.AIGeneratedDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => d.AnimalId == animalId && d.IsApproved)
                .OrderByDescending(d => d.GeneratedAt)
                .FirstOrDefaultAsync();

            return description is null ? null : ToDto(description, null);
        }

        public async Task<AIGeneratedDescriptionDto?> ReviewAsync(int id, int reviewerId, ReviewDescriptionDto dto)
        {
            var description = await _context.AIGeneratedDescriptions
                .Include(d => d.Animal)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (description is null) return null;

            description.GeneratedText = dto.FinalText;
            description.IsApproved = dto.Approve;
            description.ReviewedById = reviewerId;

            await _context.SaveChangesAsync();
            await _context.Entry(description).Reference(d => d.ReviewedBy).LoadAsync();

            return ToDto(description, null);
        }

        private async Task<string> CallGeminiApiAsync(Animal animal, BehaviorRecord? behavior)
        {
            var apiKey = _configuration["Gemini:ApiKey"]
                         ?? _configuration["GEMINI_API_KEY"]
                         ?? _configuration["GeminiApiKey"]
                         ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured. Please set 'Gemini:ApiKey' in configuration or 'GEMINI_API_KEY' environment variable.");
            }

            var promptText = BuildGeminiPrompt(animal, behavior);

            var requestPayload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = promptText }
                        }
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";
            var response = await _httpClient.PostAsync(requestUrl, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Gemini API request failed with status code {response.StatusCode}: {errorResponse}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            if (doc.RootElement.TryGetProperty("candidates", out var candidates) &&
                candidates.GetArrayLength() > 0 &&
                candidates[0].TryGetProperty("content", out var content) &&
                content.TryGetProperty("parts", out var parts) &&
                parts.GetArrayLength() > 0 &&
                parts[0].TryGetProperty("text", out var textProp))
            {
                return textProp.GetString()?.Trim() ?? string.Empty;
            }

            throw new InvalidOperationException("Failed to parse a valid response text from Gemini API.");
        }

        private static string BuildGeminiPrompt(Animal animal, BehaviorRecord? behavior)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are an expert shelter staff writer crafting warm, engaging, and compelling adoption descriptions for rescue animals at PawTrack.");
            sb.AppendLine("Write a friendly, heart-warming 2 to 3 paragraph adoption bio for the following rescue animal:");
            sb.AppendLine($"- Name: {animal.Name}");
            sb.AppendLine($"- Species: {animal.Species}");
            sb.AppendLine($"- Breed: {animal.Breed}");
            sb.AppendLine($"- Age: {animal.Age} years old");
            sb.AppendLine($"- Gender: {animal.Gender}");
            sb.AppendLine($"- Rescue Location: {animal.RescueLocation}");

            if (animal.Category != null)
            {
                sb.AppendLine($"- Category: {animal.Category.Name}");
            }

            if (behavior != null)
            {
                sb.AppendLine("Behavior & Personality Information:");
                if (!string.IsNullOrWhiteSpace(behavior.Temperament))
                {
                    sb.AppendLine($"  - Temperament: {behavior.Temperament}");
                }
                sb.AppendLine($"  - Good with kids: {(behavior.CompatibilityWithKids ? "Yes" : "No / Preferred home without young kids")}");
                sb.AppendLine($"  - Good with other pets: {(behavior.CompatibilityWithPets ? "Yes" : "No / Preferred as single pet")}");
                if (!string.IsNullOrWhiteSpace(behavior.Notes))
                {
                    sb.AppendLine($"  - Additional Notes: {behavior.Notes}");
                }
            }

            sb.AppendLine("\nRequirements:");
            sb.AppendLine("- Return ONLY the adoption description text. Do not include markdown headers or meta-commentary.");
            sb.AppendLine("- Highlight their personality traits and paint a vivid picture of them in a loving home.");
            sb.AppendLine("- Include a warm call-to-action encouraging adopters to meet them.");

            return sb.ToString();
        }

        private static AIGeneratedDescriptionDto ToDto(AIGeneratedDescription d, string? animalName) => new()
        {
            Id = d.Id,
            AnimalId = d.AnimalId,
            AnimalName = animalName ?? d.Animal?.Name,
            GeneratedText = d.GeneratedText,
            ModelVersion = d.ModelVersion,
            IsApproved = d.IsApproved,
            ReviewedByName = d.ReviewedBy?.Name,
            GeneratedAt = d.GeneratedAt
        };
    }
}
