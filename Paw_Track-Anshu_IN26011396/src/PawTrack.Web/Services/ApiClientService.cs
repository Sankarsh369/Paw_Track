using System.Net.Http.Json;
using PawTrack.Core.DTOs;
using PawTrack.Core.Entities;
using PawTrack.Core.Enums;

namespace PawTrack.Web.Services;

public class ApiClientService
{
    private readonly HttpClient _http;

    public int CurrentUserId { get; set; } = 2; // Default John Manager (Branch 1 Admin)
    public UserRole CurrentUserRole { get; set; } = UserRole.BranchAdmin;
    public int? CurrentUserBranchId { get; set; } = 1;

    public event Action? OnUserChanged;

    public ApiClientService(HttpClient http)
    {
        _http = http;
    }

    public void SwitchUser(int userId, UserRole role, int? branchId)
    {
        CurrentUserId = userId;
        CurrentUserRole = role;
        CurrentUserBranchId = branchId;
        OnUserChanged?.Invoke();
    }

    private void SetAuthHeaders(HttpRequestMessage request)
    {
        request.Headers.Add("X-User-Id", CurrentUserId.ToString());
        request.Headers.Add("X-User-Role", CurrentUserRole.ToString());
        if (CurrentUserBranchId.HasValue)
        {
            request.Headers.Add("X-Branch-Id", CurrentUserBranchId.Value.ToString());
        }
    }

    public async Task<ApiResponse<AdoptionApplication>?> SubmitApplicationAsync(SubmitApplicationDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/adoption/applications")
        {
            Content = JsonContent.Create(dto)
        };
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<AdoptionApplication>>();
    }

    public async Task<ApiResponse<List<ApplicationResponseDto>>?> GetApplicationsAsync(ApplicationStatus? status = null, int? branchId = null)
    {
        var url = "api/adoption/applications";
        var query = new List<string>();
        if (status.HasValue) query.Add($"status={status.Value}");
        if (branchId.HasValue) query.Add($"branchId={branchId.Value}");
        if (query.Count > 0) url += "?" + string.Join("&", query);

        var req = new HttpRequestMessage(HttpMethod.Get, url);
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<List<ApplicationResponseDto>>>();
    }

    public async Task<ApiResponse<Adoption>?> ApproveApplicationAsync(int applicationId)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"api/adoption/applications/{applicationId}/approve");
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<Adoption>>();
    }

    public async Task<ApiResponse?> RejectApplicationAsync(int applicationId, string reason)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, $"api/adoption/applications/{applicationId}/reject")
        {
            Content = JsonContent.Create(new RejectApplicationDto { RejectionReason = reason })
        };
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse>();
    }

    public async Task<ApiResponse<FollowUp>?> CreateFollowUpAsync(ScheduleFollowUpDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/followup")
        {
            Content = JsonContent.Create(dto)
        };
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<FollowUp>>();
    }

    public async Task<ApiResponse<FollowUp>?> CompleteFollowUpAsync(int followUpId, string? notesUpdate)
    {
        var req = new HttpRequestMessage(HttpMethod.Put, $"api/followup/{followUpId}/complete")
        {
            Content = JsonContent.Create(new CompleteFollowUpDto { NotesUpdate = notesUpdate })
        };
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<FollowUp>>();
    }

    public async Task<ApiResponse<List<AdoptionResponseDto>>?> GetAdoptionsAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/followup/adoptions");
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<List<AdoptionResponseDto>>>();
    }

    public async Task<ApiResponse<List<VisitBookingResponseDto>>?> GetVisitBookingsAsync()
    {
        var req = new HttpRequestMessage(HttpMethod.Get, "api/visit/bookings");
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<List<VisitBookingResponseDto>>>();
    }

    public async Task<ApiResponse<List<VisitSlotResponseDto>>?> GetVisitSlotsAsync(int branchId)
    {
        var req = new HttpRequestMessage(HttpMethod.Get, $"api/visit/slots/{branchId}");
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<List<VisitSlotResponseDto>>>();
    }

    public async Task<ApiResponse<VisitBooking>?> BookVisitAsync(BookVisitDto dto)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "api/visit/book")
        {
            Content = JsonContent.Create(dto)
        };
        SetAuthHeaders(req);
        var res = await _http.SendAsync(req);
        return await res.Content.ReadFromJsonAsync<ApiResponse<VisitBooking>>();
    }
}
