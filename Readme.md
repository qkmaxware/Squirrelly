<p align="center">
  <img width="120" height="120" src="logo.svg">
</p>

# Squirrelly
Squirrelly is a .NET Standard compliant git based dependency manager. Dependencies are fetched via git and versions are resolved by examining the git tags, commits, and branches. There can only be one version of a particular repository downloaded for a project at any given time. If there is a conflict between two dependencies wanting different and incompatible versions of the same repository, then an exception will be raised. Repository versions can be locked down to specific commit hashes. A manifest system can be used to parse repository names and versions from `package.json` and `csproj` like project manifest files, as well as from custom manifest formats. 
<p align="center">
  <b> START GATHERING! </b>
</p>

## License
- See [License](LICENSE.md) for license details.

## Technologies
1. .NET Standard
2. [Pidgin](https://github.com/benjamin-hodgson/Pidgin) Parser Combinator

## Usage
```cs
// Create dependency manager and set where to install packages to
var depmgr = new Manager();
depmgr.InstallPath = "./modules";

// Load dependencies from manifest or add packages programmatically
var manifest = JsonPackageManifest.LoadFromDirectory(".");

// [Optional] Set dependency locked versions
var locked = new Dictionary<PackageInfo, string>();

// Download dependencies
depmgr.Resolve(manifest, locked);
```