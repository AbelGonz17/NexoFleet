using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexoFleet.Domain.Payments;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence.Configurations;

internal sealed class PaymentCommentConfiguration : IEntityTypeConfiguration<PaymentComment>
{
    public void Configure(EntityTypeBuilder<PaymentComment> builder)
    {
        builder.ToTable("payment_comments");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Text).HasMaxLength(PaymentErrors.CommentMaxLength).IsRequired();
        builder.Property(comment => comment.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(comment => new { comment.PaymentReportId, comment.CreatedAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(comment => comment.AuthorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.Ignore(comment => comment.DomainEvents);
    }
}
