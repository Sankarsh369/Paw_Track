using PawTrack.Core.Entities;

namespace PawTrack.Core.Interfaces;

public interface IApprovalService
{
    Task<Adoption> ApproveApplicationAsync(int applicationId, User currentUser);
    Task RejectApplicationAsync(int applicationId, string rejectionReason, User currentUser);
}
