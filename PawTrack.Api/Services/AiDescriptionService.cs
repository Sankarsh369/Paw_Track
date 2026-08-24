using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.AiDescription;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class AiDescriptionService : IAiDescriptionService
    {
        private readonly PawTrackDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<AiDescriptionService> _logger;

        public AiDescriptionService(
            PawTrackDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration config,
            ILogger<AiDescriptionService> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        public async Task<AiAnimalDescriptionDto> GenerateDescriptionAsync(int animalId)
        {
            var animal = await _context.Animals
                .Include(a => a.Category)
                .Include(a => a.Branch)
                .FirstOrDefaultAsync(a => a.Id == animalId);

            if (animal is null)
            {
                throw new KeyNotFoundException($"Animal with ID {animalId} was not found.");
            }

            string modelVersion = _config["Gemini:ModelVersion"] ?? "gemini-2.5-flash";
            string apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") 
                            ?? _config["Gemini:ApiKey"] 
                            ?? string.Empty;

            string generatedText;

            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Contains("YOUR_GEMINI_API_KEY"))
            {
                _logger.LogInformation("No valid Gemini API key configured. Utilizing context-aware local template generator.");
                generatedText = GenerateFallbackDescription(animal);
            }
            else
            {
                try
                {
                    generatedText = await CallGeminiApiAsync(animal, apiKey, modelVersion);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed calling Gemini API. Falling back to local context generation.");
                    generatedText = GenerateFallbackDescription(animal);
                }
            }

            var aiDescription = new AiAnimalDescription
            {
                AnimalId = animal.Id,
                GeneratedText = generatedText,
                ModelVersion = modelVersion,
                Status = AiDescriptionStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            _context.AiAnimalDescriptions.Add(aiDescription);
            await _context.SaveChangesAsync();

            // Reload navigation properties for clean DTO mapping
            await _context.Entry(aiDescription).Reference(d => d.Animal).LoadAsync();

            return ToDto(aiDescription);
        }

        public async Task<AiAnimalDescriptionDto?> GetLatestByAnimalIdAsync(int animalId)
        {
            var description = await _context.AiAnimalDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => d.AnimalId == animalId)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync();

            return description is null ? null : ToDto(description);
        }

        public async Task<List<AiAnimalDescriptionDto>> GetHistoryByAnimalIdAsync(int animalId)
        {
            var descriptions = await _context.AiAnimalDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => d.AnimalId == animalId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return descriptions.Select(ToDto).ToList();
        }

        public async Task<AiAnimalDescriptionDto?> GetByIdAsync(int id)
        {
            var description = await _context.AiAnimalDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .FirstOrDefaultAsync(d => d.Id == id);

            return description is null ? null : ToDto(description);
        }

        public async Task<AiAnimalDescriptionDto?> UpdateDescriptionAsync(int id, UpdateAiAnimalDescriptionDto dto)
        {
            var description = await _context.AiAnimalDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (description is null) return null;

            description.GeneratedText = dto.GeneratedText;
            description.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ToDto(description);
        }

        public async Task<AiAnimalDescriptionDto?> ApproveDescriptionAsync(int id, int reviewerUserId, ApproveAiAnimalDescriptionDto dto)
        {
            var description = await _context.AiAnimalDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (description is null) return null;

            description.Status = dto.IsApproved ? AiDescriptionStatus.Approved : AiDescriptionStatus.Rejected;
            description.ReviewedById = reviewerUserId;
            description.ReviewedAt = DateTime.UtcNow;
            description.ReviewNotes = dto.ReviewNotes;
            description.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload reviewer user for DTO mapping
            await _context.Entry(description).Reference(d => d.ReviewedBy).LoadAsync();

            return ToDto(description);
        }

        private async Task<string> CallGeminiApiAsync(Animal animal, string apiKey, string modelVersion)
        {
            var client = _httpClientFactory.CreateClient();
            string endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{modelVersion}:generateContent?key={apiKey}";

            string prompt = $"Write a heartwarming, engaging, and accurate adoption bio for a rescue animal with the following details:\n" +
                            $"- Name: {animal.Name}\n" +
                            $"- Species: {animal.Species}\n" +
                            $"- Breed: {animal.Breed}\n" +
                            $"- Age: {animal.Age} years old\n" +
                            $"- Gender: {animal.Gender}\n" +
                            $"- Category: {animal.Category?.Name ?? "General"}\n" +
                            $"- Rescue Date: {animal.RescueDate:yyyy-MM-dd}\n" +
                            $"- Rescue Location: {animal.RescueLocation}\n" +
                            $"- Current Status: {animal.Status}\n\n" +
                            $"The description should highlight the animal's story, unique personality potential, and welcome potential adopters.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var response = await client.PostAsJsonAsync(endpoint, requestBody);
            response.EnsureSuccessStatusCode();

            var jsonResult = await response.Content.ReadFromJsonAsync<GeminiResponse>();
            string? text = jsonResult?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

            return string.IsNullOrWhiteSpace(text) ? GenerateFallbackDescription(animal) : text.Trim();
        }

        private static string GenerateFallbackDescription(Animal animal)
        {
            return $"Meet {animal.Name}, a lovely {animal.Age}-year-old {animal.Gender.ToString().ToLower()} {animal.Breed} {animal.Species.ToLower()}! " +
                   $"{animal.Name} was rescued from {animal.RescueLocation} on {animal.RescueDate:MMMM d, yyyy}. " +
                   $"Currently under shelter care ({animal.Status}), {animal.Name} is looking for a warm, loving home. " +
                   $"Whether playing outdoors or cuddling inside, {animal.Name} will bring immense joy and companion warmth to your family!";
        }

        private static AiAnimalDescriptionDto ToDto(AiAnimalDescription d) => new()
        {
            Id = d.Id,
            AnimalId = d.AnimalId,
            AnimalName = d.Animal?.Name ?? string.Empty,
            GeneratedText = d.GeneratedText,
            ModelVersion = d.ModelVersion,
            Status = d.Status.ToString(),
            ReviewedById = d.ReviewedById,
            ReviewerName = d.ReviewedBy?.Name,
            ReviewNotes = d.ReviewNotes,
            CreatedAt = d.CreatedAt,
            ReviewedAt = d.ReviewedAt,
            UpdatedAt = d.UpdatedAt
        };

        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }
        }

        private class GeminiContent
        {
            [JsonPropertyName("parts")]
            public List<GeminiPart>? Parts { get; set; }
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
