using ChatGPT_API.Entity;
using ChatGPT_API.Entity.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatGPT_API.Context;

public class DbLinkContext : DbContext
{
    public DbLinkContext()
    {
    }

    public DbLinkContext(DbContextOptions<DbLinkContext> options) : base(options)
    {
    }

    public DbSet<UserEntity> User { get; set; }
    public DbSet<HistoryRecordEntity> HistoryRecord { get; set; }
    public DbSet<HistoryMessagesEntity> HistoryMessages { get; set; }

    // public DbSet<CodeGradeEntity> CodeGrade { get; set; }
    public DbSet<ActivationCodeEntity> ActivationCode { get; set; }
}