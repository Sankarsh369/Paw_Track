using PawTrack.Core.Entities;
using PawTrack.Core.Enums;
using PawTrack.Services;
using Xunit;

namespace PawTrack.Tests;

public class StateMachineTests
{
    private readonly User _branchAdmin1 = new() { Id = 2, Name = "John Manager", Role = UserRole.BranchAdmin, BranchId = 1 };

    [Fact]
    public async Task EnforceStateMachine_ReApprovingAlreadyApprovedApplication_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        // App #1 is already Approved in seed dataset
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            approvalService.ApproveApplicationAsync(1, _branchAdmin1));

        Assert.Contains("Application is no longer pending", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task EnforceStateMachine_ApprovingRejectedApplication_ThrowsInvalidOperationException()
    {
        using var context = TestDbContextFactory.Create();
        var approvalService = new ApprovalService(context);

        // Reject App #3 first
        await approvalService.RejectApplicationAsync(3, "Invalid references provided.", _branchAdmin1);

        // Attempting to approve rejected application must fail
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            approvalService.ApproveApplicationAsync(3, _branchAdmin1));

        Assert.Contains("Application is no longer pending", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
