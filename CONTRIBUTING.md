# How to Contribute #

We're always looking for people to help make NzbDrone even better, there are a number of ways to contribute. To get started, <a href="http://www.clahub.com/agreements/NzbDrone/NzbDrone">sign the Contributor License Agreement</a>.

## Documentation ##
Setup guides, FAQ, the more information we have on the wiki the better.

## Development ##

### Tools required ###
- Visual Studio 2012
- HTML/Javascript editor of choice (Sublime Text/Webstorm/etc)
- npm (node package manager)
- git

### Getting started ###

1.  Fork NzbDrone 
2.  Clone (develop branch)
3.  Run `npm install`
4.  Run `grunt` - Used to compile the UI components and copy them (leave this window open)
5.  Compile in Visual Studio

### Contributing Code ###
- If you're adding a new, already requested feature, please move it to In Progress on [Trello](http://trello.nzbdrone.com "Trello") so work is not duplicated.
- Rebase from NzbDrone's develop branch, don't merge
- Make meaningful commits, or squash them
- Feel free to make a pull request before work is complete, this will let us see where its at and make comments/suggest improvements
- Reach out to us on the forums or on IRC if you have any questions
- Add tests (unit/integration)
- Commit with *nix line endings for consistency (We checkout Windows and commit *nix)
- Try to stick to one feature per request to keep things clean and easy to understand
- Use 4 spaces instead of tabs, this is the default for VS 2012 and WebStorm (to my knowledge)

### Pull Requesting ###
- You're probably going to get some comments or questions from us, they will be to ensure consistency and maintainability
- We'll try to respond to pull requests as soon as possible, if its been a day or two, please reach out to us, we may have missed it

If you have any questions about any of this, please let us know.