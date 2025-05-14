# Release Checklist

This document outlines the checklist that should be followed before releasing a new version of KafkaGenericProcessor.Core.

## Pre-Release Verification

### Code Quality
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Code coverage is adequate (minimum 80%)
- [ ] Static code analysis shows no critical issues
- [ ] Performance benchmarks show no regressions

### Documentation
- [ ] API Reference is up to date
- [ ] Examples in the documentation match the current version
- [ ] README.md is updated with any new features
- [ ] CHANGELOG.md is updated with all changes since last release
- [ ] Security Best Practices document is updated if necessary

### Version Management
- [ ] Version number is updated in project files according to [Semantic Versioning](https://semver.org/)
- [ ] Version number is consistent across all places it appears

## Release Process

### Build and Package
- [ ] Clean build succeeds
- [ ] NuGet package is generated successfully
- [ ] Package includes all required dependencies
- [ ] Package metadata is correct (version, description, authors, etc.)
- [ ] Package icon and README are included

### Testing
- [ ] NuGet package is verified by installing in a test project
- [ ] Basic functionality works with the packaged version
- [ ] Verify package is compatible with all supported .NET versions

### Release
- [ ] Create GitHub release with appropriate tag
- [ ] Upload NuGet package to nuget.org
- [ ] Verify the package appears correctly on nuget.org

## Post-Release

- [ ] Announce release on relevant channels
- [ ] Update documentation site (if applicable)
- [ ] Create next development version branch (if applicable)
- [ ] Review and triage any issues that might have missed the release
- [ ] Update project roadmap with completion dates
- [ ] Schedule retrospective to evaluate the release process

## Rollback Plan

If critical issues are discovered after release:

1. Document the issue in detail
2. Assess impact on users
3. Prepare fix in a hotfix branch
4. Follow emergency release process if needed
5. Consider whether to deprecate the problematic release