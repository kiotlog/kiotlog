## Contributing

Thank you for your interest in this project, on behalf of the authors and users, we appreciate your contribution. This document outlines some of the basic things that you need to know to save your time and ours. The goal here is to ensure that we all follow the same processes, methods, and standards.

### Contributing Process Overview

* Make your changes on a feature branch
* Verify if all tests pass locally
* Create a pull request (Travis CI will kick in)
* We will then review your code, CI logs and provide feedback
* Code will then be merged after all requirements are met

### Coding

#### Style

In general, we follow standard C# style with one notable exception: no tabs, 2 space indent. When making changes, please try to match the style of the existing code.

Please if you see compiler warnings fix them.

#### Security

As the purpose of this library is to make it easier for developers to implement secure crypto, all changes must meet the following requirements:

 * Must not introduce a weakness, or potential weakness. If there is doubt, a change won't be merged.
 * Must not add confusion to the interface. Usability is critical.
 * Must not reference libraries beyond [libsodium](https://github.com/jedisct1/libsodium) and those shipped with the .NET Framework.
 * Code must be clear, readable, well commented. Code the isn't clear makes audits more difficult.
 * Must not include crypto implementations. All implementations must be in `libsodium`.

#### Unit Tests

Unit tests must be present for all functionality that calls into `libsodium`, and should be present for functionality in this project. When possible, the test values from `libsodium` should be used. Unit tests are ran as part of the CI process, and must pass for a change to be merged.

#### Multi-Platform

Keep in mind that `libsodium-net` is multi-platform, so it's easy to break functionality in other platform you can't test. Keep an eye on travis as it will build for OSX and Linux.

#### Commits

Commit messages should be descriptive, clear, and meaningful. Every logical change should be a separate commit, while avoiding excessive commits. While it's not necessary to [squash](http://davidwalsh.name/squash-commits-git) Pull Requests to a single commit, Pull Requests should contain the minimum number of commits to clearly express the changes made. If an excessive number of commits are included in a Pull Request, the request may be closed and the submitter asked to re-submit after squashing the commits.

### Using the issue tracker

The [issue tracker](https://github.com/adamcaudill/libsodium-net/issues?state=open) is the preferred channel for [bug reports](#bug-reports), [features requests](#feature-requests) and [submitting pull requests](#pull-requests), but please respect the following restrictions:

 * Please **do not** use the issue tracker for personal support requests (use [Stack Overflow](http://stackoverflow.com) or your preferred forum).
 * Please **do not** derail or troll issues. Keep the discussion on topic and respect the opinions of others.

### Bug reports

A bug is a _demonstrable problem_ that is caused by the code in the repository. Good bug reports are extremely helpful - thank you!

Guidelines for bug reports:

 1. **Use the GitHub issue search** &mdash; check if the issue has already been reported.
 2. **Check if the issue has been fixed** &mdash; try to reproduce it using the latest `master` or development branch in the repository.
 3. **Isolate the problem** &mdash; create a test case and a live example.

A good bug report shouldn't leave others needing to chase you down for more information. Please try to be as detailed as possible in your report. What is your environment? What steps will reproduce the issue? What would you expect to be the outcome? All these details will help people to fix any potential bugs.

Example:

> Short and descriptive example bug report title
>
> A summary of the issue and the browser/OS environment in which it occurs. If suitable, include the steps required to reproduce the bug.
>
> 1. This is the first step
> 2. This is the second step
> 3. Further steps, etc.
>
> Any other information you want to share that is relevant to the issue being > reported. This might include the lines of code that you have identified as > causing the bug, and potential solutions (and your opinions on their > merits).

### Feature requests

Feature requests are welcome. But take a moment to find out whether your idea fits with the scope and aims of the project. It's up to *you* to make a strong case to convince the project's developers of the merits of this feature. It's also up to you to make the case that the change is secure, and doesn't introduce a weakness.

Please provide as much detail and context as possible.

### Security Issues

If you believe you have found a security issue, please contact me directly: adam@adamcaudill.com

I believe in coordinated disclosure whenever possible, to protect users as much as possible.

### Pull requests

Good pull requests (patches, improvements, new features) are a fantastic help. They should remain focused in scope and avoid containing unrelated commits. Please commit your changes in logical chunks, with [good commit messages](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html) to make it easier to review.

**Please ask first** before embarking on any significant pull request (e.g. implementing features, refactoring code, porting to a different language), otherwise you risk spending a lot of time working on something that the project's developers might not want to merge into the project.

If this is your first contribution, please add your name and email address to the end of the `Contributors.md` file.

Pull requests will not be merged if we do not have clean builds from our CI system. In case of issues with the CI system, the merge will be delayed until the system is corrected.
