using Dating_WebAPI.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

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
        }
    }
}