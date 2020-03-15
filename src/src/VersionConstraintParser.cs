using System;

using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace Dependency {

// Parse version constraint DSL
public static class VersionConstraintParser {
    /*
        constraint      := tagfilter | semverfilter | branch | tag | revision | and | or

        tagfilter       := [\w]+
        semverfilter    := '^' semver | '~' semver | ('>=' | '<=' | '>' | '<') semver | semver '-' semver
        branch          := 'branch=' [\w]+
        tag             := 'tag=' [\w]+
        revision        := 'revision=' [\w]+
        and             := 'and(' constraint* ')'
        or              := 'or(' constraint* ')'
    */  
    static VersionConstraintParser () {
        Parser<char, VersionConstraint> constraint  = null;
        Parser<char, VersionConstraint> and         = null;
        Parser<char, VersionConstraint> or          = null;

        Parser<char, string> identifier             = Trim(LetterOrDigit.ManyString());
        Parser<char, string> digits                 = Digit.ManyString();
        Parser<char, Semver> version                = Map(
                                                        (major, _, minor, __, patch) 
                                                            => new Semver(int.Parse(major), int.Parse(minor), int.Parse(patch)),
                                                        digits,
                                                        Char('.'),
                                                        digits,
                                                        Char('.'),
                                                        digits
                                                    );

        Parser<char, VersionConstraint> literal     = identifier.Map((tag) => (VersionConstraint)new TagConstraint(tag));
        Parser<char, ISemverComparison> greater     = Char('^').Then(version).Map((semver) => (ISemverComparison)new GreaterEqualComparison(semver));
        Parser<char, ISemverComparison> plusminor   = Char('~').Then(version).Map((semver) => (ISemverComparison)new GreaterMinorRevisionComparison(semver));
        Parser<char, ISemverComparison> ge          = String(">=").Then(version).Map((semver) => (ISemverComparison)new GreaterEqualComparison(semver));
        Parser<char, ISemverComparison> le          = String("<=").Then(version).Map((semver) => (ISemverComparison)new LessEqualComparison(semver));
        Parser<char, ISemverComparison> g           = Char('>').Then(version).Map((semver) => (ISemverComparison)new GreaterComparison(semver));
        Parser<char, ISemverComparison> l           = Char('<').Then(version).Map((semver) => (ISemverComparison)new LessComparison(semver));
        Parser<char, ISemverComparison> between     = Map((start, _, end) => (ISemverComparison)new InRangeComparison(start, end), Trim(version), Trim(Char('-')), Trim(version));
        Parser<char, VersionConstraint> semver      = greater.Or(plusminor).Or(ge).Or(le).Or(g).Or(l).Or(between).Map((comp) => (VersionConstraint)new SemverConstraint(comp));
        Parser<char, VersionConstraint> branch      = Trim(String("branch")).Then(Trim(Char('='))).Then(identifier).Map((str) => (VersionConstraint)new BranchConstraint(str));
        Parser<char, VersionConstraint> tag         = Trim(String("tag")).Then(Trim(Char('='))).Then(identifier).Map((str) => (VersionConstraint)new TagConstraint(str));
        Parser<char, VersionConstraint> revision    = Trim(String("revision")).Then(Trim(Char('='))).Then(identifier).Map((str) => (VersionConstraint)new TagConstraint(str));

        and = Trim(String("and")).Then(Trim(Char('('))).Then(Rec(() => constraint)).Many().Before(Trim(Char(')'))).Map(constraints => (VersionConstraint)new AndConstraint(constraints));
        or  = Trim(String("or")).Then(Trim(Char('('))).Then(Rec(() => constraint)).Many().Before(Trim(Char(')'))).Map(constraints => (VersionConstraint)new OrConstraint(constraints));
    
        constraint = and.Or(or).Or(revision).Or(tag).Or(branch).Or(semver).Or(literal);

        DSLparser = constraint;
    }

    private static Parser<char, T> Trim<T>(Parser<char, T> parser) {
        return SkipWhitespaces.Then(parser);
    }

    private static Parser<char, VersionConstraint> DSLparser = null;

    public static VersionConstraint Parse(string constraint) {
        return DSLparser.ParseOrThrow(constraint);
    }
}

}