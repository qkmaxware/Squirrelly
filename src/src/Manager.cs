using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dependency {

public class Manager {
	public IGit Git {get; set;}
	public string InstallPath {get; set;} = ".";
	
	private string MkTmpdir() {
		var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		if (!Directory.Exists(path)) {
			Directory.CreateDirectory(path);
		}
		return path;
	}
	
	object fslock = new object();
	
	private void CpDir(string from, string to) {
		lock (fslock) {
			if (!Directory.Exists(from)) {
				return;
			}
			
			if (Directory.Exists(to)) {
				RmDir(to);
			}
			
			//Now Create all of the directories
			foreach (string dirPath in Directory.GetDirectories(from, "*", SearchOption.AllDirectories)) {
				Directory.CreateDirectory(dirPath.Replace(from, to));
			}
			
			//Copy all the files & Replaces any files with the same name
			foreach (string newPath in Directory.GetFiles(from, "*.*", SearchOption.AllDirectories)) {
				File.Copy(newPath, newPath.Replace(from, to), true);
			}
		}
	}
	
	private void RmDir(string dir) {
		lock (fslock) {
			System.IO.DirectoryInfo di = new DirectoryInfo(dir);
			foreach (FileInfo file in di.GetFiles()) {
				file.Delete(); 
			}
			foreach (DirectoryInfo directory in di.GetDirectories()) {
				directory.Delete(true); 
			}
		}
	}
	
	public void Resolve(IDependencyManifest manifest, Dictionary<PackageInfo, string> versionlocks = null) {
		if (manifest == null) {
			return;
		}
		
		var tasks = new List<System.Threading.Tasks.Task>();
		foreach (var dep in manifest.GetProdDependencies()) {
			// Async downloading
			var task = Task.Run(() => {
				string vlock = (versionlocks != null && versionlocks.ContainsKey(dep) ? versionlocks[dep] : null);
				var path = Download(dep, vlock);
				if (path != null) {
					Resolve(manifest.LoadFromDirectory(path));
				}
			});
			tasks.Add(task);
		}
		
		// Await all downloads to be done before resolution is complete
		Task.WaitAll(tasks.ToArray());
	}
	
	private string DependencyInstallPath(PackageInfo uri) {
		return Path.Combine(InstallPath, uri.Host, uri.Package);
	}
	
	public bool IsDownloaded (PackageInfo uri) {
		return Directory.Exists(DependencyInstallPath(uri));
	}
	
	public void RemoveDownload (PackageInfo uri) {
		RmDir(DependencyInstallPath(uri));
	}
	
	public string Download (PackageInfo uri, string versionlock = null) {
		var storage = DependencyInstallPath(uri);
	
		if (!IsDownloaded(uri)) {
			// Clone repo to temp directory
			var dir = MkTmpdir();
			Git.Clone(uri.DownloadUrl, dir);

			if (versionlock != null) {
				// Use commit from the version lock
				Git.Checkout(dir, versionlock);
			} else {
				// Find commit matching version constraint
				if (uri.Constraint != null) {
					var commit = uri.Constraint.FindSatisfying(Git, dir).FirstOrDefault();
					
					// Checkout commit if exists, throw if not
					if (commit == default(string)) {
						throw new System.Data.ConstraintException ($"No commits matching constraint '{uri.Constraint}' could be found.");
					} else {
						Git.Checkout(dir, commit);
					}
				}
			}
			
			// Copy to perminant storage
			CpDir(dir, storage);
			return storage;
		} else {
			var commit = Git.CurrentCommit(storage);

			// Ensure that we are using the locked version
			if (commit != versionlock) {
				Git.Checkout(storage, commit);
			}

			// Check that the downloaded one is compatible 
			// Find commit matching version constraint
			var contains = uri.Constraint.FindSatisfying(Git, storage).Contains(commit);
			if (!contains) {
				throw new System.Data.ConstraintException($"One or more packages have incompatible dependency versions");
			}
			return null;
		}
	}
}

}