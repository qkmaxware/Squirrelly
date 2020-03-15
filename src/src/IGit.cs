using System;
using System.Collections.Generic;

namespace Dependency {
    
public interface IGit {	
	 bool IsUsable {get;}

	 void Clone (Uri uri, string outDir = ".");
	 void Checkout (string pathLike, string tag);
	 void Fetch (string pathLike);
	 void Pull (string pathLike);
	 void PullTags (string pathLike);
	 IEnumerable<string> ListCommits(string pathLike);
	 IEnumerable<string> ListBranches(string pathLike);
	 IEnumerable<string> ListTags(string pathLike);
	 string CurrentCommit(string pathLike);
	
	 IEnumerable<string> TagsForCommit (string pathLike, string commit);
	 IEnumerable<string> CommitsForTag (string pathLike, string tag);
	 IEnumerable<string> CommitsInBranch (string pathLike, string branch);
}

}