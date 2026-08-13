# Plan & Fortschritt: Metadaten in deutscher Sprache laden

> **Ziel:** Sonarr soll Serien-/Folgen-Metadaten standardmaessg auf **Deutsch** laden.
> **Status:** Implementierung abgeschlossen & verifiziert (Backend-Build, Frontend-Lint/-Build, Massen-Import getestet).
> **Letzter Stand:** 13.08.2026, 10:06

---

## 1. Ursprüngliche Recherche

- `SonarrCloudRequestBuilder`: SkyHook-URL mit festem `{language}`-Segment (`"en"`).
- SkyHook liefert nur Englisch (Test: `de`, `de-DE`, `deu`, `ger`, `query lang` -> alle 400).
- `ShowResource.Language` ist auskommentiert -> keine Sprachsteuerung ueber SkyHook.

## 2. Entscheidung: TMDB statt TVDB

- **TVDB** wurde zunaechst probiert, aber verworfen:
  - Login v4 verlangt zusaetzlich eine **PIN** (`InvalidValueType: pin required`) -> unnoetig komplex.
  - Der komplette TVDB-Weg (`TheTvdb`-Ordner, `TvdbApiKey`/`TvdbApiPin`/`TvdbEnabled`, PIN-Login, Suche/Uebersetzung) wurde **vollstaendig entfernt**.
- **TMDB gewaehlt**: einfacher **Read Access Token** (JWT, beginnt mit `eyJ...`) bzw. v3-`api_key`, keine PIN, exzellente deutsche Uebersetzungsabdeckung.

---

## 3. Implementierung: Direkter TMDB-Client als Uebersetzungsquelle

Neuer Namespace: **`NzbDrone.Core.MetadataSource.Tmdb`**

| Datei | Zweck |
|-------|-------|
| `TmdbResources.cs` | DTOs: Find/ExternalIds, Translations (Serie+Episode), Search |
| `TmdbClient.cs` | API-v3-Client (`api.themoviedb.org/3`), **max. 5 parallele Requests (SemaphoreSlim)**; Auth: `eyJ`-Token -> `Authorization: Bearer`-Header, sonst `api_key`-Query; Caches TVDB<->TMDB-ID-Mappings (7 Tage); Fehler-Logging inkl. Antwortinhalt |
| `TmdbSearchService.cs` | Suche (deutscher Titel) + `GetSeriesByTmdbId`; entfernt **(Jahr)** am Ende des Suchbegriffs (TMDB findet z.B. "1883 (2021)" nicht, nur "1883") |
| `TmdbTranslationService.cs` | `ApplyTranslations(series, episodes)`: ueberschreibt Titel/Overview der Serie & Episoden (de); Fallback auf SkyHook-Daten bei Fehler/fehlender Uebersetzung |

## 4. Konfiguration

| Datei | Aenderung |
|-------|----------|
| `IConfigService.cs` / `ConfigService.cs` | `TmdbApiKey` (string, ""), `TmdbEnabled` (bool, false) - zusaetzlich vorhanden: `MetadataLanguage` (int, Englisch) |

## 5. API & Frontend

| Datei | Aenderung |
|-------|----------|
| `MetadataSourceSettingsResource.cs` | `tmdbApiKey`, `tmdbEnabled` |
| `MetadataSourceSettingsController.cs` | `GET/PUT /api/v5/settings/metadatasource`; Validierung `TmdbApiKey` max. **500 Zeichen** (JWT-Token) |
| `useMetadataSource.ts` | Hook (useManageSettings) |
| `MetadataSourceSettings.tsx` | Seite `/settings/metadatasource`: TMDB-API-Key (Passwort) + "TMDB aktivieren"-Checkbox |
| `en.json`/`de.json`/`de_DE.json` | Keys: `TmdbApiKey`, `TmdbApiKeyHelpText`, `TmdbEnabled`, `TmdbEnabledHelpText`, `MetadataSourceSettingsLoadError` |
| `UISettings.tsx` / `UiSettingsController.cs` / `UiSettingsResource.cs` | "Metadata Language"-Dropdown + Validierung; Feld in Resource |

## 6. Refresh-Integration (Serien)

`RefreshSeriesService.RefreshSeriesInfo` ruft nach `GetSeriesInfo` (SkyHook, en) nun
`_tmdbTranslationService.ApplyTranslations(series, episodes)` auf -> deutsche Titel/Overviews.
TMDB-ID wird aus `series.TmdbId` oder per `GetTmdbIdFromTvdbId` (Lookup) ermittelt.

## 7. Suche & Import

`SkyHookProxy`:
- **Suche:** Wenn `TmdbEnabled && MetadataLanguage!=en && Key gesetzt` -> **TMDB-Suche** (deutsch, "(Jahr)" wird entfernt). Sonst SkyHook (`en`).
- **GetSeriesInfo:** Sprachsegment fest **`en`** (SkyHook akzeptiert nur `en`; deutsche Daten kommen danach via TMDB). Behebt den frueheren Importfehler `shows/de/... -> 400`.

---

## Verifikation

- `dotnet build src/Sonarr.sln` -> **0 Fehler / 0 Warnungen**
- `yarn lint` -> **0 Fehler**; `yarn build` -> **compiled successfully**
- `en.json` / `de.json` / `de_DE.json` -> gueltig
- **Massen-Import getestet:** 100+ Serien erfolgreich angelegt/refresht mit **deutschen Titeln**
- TVDB-Code vollstaendig entfernt (Backend-Suche, Frontend, Ordner `TheTvdb`, Config-Keys)

---

## Manueller Test

1. **Einstellungen -> Metadatenquelle**:
   - `TMDB API-Key` eintragen (**API Read Access Token**, beginnt mit `eyJ...` - ca. 230 Zeichen)
   - `TMDB aktivieren` aktiv, Speichern
2. **Einstellungen -> UI -> Sprache -> "Metadata Language" = Deutsch**
3. Serie suchen (z.B. "1883 (2021)") -> TMDB findet Treffer (Jahr wird entfernt)
4. Serie hinzufuegen/refreshen -> SkyHook (en) laedt Stammdaten, TMDB ueberschreibt Titel/Overviews ins Deutsche

Hinweise:
- Wo TMDB keine deutsche Uebersetzung fuehrt, bleibt der englische/Original-Titel (Fallback).
- Fehler beim TMDB-Zugriff werden geloggt (inkl. Status + Antwortinhalt); Sonarr faellt auf SkyHook(en) zurueck.

## Verwandte Notizen

- `global.json` verlangt 10.0.302 -> installiert 10.0.400 -> `rollForward: "latestFeature"` gesetzt.
- `de_DE.json` als Kopie von `de.json` beigelegt -> keine `Missing translation/culture resource`-Meldung mehr.
