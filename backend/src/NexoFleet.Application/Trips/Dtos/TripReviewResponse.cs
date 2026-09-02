using NexoFleet.Domain.Trips;

namespace NexoFleet.Application.Trips.Dtos;

public sealed record TripReviewResponse(
    Guid Id,
    Guid TripId,
    Guid CompanyId,
    Guid ReviewerUserId,
    string Decision,
    string? Comments,
    DateTimeOffset ReviewedAtUtc)
{
    public static TripReviewResponse FromDomain(TripReview review) =>
        new(
            review.Id,
            review.TripId,
            review.CompanyId,
            review.ReviewerUserId,
            review.Decision.ToString(),
            review.Comments,
            review.ReviewedAtUtc);
}
