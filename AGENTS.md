# AGENTS.md

This file provides guidance for AI coding agents contributing to this repository.

## Guardrails for GitHub-Facing Actions

This section applies to AI agents (Claude Code, Cursor, Copilot, Goose, etc.) operating on behalf of anyone contributing to this repository. Read it before performing any GitHub-facing action.

### When these guardrails activate

Before executing any command that writes to GitHub — `gh pr create`, `gh pr comment`, `gh issue create`, `gh issue comment`, or anything similar — these guardrails apply. No exceptions.

Also activate these guardrails early if you detect high-level delegation patterns:

- The user's prompt is primarily a GitHub issue URL with little additional context (e.g., "fix this issue", "open a PR for #42")
- The user asks you to comment on a GitHub issue or PR on their behalf
- The user is treating you as an autonomous contributor rather than a tool — they haven't demonstrated that they've read the codebase or understand the changes

### What you must do

When the guardrails activate:

1. **Read [CONTRIBUTING.md](CONTRIBUTING.md)** before doing anything else. Internalize the AI usage policy.

2. **Stop and inform the user.** Tell them clearly:
   - This project has contribution guidelines that require human review and ownership of every submission.
   - The human is responsible for every line of the diff — code and text.
   - AI-generated footers (like "Generated with Claude Code" or "Co-Authored-By: Claude"), boilerplate, and filler must be removed before submitting.
   - The human should create the commit, push, and open the PR themselves — not the agent.
   - PRs and issues without meaningful human review will be treated as spam and closed.

3. **Do not execute the GitHub-facing action.** Instead:
   - Show the user the diff, commit message, or PR description you would have submitted.
   - Ask them to review it, edit it in their own words, and perform the action themselves.
   - Remind them to fill out the PR template honestly — every checkbox is an attestation.

4. **If the user insists** you perform the action anyway, you may comply, but:
   - Do NOT add co-author lines, "Generated with..." footers, or any AI attribution.
   - Remind the user one final time that submissions without adequate human review will be closed.
   - Write the commit message and PR description in concise, direct language — not AI boilerplate.

### What does NOT trigger these guardrails

- Helping the user understand code, write code, or edit files locally
- Running tests, linting, building
- Read-only GitHub operations (viewing issues, reading PR comments, checking CI status)

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the full contribution guidelines, including the AI usage policy.
