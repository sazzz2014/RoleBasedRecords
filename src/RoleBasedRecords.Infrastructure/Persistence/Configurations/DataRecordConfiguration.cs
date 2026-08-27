using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RoleBasedRecords.Domain.Entities;

namespace RoleBasedRecords.Infrastructure.Persistence.Configurations;

public sealed class DataRecordConfiguration : IEntityTypeConfiguration<DataRecord>
{
    public void Configure(EntityTypeBuilder<DataRecord> builder)
    {
        builder.ToTable("data_records");

        builder.HasKey(record => record.Id)
            .HasName("pk_data_records");

        builder.Property(record => record.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(record => record.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(record => record.PublicDescription)
            .HasColumnName("public_description")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(record => record.InternalComment)
            .HasColumnName("internal_comment")
            .HasColumnType("text");

        builder.Property(record => record.CostPrice)
            .HasColumnName("cost_price")
            .HasPrecision(18, 2);

        builder.Property(record => record.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(record => record.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(record => record.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_data_records_users_created_by_user_id");

        builder.HasIndex(record => record.CreatedAt)
            .HasDatabaseName("ix_data_records_created_at");
    }
}
