# Secrets & Sensitive-Data Exposure Audit — mustangs1552/FlappyBirdGame

**Commit:** `4a5d09c0c6daa13fc84eb05aed706c23845f04e6` ("Scores are now using own JSONParser.", Matthew George, 2019-03-13T01:06:25Z)
**Lane:** hardcoded secrets/credentials/tokens/API keys + sensitive data exposure (config files, logs, committed artifacts, build scripts, .env-style files, embedded strings in game assets)
**Card:** t_b8e39de8 · Repo state at review: 353 tracked files (excluding `HermesReviews/`) — Unity 2018 WebGL project (`Unity/FlappyBird/`) + ASP.NET Core 2.1 site (`Website/PersonalProjectSite/`)

**Verdict: no credentials, tokens, API keys, or private key material are exposed anywhere in this commit.** Three Low findings — all committed development-environment residue (dev endpoints, machine paths, build artifacts) rather than exploitable secret exposure.

---

## Findings

### 1. Hardcoded development endpoints committed in game source and Unity assets — Low

- `Unity/FlappyBird/Assets/Scripts/MattRGeorge/PortfolioSiteAccess/ScoreAccess.cs:37` — `private const string ROOT_DOMAIN_URL = "http://localhost:56300/";`
- `Unity/FlappyBird/Assets/Scenes/SampleScene.unity:238` — scene override `rootDomainURL: http://localhost:56603/` (on the `Score` component instance)
- `Unity/FlappyBird/Assets/Prefabs/Player.prefab:113` — serialized value `uploadScoreURL: https://localhost:44390/HighScores/SaveNewScore/`

**What is wrong:** These are dev-machine URLs baked into source and serialized assets. They contain no credentials, so nothing is "leaked" in the secret sense, but they are committed environment configuration: if the project is built as-is, the game posts player names/scores to localhost and silently fails. Two of the three use plain `http`. Additionally `Player.prefab:113` is stale — its GUID (`1ab155c7cde89e14599f434784536d1a` → `Score.cs.meta`) resolves to a `Score.cs` that no longer declares an `uploadScoreURL` field, dead data left over from the pre-`ScoreAccess` design (SampleScene:233 sets the same property to the relative path `HighScores/SaveNewScore`).

**Fix:** Point `ScoreAccess.ROOT_DOMAIN_URL` at the production HTTPS domain (or inject it via a ScriptableObject/`ServerURL`-style preset / environment-specific build define), delete the localhost overrides in `SampleScene.unity` and `Player.prefab`, and delete the orphaned `uploadScoreURL`/`rootDomainURL` serialized blocks from the prefab.

### 2. Developer-machine absolute paths committed in tracked build intermediates — Low

- `Website/PersonalProjectSite/PersonalProjectSite/obj/Debug/netcoreapp2.1/PersonalProjectSite.csproj.FileListAbsolute.txt:1-…` — records `C:\Users\Matthew George\Documents\Workspace\PersonalProject\FlappyBirdGame\…`
- `Website/PersonalProjectSite/PersonalProjectSiteTests/obj/Debug/netcoreapp2.1/PersonalProjectSiteTests.csproj.FileListAbsolute.txt:1-…` — records `D:\SourcetreeProjects\FlappyBirdGame\…`

**What is wrong:** Per-machine MSBuild intermediate files leak the author's Windows username/home layout and a second machine's directory layout. The author's name is already public via git history, so practical impact is minimal — this is environment disclosure, not a credential. It is a symptom of the larger problem in Finding 3 (`obj/`/`bin/` tracked at all, including `*.cache`, `CopyComplete`, `project.assets.json`, Razor `*_.g.cshtml.cs`).

**Fix:** Untrack intermediates: `git rm -r --cached` the `Website/**/obj` and `Website/**/bin` trees, then ignore them (`obj/`, `bin/`, plus `*.user`). The existing `.gitignore:8` covers only `Unity/FlappyBird/obj/` — extend it to the Website side or add a root-level `obj/`/`bin/` rule.

### 3. Committed build artifacts — including a build output the .gitignore explicitly ignores — Low (hygiene, sensitive-adjacent)

- `Unity/FlappyBird/Builds/WebGL/Build/` — 4.2 MB of release build binaries (`WebGL.data.unityweb` 1.0 MB, `WebGL.wasm.code.unityweb` 2.9 MB, `WebGL.wasm.framework.unityweb`, `UnityLoader.js`, `WebGL.json`) committed even though `.gitignore:4` says `Unity/FlappyBird/Builds/` — files were added before the ignore rule and remain tracked.
- `Website/PersonalProjectSite/PersonalProjectSite/wwwroot/FlappyBirdGame/Build/` — byte-identical duplicate of the same 4.2 MB build, committed as deployable site content.
- `Website/…/bin/Debug/netcoreapp2.1/` and `Website/…/obj/…` — full Debug/Release-site build outputs (`*.dll`, `*.pdb`, `PersonalProjectSite.runtimeconfig*.json`, `PersonalProjectSite.deps.json`, Razor codegen, caches) tracked under a Web project that was never covered by any ignore rule.

**What is wrong:** Binary and intermediate artifacts in version control bloat the repo, guarantee stale-artifact drift, and broaden the surface for environment leakage of exactly the kind verified in Finding 2. (Content check: the `.unityweb` payloads are gzip streams, `1f 8b` magic verified via `od`; no plaintext or embedded config/paths were recoverable in the sampled headers, and greps of the tracked `dll`/`pdb` binaries showed no user paths or credential strings. These files are git-committed directly, not LFS pointers — LFS objects were excluded per instructions.)

**Fix:** Untrack `Unity/FlappyBird/Builds/` (the rule exists; run `git rm -r --cached Unity/FlappyBird/Builds`) and the `Website` `bin`/`obj` trees; keep only the deployment copy under `wwwroot/` if the site relies on it, or better, produce it in CI. For the path-leak history in Finding 2, consider a history rewrite (`git filter-repo`) since blobs remain in old commits.

---

## Explicitly checked and clean (this scope)

- **No hardcoded secrets of any kind:** repo-wide greps over all 353 tracked files for `api[ _-]?key / secret / token / password / credential / bearer / oauth / smtp / sendgrid / twilio / stripe`, known token formats (`AKIA…`, `ghp_…`, `sk_live/test`, `AIza…`, `xox…`, `BEGIN … PRIVATE KEY`), and high-entropy strings (≥32-hex / ≥30-char base62 runs) produced zero real hits — only `PublicKeyToken=null` assembly refs, NuGet package names, and code identifiers.
- **`appsettings.json` / `appsettings.Development.json`:** logging levels + `AllowedHosts` only; no connection strings, no secrets.
- **Connection strings (`GameController.cs:9`, `HighScoresController.cs:9`, `HomeController.cs:8`, both `*DALTests.cs:12`):** `Data Source=.\SQLEXPRESS;Initial Catalog=…;Integrated Security=true;` — no username/password embedded. (Whether Windows-auth-to-SQLExpress is an insecure default is the auth lane's question, not this one.)
- **`Database/PersonalGameSite.sql`:** schema DDL only (5 tables) — no data rows, no users/logins, no PII.
- **No `.env`-style files, key files (`.pem/.p12/.jks/.key`), `.log`, `.bak`, `.idea`, `.vscode`, `.DS_Store` tracked. No credential-bearing URLs (no `user:pass@host`).
- **`UnityConnectSettings.asset` / `ProjectSettings.asset`:** Unity Analytics enabled but in TestMode 1 with blank test URLs; Ads/Purchasing/PerformanceReporting disabled — default telemetry config, no project keys or secrets committed. `WebGL.json`: `DefaultCompany`/`FlappyBird`, no identifiers.
- **`PersonalProjectSite.csproj.user` / `launchSettings.json`:** VS scaffolder state and local dev server ports only — no secrets (dev-only files, standard to ignore going forward).
- **Personal data:** `Contact.cshtml` contains only ASP.NET template placeholder data (`425.555.0100`, `Support@example.com`); player-name handling (`Score.cs:54`, `ScoreAccess.cs`) contains test names only in code — no real PII.
- **Tracked binaries:** `dll`/`pdb` grep'd for `C:\Users`-style paths — none; `.unityweb` gzip header validated, contents not decompressed (see Finding 3 note).

## Scope covered / not covered

**Covered:** all 353 tracked files at the pinned commit (LFS smudge skipped by design): full file inventory, keyword + high-entropy + credential-URL + filename-pattern passes, manual read of every sensitive-prone text file (appsettings, launchSettings, `.csproj.user`, `.gitignore`, `.sql`, `ScoreAccess.cs`, `Score.cs`, all Controllers/DALs/`SQLUtilities`, UnityConnectSettings/ProjectSettings excerpts, prefab/scene serialized-endpoint entries), binary greps of committed dll/pdb, and header validation of committed `.unityweb` payloads.

**Not covered (other lanes of this review family):** SQL/command injection and non-parameterized query paths (`SQLUtilities.PerformSQL`), auth/authorization and insecure defaults (Integrated Security, disabled cert-pinning logic in `ScoreAccess.cs:316-328`, plaintext HTTP score submission), vulnerable/unpinned dependencies (ASP.NET Core 2.1.1 packages), and full decompression of the committed WebGL build payloads.