using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SampSharp.Documentation.NewModels
{
	public class DocsDbContext : DbContext
	{
		public DocsDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<DocVersion> DocVersions { get; set; }
		public DbSet<DocCategory> DocCategories { get; set; }
		public DbSet<DocArticle> DocArticles { get; set; }
		public DbSet<DocParagraph> DocParagraphs { get; set; }
		public DbSet<DocAsset> DocAssets { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<DocCategory>()
				.HasOne(a => a.Version)
				.WithMany(a => a.Categories)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<DocAsset>()
				.HasOne(a => a.Version)
				.WithMany(a => a.Assets)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<DocAsset>()
				.Property(x => x.Data)
				.HasColumnType("mediumblob");

			modelBuilder.Entity<DocArticle>()
				.HasOne(a => a.Category)
				.WithMany(a => a.Articles)
				.OnDelete(DeleteBehavior.Cascade);
			
			modelBuilder.Entity<DocArticle>()
				.Property(x => x.Introduction)
				.HasColumnType("longtext");
			
			modelBuilder.Entity<DocArticle>()
				.Property(x => x.Content)
				.HasColumnType("longtext");

			modelBuilder.Entity<DocParagraph>()
				.HasOne(a => a.Article)
				.WithMany(a => a.Paragraphs)
				.OnDelete(DeleteBehavior.Cascade);
		}
	}

	public class DocVersion
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public List<DocCategory> Categories { get; set; } = new List<DocCategory>();
		public List<DocAsset> Assets { get; set; } = new List<DocAsset>();
		public string Uri { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreationDate { get; set; }
	}
	
	public class DocAsset
	{
		public int Id { get; set; }
		public string Uri { get; set; }
		public DocVersion Version { get; set; }
		public byte[] Data { get; set; }
	}

	public class DocCategory
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Uri { get; set; }
		public List<DocArticle> Articles { get; set; } = new List<DocArticle>();
		public DocVersion Version { get; set; }
	}

	public class DocArticle
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string RedirectUrl { get; set; }
		public string RedirectPage { get; set; }
		public DateTime LastModification { get; set; }
		public string EditUrl { get; set; }
		public string Introduction { get; set; }
		public string Uri { get; set; }
		public string Content { get; set; }

		public List<DocParagraph> Paragraphs { get; set; } = new List<DocParagraph>();
		public DocCategory Category { get; set; }
	}

	public class DocParagraph
	{
		public int Id { get; set; }
		public  DocArticle Article { get; set; }//todo maybe remove?
		public string Name { get; set; }
		public string Uri { get; set; }
	}
}
