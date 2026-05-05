using HotelVoC.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelVoC.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<FeedbackSource> FeedbackSources => Set<FeedbackSource>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<SentimentResult> SentimentResults => Set<SentimentResult>();
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Insight> Insights => Set<Insight>();
    public DbSet<DailyReport> DailyReports => Set<DailyReport>();
}