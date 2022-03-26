using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using DocPluck.Parser;
using DocPluck.Reflection;
using DocPluck.XmlInfo;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Formatting = Newtonsoft.Json.Formatting;
using WriteState = Newtonsoft.Json.WriteState;

namespace SampSharp.DocX.Data
{
	public class ApiDocumentationImporter
	{
		private readonly DocsDbContext _dbContext;
		private readonly HttpClient _httpClient = new HttpClient();

		private string[] GetPackageIds()
		{
			// TODO: By settings

			return new []
			{
				"SampSharp.Entities"
			};
		}

		private async Task<IEnumerable<AvailablePackage>> DiscoverPackages()
		{
			var packages = new List<AvailablePackage>();

			foreach (var id in GetPackageIds())
			{
				var url = $@"http://nuget.timpotze.nl/api/v2/FindPackagesById()?id='{id}'&semVerLevel=2.0.0";

				await using var stream = await _httpClient.GetStreamAsync(url);

				var document = new XmlDocument();
				document.Load(stream);
				var nsManager = new XmlNamespaceManager(document.NameTable);
				nsManager.AddNamespace("a", "http://www.w3.org/2005/Atom");
				nsManager.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
				nsManager.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");

				foreach (XmlElement entry in document.DocumentElement.SelectNodes("//a:entry", nsManager))
				{
					var name = entry["title"].InnerText;
					var version = entry.SelectSingleNode("m:properties/d:Version", nsManager).InnerText;
					var src = entry["content"].GetAttribute("src");

					packages.Add(new AvailablePackage
					{
						Name = name,
						Version = version,
						DownloadUrl = src
					});
				}
			}

			return packages;
		}

		private readonly DocsParser _parser = new DocsParser();

		public ApiDocumentationImporter(DocsDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task ImportAllFromNuGet()
		{
			foreach (var package in await DiscoverPackages())
			{
				await ParsePackage(package);
			}
		}

		private async Task ParsePackage(AvailablePackage package)
		{
			await using var stream = await _httpClient.GetStreamAsync(package.DownloadUrl);

			var parsed = _parser.ParseFromNugetPackage(stream);
			var parsedAssembly = parsed.ParsedAssemblies.FirstOrDefault().Value;

			var ver = Version.Parse(package.Version);
			ver = new Version(ver.Major, ver.Minor, ver.Build, 0);

			JsonSerializerSettings set = new JsonSerializerSettings();
			set.Formatting = Formatting.Indented;
			set.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			set.NullValueHandling = NullValueHandling.Ignore;
	
			var x = new JsonApiWriter();
			set.Converters.Add(x);
			set.Converters.Add(new JsonDocWriter());
			
			foreach (var type in parsedAssembly.Types)
			{
				if(type.Namespace == "SampSharp.Entities.Annotations")
					continue;
				
				var result = JsonConvert.SerializeObject(type, set);

				Console.WriteLine(result);

				foreach (var m in type.Methods)
				{
					Console.WriteLine(JsonConvert.SerializeObject(m, set));
				}
				foreach(var m in type.Events)
				{
					Console.WriteLine(JsonConvert.SerializeObject(m, set));
				}
				foreach(var m in type.Properties)
				{
					Console.WriteLine(JsonConvert.SerializeObject(m, set));
				}
				foreach(var m in type.Constructors)
				{
					Console.WriteLine(JsonConvert.SerializeObject(m, set));
				}
				foreach(var m in type.Methods)
				{
					Console.WriteLine(JsonConvert.SerializeObject(m, set));
				}
			}
		}

		private class AvailablePackage
		{
			public string Name { get; set; }
			public string Version { get; set; }
			public string DownloadUrl { get; set; }
		}
	}

	public static class DocSignature
	{
		public static string GetSignature(DocsTypeInfo info)
		{
			return info.FullName;
		}
	}
	
	public class DocsTypeInfoReference
	{
		public string AssemblyName { get; set; }
		public string Version { get; set; }
		public string FullName { get; set; }
	}
	
	public class DocsPropertyInfoReference
	{
		public string FullName { get; set; }
	}
	public class DocsMethodInfoReference
	{
		public string FullName { get; set; }
	}
	public class DocsFieldInfoReference
	{
		public string FullName { get; set; }
	}

	public static class TypeExtensions
	{
		private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		public static IEnumerable<MemberInfo> GetDataMembers(this Type type)
		{
			return type.GetProperties(Flags)
				.Concat(GetDataFields(type))
				.Where(m => m.GetCustomAttribute<DataMemberAttribute>() != null);
		}

		private static IEnumerable<MemberInfo> GetDataFields(Type type)
		{
			if(type.BaseType == null || type.BaseType == typeof(object))
				return type.GetFields(Flags);

			return type.GetFields(Flags).Concat(GetDataFields(type.BaseType));
		}
	}
	public class JsonDocWriter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var ignore = new [] {"Types","Properties","Fields","Events","Methods"};

			var members = value.GetType()
				.GetDataMembers()
				.Where(m => !ignore.Contains(m.Name));
			
			writer.WriteStartObject();

			foreach (var member in members)
			{

				var memberValue = member switch
				{
					PropertyInfo prop => prop.GetValue(value),
					FieldInfo field => field.GetValue(value),
					_ => null
				};

				if (memberValue == null)
					continue;
				if (memberValue is IList ls && ls.Count == 0)
					continue;
				if (memberValue is IDictionary dc && dc.Count == 0)
					continue;

				writer.WritePropertyName(member.Name);
				serializer.Serialize(writer, memberValue);
			}

			writer.WriteEndObject();
		}

		public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(XmlDocBaseInfo).IsAssignableFrom(objectType) || typeof(XmlDerivedInfo) == objectType;
		}
	}
	public class JsonApiWriter : JsonConverter
	{
		private void WriteDataMembers(JsonWriter writer, object value, JsonSerializer serializer)
		{
			var members = value.GetType()
				.GetDataMembers();
			
			writer.WriteStartObject();

			foreach (var member in members)
			{
				var memberValue = member switch
				{
					PropertyInfo prop => prop.GetValue(value),
					FieldInfo field => field.GetValue(value),
					_ => null
				};

				if (memberValue == null)
					continue;
				if (memberValue is IList ls && ls.Count == 0)
					continue;
				if (memberValue is IDictionary dc && dc.Count == 0)
					continue;

				writer.WritePropertyName(member.Name);
				serializer.Serialize(writer, memberValue);
			}

			writer.WriteEndObject();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (writer.WriteState == WriteState.Start)
			{
				WriteDataMembers(writer, value, serializer);
				return;
			}

			if (value is DocsAssemblyInfo || value is DocsModuleInfo)
			{
				writer.WriteNull();
				return;
			}

			var refObj = value switch
			{
				DocsTypeInfo type => (object) new DocsTypeInfoReference
				{
					AssemblyName = type.Assembly?.Name.FullName,
					Version = type.Assembly?.Name.Version?.ToString(),
					FullName = type.FullName
				},
				DocsPropertyInfo prop => new DocsPropertyInfoReference {FullName = prop.Name},
				DocsMethodInfo prop => new DocsMethodInfoReference {FullName = prop.Name},
				DocsFieldInfo prop => new DocsFieldInfoReference {FullName = prop.Name},
				DocsConstructorInfo prop => new DocsMethodInfoReference {FullName = prop.Name},
				_ => throw new NotImplementedException()
			};

			serializer.Serialize(writer, refObj);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override bool CanConvert(Type objectType)
		{
			return typeof(DocsMemberInfo).IsAssignableFrom(objectType) ||
			       objectType == typeof(DocsAssemblyInfo) ||
			       objectType == typeof(DocsModuleInfo);
		}
	}

	public class DocsDbContext : DbContext
	{
		public DocsDbContext(DbContextOptions options) : base(options)
		{
		}

		public DocsDbContext(){}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
			=> optionsBuilder.UseMySql(@"foobar", mySqlOptions => mySqlOptions
				// replace with your Server Version and Type
				.ServerVersion(new Version(8, 0, 18), ServerType.MySql));

		public DbSet<ApiAssembly> ApiAssembly { get; set; }
		public DbSet<ApiAssemblyVersion> ApiVersion { get; set; }
		public DbSet<ApiType> ApiType { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<ApiAssembly>(e =>
			{
				e.HasKey(x => x.AssemblyName);
				e.HasMany(x => x.Versions);
			});
			
			modelBuilder.Entity<ApiAssemblyVersion>(e =>
			{
				e.HasKey(x => new {x.Assembly, x.Version});
				e.HasOne(x => x.Assembly);
				e.HasMany(x => x.Types);
			});

			modelBuilder.Entity<ApiType>(e =>
			{
				e.HasKey(x => new {x.TypeName, Version = x.AssemblyVersion});
				e.HasOne(x => x.AssemblyVersion);
			});
		}
	}

	public class ApiAssembly
	{
		public string AssemblyName { get; set; }
		public List<ApiAssemblyVersion> Versions { get; set; } = new List<ApiAssemblyVersion>();
	}

	public class ApiAssemblyVersion
	{
		public ApiAssembly Assembly { get; set; }
		public string Version { get; set; }
		public List<ApiType> Types { get; set; } = new List<ApiType>();
	}
	
	public class ApiType
	{
		public ApiAssemblyVersion AssemblyVersion { get; set; }
		public string TypeName { get; set; }
	}

	public class ApiProperty
	{
		public ApiType MemberType { get; set; }
		public string TypeName { get; set; }
	}
}
