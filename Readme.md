<p align="center">
  <img width="120" height="120" src="logo.svg">
</p>

# Squirrelly
Squirrelly is a .NET Standard compliant git based dependency manager. Dependencies are fetched via git and versions are resolved by examining the git tags, commits, and branches. There can only be one version of a particular repository downloaded at any given time. If there is a conflict between two dependencies wanting different and incompatible versions of the same repository then an exception will be raised. Repository versions can be locked down to specific commit hashes. A manifest system can be used to parse repository names and versions from `package.json` and `csproj` formatted project manifest files as well as from custom manifest formats. 
<p align="center">
  <b> GO GET SOME NUTS! </b>
</p>

## License
See [License](LICENSE.md) for license details.

## Technologies
1. .NET Standard
2. [Pidgin](https://github.com/benjamin-hodgson/Pidgin) Parser Combinator