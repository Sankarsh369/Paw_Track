using Microsoft.EntityFrameworkCore;
using PawTrack.Api.Data;
using PawTrack.Api.DTOs.AI;
using PawTrack.Api.Models;

namespace PawTrack.Api.Services
{
    public class AiDescriptionService : IAiDescriptionService
    {
        private readonly PawTrackDbContext _context;

        public AiDescriptionService(PawTrackDbContext context)
        {
            _context = context;
        }

        // NOTE: This uses a template-based generator so the module works fully offline
        // with no external API key required. To use a real LLM instead, swap the body
        // of BuildDraftText() for a call to your provider of choice (e.g. the Anthropic
        // Messages API) — everything else (approval workflow, storage) stays the same.
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

            var description = new AIGeneratedDescription
            {
                AnimalId = animal.Id,
                GeneratedText = BuildDraftText(animal, behavior),
                ModelVersion = "pawtrack-template-v1",
                IsApproved = false
            };

            _context.AIGeneratedDescriptions.Add(description);
            await _context.SaveChangesAsync();

            return ToDto(description, animal.Name);
        }

        public async Task<List<AIGeneratedDescriptionDto>> GetPendingAsync()
        {
            return await _context.AIGeneratedDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => !d.IsApproved)
                .OrderByDescending(d => d.GeneratedAt)
                .Select(d => ToDto(d))
                .ToListAsync();
        }

        public async Task<AIGeneratedDescriptionDto?> GetForAnimalAsync(int animalId)
        {
            var description = await _context.AIGeneratedDescriptions
                .Include(d => d.Animal)
                .Include(d => d.ReviewedBy)
                .Where(d => d.AnimalId == animalId && d.IsApproved)
                .OrderByDescending(d => d.GeneratedAt)
                .FirstOrDefaultAsync();

            return description is null ? null : ToDto(description);
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

            return ToDto(description);
        }

        private static string BuildDraftText(Animal animal, BehaviorRecord? behavior)
        {
            var pronoun = animal.Gender == Gender.Male ? "He" : animal.Gender == Gender.Female ? "She" : "They";
            var temperament = behavior?.Temperament?.ToLower() is string t && !string.IsNullOrWhiteSpace(t)
                ? t
                : "sweet and easygoing";

            var kidsLine = behavior is null ? ""
                : behavior.CompatibilityWithKids ? " gets along well with kids"
                : " does best in a home without young children";

            var petsLine = behavior is null ? ""
                : behavior.CompatibilityWithPets ? " and is comfortable around other pets"
                : " and prefers to be the only pet in the home";

            return $"Meet {animal.Name}, a {animal.Age}-year-old {animal.Breed} {animal.Species.ToLower()} " +
                   $"rescued from {animal.RescueLocation}. {pronoun} is {temperament}{kidsLine}{petsLine}. " +
                   $"{animal.Name} is looking for a patient, loving family to call {pronoun.ToLower()} own — " +
                   $"could that be you?";
        }

        private static AIGeneratedDescriptionDto ToDto(AIGeneratedDescription d, string? animalName = null) => new()
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
