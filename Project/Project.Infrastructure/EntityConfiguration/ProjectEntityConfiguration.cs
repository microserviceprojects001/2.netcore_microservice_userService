using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Project.Infrastructure.EntityConfiguration
{
    public class ProjectEntityConfiguration : IEntityTypeConfiguration<Project.Domain.AggregatesModel.Project>
    {
        public void Configure(EntityTypeBuilder<Project.Domain.AggregatesModel.Project> builder)
        {
            builder.ToTable("Projects");
            builder.HasKey(p => p.Id);

            // 必需字段
            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.Company)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(p => p.Introduction)
                .IsRequired();

            // 字符串类型可选字段
            builder.Property(p => p.Avatar)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(p => p.OriginBPFile)
                .HasMaxLength(512)
                .IsRequired(false);

            builder.Property(p => p.FormatBPFile)
                .HasMaxLength(512)
                .IsRequired(false);

            builder.Property(p => p.Provice)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.Property(p => p.City)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.Property(p => p.AreaName)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.Property(p => p.FinStage)
                .HasMaxLength(32)
                .IsRequired(false);

            builder.Property(p => p.UserName)
                .HasMaxLength(64)
                .IsRequired(false);

            builder.Property(p => p.Tags)
                .HasMaxLength(256)
                .IsRequired(false);

            builder.Property(p => p.FinPercentage)
                .HasMaxLength(20)
                .IsRequired(false);

            // 值类型字段 - 必须为必需字段，使用默认值
            builder.Property(p => p.ProvinceId)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.CityId)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.AreaId)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.FinMoney)
                .HasColumnType("decimal(18,2)")
                .IsRequired()
                .HasDefaultValue(0m);

            builder.Property(p => p.Income)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Revenue)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.Valuation)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(p => p.BrokerageOptions)
                .IsRequired()
                .HasDefaultValue(0);

            // 布尔类型字段
            builder.Property(p => p.ShowSecurityInfo)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(p => p.OnPlatform)
                .IsRequired()
                .HasDefaultValue(false);

            // 时间字段
            builder.Property(p => p.RegisterTime)
                .IsRequired()
                .HasDefaultValue(DateTime.MinValue);

            builder.Property(p => p.CreateTime)
                .IsRequired();

            builder.Property(p => p.UpdateTime)
                .IsRequired()  // 改为必需字段
                .HasDefaultValue(DateTime.MinValue);  // 设置默认值

            // 配置导航属性
            builder.HasMany(p => p.Properties)
                .WithOne()
                .HasForeignKey("ProjectId")
                .IsRequired(false);

            builder.HasOne(p => p.VisibleRule)
                .WithOne()
                .HasForeignKey<Project.Domain.AggregatesModel.ProjectVisibleRule>("ProjectId")
                .IsRequired(false);

            builder.HasMany(p => p.Contributors)
                .WithOne()
                .HasForeignKey("ProjectId")
                .IsRequired(false);

            builder.HasMany(p => p.Viewers)
                .WithOne()
                .HasForeignKey("ProjectId")
                .IsRequired(false);
        }
    }
}
// 方案1：删除 ProjectPropertyEntityConfiguration（推荐） 如果 
// ProjectProperty
//  是值对象
// 删除或注释掉 OwnsMany 配置

// builder.OwnsMany(p => p.Properties, prop =>
// {
//     prop.WithOwner().HasForeignKey("ProjectId");
//     prop.HasKey("Id");
//     prop.Property(p => p.Key).HasMaxLength(50);
//     prop.Property(p => p.Text).HasMaxLength(100);
//     prop.Property(p => p.Value).HasMaxLength(200);
// })