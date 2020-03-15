using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Dependency {

public interface ISemverComparison {
    bool CompareTo(Semver y);
}

public class GreaterComparison : ISemverComparison {
    public Semver Root {get; private set;}
    public GreaterComparison() {}
    public GreaterComparison(Semver semver) {
        this.Root = semver;
    }
    public bool CompareTo(Semver y) {
        return y > Root;
    }
}

public class GreaterEqualComparison : ISemverComparison {
    public Semver Root {get; private set;}
    public GreaterEqualComparison() {}
    public GreaterEqualComparison(Semver semver) {
        this.Root = semver;
    }
    public bool CompareTo(Semver y) {
        return y >= Root;
    }
}

public class LessComparison : ISemverComparison {
    public Semver Root {get; private set;}
    public LessComparison() {}
    public LessComparison(Semver semver) {
        this.Root = semver;
    }
    public bool CompareTo(Semver y) {
        return y < Root;
    }
}

public class LessEqualComparison : ISemverComparison {
    public Semver Root {get; private set;}
    public LessEqualComparison() {}
    public LessEqualComparison(Semver semver) {
        this.Root = semver;
    }
    public bool CompareTo(Semver y) {
        return y <= Root;
    }
}

public class GreaterMinorRevisionComparison : ISemverComparison {
    public Semver Root {get; private set;}
    public GreaterMinorRevisionComparison() {}
    public GreaterMinorRevisionComparison(Semver semver) {
        this.Root = semver;
    }
    public bool CompareTo(Semver y) {
        return Root.Major == y.Major && y.Minor > Root.Minor;
    }
}

public class InRangeComparison: ISemverComparison {
    public Semver Lower {get; private set;}
    public Semver Higher {get; private set;}
    public InRangeComparison() {}
    public InRangeComparison(Semver a, Semver b) {
        this.Lower = (a < b) ? a : b;
        this.Higher = (a > b) ? a : b;
    }
    public bool CompareTo(Semver y) {
        return y >= Lower && y <= Higher;
    }
}

public struct Semver {
    // https://semver.npmjs.com/
    public int Major {get; private set;}
    public int Minor {get; private set;}
    public int Patch {get; private set;}

    private static Regex regex = new Regex(@"(?<major>\d+)(?:\.(?<minor>\d+))?(?:\.(?<patch>\d+))?");

    public Semver(int major, int minor, int patch) {
        this.Major = major;
        this.Minor = minor;
        this.Patch = patch;
    }

    public static bool TryParse(string text, out Semver semver) {
        try {
            var match = regex.Match(text);
            semver = new Semver(0,0,0);
            var major = match.Groups["major"];
            var minor = match.Groups["minor"];
            var patch = match.Groups["patch"];

            if (major.Success)
                semver.Major = int.Parse(major.Value);
            if (minor.Success)
                semver.Minor = int.Parse(minor.Value);
            if (patch.Success)
                semver.Patch = int.Parse(patch.Value);
            
            return true;
        } catch {
            semver = default(Semver);
            return false;
        }
    }

    public static bool operator == (Semver lhs, Semver rhs) {
        return lhs.Major == rhs.Major && lhs.Minor == rhs.Minor && lhs.Patch == rhs.Patch;
    }
    public static bool operator != (Semver lhs, Semver rhs) {
        return lhs.Major != rhs.Major || lhs.Minor != rhs.Minor || lhs.Patch != rhs.Patch;
    }

    public static bool operator >= (Semver lhs, Semver rhs) {
        return lhs > rhs || lhs == rhs;
    }

    public static bool operator <= (Semver lhs, Semver rhs) {
        return lhs < rhs || lhs == rhs;
    }

    public static bool operator > (Semver lhs, Semver rhs) {
        if (lhs.Major > rhs.Major)
            return true;
        if (lhs.Major == rhs.Major && lhs.Minor > rhs.Minor)
            return true;
        if (lhs.Major == rhs.Major && lhs.Minor == rhs.Minor && lhs.Patch > rhs.Patch)
            return true;
        return false;
    }

    public static bool operator <  (Semver lhs, Semver rhs) {
        if (lhs.Major < rhs.Major)
            return true;
        if (lhs.Major == rhs.Major && lhs.Minor < rhs.Minor)
            return true;
        if (lhs.Major == rhs.Major && lhs.Minor == rhs.Minor && lhs.Patch < rhs.Patch)
            return true;
        return false;
    }

    public override bool Equals(object obj) {
        return obj switch {
            Semver sem => sem == this,
            _ => base.Equals(obj)
        };
    }

    public override int GetHashCode() {
        return HashCode.Combine(Major, Minor, Patch);
    }

    public override string ToString(){
        return Major + "." + Minor + "." + Patch; 
    }
}

}