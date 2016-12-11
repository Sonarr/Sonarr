# How to Contribute #

We're always looking for people to help make Sonarr even better, there are a number of ways to contribute.

## Documentation ##
Setup guides, FAQ, the more information we have on the wiki the better.

## Development ##

### Tools required ###
- Visual Studio 2015
- HTML/Javascript editor of choice (Sublime Text/Webstorm/Atom/etc)
- npm (node package manager)
- git

### Getting started ###

1.  Fork Sonarr
2.  Clone (develop branch) *you may need pull in submodules separately if you client doesn't clone them automatically (CurlSharp)*
3.  Run `npm install`
4.  Run `npm start` - Used to compile the UI components and copy them.
    Leave this window open.
    If you have gulp globally installed you can use `gulp watch` instead
5.  Compile in Visual Studio

### Contributing Code ###
- If you're adding a new, already requested feature, please comment on [Github Issues](https://github.com/Sonarr/Sonarr/issues "Github Issues") so work is not duplicated (If you want to add something not already on there, please talk to us first)
- Rebase from Sonarr's develop branch, don't merge
- Make meaningful commits, or squash them
- Feel free to make a pull request before work is complete, this will let us see where its at and make comments/suggest improvements
- Reach out to us on the forums or on IRC if you have any questions
- Add tests (unit/integration)
- Commit with *nix line endings for consistency (We checkout Windows and commit *nix)
- One feature/bug fix per pull request to keep things clean and easy to understand
- Use 4 spaces instead of tabs, this is the default for VS 2012 and WebStorm (to my knowledge)

### Pull Requesting ###
- Only make pull requests to develop, never master, if you make a PR to master we'll comment on it and close it
- You're probably going to get some comments or questions from us, they will be to ensure consistency and maintainability
- We'll try to respond to pull requests as soon as possible, if its been a day or two, please reach out to us, we may have missed it
- Each PR should come from its own [feature branch](http://martinfowler.com/bliki/FeatureBranch.html) not develop in your fork, it should have a meaningful branch name (what is being added/fixed)
  - new-feature (Good)
  - fix-bug (Good)
  - patch (Bad)
  - develop (Bad)

If you have any questions about any of this, please let us know.
