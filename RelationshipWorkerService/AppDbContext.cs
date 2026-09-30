using Microsoft.EntityFrameworkCore;
using RelationshipWorkerService.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelationshipWorkerService
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { 
        }

        public DbSet<Prompt> Prompts => Set<Prompt>();
        public DbSet<User> Users => Set<User>();
        public DbSet<PromptResponse> PromptResponses => Set<PromptResponse>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Prompt>().ToTable("prompts");
            modelBuilder.Entity<PromptResponse>().ToTable("prompt_responses");

        }
    }
}