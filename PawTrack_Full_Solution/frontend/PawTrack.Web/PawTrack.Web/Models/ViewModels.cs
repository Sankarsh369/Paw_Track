namespace PawTrack.Web.Models
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class BranchModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RegionCity { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Phone { get; set; }
    }

    public class CategoryModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AnimalModel
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Species { get; set; } = string.Empty;
        public string Breed { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public DateTime RescueDate { get; set; }
        public string RescueLocation { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? MicrochipNumber { get; set; }
    }

    public class MedicalRecordModel
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public int VeterinarianId { get; set; }
        public string? VeterinarianName { get; set; }
        public string Diagnosis { get; set; } = string.Empty;
        public string? Treatment { get; set; }
        public string? Medication { get; set; }
        public DateTime? VaccinationDate { get; set; }
        public DateTime CheckupDate { get; set; }
    }

    public class BehaviorRecordModel
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? AssessedByName { get; set; }
        public string Temperament { get; set; } = string.Empty;
        public bool CompatibilityWithKids { get; set; }
        public bool CompatibilityWithPets { get; set; }
        public string? SpecialRequirements { get; set; }
        public DateTime AssessedAt { get; set; }
    }

    public class VisitSlotModel
    {
        public int Id { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public bool IsFull => BookedCount >= Capacity;
    }

    public class VisitBookingModel
    {
        public int Id { get; set; }
        public int VisitSlotId { get; set; }
        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? AdopterName { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime BookedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class AdoptionApplicationModel
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? AdopterName { get; set; }
        public int? VisitBookingId { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class AdoptionModel
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? AdopterName { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime AdoptionDate { get; set; }
    }

    public class PaymentModel
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string? DonorName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string? RazorpayOrderId { get; set; }
        public string? RazorpayKeyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }

    public class AiDescriptionModel
    {
        public int Id { get; set; }
        public int AnimalId { get; set; }
        public string? AnimalName { get; set; }
        public string GeneratedText { get; set; } = string.Empty;
        public string ModelVersion { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class ChatMessageModel
    {
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string MessageText { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }

    public class ChatConversationModel
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public bool HandedOffToStaff { get; set; }
        public List<ChatMessageModel> Messages { get; set; } = new();
    }

    public class BranchReportModel
    {
        public int BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalAnimals { get; set; }
        public int AvailableAnimals { get; set; }
        public int AdoptedAnimals { get; set; }
        public int PendingApplications { get; set; }
        public int VisitsThisMonth { get; set; }
        public decimal DonationTotal { get; set; }
        public decimal AdoptionFeeTotal { get; set; }
    }

    public class OrgDashboardModel
    {
        public int TotalBranches { get; set; }
        public int TotalAnimals { get; set; }
        public int TotalAdoptions { get; set; }
        public int TotalActiveApplications { get; set; }
        public decimal TotalDonations { get; set; }
        public decimal TotalAdoptionFees { get; set; }
        public List<BranchReportModel> ByBranch { get; set; } = new();
    }

    public class ApiError
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
    }
}
