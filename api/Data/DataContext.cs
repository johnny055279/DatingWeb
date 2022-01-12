using System;
using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Dating_WebAPI.Data
{
    // 使用自訂的IdentityUser、IdentityRole，這些東西會產生Identity系列的資料表。
    // 寫在IdentityDbContext<>裡面的東西就可以不用在另外宣告DbSet
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
                                                 IdentityUserClaim<int>, AppUserRole,
                                                 IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        // 名字要跟DataBase裡面一樣，不然會抓不到!!

        public DbSet<UserLike> Likes { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Connection> Connections { get; set; }

        // 覆寫DbContext的function，以自訂建立Model的參數條件
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 不加這一段Migration會出錯。
            base.OnModelCreating(modelBuilder);

            // 每個都 User 可以有許多相關聯 Roles ，而且每個都 Role 可以與許多相關聯 Users 。
            // 這是多對多關聯性，需要資料庫中的聯結資料表。 聯結資料表是由 UserRole 實體表示。
            modelBuilder.Entity<AppUser>().HasMany(n => n.UserRoles).WithOne(n => n.User).HasForeignKey(n => n.UserId).IsRequired();
            modelBuilder.Entity<AppRole>().HasMany(n => n.UserRoles).WithOne(n => n.Role).HasForeignKey(n => n.RoleId).IsRequired();

            // 建立複合式主鍵
            modelBuilder.Entity<UserLike>().HasKey(key => new { key.SourceUserId, key.LikeUserId });

            // 建立資料表關聯性，這裡為多對多。使用MSSQL的時候一個OnDelete要是NoAction，不然Migration會報錯。
            modelBuilder.Entity<UserLike>().HasOne(n => n.SourceUser).WithMany(n => n.LikedUsers).HasForeignKey(n => n.SourceUserId).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserLike>().HasOne(n => n.LikeUser).WithMany(n => n.LikeByUser).HasForeignKey(n => n.LikeUserId).OnDelete(DeleteBehavior.Cascade);

            // 設定當資料刪除時，另一個相關聯的FK會變成null，用此來控制Message發送與接收方是否為單一刪除或是兩邊都刪除。
            modelBuilder.Entity<Message>().HasOne(n => n.Recipient).WithMany(n => n.MessagesReceived).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>().HasOne(n => n.Sender).WithMany(n => n.MessagesSent).OnDelete(DeleteBehavior.Restrict);

            // allow datacontext using utc format...(一個微軟挖的坑，有大神幫忙修正)
            modelBuilder.ApplyUtcDateTimeConverter();
        }
    }

    // fix datetime not showing utc format (from microsoft git hub solution)
    public static class UtcDateAnnotation
    {
        private const String IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
          new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
          new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
          builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
          ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}