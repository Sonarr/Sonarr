## TMDb Metadata Source Plan

### Goal

Add TMDb as a selectable metadata source in Sonarr so users can switch between TVDB and TMDb from `/settings/metadatasource`.

### Scope

- Add a persisted metadata source setting with TMDb credentials.
- Add a runtime selector so existing callers use the configured metadata provider.
- Implement a TMDb metadata provider for series search and details.
- Update series lookup/add/refresh flows to work with the selected source.
- Update the metadata source settings UI.
- Add backend and integration tests.

### Implementation Checklist

- [ ] Add config model values:
  - [ ] `MetadataSource`
  - [ ] `TmdbApiKey` or bearer token storage
- [ ] Add V5 settings endpoint/resource for metadata source settings.
- [ ] Add frontend settings model and page for `/settings/metadatasource`.
- [ ] Introduce a metadata source selector/facade implementing:
  - [ ] `ISearchForNewSeries`
  - [ ] `IProvideSeriesInfo`
- [ ] Keep TVDB flow through `SkyHookProxy`.
- [ ] Add TMDb provider:
  - [ ] search TV series
  - [ ] lookup by TMDb ID
  - [ ] map via IMDb/TVDB external IDs
  - [ ] get show details
  - [ ] get season details
  - [ ] map series/episodes/images/ratings/external IDs
- [ ] Update add/refresh code paths for provider-selected lookups.
- [ ] Preserve TVDB compatibility where required for existing Sonarr flows.
- [ ] Add tests:
  - [ ] settings controller/resource coverage
  - [ ] selector/facade unit tests
  - [ ] TMDb proxy mapping/HTTP tests
  - [ ] series lookup/add/refresh integration coverage
- [ ] Verify with:
  - [ ] targeted backend tests
  - [ ] integration tests
  - [ ] `yarn lint`
  - [ ] `yarn build`
- [ ] Create branch, commit, and open PR when complete.

### Notes

- First implementation target: fully selectable source in UI and runtime, while preserving existing TVDB-based behavior where Sonarr still depends on a TVDB identifier.
- If TMDb content lacks a usable TVDB mapping, the code should fail clearly rather than silently producing partially-supported series records.
