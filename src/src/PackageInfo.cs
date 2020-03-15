using System;

namespace Dependency {

public class PackageInfo {
	public Uri DownloadUrl;
	public VersionConstraint Constraint;
	
	public string Host {get;}
	public string Package {get;}
}

}