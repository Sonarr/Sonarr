# Repository Guidelines

## Working Rules

- Work from a git worktree, not directly in the main checkout.
- Do not use emojis in commits, pull requests, comments, or docs.
- Do not mention assistant product names in commits, pull requests, comments, or repo files. Use role-based terms such as `builder` or `reviewer` if needed.
- This fork uses `develop` as the integration branch and `master` as the production branch.

## Branch Guidance

- Land ongoing fork work on `develop` unless the user explicitly asks for a different target.
- Treat `master` as the production or release branch.
- If work changes release or deploy behavior, update any nearby workflow or release docs in the same change.
## Git worktrees

Worktree directory preference: create git worktrees under `~/worktrees/<repo>/<branch>`, never inside this clone or `~/Documents/Repos.nosync/`. Merged/clean worktrees are auto-pruned weekly by `infra/scripts/prune-worktrees.sh` (launchd `dev.staros.worktree-janitor`).
