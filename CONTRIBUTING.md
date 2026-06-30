# How to Contribute

We're always looking for people to help make Sonarr even better, there are a number of ways to contribute.

## Documentation

Setup guides, [FAQ](https://wiki.servarr.com/sonarr/faq), the more information we have on the [wiki](https://wiki.servarr.com/sonarr) the better.

## Development

### Tools required

- Visual Studio 2019 or higher (https://www.visualstudio.com/vs/). The community version is free and works (https://www.visualstudio.com/downloads/) or [Jetbrains Rider](https://www.jetbrains.com/rider/)
- HTML/Javascript editor of choice (VS Code/Webstorm/etc)
- [Git](https://git-scm.com/downloads)
- [NodeJS](https://nodejs.org/en/download/) (Node 10.X.X or higher)
- [Yarn](https://yarnpkg.com/)

### Getting started

1. Fork Sonarr
2. Clone the repository into your development machine. [_info_](https://docs.github.com/en/get-started/quickstart/fork-a-repo)
3. Install the required Node Packages `yarn install`
4. Start webpack to monitor your dev environment for any frontend changes that need post processing using `yarn start` command.
5. Build the project in Visual Studio, Setting startup project to `Sonarr.Console` and framework to `x86`
6. Debug the project in Visual Studio
7. Open http://localhost:8989

### Issues

All issues must follow the provided templates, these templates help us triage and review contributions efficiently issues that do not use the templates may be closed without notice.

- We expect that all issues are opened by a human selecting the appropriate issue template while opening the issue
- Issues opened automatically or by other means may be closed automatically
- We also expect discussions on issues to be conducted by humans, we are not interested in conversing with AI or triaging AI hallucinations
- Bug reports for issues that generate logs must contain a link to the approproate trace logs. See the [Wiki](https://wiki.servarr.com/sonarr/troubleshooting#logging-and-log-files) for more information

### How to Contribute

- We prefer small PRs that focus on a single issue
- Discuss before building large features. For major features or architectural changes, please open an issue first. Once we've agreed on a solution the work can begin
- Small bug fixes or improvements can usually go straight to PR, but we may ask for additional information, including logs before reviewing
- Understand the code before changing it. Follow existing code structure and standards
- Add tests (unit/integration) when appropriate
- Commit with \*nix line endings for consistency
- Test thoroughly, all code must be tested to ensure it compiles and works correctly
- You are responsible for all code and you should understand it's functionality, including reviewing it before submission

## AI tools

AI tools can be useful, but unreviewed AI slop or automated AI agents is not.

The standard is the same regardless of how the code was written: **you are responsible for every line of your contribution.** If you can't explain why a line is there and why it's correct, it shouldn't be in your PR.

**Specifically:**

- If you use AI to help write code, you must understand every line of what you're submitting
- Do not paste raw AI output into issues, PRs, or comments. If you use AI to help draft text, rewrite it in your own words
- Remove AI-generated footers, co-author attributions, and "Generated with..." signatures before submitting. Their presence tells us you didn't review your own submission carefully enough to notice them
- Automated submissions — bots or agents opening issues or posting PRs without meaningful human review — will be treated as spam

If you have any questions about any of this, please let us know.
