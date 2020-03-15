using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Dependency {

public class LocalGitInstall : IGit {

	public bool IsUsable {
		get {
			var process = MakeProcess("--help");
			process.Start();
			process.WaitForExit();
			return process.ExitCode == 0;
		}
	}

	private Process MakeProcess(string args, string dir = ".") {
		var info = new ProcessStartInfo();
		info.FileName = "git";
		info.WorkingDirectory = dir;
		info.Arguments  = args;
		info.RedirectStandardOutput = true;
		info.RedirectStandardError = true;
		info.RedirectStandardInput = true;
		info.UseShellExecute = false;
		info.CreateNoWindow = true;
		
		var process = new Process();
		process.StartInfo = info;
		process.EnableRaisingEvents = true;
		return process;
	}
	private void RunProcess (string args, string dir = ".") {
		var process = MakeProcess(args, dir);
		// process.OutputDataReceived += ()=>{}
		process.Start();
		process.WaitForExit();
		if (process.ExitCode != 0) {
			throw new InvalidOperationException(process.StandardError.ReadToEnd());
		}
	}
	private IEnumerable<string> RunProcessPipe(string args, string dir = ".") {
		var process = MakeProcess(args, dir);
		process.Start();
		
		string line;
		while ((line = process.StandardOutput.ReadLine()) != null) {
			yield return line;
		}
		process.WaitForExit();
	}

	private void Delete(string dir) {
		System.IO.DirectoryInfo di = new DirectoryInfo(dir);
		foreach (FileInfo file in di.GetFiles()) {
			file.Attributes = FileAttributes.Normal;
			file.Delete();
		}
		foreach (DirectoryInfo directory in di.GetDirectories()) {
			directory.Attributes = FileAttributes.Normal;
			directory.Delete(true); 
		}
	}

	public void Clone (Uri uri, string outDir){
		if (Directory.Exists(outDir)) {
			Delete(outDir);
		}
		RunProcess($"clone \"{uri}\" \"{outDir}\"");
	}
	
	public void Checkout (string pathLike, string tag) {
		RunProcess($"checkout {tag}", pathLike);
	}
	
	public void Fetch (string pathLike) {
		RunProcess($"fetch --all", pathLike);
	}
	
	public void Pull (string pathLike) {
		RunProcess($"pull", pathLike);
	}
	
	public void PullTags (string pathLike) {
		RunProcess("pull --tags", pathLike);
	}
	
	public IEnumerable<string> ListCommits(string pathLike) {
		return RunProcessPipe("log --all --pretty=format:\"%H\"", pathLike);
	}
	
	public IEnumerable<string> ListBranches(string pathLike) {
		return RunProcessPipe("branch -a", pathLike);
	}
	
	public IEnumerable<string> ListTags(string pathLike) {
		return RunProcessPipe("tag -l", pathLike);
	}
	
	public string CurrentCommit(string pathLike) {
		return RunProcessPipe("rev-parse HEAD", pathLike).FirstOrDefault();
	}
	
	public IEnumerable<string> TagsForCommit (string pathLike, string commit) {
		return RunProcessPipe($"tag --points-at {commit}", pathLike);
	}
	
	public IEnumerable<string> CommitsForTag (string pathLike, string tag) {
		return RunProcessPipe($"rev-parse {tag}^{{}}", pathLike);
	}
	
	public IEnumerable<string> CommitsInBranch (string pathLike, string branch) {
		return RunProcessPipe($"log {branch}", pathLike);
	}
}

}