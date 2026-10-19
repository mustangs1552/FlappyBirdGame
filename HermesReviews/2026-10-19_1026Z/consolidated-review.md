# Consolidated Security Review — `mustangs1552/FlappyBirdGame`

| Field | Value |
|---|---|
| Repository | `mustangs1552/FlappyBirdGame` (public) |
| Commit reviewed | `4a5d09c0c6daa13fc84eb05aed706c23845f04e6` — "Scores are now using own JSONParser." |
| Commit date / author | 2019-03-13T01:06:25Z (2019-03-12 21:06:25 −0400) / Matthew George |
| Review date | 2026-10-19 |
| Root card | `t_5cd2d674` (this consolidated report) |
| Lane cards | `t_af68f023` (injection / auth / deserialization), `t_a0c435e2` (dependencies / supply chain), `t_b8e39de8` (secrets / sensitive data) |
| Findings | **2 Critical · 3 High · 10 Medium · 8 Low** (23 total) |
| Verdict | The hand-written code is small, tidy, and free of the classic exploit classes — **no committed secrets, no SQL injection, no command injection, no path traversal, no unsafe deserialization**. The risk is concentrated in one place: the **ASP.NET Core 2.1 website**, which is an EOL framework carrying two CRITICAL, essentially unfixable CVEs, fronting a **fully unauthenticated write endpoint**. |

---

## 1. What this repo is

Two deliverables in one tree:

- **`Unity/FlappyBird/`** — a Unity 2018.2.2f1 WebGL game (8 C# scripts, ~all first-party), plus its committed 4.0 MB build output.
- **`Website/PersonalProjectSite/`** — an ASP.NET Core 2.1 MVC site (`netcoreapp2.1`) that hosts the game in an `iframe` and stores high scores in SQL Server via `System.Data.SqlClient`, with an MSTest project alongside.
- **`Database/PersonalGameSite.sql`** — schema DDL for 5 tables, no data.

353 tracked files at the pin. The website is where nearly all risk lives; the Unity half is old and carries telemetry baggage but is essentially dependency-clean.

## 2. Method and independent verification

The three lane cards each did a read-only pass (blobless clones; no code modified, no issues/PRs touched, no `gh` write calls). As the root card I did **not** simply concatenate their reports — I re-cloned at the pin and independently confirmed every headline citation before grading:

- Clone `HEAD` == `4a5d09c0c6daa13fc84eb05aed706c23845f04e6`, subject and author match the pin; no `HermesReviews/` commits present in the reviewed tree.
- `git ls-tree -r`: **353** tracked files; **75** under `bin/`/`obj/` — both figures quoted by the lanes reproduce exactly.
- Read in full at the pin: `HighScoresController.cs`, `Startup.cs`, `PersonalProjectSite.csproj`, `Views/Game/Index.cshtml`, `HighScoresDAL.AddHighScore`, `SQLUtilities.PerformSQL`, `_Layout.cshtml`, `UIManager.SaveHighScore`, `JSONParser.ParseJSON/ReadObject`, `.gitignore`, `appsettings.json`, `UnityConnectSettings.asset`.
- Confirmed absence of `[Authorize]`, `[ValidateAntiForgeryToken]`, `AddAntiforgery`, `AddIdentity`, `AddAuthentication` in **all hand-written source** (matches exist only inside `obj/`/`bin/` build artifacts).
- Confirmed package versions in the committed runtime manifest: `Microsoft.AspNetCore.App/2.1.1`, `System.Text.Encodings.Web/4.5.0`, `Newtonsoft.Json/11.0.2`.
- Confirmed no `.gitattributes`/`.gitconfig` for LFS → the `*.unityweb` blobs are real 4 MB objects, not pointers.
- Ran an independent secret sweep (keyword + credential-format patterns) over the 93 hand-written/config tracked text files. Every hit is accounted for below (§5).

Where a lane's grade and my consolidated grade differ, the change is noted inline.

## 3. Findings by severity

### CRITICAL

#### C-1 — ASP.NET Core 2.1.1: end-of-support framework with an unfixable CRITICAL CVE in the request pipeline
**File:** `Website/PersonalProjectSite/PersonalProjectSite/PersonalProjectSite.csproj:4` (`netcoreapp2.1`), `:8` (`Microsoft.AspNetCore.App` with **no `Version`**); version pinned in practice by `bin/Debug/netcoreapp2.1/PersonalProjectSite.runtimeconfig.json` → `2.1.1`.

**What is wrong.** .NET Core 2.1 reached end of support **2021-08-21**; its final release is 2.1.30 and it will never receive another security fix. The resolved graph at this commit carries **45 advisories across 24 packages**, but the decisive one is:

- `CVE-2025-55315` (GHSA grade **CRITICAL**, CVSS 9.9) in `Microsoft.AspNetCore.Server.Kestrel.Core` — HTTP request smuggling / security-feature bypass on the public listener. The affected range is `>= 0, < 2.3.6`, so **every 2.1 build is affected and no 2.1 patch exists or will ever exist**.

Because the app targets 2.1 and roll-forward stays within 2.1.x (ceiling 2.1.30), the honest statement is: *this app can only ever run an EOL framework whose newest possible patch still carries 2025- and 2026-era unpatched CVEs.* The `Microsoft.AspNetCore.App` reference with no `Version` attribute also means the actual framework version is decided by whichever SDK happens to be installed.

**Grade.** Critical **if the site is deployed and internet-reachable** — the committed `wwwroot/FlappyBirdGame/` deployment payload and the WebGL hosting imply exactly that. It degrades to High for an archive-only repository. Consolidated grade kept at Critical.

**Fix.** Retarget to a supported LTS and rebuild:
```xml
<TargetFramework>net10.0</TargetFramework>   <!-- .NET 10 LTS, supported to ~Nov 2028 -->
```
Migrate to `Microsoft.Data.SqlClient` in the same pass (see H-2). Note 2.1 → 3.1 is *not* an acceptable interim hop (EOL 2022-12-13); the shortest supported hop is .NET 8 or straight to .NET 10. Then pin the SDK via `global.json` and enable a lockfile (M-9) so the fix stays fixed.

---

#### C-2 — `System.Text.Encodings.Web` 4.5.0: CVE-2021-26701 remote code execution
**File:** resolved through `Microsoft.AspNetCore.Html.Abstractions` / `Http.Abstractions` / `WebUtilities` / `Microsoft.Extensions.WebEncoders` 2.1.1; present as a runtime asset at `bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json` (`System.Text.Encodings.Web/4.5.0` — verified at the pin).

**What is wrong.** `CVE-2021-26701` (CVSS 9.8, `AV:N/AC:L/PR:N/UI:N`) is an RCE in the HTML/JavaScript encoder — i.e. the exact component a Razor MVC site invokes to encode every piece of dynamic output. Fixed in 4.5.1 / 4.7.2 / 5.0.1; this graph carries **4.5.0**. Reachability of the vulnerable path from untrusted input in *this* app is not proven (Razor-encoded output is limited), but the affected component is on the hot path of every rendered page and the fix is a one-line version bump.

**Fix.**
```xml
<PackageReference Include="System.Text.Encodings.Web" Version="4.7.2" />
```
After the C-1 retarget the framework-supplied encoder covers it — but do not rely on the 2.1 shared framework's copy, which is the vulnerable one.

---

### HIGH

#### H-1 — Unauthenticated, unvalidated score-insert endpoint (leaderboard integrity)
**File:** `Website/PersonalProjectSite/PersonalProjectSite/Controllers/HighScoresController.cs:11-24` (read in full at the pin: `[HttpPost]` on line 11, action on line 12, `int.Parse` on lines 17 and 19), with `Models/DALs/HighScoresDAL.cs:72-93` and `Utilities/SQLUtilities.cs:61`.

**What is wrong.** `POST /HighScores/SaveNewScore?gameID=&username=&score=` is fully public. There is **no authentication or authorization anywhere in the project** — verified: no `[Authorize]`, no `AddIdentity`, no `AddAuthentication`, no identity system at all. There is no rate limiting and no validation that `username` is well-formed or that `score` is a plausible game result. Anyone, or any bot, can insert unbounded rows with any username and any score: the leaderboard — the site's only user-facing data — can be flooded, spoofed or trivially poisoned.

The only checks on the write path are `username` non-empty (`HighScoresDAL.cs:78`) and `int.Parse` in the controller, which throws `FormatException` → 500 on malformed input rather than rejecting with 400. `UIManager.cs:36`'s intended empty-username guard is broken too (see L-4), so an empty name can even originate from the game itself. The game is client-authoritative by design, which is precisely why the missing server-side gate matters.

**Not SQL injection.** `SQL_ADD_HIGHSCORE` (`HighScoresDAL.cs:16`) is a constant with `@gameID/@scoreUsername/@score` bound via `cmd.Parameters.AddWithValue`, and `PerformSQL` routes non-`SELECT` text through `ExecuteNonQuery`. The problem is purely missing auth and validation (see §5).

**Fix.** Minimum viable gate on the write path: a server-verified game-session token (HMAC issued at game start, validated at submit), server-side binding of `score` to session progress, length/charset validation on `username`, caps on score vs. achievable maximum and elapsed play time, and per-IP/username rate limiting. For a real deployment: authenticated score submission plus a moderation/delete path.

---

#### H-2 — `System.Data.SqlClient` 4.5.1: CVE-2024-0056 TLS protection bypass
**File:** `Website/PersonalProjectSite/PersonalProjectSite/PersonalProjectSite.csproj:10` (verified at the pin: `<PackageReference Include="System.Data.SqlClient" Version="4.5.1" />`).

**What is wrong.** The app's only data-access dependency is pinned six years behind:
- `CVE-2024-0056` (**HIGH**, 8.7) — SQL data-provider security-feature bypass; an on-path attacker can defeat the driver's connection protection.
- `CVE-2022-41064` (moderate) — information disclosure.

It matters here because every query goes over a network `SqlConnection` (`SQLUtilities.cs:49-57`, invoked by both DALs). Fixed in 4.8.5/4.8.6; the package is superseded by `Microsoft.Data.SqlClient`.

**Fix.** Move to `Microsoft.Data.SqlClient` 6.x and adapt the two DALs; as a stop-gap pin `System.Data.SqlClient` `4.8.6`. Require `Encrypt=True;TrustServerCertificate=False` in the connection string.

---

#### H-3 — `Newtonsoft.Json` 11.0.2: CVE-2024-21907 process-killing DoS
**File:** `bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json` (`Newtonsoft.Json/11.0.2` — verified), pulled in from `Microsoft.AspNet.WebApi.Client 5.2.6` (floor 10.0.1) and unified upward by the CodeGeneration/SDK path.

**What is wrong.** `CVE-2024-21907` (**HIGH**, 7.5, `C:N/I:N/A:H`) — improper handling of exceptional conditions lets a crafted JSON document drive a `StackOverflowException`, which is uncatchable and kills the process. Fixed in 13.0.1. Note the irony: the Unity client replaced JSON parsing with its own `JSONParser.cs` at this very commit, while the server still carries the vulnerable library.

**Fix.** `<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />`, or drop it entirely for `System.Text.Json` after the C-1 retarget.

---

### MEDIUM

#### M-1 — No CSRF/anti-forgery protection on the state-changing endpoint
**File:** `Startup.cs:25-36` (verified: `services.AddMvc()` on line 35, **no** `AddAntiforgery`), `Controllers/HighScoresController.cs:11` (no `[ValidateAntiForgeryToken]`).

Any page a user visits can silently cross-origin POST to `/HighScores/SaveNewScore`; the response is unreadable but the write lands. Impact is bounded to leaderboard rows because no user accounts or sessions exist to ride on — Medium, not High.

**Fix.** `services.AddAntiforgery()` + `[ValidateAntiForgeryToken]`, with the game obtaining and consuming the token; or switch the endpoint to the per-session token from H-1.

#### M-2 — Scores submitted over plaintext HTTP, with dead and unsafe cert-pinning code
**File:** `Unity/FlappyBird/Assets/Scripts/MattRGeorge/PortfolioSiteAccess/ScoreAccess.cs:37` (verified: `private const string ROOT_DOMAIN_URL = "http://localhost:56300/";`); request construction `:180-197` (upload), `:202-230` (fetch); stale endpoint in `Assets/Prefabs/Player.prefab:113` (`https://localhost:44390/HighScores/SaveNewScore/`).

All score uploads and reads go over unencrypted HTTP with no certificate-validation path: trivially sniffable and actively MITM-able in a deployed build. `AcceptAllSelfSignedCerts` (`:316-328`) is currently dead code (its instantiation sites at `:154/:190/:214` are commented out), but as written its "pin" is `PUBLIC_KEY = "PublicKey"` compared as a literal string — re-enabling it is the obvious next step and it is not safe. The localhost constants are baked constants, not per-environment config.

**Fix.** `https://` for the base URL, move it out of code into a serialized/ScriptableObject per-environment setting, delete the stale `uploadScoreURL` field from `Player.prefab`, and either delete `AcceptAllSelfSignedCerts` or implement real SPKI pinning.

#### M-3 — DB-backed `iframe` source rendered without scheme or origin validation
**File:** `Views/Game/Index.cshtml:7` (verified: `<iframe src="@Model.GameSrc" ...>`); data written via `GamesDAL.AddGame` (`Models/DALs/GamesDAL.cs:62-92`) and seeded from `Database/PersonalGameSite.sql`.

`gameSrc` is `varchar(255)` inserted with zero format validation and rendered directly as an `iframe src`. Razor encodes the attribute so this is **not** stored XSS, but whoever can write to `Games` controls what page loads inside the site's frame: any URL, any scheme (`javascript:` in an `iframe src` executes on some legacy engines, `data:`/`file:` similarly). There is also no `X-Frame-Options`/`frame-ancestors`, so the site's own pages can be framed. Grade Medium, or Low if the site is genuinely single-admin.

**Fix.** Validate `gameSrc` on insert against an allowlist of `https://` hosts and reject other schemes; add `X-Frame-Options: DENY` / `frame-ancestors 'none'`.

#### M-4 — Vendored jQuery 3.3.1 with published prototype-pollution and XSS advisories
**File:** `wwwroot/lib/jquery/dist/jquery.js` (header: `v3.3.1`), `…/jquery.min.js`, `.bower.json:2`; referenced at `Views/Shared/_Layout.cshtml:57-61` (CDN, SRI present) and `:52` (development).

`CVE-2019-11358` (prototype pollution), `CVE-2020-11022`, `CVE-2020-11023` (XSS via `.html()`/`.append()`), all moderate, fixed in 3.4.0/3.5.0. Loaded on every production page. Exploitability depends on whether untrusted data reaches `html()/append()/attr()` — the views render all model data through Razor encoding, so no reachable sink was found — but the version-level risk is unambiguous.

**Fix.** Upgrade vendored copy to jQuery 3.7.1, update `_Layout.cshtml` and its SRI hash to match.

#### M-5 — Vendored Bootstrap 3.3.7 (EOL 2019), seven XSS advisories, and a CDN CSS link with no SRI
**File:** `wwwroot/lib/bootstrap/.bower.json:2`, `dist/css/*`, `dist/js/*`; `Views/Shared/_Layout.cshtml:13-15` (CDN CSS with `asp-fallback-*` but **no `integrity`/`crossorigin`** — verified), `:63-67` (CDN JS, SRI present).

`CVE-2016-10735`, `CVE-2018-14040`, `CVE-2018-14042`, `CVE-2018-20676`, `CVE-2018-20677`, `CVE-2019-8331`, `CVE-2024-6485` — all moderate XSS via `data-*` attributes and tooltip/popover content. The 3.x line ended at 3.4.1 and is unsupported. The unprotected stylesheet CDN reference is the one link in the SRI set that a CDN or transport compromise goes unmitigated.

**Fix.** Move to Bootstrap 5.3.x (local copy, drop glyphicons) or at minimum vendored 3.4.1; add `crossorigin="anonymous" integrity="sha384-…"` to line 13 or serve the vendored copy only.

#### M-6 — `jquery-validation` 1.17.0 ReDoS/XSS
**File:** `wwwroot/lib/jquery-validation/.bower.json:2`; loaded via `Views/Shared/_ValidationScriptsPartial.cshtml:6-14` (CDN, versions pinned in the URL, SRI present).

`CVE-2021-21252` and `CVE-2022-31147` (ReDoS in the `url`/`url2` validators, fixed 1.19.3/1.19.5), `CVE-2025-3573` (XSS, fixed 1.20.0), `CVE-2021-43306` (low) — three of four are upstream **HIGH**, but impact here is confined to the visitor's own browser (client-side validation only, no server-side regex fed by the same pattern), so consolidated grade is Medium. `jquery-validation-unobtrusive` 3.2.9 has no advisories but is from 2017.

**Fix.** jQuery Validation 1.20.0+, Unobtrusive 4.0.0, vendored locally with a `.bower.json`/`package.json` entry so SCA tooling can see them.

#### M-7 — Design-time scaffolding package is a production dependency (a compiler in the web app's manifest)
**File:** `PersonalProjectSite.csproj:9` (`Microsoft.VisualStudio.Web.CodeGeneration.Design` 2.1.6 — verified), also in the test csproj at `:11`.

An ordinary `PackageReference` (not `PrivateAssets`) drags the whole scaffolding stack into the app: Roslyn `Microsoft.CodeAnalysis.CSharp/Workspaces 2.8.0`, EF Core `Design`/`Tools`/`Relational`/`SqlServer`, `NuGet.Frameworks`. The committed `deps.json` lists **33** `…CodeGeneration*` entries as runtime components. Unnecessary attack surface and avoidable upgrade coupling; it is not needed in the test project either.

**Fix.** Remove from both projects; if kept for developer convenience, mark `PrivateAssets="all"` / `DevelopmentDependency` so `dotnet publish` excludes it.

#### M-8 — 75 committed build-output files (2.9 MB) of unverifiable provenance, including third-party DLLs
**File:** `Website/PersonalProjectSite/PersonalProjectSite/bin/Debug/netcoreapp2.1/`, `…/obj/Debug/netcoreapp2.1/` (54 files), `PersonalProjectSiteTests/bin|obj`; `.gitignore:1-8` (verified — covers only `Unity/FlappyBird/obj/`, never `Website/` `bin`/`obj`). Count verified at the pin: exactly 75 tracked `bin|obj` files.

The committed assemblies are a **2019 Debug build from the author's workstation**; nothing ties them to this tree, so a deploy that copies `bin/` runs code that cannot be verified against source — the classic prebuilt-binary supply-chain gap. Three `Microsoft.VisualStudio.TestPlatform.MSTest*` DLLs are vendored inside with no license or provenance file. `obj/` adds `project.assets.json`, `*.nuget.cache` and Razor codegen output that go stale silently.

**Fix.** `git rm -r --cached` the four `bin`/`obj` trees, then add `[Bb]in/` and `[Oo]bj/` to `.gitignore`. Build from source in a pipeline (L-6).

#### M-9 — No lockfile, no SDK pin, and a floating framework reference: restores are not reproducible
**File:** `PersonalProjectSite.csproj:8` (no `Version`); absence of `global.json`, `packages.lock.json`, `NuGet.config` (all verified absent).

Three drift sources: (1) the framework reference resolves against whichever SDK is installed; (2) no `global.json` means different machines restore differently; (3) no lockfile means the 295-package graph is only *recorded* after the fact in `obj/project.assets.json`, which is a cache, not a contract — a `dotnet restore` today can silently pick different transitive versions than the committed build used.

**Fix.** `<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>`, commit `packages.lock.json`, add a `global.json` pinning the SDK with `"rollForward": "latestPatch"`, and use `dotnet restore --locked-mode` in CI. This is a prerequisite for any of the CVE fixes being *verifiably* fixed.

#### M-10 — Shipped WebGL build reports telemetry to Unity at startup; unused ads/analytics/purchasing packages retained
**File:** `Unity/FlappyBird/ProjectSettings/UnityConnectSettings.asset:20-22` (verified: `UnityAnalyticsSettings: m_Enabled: 1`, `m_InitializeOnStartup: 1`, `m_TestMode: 1`), `:14` (`CrashReportingSettings: m_Enabled: 1`); `Packages/manifest.json:3-6`; `ProjectSettings.asset` (`cloudProjectId`, `organizationId: mustangs1552`).

No game script references Analytics, Ads or IAP (`grep` over `Assets/Scripts/`: **0 hits**), yet the built payload initializes the analytics runtime on page load and begins sending events to Unity's cloud endpoints — third-party network egress and visitor data collection nobody asked for. `com.unity.ads 2.0.8` and `com.unity.purchasing 2.0.3` remain in the manifest with their settings disabled: dead SDK surface compiled against the project. `cloudProjectId`/`organizationId` are identifiers, not credentials.

**Fix.** Set `m_Enabled: 0` and `m_InitializeOnStartup: 0` for analytics, `m_Enabled: 0` for crash reporting; remove `com.unity.ads` and `com.unity.purchasing`; rebuild and re-commit the WebGL output. If analytics is wanted, make it a conscious decision with a privacy notice.

---

### LOW

#### L-1 — Development endpoints and developer-machine paths committed
**Files:** `ScoreAccess.cs:37` (`http://localhost:56300/`), `Assets/Scenes/SampleScene.unity:238` (`http://localhost:56603/`), `Assets/Prefabs/Player.prefab:113` (`https://localhost:44390/…`, stale — its GUID resolves to a `Score.cs` that no longer declares `uploadScoreURL`), plus `obj/**/*.FileListAbsolute.txt` (`C:\Users\Matthew George\…`, `D:\SourcetreeProjects\…`), `bin/Debug/netcoreapp2.1/PersonalProjectSite.runtimeconfig.dev.json` (`C:\Users\musta\.nuget\packages`), `PersonalProjectSite.csproj.user`.

No credentials — these are environment residue: dev-machine host/port assumptions baked into source and serialized assets, and the author's Windows username/home layout exposed. Practical impact minimal (the author's name is already public in git history), but it is exactly the residue that M-8/L-2 exist to stop, and two of the three URLs are plain `http`.

**Fix.** Inject the base URL per environment, delete the localhost overrides and the orphaned prefab field, untrack `bin`/`obj`, and add `*.csproj.user`, `*.FileListAbsolute.txt` to `.gitignore`. For the path leak in history, `git filter-repo` if it matters.

#### L-2 — 4.2 MB Unity WebGL build committed twice, under a `.gitignore` rule that says otherwise
**Files:** `Unity/FlappyBird/Builds/WebGL/Build/` (17 files: `WebGL.data.unityweb` 1.0 MB, `WebGL.wasm.code.unityweb` 2.9 MB, framework, `UnityLoader.js`, `WebGL.json`) — `.gitignore:4` ignores `Unity/FlappyBird/Builds/` but the files were added before the rule and stay tracked; `Website/…/wwwroot/FlappyBirdGame/Build/` — byte-identical duplicate (same blob SHAs) served as deployable content.

Content check came back clean: the `.unityweb` payloads are verified gzip streams, no recoverable plaintext or embedded config in the sampled headers, `WebGL.json` lists only relative asset URLs (no remote code fetch). The issue is hygiene and drift, not payload.

**Fix.** `git rm -r --cached Unity/FlappyBird/Builds`; keep the `wwwroot` copy as the single deployment artifact, or better, produce it in CI.

#### L-3 — Hand-rolled JSON parser: unbounded recursion and O(n²) string handling
**File:** `Unity/FlappyBird/Assets/Scripts/MattRGeorge/Utilities/JSONParser.cs` — `ParseJSON:222`, `ReadObject:132-188`, `ReadString:111-124`, `ReadObjectArray:17`; consumer `ScoreAccess.cs:163, 224-225`.

The recursive `ReadObject`/`ReadArray`/`ReadObjectArray` cycle has no depth or size limit, so a deeply nested server response (`[{[{…`) overflows the stack and crashes the client; quadratic concatenation in `ReadString`/`ReadPrimitiveValue` makes large responses hang it. It also discards JSON escape sequences (ignoring backslashes), corrupting `\"` inside server strings — a correctness bug. `ScoreValue(JSONObject)` silently maps parse failures to `0`. Client-only and attacker-controlled only via the M-2 MITM path, hence Low.

**Fix.** Use a bounded parser (depth cap + max input length) or Unity's `JsonUtility`; validate parsed fields against expected ranges instead of defaulting to `0`.

#### L-4 — Robustness and hygiene nits (no standalone exploit found)
- `Controllers/HighScoresController.cs:17-19` — `int.Parse` on untrusted query strings throws unhandled `FormatException` → 500. Use `int.TryParse` and return 400.
- `Utilities/SQLUtilities.cs:45,61` — `PerformSQL` accepts arbitrary SQL text from any caller *and* sniffs the statement type with `sqlString.Substring(0, 6).ToUpper() == "SELECT"`. Every current caller passes a constant with bound parameters, so it is **not** injectable today, but the free-form overload plus 6-character heuristic is a design that invites future injection. Deprecate the free-form overload in favor of the parameterized one.
- `Assets/Scripts/UIManager.cs:36` — `if (inputField.text != null || inputField.text != "")` is always true (`||` instead of `&&`), so the intended empty-username guard never fires and empty names can be submitted. Switch to `&&`.
- Models use `uint` (`GamesModel.cs:1` and siblings) — CLS-uncompliant, no security impact.

#### L-5 — 2018-era test toolchain
**File:** `PersonalProjectSiteTests.csproj:10-13` — `Microsoft.NET.Test.Sdk` 15.9.0, `MSTest.TestAdapter/TestFramework` 1.3.2.

No advisories published, so not a vulnerability — but seven years behind, on unsupported branches, and the `netcoreapp2.1` target means the tests cannot run on a modern runner. Move to .NET Test SDK 17.x + MSTest 3.x (or xUnit) on the retargeted framework.

#### L-6 — No CI at all (and therefore none of the classic CI abuse vectors)
Verified absent: `.github/workflows/**`, `.gitlab-ci.yml`, `azure-pipelines.yml`, `Jenkinsfile`, `Dockerfile`, `Makefile`, `*.sh`, `*.ps1`, `*.bat`.

No automated SCA, no rebuild-and-compare against the committed binaries, no locked restore — this review is a point-in-time manual sweep. The flip side, recorded so it is not mistaken for an omission: the repo therefore contains **no CI secrets, no `curl … | bash` installers, no build scripts that execute third-party code, and no npm/`postinstall` hooks** (there is no `package.json` at all).

**Fix.** A minimal workflow: `dotnet restore --locked-mode` → `dotnet build -c Release` → `dotnet test` → `dotnet list package --vulnerable --include-transitive`, plus a job that fails if `bin`/`obj` are tracked.

#### L-7 — Third-party art/audio/font assets with unverified license provenance
**Files:** `Assets/FlappyCopters/_ReadMe/_READ_ME.txt:5-9` (font "zorque" — "download … from the internet. Make sure you adhere to license terms"), `:13-15` (sound effects from `sounds-resource.com`, same caveat); `Assets/Birds Pack/**` (no license file); six `*.ai` sources (~6.9 MB).

The asset pack's own readme declines to state a license and points at third-party sites; the bird sprites ship no license at all. That is a redistribution/legal risk rather than a code-execution one, but these assets are embedded into the shipped WebGL build, so it belongs in a security/risk review.

**Fix.** Record explicit license/attribution in `THIRD_PARTY.md` or replace with CC0/original assets; consider moving `.ai` sources out of the repo.

#### L-8 — Unity 2018.2.2f1: EOL editor, committed build cannot be rebuilt with fixes
**File:** `ProjectSettings/ProjectVersion.txt:1` (verified: `m_EditorVersion: 2018.2.2f1`).

The editor line is long unsupported, so the committed WebGL payload cannot be rebuilt with security fixes, and anyone opening the project is asked to install an unpatched 2018 editor. For accuracy: `CVE-2025-59489` (the Unity Runtime vulnerability, Oct 2025) affects Android/Windows/Linux/macOS builds and **does not apply to this WebGL-only build** — cited only as the kind of issue an EOL toolchain can never be patched for. No Unity package here carries a published advisory of its own; `com.unity.ads`/`analytics`/`purchasing` 2.x are simply abandoned lines.

**Fix.** Nothing required if the artifact is frozen — document that the WebGL build is a 2019 release artifact, not rebuildable with modern tooling. If maintained, port to Unity 6 LTS and re-export.

---

## 4. Remediation order

1. **C-1** — retarget to .NET 10 LTS. This single change subsumes C-2 and most of the framework advisories (CVE-2025-55315 becomes moot), and is a precondition for the rest being verifiable.
2. **H-2 / H-3 / M-9** — `Microsoft.Data.SqlClient` 6.x, `Newtonsoft.Json` 13.0.3 (or `System.Text.Json`), and a lockfile + `global.json` pin so fixes stay fixed.
3. **H-1 / M-1** — put a gate on `SaveNewScore`: session token, server-side score validation, rate limiting, anti-forgery.
4. **M-8 / L-1 / L-2** — untrack `bin`/`obj` and `Unity/FlappyBird/Builds`, extend `.gitignore`, stop committing build output.
5. **M-4 / M-5 / M-6** — front-end upgrades (jQuery 3.7.1, Bootstrap 5.3.x, jQuery Validation 1.20.0+) and add SRI to the one unprotected CDN link.
6. **M-2 / M-3 / M-10** — HTTPS + real pinning for the game client, allowlist `gameSrc` + `X-Frame-Options`, disable Unity analytics/crash reporting and drop unused SDKs.
7. **M-7, L-3, L-4, L-5, L-6, L-7, L-8** — hygiene, parser hardening, test toolchain, CI, asset licensing, toolchain documentation.

## 5. Explicitly checked and clean

Listed so nobody re-litigates these as open questions — each was examined by at least one lane and re-confirmed at the pin by the root.

- **Secrets / credentials — clean.** Every one of the 353 tracked files was enumerated and scanned (keyword, high-entropy, credential-URL, filename-pattern passes, plus manual reads of every config/DAL/controller/Unity-settings file and binary greps). Zero credentials, tokens, API keys or private keys. An independent root-level re-sweep over the 93 hand-written/config text files accounted for every hit:
  - `appsettings.json` / `appsettings.Development.json` — log levels and `AllowedHosts: "*"` only.
  - Connection strings (`GameController.cs:9`, `HighScoresController.cs:9`, `HomeController.cs:8`, both `*DALTests.cs:12`) — `Data Source=.\SQLEXPRESS;…;Integrated Security=true;`, **no username or password**. Hardcoding them rather than reading configuration is a bad practice (and `AllowedHosts: "*"` plus Integrated-Security-only auth are loose defaults), but nothing is leaked.
  - `psp2PackagePassword: F69A…exui (32 chars)` (`ProjectSettings.asset:495`) — **scanner bait, not a secret.** It sits in an otherwise entirely blank PS Vita platform block (`psp2NPCommsPassphrase`, `metroCertificatePassword`, `ps4NPTitleSecret` all empty) in a project still named `Template_2D`, i.e. the stock Unity 2D template default value. No mobile/console signing material is present.
  - `database/PersonalGameSite.sql` — schema DDL for 5 tables, no rows, no users, no PII.
  - No `.env`-style files, `.pem/.p12/.jks/.key`, logs, `.bak`, `.idea`, `.vscode` or `.DS_Store` tracked; no `user:pass@host` URLs. `Contact.cshtml` holds only ASP.NET template placeholders (`425.555.0100`).
  - Unity `cloudProjectId`/`organizationId` are identifiers, not credentials.
- **SQL injection — none.** Every query string in `GamesDAL`/`HighScoresDAL` is a constant with `@`-parameters bound via `cmd.Parameters.AddWithValue` (`SQLUtilities.cs:49-58`). No concatenated SQL exists at this commit. (The *design* smell in L-4 is future risk, not a present hole.)
- **Unsafe deserialization — none.** No `BinaryFormatter`, no `XmlSerializer`, no type-name-based JSON deserialization. The only parser is the first-party `JSONParser.cs`, which builds no object graph from type names (its weaknesses are availability-only, L-3).
- **Command injection / path traversal — none.** No `System.IO`, `Process.Start` or shell invocation in any hand-written code, website or game; no file or network state written from user input.
- **Auth surface — nothing misconfigured, because there is no auth.** No login/registration/identity system exists at all: no default credentials, no session config to get wrong. The whole finding is the missing gate (H-1).
- **Memory safety — not applicable.** Entirely managed C#/Unity: no `unsafe` blocks, pointers or P/Invoke.
- **Vendored JS is not tampered with.** jQuery/Bootstrap/Validation carry intact upstream banners and standard UMD footers; no appended payload, no `atob(`/`fromCharCode` obfuscation, no `eval(` outside upstream code. Old, not backdoored.
- **SRI coverage.** Present on all four CDN `<script>` tags (`_Layout.cshtml:60-61`, `:66-67`; `_ValidationScriptsPartial.cshtml:10,14`). Only the CDN stylesheet (M-5) lacks it.
- **No remote code fetch at build or run time.** No `curl | bash`, no install hooks, no non-default `NuGet.config` feed, no downloaded MSBuild `.targets`; `WebGL.json` uses only relative asset URLs.
- **The two WebGL copies are in sync** — identical blob SHAs between `Unity/FlappyBird/Builds/WebGL/` and `wwwroot/FlappyBirdGame/`, so no silent drift between source-of-truth build and what the site serves.
- **Unity manifest fully pinned** — 35 exact versions, no `^`/`*`/`latest` (a genuine positive; the .NET side is the opposite, M-9).
- **Game logic** — score counting (`FlappyPlayer.OnTriggerExit2D`, `Score.cs`), pillar spawning and UI flow show no exploitable logic bug. The game is client-authoritative by design, which is exactly why H-1 matters.

## 6. Scope and limits

**Covered.** All 10 hand-written website controllers/DALs/utilities and Razor views; all 8 Unity C# scripts (read end-to-end); SQL schema; all manifests (`Unity/Packages/manifest.json`, two `.csproj`, Bower `.bower.json`); the full resolved .NET graph (295 packages) extracted from the committed `project.assets.json` + `deps.json` and checked against OSV.dev (55 package↔advisory pairs, 45 distinct advisories, 24 packages); vendored front-end libraries and their SRI; committed binaries/artifacts and their provenance; Unity project/telemetry settings; secrets sweep across all 353 files.

**Not covered.** No code was built or executed and no requests were sent (static review only — H-1 was not demonstrated against a live host). Committed compiled binaries (`bin/`, `obj/`, `*.unityweb` payloads) were not decompiled; only magic bytes, gzip validation and string greps, so a compromised payload inside them cannot be positively excluded. Third-party vendored JS was checked for tampering by heuristic, not audited line by line. Runtime/deployment configuration (web server, TLS termination, SQL Server hardening, whether the site is actually public) is not visible from the repository — the Critical grades for C-1 assume internet exposure. Git history beyond the pin was not swept beyond what the LFS/artifact checks needed.

---

### Family map (for traceability)

| Card | Scope | Doc in this folder | Findings as graded in that lane |
|---|---|---|---|
| `t_5cd2d674` (root) | Consolidation + independent verification | `consolidated-review.md` (this file) | 2C / 3H / 10M / 8L |
| `t_af68f023` | Injection, deserialization, auth/authz, insecure defaults | `injection-auth-review.md` | 0C / 1H / 3M / 2L |
| `t_a0c435e2` | Dependencies, supply chain, vendored code, build/CI | `dependency-supply-chain.md` | 2C / 2H / 7M / 5L |
| `t_b8e39de8` | Hardcoded secrets, sensitive-data exposure | `secrets-sensitive-data.md` | 0C / 0H / 0M / 3L (no secrets exposed) |

Consolidation merged duplicate findings across lanes (dev endpoints appeared in both the secrets and injection lanes; `bin`/`obj` provenance in both the secrets and dependency lanes) and re-graded nothing downward without stating why. Lane docs are reproduced verbatim and may overlap each other — that is expected; this file is the authoritative merged view.
