using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace Dependency {

public interface IDependencyManifest {
	IEnumerable<PackageInfo>  GetProdDependencies();
	IEnumerable<PackageInfo>  GetDevDependencies();
	IDependencyManifest LoadFromDirectory(string pathLike);
}

public interface IPackageSpecificationDeserializer<From> {
	PackageInfo Deserialize(From from);
}

public class CsprojManifest: IDependencyManifest, IPackageSpecificationDeserializer<XmlElement> {

	private List<PackageInfo> dependencies = new List<PackageInfo>();
	private List<PackageInfo> devDependencies = new List<PackageInfo>();

	public PackageInfo Deserialize(XmlElement spec) {
		var dep = new PackageInfo();
		dep.DownloadUrl = new Uri(spec.GetAttribute("Include"));
		dep.Constraint = spec.HasAttribute("Version") ? VersionConstraintParser.Parse(spec.GetAttribute("Version")) : null;
		throw new NotImplementedException();
	}

	public IEnumerable<PackageInfo> GetProdDependencies() {
		return dependencies.AsReadOnly();
	}

	public IEnumerable<PackageInfo> GetDevDependencies() {
		return devDependencies.AsReadOnly();
	}

	public IDependencyManifest LoadFromDirectory(string pathLike) {
		try {
			CsprojManifest manifest = new CsprojManifest();
			var proj = Directory.EnumerateFiles(pathLike, "*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
			using (var reader = new StreamReader(Path.Combine(pathLike, proj))) {
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(reader.ReadToEnd());
				var references = doc.GetElementsByTagName("PackageReference");
				foreach (XmlNode reference in references) {
					if (reference is XmlElement) {
						var pkg = Deserialize((XmlElement)reference);
						dependencies.Add(pkg); // TODO devDependencies
					}
				}
			}
			return manifest;
		} catch {
			return null;
		}
	}
}

public class JsonPackageManifest: IDependencyManifest, IPackageSpecificationDeserializer<KeyValuePair<string, string>> {
	// Raw representation of a package.json file
	private class Raw {
		public string name {get; set;}
		public Dictionary<string, string> dependencies {get; set;}
		public Dictionary<string, string> devDependencies {get; set;}
	}

	private List<PackageInfo> dependencies = new List<PackageInfo>();
	private List<PackageInfo> devDependencies = new List<PackageInfo>();

	public PackageInfo Deserialize(KeyValuePair<string, string> spec) {
		var dep = new PackageInfo();
		dep.DownloadUrl = new Uri(spec.Key);
		dep.Constraint = VersionConstraintParser.Parse(spec.Value);
		return dep;
	}

	public IEnumerable<PackageInfo> GetProdDependencies() {
		return dependencies.AsReadOnly();
	}

	public IEnumerable<PackageInfo> GetDevDependencies() {
		return devDependencies.AsReadOnly();
	}

	public IDependencyManifest LoadFromDirectory(string pathLike) {
		try{
			JsonPackageManifest manifest = new JsonPackageManifest();
			using (var reader = new StreamReader(Path.Combine(pathLike, "package.json"))) {
				var package = JsonSerializer.Deserialize<Raw>(reader.ReadToEnd());

				foreach (var dep in package.dependencies) {
					manifest.dependencies.Add(Deserialize(dep));
				}

				foreach (var dep in package.devDependencies) {
					manifest.devDependencies.Add(Deserialize(dep));
				}
			}
			return manifest;
		} catch {
			return null;
		}
	}
}

}