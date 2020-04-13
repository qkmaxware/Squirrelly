using System;

namespace Dependency {

public class PackageInfo {
	public Uri DownloadUrl {get; set;}
	public VersionConstraint Constraint {get; set;}
	
	public string Host {get; set;}
	public string Package {get; set;}
}

}