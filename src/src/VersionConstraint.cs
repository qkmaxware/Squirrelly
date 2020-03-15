using System.Collections.Generic;
using System.Linq;

namespace Dependency {

public abstract class VersionConstraint {
	public abstract IEnumerable<string> FindSatisfying (IGit git, string repo);
}

public class SemverConstraint : VersionConstraint {
    public ISemverComparison Condition {get; set;}

	public SemverConstraint(ISemverComparison condition) {
		this.Condition = condition;
	}

	public override IEnumerable<string> FindSatisfying(IGit git, string repo) {
		return git.ListTags(repo).Where((tag) => {
			// Match tag against constraint	
			Semver semver;
			if (Semver.TryParse(tag, out semver)) {
				return Condition.CompareTo(semver);
			}
			return false;
		}).SelectMany((tag) => {
			return git.CommitsForTag(repo, tag);
		});
    }
}

public class TagConstraint : VersionConstraint {
	public string Tag {get; private set;}
	public TagConstraint(string tag) {
		this.Tag = tag;
	}
	public override IEnumerable<string> FindSatisfying (IGit git, string repo) {
		return git.CommitsForTag(repo, Tag);
	}
}

public class BranchConstraint : VersionConstraint {
	public string Branch {get; private set;}
	public BranchConstraint (string branch) {
		this.Branch = branch;
	}
	public override IEnumerable<string> FindSatisfying (IGit git, string repo) {
		return git.CommitsInBranch(repo, Branch);
	}
}

public class RevisionConstraint: VersionConstraint {
	public string Revision {get; private set;}
	public RevisionConstraint(string rev) {
		this.Revision = rev;
	}
	public override IEnumerable<string> FindSatisfying (IGit git, string repo) {
		return git.ListCommits(repo).Where(hash => hash?.Equals(Revision) ?? false);
	}
}

public class AndConstraint : VersionConstraint {
	private List<VersionConstraint> constraints = new List<VersionConstraint>();
	public AndConstraint() {}
	public AndConstraint(IEnumerable<VersionConstraint> constraints) {
		this.constraints.AddRange(constraints);
	}
	public void And(VersionConstraint constraint) {
		constraints.Add(constraint);
	}
	public override IEnumerable<string> FindSatisfying (IGit git, string repo) {
		if (constraints.Count < 1) {
			return new string[0];
		} else {
			var start = constraints.First().FindSatisfying(git, repo);
			for (int i = 1; i < constraints.Count; i++) {
				start = start.Intersect(constraints[i].FindSatisfying(git, repo));
			}
			return start;
		}
	}
}

public class OrConstraint : VersionConstraint {
	private List<VersionConstraint> constraints = new List<VersionConstraint>();
	public OrConstraint() {}
	public OrConstraint(IEnumerable<VersionConstraint> constraints) {
		this.constraints.AddRange(constraints);
	}
	public void Or(VersionConstraint constraint) {
		constraints.Add(constraint);
	}
	public override IEnumerable<string> FindSatisfying (IGit git, string repo) {
		if (constraints.Count < 1) {
			return new string[0];
		} else {
			var start = constraints.First().FindSatisfying(git, repo);
			for (int i = 1; i < constraints.Count; i++) {
				start = start.Union(constraints[i].FindSatisfying(git, repo));
			}
			return start;
		}
	}
}

}