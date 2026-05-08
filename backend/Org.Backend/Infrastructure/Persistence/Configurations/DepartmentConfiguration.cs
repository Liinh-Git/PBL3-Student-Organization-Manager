using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Org.Backend.Domain.Entities;
using Org.Backend.Domain.Enums;

namespace Org.Backend.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(e => e.Id);
        
        // BaseEntity properties
        BaseEntityProperties.ConfigureBaseEntityProperties(builder);

        builder.Property(e => e.OrgId)
            .IsRequired();

        builder.Property(e => e.DeptName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.Function)
            .HasMaxLength(500);

        builder.Property(e => e.ManagerId)
            .IsRequired(false);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(e => e.OrgId);
        builder.HasIndex(e => e.ManagerId);
        builder.HasIndex(e => new { e.OrgId, e.Code });

        // Relationships
        builder.HasOne(e => e.Organization)
            .WithMany(e => e.Departments)
            .HasForeignKey(e => e.OrgId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Manager)
            .WithMany(e => e.ManagedDepartments)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Members)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.OwnedCategories)
            .WithOne(e => e.OwnerDepartment)
            .HasForeignKey(e => e.OwnerDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.AssignedTasks)
            .WithOne(e => e.Department)
            .HasForeignKey(e => e.DeptId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
