# How to Contribute #

We're always looking for people to help make Sonarr even better, there are a number of ways to contribute.

## Documentation ##
Setup guides, [FAQ](https://wiki.servarr.com/Sonarr_FAQ), the more information we have on the [wiki](https://wiki.servarr.com/Sonarr) the better.

## Development ##

### Tools required ###
- Visual Studio 2019 or higher (https://www.visualstudio.com/vs/).  The community version is free and works (https://www.visualstudio.com/downloads/).
- HTML/Javascript editor of choice (VS Code/Sublime Text/Webstorm/Atom/etc)
- [Git](https://git-scm.com/downloads)
- [NodeJS](https://nodejs.org/en/download/) (Node 10.X.X or higher)
- [Yarn](https://yarnpkg.com/)

### Getting started ###

1. Fork Sonarr
2. Clone the repository into your development machine. [*info*](https://help.github.com/articles/working-with-repositories)
3. Install the required Node Packages `yarn install`
4. Start gulp to monitor your dev environment for any changes that need post processing using `yarn start` command.
5. Build the project in Visual Studio, Setting startup project to `Sonarr.Console` and framework to `x86`
6. Debug the project in Visual Studio
7. Open http://localhost:8989

### Contributing Code ###
- If you're adding a new, already requested feature, please comment on [Github Issues](https://github.com/Sonarr/Sonarr/issues "Github Issues") so work is not duplicated (If you want to add something not already on there, please talk to us first)
- Rebase from Sonarr's develop (currently phantom-develop) branch, don't merge
- Make meaningful commits, or squash them
- Feel free to make a pull request before work is complete, this will let us see where its at and make comments/suggest improvements
- Reach out to us on our [forums](https://forums.sonarr.tv/), [subreddit](https://www.reddit.com/r/sonarr/), [discord](https://discord.gg/Ex7FmFK), or [IRC](http://webchat.freenode.net/?channels=#sonarr) if you have any questions
- Add tests (unit/integration)
- Commit with *nix line endings for consistency (We checkout Windows and commit *nix)
- One feature/bug fix per pull request to keep things clean and easy to understand
- Use 4 spaces instead of tabs, this should be the default for VS 2019 and WebStorm

### Pull Requesting ###
- Only make pull requests to develop (currently phantom-develop), never master, if you make a PR to master we'll comment on it and close it
- You're probably going to get some comments or questions from us, they will be to ensure consistency and maintainability
- We'll try to respond to pull requests as soon as possible, if its been a day or two, please reach out to us, we may have missed it
- Each PR should come from its own [feature branch](http://martinfowler.com/bliki/FeatureBranch.html) not develop in your fork, it should have a meaningful branch name (what is being added/fixed)
  - new-feature (Good)
  - fix-bug (Good)
  - patch (Bad)
  - develop (Bad)

If you have any questions about any of this, please let us know.
