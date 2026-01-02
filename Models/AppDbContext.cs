using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASQL_Online_Exam_.DTO;

namespace ASQL_Online_Exam_.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Branch> Branches { get; set; } = null!;
        public virtual DbSet<Choice> Choices { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<ExamDetail> ExamDetails { get; set; } = null!;
        public virtual DbSet<ExamQuestionsPool> ExamQuestionsPools { get; set; } = null!;
        public virtual DbSet<Instructor> Instructors { get; set; } = null!;
        public virtual DbSet<ModelAnswer> ModelAnswers { get; set; } = null!;
        public virtual DbSet<Question> Questions { get; set; } = null!;
        public virtual DbSet<Student> Students { get; set; } = null!;
        public virtual DbSet<StudentExam> StudentExams { get; set; } = null!;
        public virtual DbSet<Teach> Teaches { get; set; } = null!;
        public virtual DbSet<Topic> Topics { get; set; } = null!;
        public virtual DbSet<Track> Tracks { get; set; } = null!;

        public virtual DbSet<SpResultDTO> SpResults { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=DESKTOP-KA8DFBB;Database=AdvancedSQLExamSystem;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("Branch");

                entity.Property(e => e.BranchId).HasColumnName("branchId");

                entity.Property(e => e.BranchLocation)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("branchLocation");

                entity.Property(e => e.BranchName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("branchName");

                entity.HasMany(d => d.Tracks)
                    .WithMany(p => p.Branches)
                    .UsingEntity<Dictionary<string, object>>(
                        "BranchTrack",
                        l => l.HasOne<Track>().WithMany().HasForeignKey("TrackId").HasConstraintName("FK__branchTra__track__3C69FB99"),
                        r => r.HasOne<Branch>().WithMany().HasForeignKey("BranchId").HasConstraintName("FK__branchTra__branc__3B75D760"),
                        j =>
                        {
                            j.HasKey("BranchId", "TrackId").HasName("PK__branchTr__4045E2CA6A3181BD");

                            j.ToTable("branchTracks");

                            j.IndexerProperty<int>("BranchId").HasColumnName("branchId");

                            j.IndexerProperty<int>("TrackId").HasColumnName("trackId");
                        });
            });

            modelBuilder.Entity<SpResultDTO>().HasNoKey();
            base.OnModelCreating(modelBuilder);



            modelBuilder.Entity<Choice>(entity =>
            {
                entity.ToTable("choice");

                entity.Property(e => e.ChoiceId).HasColumnName("choiceId");

                entity.Property(e => e.ChoiceText)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("choiceText");

                entity.Property(e => e.QuestionId).HasColumnName("questionId");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.Choices)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__choice__question__619B8048");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.Property(e => e.CourseId).HasColumnName("courseId");

                entity.Property(e => e.CourseCode)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("courseCode");

                entity.Property(e => e.CourseDiscription)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("courseDiscription");

                entity.Property(e => e.CourseName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("courseName");
            });

            modelBuilder.Entity<ExamDetail>(entity =>
            {
                entity.HasKey(e => e.ExamId)
                    .HasName("PK__examDeta__A56D125F4A7AA058");

                entity.ToTable("examDetails");

                entity.Property(e => e.ExamId).HasColumnName("examId");

                entity.Property(e => e.CourseId).HasColumnName("courseID");

                entity.Property(e => e.ExamDate)
                    .HasColumnType("date")
                    .HasColumnName("examDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.ExamDescription)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("examDescription");

                entity.Property(e => e.ExamDuration).HasColumnName("examDuration");

                entity.Property(e => e.ExamTitle)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("examTitle");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.ExamDetails)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__examDetai__cours__534D60F1");
            });

            modelBuilder.Entity<ExamQuestionsPool>(entity =>
            {
                entity.HasKey(e => new { e.ExamQuestionsPool1, e.ExamId, e.QuestionId, e.StudentId })
                    .HasName("PK__examQues__2550ED3E96C6CB95");

                entity.ToTable("examQuestionsPool");

                entity.Property(e => e.ExamQuestionsPool1)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("examQuestionsPool");

                entity.Property(e => e.ExamId).HasColumnName("examId");

                entity.Property(e => e.QuestionId).HasColumnName("questionId");

                entity.Property(e => e.StudentId).HasColumnName("studentId");

                entity.Property(e => e.ChoiceAnswerId).HasColumnName("choiceAnswerId");

                entity.HasOne(d => d.ChoiceAnswer)
                    .WithMany(p => p.ExamQuestionsPools)
                    .HasForeignKey(d => d.ChoiceAnswerId)
                    .HasConstraintName("FK__examQuest__choic__6B24EA82");

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.ExamQuestionsPools)
                    .HasForeignKey(d => d.ExamId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__examQuest__examI__693CA210");

                entity.HasOne(d => d.Question)
                    .WithMany(p => p.ExamQuestionsPools)
                    .HasForeignKey(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__examQuest__quest__6A30C649");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ExamQuestionsPools)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__examQuest__stude__6C190EBB");
            });

            modelBuilder.Entity<Instructor>(entity =>
            {
                entity.ToTable("Instructor");

                entity.HasIndex(e => e.InstructorSocialSecurityNumber, "UQ__Instruct__71F5B655B4AE1908")
                    .IsUnique();

                entity.Property(e => e.InstructorId).HasColumnName("instructorId");

                entity.Property(e => e.InstructorActive)
                    .HasColumnName("instructorActive")
                    .HasDefaultValueSql("((1))");

                entity.Property(e => e.InstructorBranchId).HasColumnName("instructorBranchId");

                entity.Property(e => e.InstructorEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("instructorEmail");

                entity.Property(e => e.InstructorName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("instructorName");

                entity.Property(e => e.InstructorPasswordHased)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("instructorPasswordHased");

                entity.Property(e => e.InstructorPhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("instructorPhoneNumber");

                entity.Property(e => e.InstructorSocialSecurityNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("instructorSocialSecurityNumber");

                entity.Property(e => e.InstructorhireDate)
                    .HasColumnType("date")
                    .HasColumnName("instructorhireDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.IsntructorPasswordSalt)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("isntructorPasswordSalt");

                entity.HasOne(d => d.InstructorBranch)
                    .WithMany(p => p.Instructors)
                    .HasForeignKey(d => d.InstructorBranchId)
                    .HasConstraintName("FK__Instructo__instr__4222D4EF");
            });

            modelBuilder.Entity<ModelAnswer>(entity =>
            {
                entity.HasKey(e => new { e.QuestionId, e.ChoiceId })
                    .HasName("PK__modelAns__03546FA09A6979D8");

                entity.ToTable("modelAnswer");

                entity.HasIndex(e => e.QuestionId, "UQ__modelAns__6238D49376525A4E")
                    .IsUnique();

                entity.Property(e => e.QuestionId).HasColumnName("questionID");

                entity.Property(e => e.ChoiceId).HasColumnName("choiceId");

                entity.HasOne(d => d.Choice)
                    .WithMany(p => p.ModelAnswers)
                    .HasForeignKey(d => d.ChoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__modelAnsw__choic__66603565");

                entity.HasOne(d => d.Question)
                    .WithOne(p => p.ModelAnswer)
                    .HasForeignKey<ModelAnswer>(d => d.QuestionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__modelAnsw__quest__656C112C");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("question");

                entity.Property(e => e.QuestionId).HasColumnName("questionId");

                entity.Property(e => e.CourseId).HasColumnName("courseId");

                entity.Property(e => e.QuestionGrade).HasColumnName("questionGrade");

                entity.Property(e => e.QuestionText)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("questionText");

                entity.Property(e => e.QuestionType).HasColumnName("questionType");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Questions)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__question__course__5EBF139D");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student");

                entity.HasIndex(e => e.StudentSocialSecurityNumber, "UQ__Student__846036914CA095B6")
                    .IsUnique();

                entity.Property(e => e.StudentId).HasColumnName("studentID");

                entity.Property(e => e.StudentEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("studentEmail");

                entity.Property(e => e.StudentEnrollementDate)
                    .HasColumnType("date")
                    .HasColumnName("studentEnrollementDate")
                    .HasDefaultValueSql("(getdate())");

                entity.Property(e => e.StudentName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("studentName");

                entity.Property(e => e.StudentPasswordHashed)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("studentPasswordHased");

                entity.Property(e => e.StudentPasswordSalt)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("studentPasswordSalt");

                entity.Property(e => e.StudentPhoneNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("studentPhoneNumber");

                entity.Property(e => e.StudentSocialSecurityNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("studentSocialSecurityNumber");

                entity.HasOne(d => d.Track)
                    .WithMany(p => p.Students)
                    .HasForeignKey(d => d.TrackId)
                    .HasConstraintName("FK__Student__TrackId__46E78A0C");
            });

            modelBuilder.Entity<StudentExam>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.ExamId })
                    .HasName("PK__studentE__A74707195C302268");

                entity.ToTable("studentExam");

                entity.Property(e => e.StudentId).HasColumnName("studentId");

                entity.Property(e => e.ExamId).HasColumnName("examId");

                entity.Property(e => e.Grade).HasColumnName("grade");

                entity.HasOne(d => d.Exam)
                    .WithMany(p => p.StudentExams)
                    .HasForeignKey(d => d.ExamId)
                    .HasConstraintName("FK__studentEx__examI__571DF1D5");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.StudentExams)
                    .HasForeignKey(d => d.StudentId)
                    .HasConstraintName("FK__studentEx__stude__5629CD9C");
            });

            modelBuilder.Entity<Teach>(entity =>
            {
                entity.HasKey(e => new { e.InstructorId, e.CourseId, e.TeachingYear })
                    .HasName("PK__teaches__DD8A02C2369B9A8E");

                entity.ToTable("teaches");

                entity.Property(e => e.InstructorId).HasColumnName("instructorId");

                entity.Property(e => e.CourseId).HasColumnName("courseId");

                entity.Property(e => e.TeachingYear)
                    .HasColumnType("date")
                    .HasColumnName("teachingYear")
                    .HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Teaches)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__teaches__courseI__5BE2A6F2");

                entity.HasOne(d => d.Instructor)
                    .WithMany(p => p.Teaches)
                    .HasForeignKey(d => d.InstructorId)
                    .HasConstraintName("FK__teaches__instruc__5AEE82B9");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.ToTable("Topic");

                entity.Property(e => e.TopicId).HasColumnName("topicId");

                entity.Property(e => e.CourseId).HasColumnName("courseId");

                entity.Property(e => e.TopicName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("topicName");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Topics)
                    .HasForeignKey(d => d.CourseId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Topic__courseId__4F7CD00D");
            });

            modelBuilder.Entity<Track>(entity =>
            {
                entity.ToTable("Track");

                entity.Property(e => e.TrackId).HasColumnName("trackId");

                entity.Property(e => e.TrackName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("trackName");

                entity.HasMany(d => d.Courses)
                    .WithMany(p => p.Tracks)
                    .UsingEntity<Dictionary<string, object>>(
                        "TrackCourse",
                        l => l.HasOne<Course>().WithMany().HasForeignKey("CourseId").HasConstraintName("FK__trackCour__cours__4CA06362"),
                        r => r.HasOne<Track>().WithMany().HasForeignKey("TrackId").HasConstraintName("FK__trackCour__track__4BAC3F29"),
                        j =>
                        {
                            j.HasKey("TrackId", "CourseId").HasName("PK__trackCou__571F7DAF4EA6AF1A");

                            j.ToTable("trackCourse");

                            j.IndexerProperty<int>("TrackId").HasColumnName("trackId");

                            j.IndexerProperty<int>("CourseId").HasColumnName("courseId");
                        });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
