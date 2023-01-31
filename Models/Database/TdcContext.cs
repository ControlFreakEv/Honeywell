using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace Honeywell.Database
{
    public class TdcContext : DbContext
    {
        public DbSet<DbTdcGraph> TdcGraphs { get; set; }
        public DbSet<DbTdcNode> TdcNodes { get; set; }
        public DbSet<DbTdcModule> TdcModules { get; set; }
        public DbSet<DbTdcTag> TdcTags { get; set; }
        public DbSet<DbTdcParameter> TdcParameters { get; set; }
        public DbSet<DbTdcCL> TdcCLs { get; set; }
        public DbSet<DbTdcCLRefs> TdcCLRefs { get; set; }
        public DbSet<DbTdcSourceCrossComm> SourceTdcCrossComms { get; set; }
        public DbSet<DbTdcTargetCrossComm> TargetTdcCrossComms { get; set; }
        public DbSet<DbTdcFileRef> TdcFileRefs { get; set; }
        public DbSet<DbProject> Projects { get; set; }
        public DbSet<DbConfigTemplates> ConfigTemplates { get; set; }
        public DbSet<DbExperionFunctionBlock> ExperionFunctionBlocks { get; set; }
        public DbSet<DbExperionParameter> ExperionParameters { get; set; }
        public DbSet<DbTdcConnections> ParameterConnections { get; set; }
        public static string ParserConnectionString { get; set; } = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TDC.db");

        public static string? ConnectionString { get; set; }
        public static bool Server { get; set; }

        public TdcContext(bool deleteExisting = false)
        {
            if (deleteExisting)
                Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                //ConnectionString = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "TDC.db");
                ConnectionString = ParserConnectionString;
                Server = false;
            }

            if (Server)
                options.UseSqlServer($"Data Source={ConnectionString}").UseLazyLoadingProxies();
            else
                options.UseSqlite($"Data Source={ConnectionString}").UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbTdcGraph>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Xml);
            });

            modelBuilder.Entity<DbTdcModule>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);

                entity.Property(e => e.Desc);

                entity.HasOne(d => d.ConfigTemplate)
               .WithMany(p => p.Modules)
               .HasForeignKey(d => d.ConfigTemplateId)
               .OnDelete(DeleteBehavior.SetNull)
               .IsRequired(false)
               .HasConstraintName($"FK_{nameof(DbTdcTag.Nodes)}_{nameof(DbTdcNode.Tag)}");
            });


            modelBuilder.Entity<DbTdcNode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.NodeId);

                entity.HasOne(d => d.Tag)
               .WithMany(p => p.Nodes)
               .HasForeignKey(d => d.TagId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false)
               .HasConstraintName($"FK_{nameof(DbTdcTag.Nodes)}_{nameof(DbTdcNode.Tag)}");

                entity.HasOne(d => d.Graph)
                .WithMany(p => p.Nodes)
                .HasForeignKey(d => d.GraphId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName($"FK_{nameof(DbTdcTag.Nodes)}_{nameof(DbTdcNode.Graph)}");

                entity.HasOne(d => d.Module)
                .WithMany(p => p.Nodes)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false)
                .HasConstraintName($"FK_{nameof(DbTdcTag.Nodes)}_{nameof(DbTdcNode.Module)}");
            });

            modelBuilder.Entity<DbTdcParameter>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);

                entity.Property(e => e.Value);

                entity.Property(e => e.RawValue);

                entity.HasOne(d => d.Tag)
                .WithMany(p => p.Params)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbTdcTag.Params)}_{nameof(DbTdcParameter.Tag)}");
            });

            modelBuilder.Entity<DbTdcTag>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);

                entity.Property(e => e.PointType);

                entity.Property(e => e.EbFile);

                entity.Property(e => e.Desc);

                entity.Property(e => e.PackageExists);

                entity.Property(e => e.AM);

                entity.Property(e => e.HWYNUM);

                entity.Property(e => e.BOXNUM);

                entity.Property(e => e.NTWKNUM);

                entity.Property(e => e.NODENUM);

                entity.Property(e => e.SLOTNUM);

                entity.Property(e => e.MODNUM);

                entity.Property(e => e.NODETYP);

                entity.Property(e => e.PVALGID);

                entity.Property(e => e.CTLALGID);

                entity.Property(e => e.LcnAddress);

                entity.HasOne(d => d.Project)
                .WithMany(p => p.Tags)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false)
                .HasConstraintName($"FK_{nameof(DbProject.Tags)}_{nameof(DbTdcTag.Project)}");
            });

            modelBuilder.Entity<DbTdcCL>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileName);

                entity.Property(e => e.OriginalContent);

                entity.Property(e => e.Content);

                entity.Property(e => e.Indicators);

                entity.HasOne(d => d.Project)
                .WithMany(p => p.CLs)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false)
                .HasConstraintName($"FK_{nameof(DbProject.CLs)}_{nameof(DbTdcCL.Project)}");
            });

            modelBuilder.Entity<DbTdcCLRefs>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TagId);

                entity.Property(e => e.CLId);

                entity.Property(e => e.CLAttachedToThisPoint);

                //entity.Property(e => e.PacakgeInstance).IsRequired(false);

                entity.HasOne(d => d.Tag)
                .WithMany(p => p.CLTagReferences)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbTdcTag.CLTagReferences)}_{nameof(DbTdcCLRefs.Tag)}");

                entity.HasOne(d => d.CL)
                .WithMany(p => p.CLTagReferences)
                .HasForeignKey(d => d.CLId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbTdcCL.CLTagReferences)}_{nameof(DbTdcCLRefs.CL)}");
            });

            modelBuilder.Entity<DbTdcSourceCrossComm>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.NodeId);

                entity.HasOne(d => d.Node)
               .WithMany(p => p.SourceCrossComms)
               .HasForeignKey(d => d.NodeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false)
               .HasConstraintName($"FK_{nameof(DbTdcNode.SourceCrossComms)}_{nameof(DbTdcSourceCrossComm.Node)}");
            });

            modelBuilder.Entity<DbTdcTargetCrossComm>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.NodeId);

                entity.HasOne(d => d.Node)
               .WithMany(p => p.TargetCrossComms)
               .HasForeignKey(d => d.NodeId)
               .OnDelete(DeleteBehavior.Cascade)
               .IsRequired(false)
               .HasConstraintName($"FK_{nameof(DbTdcNode.TargetCrossComms)}_{nameof(DbTdcTargetCrossComm.Node)}");
            });

            modelBuilder.Entity<DbTdcFileRef>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileType);

                entity.Property(e => e.Value);

                entity.HasOne(d => d.Tag)
                .WithMany(p => p.TdcFileRefs)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbTdcTag.TdcFileRefs)}_{nameof(DbTdcParameter.Tag)}");
            });

            modelBuilder.Entity<DbProject>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);
            });

            modelBuilder.Entity<DbConfigTemplates>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.TypicalName);
            });

            modelBuilder.Entity<DbExperionFunctionBlock>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);

                entity.HasOne(d => d.ConfigTemplate)
                .WithMany(p => p.FunctionBlocks)
                .HasForeignKey(d => d.ConfigTemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbExperionFunctionBlock.ConfigTemplate)}_{nameof(DbConfigTemplates.FunctionBlocks)}");
            });

            modelBuilder.Entity<DbExperionParameter>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Name);

                entity.Property(e => e.Value);

                entity.HasOne(d => d.FunctionBlock)
                .WithMany(p => p.Parameters)
                .HasForeignKey(d => d.FunctionBlockId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbExperionParameter.FunctionBlock)}_{nameof(DbTdcModule.ExperionParameters)}");
            });

            modelBuilder.Entity<DbTdcConnections>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.NodeId);

                entity.Property(e => e.Parameter);

                entity.Property(e => e.ConnectedNodeName);

                entity.Property(e => e.ConnectedNodeParameter);

                entity.HasOne(d => d.Node)
                .WithMany(p => p.ParameterConnections)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName($"FK_{nameof(DbTdcConnections.Node)}_{nameof(DbTdcNode.ParameterConnections)}");
            });
        }
    }
}