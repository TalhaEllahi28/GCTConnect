using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GCTConnect.Models;

public partial class GctConnectContext : DbContext
{
    public GctConnectContext()
    {
    }

    public GctConnectContext(DbContextOptions<GctConnectContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<ChatbotQuery> ChatbotQueries { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<File> Files { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<GroupMember> GroupMembers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserProfile> UserProfiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-1VNNIKF;Database=GCT Connect;Trusted_Connection=True; TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.HasKey(e => e.AnnouncementId).HasName("PK__Announce__C640A82DA184780E");

            entity.Property(e => e.AnnouncementId).HasColumnName("announcement_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");

            entity.HasOne(d => d.Department).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_AnnouncementDepartment");
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PK__Batches__DBFC04319DEBEA2C");

            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.BatchYear).HasColumnName("batch_year");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.StudentsCount)
                .HasDefaultValue(0)
                .HasColumnName("students_count");

            entity.HasOne(d => d.Department).WithMany(p => p.Batches)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Department");
        });

        modelBuilder.Entity<ChatbotQuery>(entity =>
        {
            entity.HasKey(e => e.QueryId).HasName("PK__Chatbot___E793E349F035136C");

            entity.ToTable("Chatbot_Queries");

            entity.Property(e => e.QueryId).HasColumnName("query_id");
            entity.Property(e => e.QueryText).HasColumnName("query_text");
            entity.Property(e => e.Response).HasColumnName("response");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.ChatbotQueries)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_QueryUser");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__C223242258F5AE52");

            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.HodId).HasColumnName("hod_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Hod).WithMany(p => p.Departments)
                .HasForeignKey(d => d.HodId)
                .HasConstraintName("FK_HOD");
        });

        modelBuilder.Entity<File>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Files__07D884C61A79B769");

            entity.Property(e => e.FileId).HasColumnName("file_id");
            entity.Property(e => e.FileName)
                .HasMaxLength(255)
                .HasColumnName("file_name");
            entity.Property(e => e.FilePath)
                .HasMaxLength(255)
                .HasColumnName("file_path");
            entity.Property(e => e.FileType)
                .HasMaxLength(50)
                .HasColumnName("file_type");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("uploaded_at");

            entity.HasOne(d => d.Group).WithMany(p => p.Files)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileGroup");

            entity.HasOne(d => d.Sender).WithMany(p => p.Files)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FileSender");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.GroupId).HasName("PK__Groups__D57795A035C1DB83");

            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.GroupName)
                .HasMaxLength(255)
                .HasColumnName("group_name");
            entity.Property(e => e.IsPrivate)
                .HasDefaultValue(true)
                .HasColumnName("is_private");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Groups)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CreatedBy");

            entity.HasOne(d => d.Department).WithMany(p => p.Groups)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_DepartmentGroup");
        });

        modelBuilder.Entity<GroupMember>(entity =>
        {
            entity.HasKey(e => e.GroupMemberId).HasName("PK__Group_Me__F3C66B8C53A4C74E");

            entity.ToTable("Group_Members");

            entity.Property(e => e.GroupMemberId).HasColumnName("group_member_id");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("joined_at");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Group).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("FK_Group");

            entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserGroup");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__0BBF6EE673D7BE26");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.GroupId).HasColumnName("group_id");
            entity.Property(e => e.IsCurrentUser).HasDefaultValue(false);
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Group).WithMany(p => p.Messages)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MessageGroup");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MessageSender");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__B9BE370F126AE019");

            entity.HasIndex(e => e.RollNumber, "UQ_Student_RollNumber")
                .IsUnique()
                .HasFilter("([role]='Students')");

            entity.HasIndex(e => e.Email, "UQ__Users__AB6E616444ACF4BF").IsUnique();

            entity.HasIndex(e => e.Username, "UQ__Users__F3DBC572DE004BF3").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BatchId).HasColumnName("batch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Gender).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.ProfilePic)
                .HasMaxLength(255)
                .HasColumnName("profile_pic");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.RollNumber)
                .HasMaxLength(50)
                .HasColumnName("roll_number");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");

            entity.HasMany(d => d.Batches).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserBatch",
                    r => r.HasOne<Batch>().WithMany()
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Batch"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_User"),
                    j =>
                    {
                        j.HasKey("UserId", "BatchId").HasName("PK__User_Bat__B401F74CFFC9DA6B");
                        j.ToTable("User_Batches");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("BatchId").HasColumnName("batch_id");
                    });
        });

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User_Pro__B9BE370FD5AE57D1");

            entity.ToTable("User_Profiles");

            entity.Property(e => e.UserId)
                .ValueGeneratedNever()
                .HasColumnName("user_id");
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.SocialLinks).HasColumnName("social_links");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.User).WithOne(p => p.UserProfile)
                .HasForeignKey<UserProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserProfile");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
