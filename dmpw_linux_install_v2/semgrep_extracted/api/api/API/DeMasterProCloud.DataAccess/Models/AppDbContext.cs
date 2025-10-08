using Microsoft.EntityFrameworkCore;
using System;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("User ID=postgres;Password=postgres;Server=localhost;Port=5432;Database=demasterpro_v3;Pooling=true;");
            }
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }

        [DbFunction("dec_a", "pdb")]
        public string Dec(string text)
        {
            throw new NotSupportedException();
        }

        [DbFunction("enc_a", "pdb")]
        public string Enc(string text)
        {
            throw new NotSupportedException();
        }


        [DbFunction("dec", "pdb")]
        public string Dec(string text1, string text2, string text3, int integer1)
        {
            throw new NotSupportedException();
        }

        [DbFunction("enc", "pdb")]
        public string Enc(string text1, string text2, string text3)
        {
            throw new NotSupportedException();
        }

        //public string Dec(string text) => throw new NotSupportedException();
        //public string Enc(string text) => throw new NotSupportedException();

        public virtual DbSet<SystemInfo> SystemInfo { get; set; }
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<Company> Company { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<EventLog> EventLog { get; set; }
        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Holiday> Holiday { get; set; }
        public virtual DbSet<IcuDevice> IcuDevice { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<SystemLog> SystemLog { get; set; }
        public virtual DbSet<AccessTime> AccessTime { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<AccessGroup> AccessGroup { get; set; }
        public virtual DbSet<AccessGroupDevice> AccessGroupDevice { get; set; }
        public virtual DbSet<UnregistedDevice> UnregistedDevice { get; set; }
        public virtual DbSet<Building> Building { get; set; }
        public virtual DbSet<Visit> Visit { get; set; }


        public virtual DbSet<Card> Card { get; set; }

        public virtual DbSet<WorkingType> WorkingType { get; set; }
        public virtual DbSet<Attendance> Attendance { get; set; }
        public virtual DbSet<AttendanceLeave> AttendanceLeave { get; set; }
        public virtual DbSet<AttendanceSetting> AttendanceSetting { get; set; }

        public virtual DbSet<PlugIn> PlugIn { get; set; }
        public virtual DbSet<VisitSetting> VisitSetting { get; set; }
        public virtual DbSet<AccessSetting> AccessSetting { get; set; }

        public virtual DbSet<VisitHistory> VisitHistory { get; set; }
        
        public virtual DbSet<DynamicRole> DynamicRole { get; set; }
        public virtual DbSet<CompanyAccount> CompanyAccount { get; set; }
        public virtual DbSet<Camera> Camera { get; set; }
        
        public virtual DbSet<Vehicle> Vehicle { get; set; }
        public virtual DbSet<Face> Face { get; set; }
        public virtual DbSet<Notification> Notification { get; set; }

        public virtual DbSet<ShortenLink> ShortenLink { get; set; }
        public virtual DbSet<LeaveRequestSetting> LeaveRequestSetting { get; set; }
        public virtual DbSet<FingerPrint> FingerPrint { get; set; }

        public virtual DbSet<HeaderSetting> HeaderSetting { get; set; }

        public virtual DbSet<DataListSetting> DataListSetting { get; set; }
        
        // Card Issuing
        public virtual DbSet<AttendanceLeaveRequest> AttendanceLeaveRequest { get; set; }
        public virtual DbSet<FirmwareVersion> FirmwareVersion { get; set; }
        
        // Department Level
        public virtual DbSet<DepartmentDevice> DepartmentDevice { get; set; }
        public virtual DbSet<NationalIdCard> NationalIdCard { get; set; }

        public virtual DbSet<DeviceReader> DeviceReader { get; set; }
        // Access Schedule
        public virtual DbSet<AccessSchedule> AccessSchedule { get; set; }
        public virtual DbSet<AccessWorkShift> AccessWorkShift { get; set; }
        public virtual DbSet<WorkShift> WorkShift { get; set; }
        public virtual DbSet<UserAccessSchedule> UserAccessSchedule { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(Dec), new[] { typeof(string) }));
            modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(Enc), new[] { typeof(string) }));

            modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(Dec), new[] { typeof(string), typeof(string), typeof(string), typeof(int) }));
            modelBuilder.HasDbFunction(typeof(AppDbContext).GetMethod(nameof(Enc), new[] { typeof(string), typeof(string), typeof(string) }));

            modelBuilder.Entity<SystemInfo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.ParentId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Building)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Building_Company");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Building_Building");

            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.CompanyId);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();


            });
            modelBuilder.Entity<AccessGroup>(entity =>
            {
                //entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.IsDefault);
                entity.Property(e => e.IsDeleted);
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.AccessGroup)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessGroup_Company");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Child)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_AccessGroup_AccessGroup");
            });

            modelBuilder.Entity<AccessGroupDevice>(entity =>
            {
                entity.HasKey(e => new { e.AccessGroupId, e.IcuId });

                entity.HasIndex(e => e.AccessGroupId);
                entity.HasIndex(e => e.IcuId);

                entity.HasOne(d => d.Icu)
                    .WithMany(p => p.AccessGroupDevice)
                    .HasForeignKey(d => d.IcuId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessGroupDevice_IcuDevice");

                entity.HasOne(d => d.AccessGroup)
                    .WithMany(p => p.AccessGroupDevice)
                    .HasForeignKey(d => d.AccessGroupId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessGroupDevice_User");

                entity.HasOne(d => d.Tz)
                    .WithMany(p => p.AccessGroupDevice)
                    .HasForeignKey(d => d.TzId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AccessGroupDevice_AccessTime");

                // entity.HasOne(d => d.ElevatorFloor)
                //     .WithMany(p => p.AccessGroupDevice)
                //     .HasForeignKey(d => d.ElevatorFloorId)
                //     .OnDelete(DeleteBehavior.ClientSetNull)
                //     .HasConstraintName("FK_AccessGroupDevice_ElevatorFloor");
            });

            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.DynamicRoleId);
                entity.Property(e => e.LoginConfig).IsRequired(false);

                entity.HasOne(d => d.DynamicRole)
                    .WithMany(p => p.Account)
                    .HasForeignKey(d => d.DynamicRoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Account_DynamicRole");

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RefreshToken)
                    .HasMaxLength(256);

                entity.HasIndex(e => e.CreateDateRefreshToken);
                entity.Property(e => e.DeviceToken).IsRequired(false);
            });

            modelBuilder.Entity<Company>(entity =>
            {
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Contact).HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.EventLogStorageDurationInDb).HasDefaultValue(24);
                entity.Property(e => e.EventLogStorageDurationInFile).HasDefaultValue(24);
                entity.Property(e => e.TimeRecheckAttendance).HasDefaultValue(15);
                entity.Property(e => e.TimeLimitStoredImage).HasDefaultValue(365);
                entity.Property(e => e.TimeLimitStoredVideo).HasDefaultValue(365);

                entity.Property(e => e.Remarks).HasColumnType("character varying");
            });

            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.ParentId);

                entity.Property(e => e.DepartName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.DepartNo)
                    .IsRequired()
                    .HasColumnType("character varying");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Department)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Department_Company");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.InverseParent)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Department_Department");

                entity.HasOne(d => d.DepartmentManager)
                    .WithMany(p => p.Department)
                    .HasForeignKey(d => d.DepartmentManagerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Department_Account");

            });

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<EventLog>(entity =>
            {
                entity.HasIndex(e => e.Id);
                //entity.HasKey(e => e.CardId)
                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.IcuId);

                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.Antipass).HasMaxLength(10);

                entity.Property(e => e.CardId).HasMaxLength(50);

                entity.Property(e => e.DoorName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.KeyPadPw).HasMaxLength(256);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.EventLog)
                    .HasForeignKey(d => d.CompanyId)
                    .HasConstraintName("FK_EventLog_Company");

                entity.HasOne(d => d.Icu)
                    .WithMany(p => p.EventLog)
                    .HasForeignKey(d => d.IcuId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EventLog_IcuDevice");

                entity.HasOne(d => d.Camera)
                    .WithMany(p => p.EventLog)
                    .HasForeignKey(d => d.CameraId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EventLog_Camera");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.EventLog)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EventLog_User");

                entity.HasOne(d => d.Visit)
                    .WithMany(p => p.EventLog)
                    .HasForeignKey(d => d.VisitId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_EventLog_Visit");

                entity.HasIndex(p => new { p.CompanyId, p.IcuId, p.EventTime, p.CardId }).IsUnique();
            });

            modelBuilder.Entity<Holiday>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Remarks).HasColumnType("character varying");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Holiday)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Holiday_Company");
            });

            modelBuilder.Entity<IcuDevice>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.DeviceAddress).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IpAddress).HasColumnType("character varying");
                entity.Property(e => e.MacAddress).HasMaxLength(20);
                entity.Property(e => e.ServerIp).HasMaxLength(50);
                entity.Property(e => e.ServerPort).HasMaxLength(8);
                entity.Property(e => e.FirmwareVersion).HasMaxLength(100);
                entity.Property(e => e.VersionReader0).HasMaxLength(40);
                entity.Property(e => e.VersionReader1).HasMaxLength(40);
                entity.Property(e => e.NfcModuleVersion).HasMaxLength(50);
                //entity.Property(e => e.ExtraVersion).HasMaxLength(20);
                entity.Property(e => e.DoorStatus).HasMaxLength(20);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.IcuDevice)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_IcuDevice_Company");

                entity.HasOne(d => d.Building)
                    .WithMany(p => p.IcuDevice)
                    .HasForeignKey(d => d.BuildingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_IcuDevice_Building");

                entity.HasOne(d => d.ActiveTz)
                    .WithMany(p => p.DoorActive)
                    .HasForeignKey(d => d.ActiveTzId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_IcuDevice_AccessTime1");

                entity.HasOne(d => d.PassageTz)
                    .WithMany(p => p.DoorPassage)
                    .HasForeignKey(d => d.PassageTzId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_IcuDevice_AccessTime");

            });

            modelBuilder.Entity<Setting>(entity =>
            {
                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Value).HasColumnType("character varying");
            });

            modelBuilder.Entity<SystemLog>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.CreatedBy);

                entity.Property(e => e.Content).HasColumnType("character varying");

                entity.Property(e => e.ContentDetails).HasColumnType("character varying");

                entity.Property(e => e.ContentIds).HasColumnType("character varying");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.SystemLog)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SystemLog_Company");

                entity.HasOne(d => d.CreatedByNavigation)
                    .WithMany(p => p.SystemLog)
                    .HasForeignKey(d => d.CreatedBy)
                    .HasConstraintName("FK_SystemLog_Account");
            });

            modelBuilder.Entity<AccessTime>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.FriTime1).HasMaxLength(50);

                entity.Property(e => e.FriTime2).HasMaxLength(50);

                entity.Property(e => e.FriTime3).HasMaxLength(50);

                entity.Property(e => e.HolType1Time1).HasMaxLength(50);

                entity.Property(e => e.HolType1Time2).HasMaxLength(50);

                entity.Property(e => e.HolType1Time3).HasMaxLength(50);

                entity.Property(e => e.HolType2Time1).HasMaxLength(50);

                entity.Property(e => e.HolType2Time2).HasMaxLength(50);

                entity.Property(e => e.HolType2Time3).HasMaxLength(50);

                entity.Property(e => e.HolType3Time1).HasMaxLength(50);

                entity.Property(e => e.HolType3Time2).HasMaxLength(50);

                entity.Property(e => e.HolType3Time3).HasMaxLength(50);

                entity.Property(e => e.MonTime1).HasMaxLength(50);

                entity.Property(e => e.MonTime2).HasMaxLength(50);

                entity.Property(e => e.MonTime3).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Remarks).HasColumnType("character varying");

                entity.Property(e => e.SatTime1).HasMaxLength(50);

                entity.Property(e => e.SatTime2).HasMaxLength(50);

                entity.Property(e => e.SatTime3).HasMaxLength(50);

                entity.Property(e => e.SunTime1).HasMaxLength(50);

                entity.Property(e => e.SunTime2).HasMaxLength(50);

                entity.Property(e => e.SunTime3).HasMaxLength(50);

                entity.Property(e => e.ThurTime1).HasMaxLength(50);

                entity.Property(e => e.ThurTime2).HasMaxLength(50);

                entity.Property(e => e.ThurTime3).HasMaxLength(50);

                entity.Property(e => e.TueTime1).HasMaxLength(50);

                entity.Property(e => e.TueTime2).HasMaxLength(50);

                entity.Property(e => e.TueTime3).HasMaxLength(50);

                entity.Property(e => e.WedTime1).HasMaxLength(50);

                entity.Property(e => e.WedTime2).HasMaxLength(50);

                entity.Property(e => e.WedTime3).HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.AccessTime)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessTime_Company");
            });



            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasIndex(e => e.DepartmentId);

                entity.Property(e => e.Address).HasMaxLength(100);

                //entity.Property(e => e.CardId).HasMaxLength(50);

                entity.Property(e => e.City).HasMaxLength(100);

                entity.Property(e => e.FirstName).HasMaxLength(200);

                entity.Property(e => e.HomePhone).HasMaxLength(20);

                entity.Property(e => e.Job).HasMaxLength(100);

                entity.Property(e => e.KeyPadPw).HasMaxLength(256);

                entity.Property(e => e.LastName).HasMaxLength(100);

                entity.Property(e => e.Nationality).HasMaxLength(100);

                entity.Property(e => e.OfficePhone).HasMaxLength(20);

                entity.Property(e => e.Position).HasMaxLength(100);

                entity.Property(e => e.PostCode).HasMaxLength(20);

                entity.Property(e => e.Remarks).HasColumnType("character varying");

                entity.Property(e => e.Responsibility).HasMaxLength(100);
                entity.Property(e => e.AliasDataInfo).IsRequired(false);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Company1");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Department");

                entity.HasOne(d => d.AccessGroup)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.AccessGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_AccessGroup");

                entity.HasOne(d => d.WorkingType)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.WorkingTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_WorkingType");

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.User)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_User_Account");

                entity.HasOne(e => e.NationalIdCard)
                    .WithOne(e => e.User)
                    .HasForeignKey<NationalIdCard>(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_User_NationalIdCard")
                    .IsRequired(false);
            });

            modelBuilder.Entity<UnregistedDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);

                entity.Property(e => e.DeviceAddress).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.IpAddress).IsRequired().HasMaxLength(50);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.UnregistedDevice)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UnregistedDevice_Company");
            });

            //KJO - Visit 20190624
            modelBuilder.Entity<Visit>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CardId).HasMaxLength(50);

                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.Property(e => e.InvitePhone).HasMaxLength(30);

                entity.Property(e => e.Position).HasMaxLength(100);
                //entity.Property(e => e.VisitorName).IsRequired().HasMaxLength(255);

                //entity.Property(e => e.IsDeleted);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Visit)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Visit_Company");

                entity.HasOne(d => d.AccessGroup)
                    .WithMany(p => p.Visit)
                    .HasForeignKey(d => d.AccessGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Visit_AccessGroup");

                entity.HasOne(e => e.NationalIdCard)
                    .WithOne(e => e.Visit)
                    .HasForeignKey<NationalIdCard>(e => e.VisitId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Visit_NationalIdCard")
                    .IsRequired(false);
            });

            //KJO - Card 20190701
            modelBuilder.Entity<Card>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CardId).HasMaxLength(50);


                //entity.Property(e => e.VisitorName).IsRequired().HasMaxLength(255);

                //entity.Property(e => e.IsDeleted);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.Card)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Card_Company");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Card)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Card_User");

                entity.HasOne(d => d.Visit)
                    .WithMany(p => p.Card)
                    .HasForeignKey(d => d.VisitId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Card_Visit");
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Type).IsRequired();
                //entity.Property(e => e.Start).IsRequired();
                //entity.Property(e => e.End).IsRequired();

                entity.Property(e => e.Date).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Attendance)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Attendance_Company");

            });

            modelBuilder.Entity<AttendanceLeave>(entity =>
            {
                entity.HasIndex(e => e.UserId);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Start).IsRequired();
                entity.Property(e => e.End).IsRequired();

                entity.Property(e => e.Date).IsRequired();

                entity.HasOne(d => d.User)
                    .WithMany(p => p.AttendanceLeave)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AttendanceLeave_Company");

            });

            modelBuilder.Entity<WorkingType>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.Property(e => e.Name).HasMaxLength(100);

                entity.Property(e => e.WorkingDay).HasColumnType("jsonb");
                entity.Property(e => e.LunchTime).HasColumnType("jsonb");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.WorkingType)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_WorkingType_Company");
            });

            modelBuilder.Entity<AttendanceSetting>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.ApproverAccounts).HasColumnType("jsonb");

                entity.Property(e => e.InReaders).HasColumnType("jsonb");
                entity.Property(e => e.OutReaders).HasColumnType("jsonb");

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.AttendanceSetting)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AttendanceSetting_Company");

            });

            modelBuilder.Entity<PlugIn>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.PlugIns).HasColumnType("jsonb");

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.PlugIn)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Solution_Company");

            });

            modelBuilder.Entity<VisitSetting>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.FirstApproverAccounts).HasColumnType("jsonb");
                entity.Property(e => e.SecondsApproverAccounts).HasColumnType("jsonb");
                entity.Property(e => e.PersonalInvitationLink).HasDefaultValue("register-visit");
                entity.Property(e => e.ListVisitPurpose).IsRequired(false);

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.VisitSetting)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_VisitSetting_Company");

            });

            modelBuilder.Entity<AccessSetting>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.Property(e => e.FirstApproverAccounts).HasColumnType("jsonb");
                entity.Property(e => e.SecondApproverAccounts).HasColumnType("jsonb");

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.AccessSetting)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessSetting_Company");

                //entity.HasOne(d => d.AccessTime)
                //    .WithMany(p => p.AccessSetting)
                //    .HasForeignKey(d => d.AccessTimeId)
                //    .OnDelete(DeleteBehavior.Cascade)
                //    .HasConstraintName("FK_AccessSetting_AccessTime");
            });

            modelBuilder.Entity<VisitHistory>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.VisitHistory)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_VisitHistory_Company");
            });

            modelBuilder.Entity<DynamicRole>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.Property(e => e.Description).IsRequired(false);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.DynamicRole)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Role_Company");
            });

            modelBuilder.Entity<Face>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasIndex(m => m.Id);
                entity.HasIndex(m => m.CompanyId);
                entity.HasIndex(m => m.UserId);
                entity.Property(m => m.Id).ValueGeneratedOnAdd();

                entity.HasOne(m => m.User)
                   .WithMany(p => p.Face)
                   .HasForeignKey(d => d.UserId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_Face_User");

                entity.HasOne(m => m.Company)
                   .WithMany(p => p.Face)
                   .HasForeignKey(d => d.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_Face_Company");
            });


            modelBuilder.Entity<HeaderSetting>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.HeaderSetting)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Header_Company");


                entity.HasIndex(e => e.AccountId);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.HeaderSetting)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Header_Account");
            });

            modelBuilder.Entity<CompanyAccount>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.AccountId);
                entity.HasIndex(e => e.DynamicRoleId);

                entity.HasOne(d => d.Company)
                   .WithMany(p => p.CompanyAccount)
                   .HasForeignKey(d => d.CompanyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CompanyAccount_Company");

                entity.HasOne(d => d.Account)
                   .WithMany(p => p.CompanyAccount)
                   .HasForeignKey(d => d.AccountId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CompanyAccount_Account");

                entity.HasOne(d => d.DynamicRole)
                   .WithMany(p => p.CompanyAccount)
                   .HasForeignKey(d => d.DynamicRoleId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("FK_CompanyAccount_DynamicRole");

                entity.HasIndex(p => new { p.CompanyId, p.AccountId }).IsUnique();

            });

            modelBuilder.Entity<Camera>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.CompanyId);
                entity.HasIndex(e => e.IcuDeviceId);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.HasIndex(e => new { e.Name }).IsUnique();
                entity.HasIndex(e => new { e.CameraId }).IsUnique();
                entity.HasIndex(e => new { e.UrlStream }).IsUnique(false);
                entity.HasIndex(e => new { e.VmsUrlStream }).IsUnique(false);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.VisitId);
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Vehicle)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Vehicle_User");

                entity.HasOne(d => d.Visit)
                    .WithMany(p => p.Vehicle)
                    .HasForeignKey(d => d.VisitId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Vehicle_Visit");
            });

            modelBuilder.Entity<DataListSetting>(entity =>
            {
                entity.HasIndex(e => e.CompanyId);

                entity.HasOne(d => d.Company)
                    .WithMany(p => p.DataListSetting)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Header_Company");

                entity.HasIndex(e => e.AccountId);

                entity.HasOne(d => d.Account)
                    .WithMany(p => p.DataListSetting)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Header_Account");
            });

            modelBuilder.Entity<ShortenLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasIndex(e => e.FullPath).IsUnique();
                entity.HasIndex(e => e.ShortPath).IsUnique();
                entity.Property(e => e.LocationOrigin).IsRequired();
            });

            modelBuilder.Entity<LeaveRequestSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Company)
                    .WithOne(p => p.LeaveRequestSetting)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_LeaveRequestSetting_Company");
            });

            modelBuilder.Entity<AttendanceLeaveRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Attendance)
                    .WithMany(p => p.AttendanceLeaveRequest)
                    .HasForeignKey(e => e.AttendanceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AttendanceLeaveRequest_Attendance");

                entity.HasOne(d => d.AttendanceLeave)
                    .WithMany(p => p.AttendanceLeaveRequest)
                    .HasForeignKey(e => e.AttendanceLeaveId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AttendanceLeaveRequest_AttendanceLeave");
            });

            modelBuilder.Entity<FingerPrint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Card)
                    .WithMany(p => p.FingerPrint)
                    .HasForeignKey(e => e.CardId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_FingerPrint_Card");
            });

            modelBuilder.Entity<FirmwareVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Version).IsRequired();
                entity.Property(e => e.FileName).IsRequired();
                entity.Property(e => e.LinkFile).IsRequired();
            });

            modelBuilder.Entity<DepartmentDevice>(entity =>
            {
                entity.HasKey(e => new { e.DepartmentId, e.IcuId });

                entity.HasIndex(e => e.DepartmentId);
                entity.HasIndex(e => e.IcuId);

                entity.HasOne(d => d.Icu)
                    .WithMany(p => p.DepartmentDevice)
                    .HasForeignKey(d => d.IcuId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DepartmentDevice_IcuDevice");

                entity.HasOne(d => d.Department)
                    .WithMany(p => p.DepartmentDevice)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DepartmentDevice_Department");
            });

            modelBuilder.Entity<NationalIdCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CCCD).IsRequired(false);
                entity.Property(e => e.FullName).IsRequired(false);
                entity.Property(e => e.Sex).IsRequired(false);
                entity.Property(e => e.Nationality).IsRequired(false);
                entity.Property(e => e.Nation).IsRequired(false);
                entity.Property(e => e.Religion).IsRequired(false);
                entity.Property(e => e.District).IsRequired(false);
                entity.Property(e => e.Address).IsRequired(false);
                entity.Property(e => e.IdentityCharacter).IsRequired(false);
                entity.Property(e => e.FatherName).IsRequired(false);
                entity.Property(e => e.MotherName).IsRequired(false);
                entity.Property(e => e.HusbandOrWifeName).IsRequired(false);
                entity.Property(e => e.CMND).IsRequired(false);
                entity.Property(e => e.Avatar).IsRequired(false);
            });

            modelBuilder.Entity<DeviceReader>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired(false);
                entity.Property(e => e.IpAddress).IsRequired(false);
                entity.Property(e => e.MacAddress).IsRequired(false);

                entity.HasOne(d => d.IcuDevice)
                    .WithMany(p => p.DeviceReader)
                    .HasForeignKey(d => d.IcuDeviceId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_DeviceReader_IcuDevice");
            });

            // Access Schedule
            modelBuilder.Entity<AccessWorkShift>(entity =>
            {
                entity.HasKey(e => new { e.AccessScheduleId, e.WorkShiftId });
                entity.HasOne(d => d.AccessSchedule)
                    .WithMany(p => p.AccessWorkShift)
                    .HasForeignKey(d => d.AccessScheduleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessWorkShift_AccessSchedule");

                entity.HasOne(d => d.WorkShift)
                    .WithMany(p => p.AccessWorkShift)
                    .HasForeignKey(d => d.WorkShiftId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessWorkShift_WorkShift");
            });
            modelBuilder.Entity<UserAccessSchedule>(entity =>
            {
                entity.HasKey(e => new { e.AccessScheduleId, e.UserId });
                entity.HasOne(d => d.AccessSchedule)
                    .WithMany(p => p.UserAccessSchedule)
                    .HasForeignKey(d => d.AccessScheduleId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UserAccessSchedule_AccessSchedule");
                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserAccessSchedule)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_UserAccessSchedule_User");
            });

            // Access Schedule
            modelBuilder.Entity<AccessSchedule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Content).IsRequired(false);
                entity.Property(e => e.DoorIds).IsRequired(false);
                entity.HasOne(d => d.Department)
                    .WithMany(p => p.AccessSchedule)
                    .HasForeignKey(d => d.DepartmentId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessSchedule_Department");
                entity.HasOne(d => d.Company)
                    .WithMany(p => p.AccessSchedule)
                    .HasForeignKey(d => d.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AccessSchedule_Company");

            });
            modelBuilder.Entity<WorkShift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name).IsRequired(false);
            });

        }
    }
}