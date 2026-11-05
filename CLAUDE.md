# Project notes for Claude

## Git workflow
- **Always work on the `master` branch.** Commit and push directly to `master`; do not create feature branches.

## Azure DevOps mirror
- GitHub (`ControlFreakEv/Honeywell`) is the source of truth.
- `.github/workflows/mirror-to-azure.yml` copies every branch and tag to
  `https://dev.azure.com/SeaconEngineering/Honeywell/_git/Honeywell`.
- Runs on every push, on branch/tag delete, hourly (cron), and manually via
  Actions → "Mirror to Azure DevOps" → Run workflow.
- Auth: repository secret `AZURE_DEVOPS_PAT` (PAT needs **Code: Read & Write** scope).
- Pushes are forced, so commits made directly in Azure will be overwritten. Make changes in GitHub only.
- Branches deleted in GitHub are not deleted in Azure (no `--prune`).
