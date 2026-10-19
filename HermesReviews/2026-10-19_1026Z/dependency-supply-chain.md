# Dependency & Supply-Chain Risk Assessment — `mustangs1552/FlappyBirdGame`

| Field | Value |
|---|---|
| Repository | `mustangs1552/FlappyBirdGame` (public) |
| Commit reviewed | `4a5d09c0c6daa13fc84eb05aed706c23845f04e6` — "Scores are now using own JSONParser." |
| Commit date / author | 2019-03-13T01:06:25Z (Tue Mar 12 21:06:25 2019 −0400) / Matthew George |
| Review date | 2026-10-19 |
| Scope | Dependency inventory, unpinned/floating versions, known-vulnerable & abandoned packages, vendored third-party code, committed binaries, build/CI abuse paths |
| Method | Read-only: blobless shallow clone at the pinned SHA (`git rev-parse HEAD` verified = pin), file inventory over `git ls-tree`, manifest parsing, committed `obj/project.assets.json` + `bin/*.deps.json` graph extraction, and OSV.dev `querybatch` against all 295 resolved NuGet packages plus the 4 vendored npm libraries. No project code was installed, built or executed. |
| Findings | **2 Critical · 2 High · 7 Medium · 5 Low** |

---

## Summary

The repository is two deliverables in one tree: a **Unity 2018.2.2f1 WebGL game** (source + committed
build output) and an **ASP.NET Core 2.1 MVC site** (`netcoreapp2.1`) with an MSTest project that hosts
the game and a SQL Server high-score table.

The Unity half is close to dependency-clean: 35 exactly-pinned package/module entries, no scripting
third-party libraries, a self-written `JSONParser.cs` with no external parser dependency. Its issues are
age and telemetry, not supply-chain depth.

The .NET half is where the risk is. The framework line (**ASP.NET Core 2.1**, end of support
**2021-08-21**, last patch 2.1.30) carries **45 advisories across 24 packages in the resolved graph**,
including two **CRITICAL** CVEs — Kestrel request smuggling `CVE-2025-55315` (no 2.1 fix will ever exist)
and the encoder RCE `CVE-2021-26701`. A design-time scaffolding package is also pulled into the
production dependency graph, there is no lockfile or SDK pin, and 75 build-output files (2.9 MB,
including third-party DLLs and this project's own debug assemblies) are committed, so what a deploy
actually runs cannot be tied back to source.

The vendored front-end libraries (jQuery 3.3.1, Bootstrap 3.3.7, jquery-validation 1.17.0) are old and
carry published XSS/ReDoS advisories, but I found **no evidence of tampering** in them — headers, footers
and content are consistent with upstream releases, with no obfuscated `eval`/`atob` payloads appended.

---

## 1. Build systems present

| System | Manifest / config | Notes |
|---|---|---|
| Unity 2018.2.2f1 | `Unity/FlappyBird/Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt` | Editor version pinned in text; **no** `packages-lock.json`. 30 `com.unity.modules.*` built-ins + 5 real packages. |
| .NET SDK (`netcoreapp2.1`) | `PersonalProjectSite.csproj`, `PersonalProjectSiteTests.csproj`, `PersonalProjectSite.sln` | `Microsoft.NET.Sdk.Web`; **no** `global.json`, **no** `packages.lock.json`, **no** `NuGet.config`. |
| Bower (vendored, legacy) | `wwwroot/lib/*/.bower.json` only | Library files are checked in; there is **no** `bower.json`/`package.json` at repo root, so nothing declares or upgrades them. |
| CI / container | **none** | No `.github/workflows`, GitLab CI, Azure Pipelines, Jenkinsfile, Dockerfile, Makefile, or any `.sh`/`.ps1`/`.bat` script anywhere in the 353 tracked files. |

Two details worth flagging up front:

- **There is no Git LFS in this repo.** There is no `.gitattributes` and no `.lfsconfig`, so the
  `*.unityweb` blobs are real objects, not ~130-byte pointers: 3.0 MB wasm code, 1.0 MB data,
  90 KB framework (4.0 MB total under `Builds/`). The identical blobs are committed a second time
  under `wwwroot/FlappyBirdGame/Build/` (git deduplicates them, so no extra repo weight).
- **`.gitignore` contradicts what is tracked.** Line 4 ignores `Unity/FlappyBird/Builds/`, yet 17 files
  under it are tracked; lines 1–8 never mention `bin/` or `obj/`, which is why 75 .NET build-output
  files are in the tree (see M-5).

---

## 2. Dependency inventory

### 2.1 .NET — direct references

| Project | Package | Declared version | Pinned? | Note |
|---|---|---|---|---|
| `PersonalProjectSite.csproj:8` | `Microsoft.AspNetCore.App` | **none (no `Version` attribute)** | ✗ | Resolves to whatever the installed SDK's targeting pack supplies — 2.1.1 here (see `bin/…/PersonalProjectSite.runtimeconfig.json`). |
| `PersonalProjectSite.csproj:9` | `Microsoft.VisualStudio.Web.CodeGeneration.Design` | `2.1.6` | ✓ | Design-time scaffolder, declared as a normal (non-`PrivateAssets`) production reference. |
| `PersonalProjectSite.csproj:10` | `System.Data.SqlClient` | `4.5.1` | ✓ | Only true application data-access dependency. |
| `PersonalProjectSiteTests.csproj:10` | `Microsoft.NET.Test.Sdk` | `15.9.0` | ✓ | 2018-era. |
| `PersonalProjectSiteTests.csproj:11` | `Microsoft.VisualStudio.Web.CodeGeneration.Design` | `2.1.6` | ✓ | Not needed by tests. |
| `PersonalProjectSiteTests.csproj:12–13` | `MSTest.TestAdapter` / `MSTest.TestFramework` | `1.3.2` / `1.3.2` | ✓ | 2018-era. |

Target framework: `netcoreapp2.1` (`PersonalProjectSite.csproj:4`). Runtime framework request:
`Microsoft.AspNetCore.App 2.1.1` (`bin/Debug/netcoreapp2.1/PersonalProjectSite.runtimeconfig.json:4–6`).

### 2.2 .NET — resolved restore graph

**295 packages** are recorded in the committed `obj/project.assets.json` (549 KB) and 33 of those are
`Microsoft.VisualStudio.Web.CodeGeneration*` entries that have no business being in a runtime graph.
Everything else is the ASP.NET Core 2.1.1 / `Microsoft.AspNetCore.App` 2.1.1 metapackage closure
(Microsoft.NETCore.App 2.1.0 runtime, Kestrel 2.1.1, Razor 2.1.1, EF Core 2.1.1, Roslyn 2.8.0,
IdentityModel 5.2.0, and the `System.*` 4.3.0/4.5.x support packages). Non-Microsoft third-party
dependencies in the graph number effectively zero apart from `Newtonsoft.Json 11.0.2`
(unified up from `Microsoft.AspNet.WebApi.Client`'s 10.0.1 floor) and `Remotion.Linq 2.2.0` (EF Core 2.1).

### 2.3 Unity packages (`Unity/FlappyBird/Packages/manifest.json`, all exactly pinned)

| Package | Version | In use by game code? | Note |
|---|---|---|---|
| `com.unity.ads` | `2.0.8` | **no** (grep for `UnityEngine.Advertisements`/`Advertisement`: 0 hits) | Ads SDK; disabled in `UnityConnectSettings.asset:26`. |
| `com.unity.analytics` | `2.0.16` | **no** (0 hits) | Yet **enabled and initialized on startup** — see M-7. |
| `com.unity.purchasing` | `2.0.3` | **no** (0 hits / no IAP calls) | Disabled in settings (`:17`), still in the build. |
| `com.unity.package-manager-ui` | `1.9.11` | n/a (editor-only) | EOL. |
| `com.unity.textmeshpro` | `1.2.4` | n/a | EOL line; superseded by 3.x. |
| `com.unity.modules.*` | `1.0.0` × 30 | engine built-ins | Includes unused modules (`vehicles`, `cloth`, `terrain`, `vr`, `xr`, `ai`, `video`) — dead surface, no version risk. |

### 2.4 Vendored front-end libraries (`wwwroot/lib`, managed by Bower, checked in)

| Library | Vendored version | Files | Advisories (OSV) |
|---|---|---|---|
| `jquery` | **3.3.1** | `dist/jquery.js`, `dist/jquery.min.js`, `.min.map` | CVE-2019-11358 (prototype pollution), CVE-2020-11022, CVE-2020-11023 — all moderate |
| `bootstrap` | **3.3.7** | `dist/css/*`, `dist/js/*`, `dist/fonts/*` | CVE-2016-10735, CVE-2018-14040, CVE-2018-14042, CVE-2018-20676, CVE-2018-20677, CVE-2019-8331, CVE-2024-6485 — all moderate |
| `jquery-validation` | **1.17.0** | `dist/jquery.validate*.js`, `additional-methods*.js` | CVE-2021-21252 (high), CVE-2021-43306 (low), CVE-2022-31147 (high), CVE-2025-3573 (moderate) |
| `jquery-validation-unobtrusive` | **3.2.9** | `jquery.validate.unobtrusive*.js` | none published |

### 2.5 Generated / committed binary artifacts

| Path group | Count | Size | Provenance |
|---|---|---|---|
| `PersonalProjectSite/bin/Debug/netcoreapp2.1/` | 8 | 168 KB | Debug build of this project + `deps.json`/`runtimeconfig*.json` |
| `PersonalProjectSiteTests/bin/…` | 13 | part of 2.9 MB | Includes **third-party vendored DLLs** (`Microsoft.VisualStudio.TestPlatform.MSTest.TestAdapter.dll`, `MSTestAdapter.PlatformServices*.dll`) with no license/provenance file |
| `PersonalProjectSite{,Tests}/obj/` | 54 | rest of 2.9 MB | `project.assets.json`, `*.nuget.cache`, `*.csprojAssemblyReference.cache`, generated `AssemblyInfo.cs`, Razor `.g.cshtml.cs` |
| `Unity/FlappyBird/Builds/WebGL/` | 17 | 4.0 MB | Unity 2018.2.2f1 build (gzip `*.unityweb`, `UnityLoader.js`, `UnityProgress.js`) |
| `wwwroot/FlappyBirdGame/` | 17 | deduplicated | Byte-identical copies of the above (same blob SHAs) |

---

## 3. Findings

### C-1 — CRITICAL — ASP.NET Core 2.1.1: end-of-support framework with an unfixable critical CVE in the request pipeline

**Files:** `Website/PersonalProjectSite/PersonalProjectSite/PersonalProjectSite.csproj:4` (`netcoreapp2.1`), `:8` (`Microsoft.AspNetCore.App`, no version); `Website/PersonalProjectSite/PersonalProjectSite/bin/Debug/netcoreapp2.1/PersonalProjectSite.runtimeconfig.json:3–6`.

**What is wrong.** The site targets `netcoreapp2.1` and requests `Microsoft.AspNetCore.App 2.1.1`. .NET Core 2.1 reached end of support on **2021-08-21**, its final release is 2.1.30, and it receives no security fixes. Querying OSV for the resolved graph returns **45 advisories across 24 packages**, including two CRITICAL-rated ones, and the highest-impact set has no 2.1 fix at all:

| Advisory | GHSA grade | Component | Situation on the 2.1 line |
|---|---|---|---|
| `CVE-2025-55315` | **CRITICAL** (CVSS 9.9, `AV:N/AC:L/PR:L/S:C/C:H/I:H/A:L`) | `Microsoft.AspNetCore.Server.Kestrel.Core` | Affected range is **`>= 0, < 2.3.6`** — every 2.1 build is affected and **no 2.1 patch exists or will exist**. HTTP request smuggling / security-feature bypass on the public listener. |
| `CVE-2020-0603` | HIGH | `Microsoft.AspNetCore.App` | ASP.NET Core RCE (fix 2.1.15 — obsolete line nonetheless). |
| `CVE-2020-1045` | HIGH | `Microsoft.AspNetCore.Http`, `…App` | Cookie parsing failure → auth-cookie manipulation (fix 2.1.22). |
| `CVE-2020-1597` | HIGH | `Microsoft.AspNetCore.App` | DoS (fix 2.1.21). |
| `CVE-2023-33170` | HIGH | `Microsoft.AspNetCore.Identity` | Security-feature bypass / account-lockout (fix 2.1.39). |
| `CVE-2023-38180` | HIGH | `…Server.Kestrel.Transport.Sockets` | DoS (fix 2.1.40). |
| `CVE-2026-70159` | HIGH | `Microsoft.AspNetCore.SignalR.Core` | DoS (fix 1.2.14). |
| `CVE-2019-1075`, `CVE-2019-1302`, `CVE-2019-0564`, `CVE-2021-1723`, `CVE-2021-34532`, `CVE-2018-8409`, `CVE-2019-0545`, `CVE-2020-1108`, `CVE-2020-1147`, `CVE-2021-1721`, `CVE-2021-34485` | moderate/high | Kestrel, HttpSys, SpaServices, JwtBearer, `System.Net.Http`, `Microsoft.NETCore.App` | Open redirect, EoP, information disclosure, DoS. |

**Important nuance, stated so the root card can grade it correctly:** the versions above are the *build-time* package versions; at runtime the host loads whatever ASP.NET Core 2.1.x shared framework is installed (roll-forward keeps it on 2.1.x, ceiling **2.1.30**). So the honest statement is: *this app can only ever run an EOL framework whose newest possible patch (2021) still carries 2025- and 2026-era unpatched CVEs.* Severity is Critical **if the site is deployed and internet-reachable** (which the `wwwroot` deployment material and WebGL hosting strongly imply); it degrades to High for an archive-only repository.

**Fix.** Retarget the project to a supported LTS line and rebuild:
```xml
<TargetFramework>net10.0</TargetFramework>   <!-- .NET 10 LTS, supported to ~Nov 2028 -->
```
Work involved is modest for this codebase (MVC + Razor + `System.Data.SqlClient`; migrate to
`Microsoft.Data.SqlClient` 5.x/6.x at the same time — see H-1). If a quick interim step is needed,
2.1 → **3.1** is not acceptable either (EOL 2022-12-13); the shortest supported hop is
`.NET 8` (`8.0.21+`, supported to Nov 2026) or straight to `.NET 10`. After retargeting, pin the SDK
(`global.json`) and enable a lockfile (M-6).

---

### C-2 — CRITICAL — `System.Text.Encodings.Web` 4.5.0 (CVE-2021-26701, remote code execution)

**Files:** resolved in `PersonalProjectSite.csproj` graph via `Microsoft.AspNetCore.Html.Abstractions` / `Http.Abstractions` / `WebUtilities` / `Microsoft.Extensions.WebEncoders` 2.1.1; present as a runtime asset at `bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json` (`System.Text.Encodings.Web/4.5.0`).

**What is wrong.** `CVE-2021-26701` (GHSA-ghhp-997w-qr28, **CRITICAL**, CVSS 9.8:
`AV:N/AC:L/PR:N/UI:N/S:U/C:H/I:H/A:H`) is a remote code execution flaw in the HTML/JavaScript
encoder — the exact code path a Razor MVC site uses to encode every piece of dynamic output. Fixed in
4.5.1/4.7.2/5.0.1; the graph carries **4.5.0**.

**Fix.**
```xml
<PackageReference Include="System.Text.Encodings.Web" Version="4.7.2" />
```
and after the retarget in C-1 the framework-supplied encoder (8.0.x/10.0.x) covers it — do not rely on
the 2.1 shared framework's copy, which is the vulnerable one.

---

### H-1 — HIGH — `System.Data.SqlClient` 4.5.1 pinned directly (CVE-2024-0056 TLS bypass, CVE-2022-41064)

**File:** `Website/PersonalProjectSite/PersonalProjectSite/PersonalProjectSite.csproj:10`.

**What is wrong.** The app's only data-access dependency is pinned six years behind current:
- `CVE-2024-0056` (GHSA-98g6-xh36-x2p7, **HIGH**, 8.7, `S:C/C:H/I:H/A:N`) — SQL data-provider security
  feature bypass; an on-path attacker can defeat the driver's protection of the connection. Relevant
  here because every query goes through `Utilities/SQLUtilities.cs:49,52`
  (`new SqlConnection(connString)` / `new SqlCommand(...)`), invoked by `Models/DALs/GamesDAL.cs` and
  `Models/DALs/HighScoresDAL.cs`, i.e. over a network connection to SQL Server.
- `CVE-2022-41064` (GHSA-8g2p-5pqh-5jmc, moderate) — information disclosure.
- Fixed in 4.8.5 / 4.8.6; the package itself is superseded by `Microsoft.Data.SqlClient`.

**Fix.** Replace with `Microsoft.Data.SqlClient` (current 6.x) and adapt the two DALs, or as a stop-gap
pin `System.Data.SqlClient` `4.8.6`:
```xml
<PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.1" />
```
and require `Encrypt=True;TrustServerCertificate=False` in the connection string.

---

### H-2 — HIGH — `Newtonsoft.Json` 11.0.2 in the runtime graph (CVE-2024-21907)

**File:** `…/bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json` (`Newtonsoft.Json/11.0.2`), from
`Microsoft.AspNet.WebApi.Client 5.2.6` (floor 10.0.1) unified upward by the CodeGeneration/SDK path.

**What is wrong.** `CVE-2024-21907` (GHSA-5crp-9r3c-p9vr, **HIGH**, 7.5, `C:N/I:N/A:H`) — improper
handling of exceptional conditions lets a crafted JSON document drive a `StackOverflowException` and
kill the process (unhandled exception on a thread pool thread is not catchable). Fixed in 13.0.1;
graph carries 11.0.2. Note the repo already replaced JSON *parsing* in the Unity client with its own
`JSONParser.cs` for the score API — the server side still uses Newtonsoft.

**Fix.** `<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />` (or move to
`System.Text.Json` on the retargeted framework and drop Newtonsoft entirely).

---

### M-1 — MEDIUM — Vendored jQuery 3.3.1 with published prototype-pollution and XSS advisories

**Files:** `Website/PersonalProjectSite/PersonalProjectSite/wwwroot/lib/jquery/dist/jquery.js` (header confirms `v3.3.1`), `…/jquery.min.js`, `.bower.json:2`; referenced at `Views/Shared/_Layout.cshtml:57–61` (production CDN) and `:52` (development).

**What is wrong.** jQuery 3.3.1 is affected by `CVE-2019-11358` (prototype pollution via
`jQuery.extend(true, …)`), `CVE-2020-11022` and `CVE-2020-11023` (XSS when `.html()`/`.append()` receive
untrusted HTML) — all GHSA *moderate*, all fixed in 3.5.0/3.4.0. The library is loaded on **every**
production page, both from `ajax.aspnetcdn.com` and (as fallback) from the repo. Whether it is
exploitable depends on whether untrusted data reaches `html()`/`append()`/`attr()` — the sibling
injection card should confirm; the version-level risk is unambiguous.

**Fix.** Upgrade the vendored copy to **jQuery 3.7.1** (drop-in for this usage), update `_Layout.cshtml`
to the same version, and update the SRI hash on line 61 to the value for the new file. Do not keep the
3.3.1 CDN URL.

---

### M-2 — MEDIUM — Vendored Bootstrap 3.3.7 (EOL since 2019) with seven XSS advisories, and a CDN CSS link without SRI

**Files:** `wwwroot/lib/bootstrap/.bower.json:2` (`3.3.7`), `dist/css/*`, `dist/js/*`; `Views/Shared/_Layout.cshtml:13–15` (CDN CSS, **no `integrity` attribute**), `:63–67` (CDN JS, SRI present).

**What is wrong.** Bootstrap 3.3.7 carries `CVE-2016-10735`, `CVE-2018-14040`, `CVE-2018-14042`,
`CVE-2018-20676`, `CVE-2018-20677`, `CVE-2019-8331` and `CVE-2024-6485` (all GHSA *moderate*, XSS via
`data-*` attributes / tooltip & popover content). The 3.x line ended at 3.4.1 and has been unsupported
since 2019. Separately, the stylesheet pulled from `ajax.aspnetcdn.com` (line 13) has fallback attributes
but **no `integrity`/`crossorigin`**, so a CDN or transport compromise of that file is unmitigated —
a gap the two sibling `<script>` tags (lines 57–68) do not have.

**Fix.** Upgrade to **Bootstrap 5.3.x** (local copy, drop the `glyphicons` font dependency) or, as a
minimum, vendored **3.4.1**; on line 13 add
`crossorigin="anonymous" integrity="sha384-…"` matching the served file, or serve the vendored copy only
and delete the CDN reference.

---

### M-3 — MEDIUM — `jquery-validation` 1.17.0 / `jquery-validation-unobtrusive` 3.2.9 (ReDoS, XSS)

**Files:** `wwwroot/lib/jquery-validation/.bower.json:2` (`1.17.0`), `dist/jquery.validate*.js`; `…/jquery-validation-unobtrusive/.bower.json:2` (`3.2.9`); loaded from `Views/Shared/_ValidationScriptsPartial.cshtml:2–3` (dev) and `:6–14` (production CDN, versions `1.17.0`/`3.2.9` pinned in the URL, both with `integrity` + `crossorigin`).

**What is wrong.** Upstream grades three of these **HIGH**: `CVE-2021-21252`, `CVE-2022-31147`
(ReDoS in the `url`/`url2` validators, fixed 1.19.3/1.19.5) and `CVE-2025-3573` (XSS, fixed 1.20.0).
In *this* context the impact is limited to the visitor's own browser (client-side validation only; the
sibling cards should confirm no server-side regex is fed by the same pattern), so I grade the finding
Medium while recording the upstream grade. `jquery-validation-unobtrusive` 3.2.9 has no published
advisories but is from 2017.

**Fix.** jQuery Validation **1.20.0+** and jQuery Validation Unobtrusive **4.0.0**, both vendored locally
with a `.bower.json`/`package.json` entry so the versions are at least discoverable by SCA tooling.

---

### M-4 — MEDIUM — Design-time scaffolding package is a production dependency

**File:** `Website/PersonalProjectSite/PersonalProjectSite/PersonalProjectSite.csproj:9` (`Microsoft.VisualStudio.Web.CodeGeneration.Design` 2.1.6; also in the *test* csproj at `:11`).

**What is wrong.** `…CodeGeneration.Design` exists to run `dotnet aspnet-codegenerator` at design time. In
this project it is an ordinary `PackageReference`, so it drags the whole scaffolding stack — and
transitively Roslyn (`Microsoft.CodeAnalysis.CSharp/Workspaces 2.8.0`, `Common`, `Analyzers`), EF Core
`Design`/`Tools`/`Relational`/`SqlServer 2.1.1` and `NuGet.Frameworks 4.7.0` — into the app.
The committed `bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json` contains **33**
`…CodeGeneration*` entries, i.e. the deployment manifest lists code-generation and compiler assemblies
as runtime components. That is unnecessary attack surface (a compiler in the web app's dependency set)
and an avoidable upgrade-coupling liability. `Microsoft.NET.Test.Sdk`/`MSTest` in the test project are
correctly scoped, but `CodeGeneration.Design` is not needed there at all.

**Fix.** Remove it from both projects; scaffolder packages belong at most in a tools manifest. If it must
stay for developer convenience, mark it:
```xml
<PackageReference Include="Microsoft.VisualStudio.Web.CodeGeneration.Design" Version="9.0.0"
                  PrivateAssets="all" IncludeAssets="runtime; build; native; contentfiles; analyzers" />
```
and add `<IsPackable>`/`<DevelopmentDependency>` semantics so `dotnet publish` excludes it.

---

### M-5 — MEDIUM — 75 committed build-output files (2.9 MB) of unverifiable provenance, including third-party DLLs

**Files:** `Website/PersonalProjectSite/PersonalProjectSite/bin/Debug/netcoreapp2.1/` (`PersonalProjectSite.dll`, `PersonalProjectSite.Views.dll`, `*.pdb`, `deps.json`, `runtimeconfig*.json`), `…/obj/Debug/netcoreapp2.1/` (54 files), `…/PersonalProjectSiteTests/bin/…` and `…/obj/…`; `.gitignore:1–8` (no `bin`/`obj` rule for `Website/`).

**What is wrong.** `bin/` and `obj/` are in version control. Consequences:
- The committed assemblies are a **Debug** build produced on the author's workstation in 2019; nothing
  in the repo ties them to this commit's source. A deploy from a fresh clone that copies `bin/` runs
  code that cannot be verified against the tree (the classic prebuilt-binary supply-chain gap).
- Third-party binaries are vendored inside them with no license or provenance: three
  `Microsoft.VisualStudio.TestPlatform.MSTest*` DLLs from the author's NuGet cache.
- `obj/` adds churn-prone caches (`*.csprojAssemblyReference.cache`, `*.nuget.cache`,
  `project.assets.json`) that go stale silently and can mislead a restore.

**Fix.** Remove them from tracking and prevent recurrence:
```bash
git rm -r --cached Website/PersonalProjectSite/PersonalProjectSite/bin \
                     Website/PersonalProjectSite/PersonalProjectSite/obj \
                     Website/PersonalProjectSite/PersonalProjectSiteTests/bin \
                     Website/PersonalProjectSite/PersonalProjectSiteTests/obj
printf '\n# build output\n[Bb]in/\n[Oo]bj/\n' >> .gitignore
```
Build artifacts should come from a reproducible `dotnet publish` (see M-6 / L-3), not from the repo.

---

### M-6 — MEDIUM — No lockfile, no SDK pin, and a floating framework reference: restores are not reproducible

**Files:** `PersonalProjectSite.csproj:8` (`<PackageReference Include="Microsoft.AspNetCore.App" />` — no `Version`); absence of `global.json`, `packages.lock.json`, `NuGet.config`.

**What is wrong.** Three independent sources of drift:
1. `Microsoft.AspNetCore.App` with no version is resolved by whichever SDK is installed (2.1.1 here).
2. No `global.json` → different machines restore with different SDKs; on modern SDKs this reference
   also produces `NETSDK1080`.
3. No lockfile → the transitive graph (295 packages) is only *recorded* after the fact in a committed
   `obj/project.assets.json`, which is a cache, not a verified contract. `RestorePackagesWithLockFile`
   is off, so a `dotnet restore` today can silently pick different transitive versions than the ones
   the committed build used.

**Fix.**
```xml
<!-- csproj -->
<RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>
<PackageReference Include="Microsoft.AspNetCore.App" />   <!-- becomes FrameworkReference on 3.0+ -->
```
commit `packages.lock.json`, add a `global.json` pinning `"rollForward": "latestPatch"`, and use
`dotnet restore --locked-mode` in CI. This is also a prerequisite for meaningfully *fixing* any of the
CVE findings — otherwise "fixed" is unverifiable.

---

### M-7 — MEDIUM — Shipped WebGL build reports telemetry to Unity at startup; unused ads/analytics/purchasing packages still in the project

**Files:** `Unity/FlappyBird/ProjectSettings/UnityConnectSettings.asset:19–22` (`UnityAnalyticsSettings: m_Enabled: 1`, `m_InitializeOnStartup: 1`, `m_TestMode: 1`), `:11–15` (`CrashReportingSettings: m_Enabled: 1`, `m_CaptureEditorExceptions: 1`); `Unity/FlappyBird/Packages/manifest.json:3–6`; `Unity/FlappyBird/ProjectSettings/ProjectSettings.asset:621` (`cloudProjectId: 93d611db-…`), `:623` (`organizationId: mustangs1552`).

**What is wrong.** No game script references Unity Analytics, Ads or IAP (`grep` over
`Assets/Scripts/` for `UnityEngine.Analytics|Advertisements|Purchasing|IAP|Advertisement`: **0 hits**),
but the project settings enable Analytics with `m_InitializeOnStartup: 1`, so the built WebGL payload
initializes the analytics runtime and begins sending events to Unity's cloud endpoints on page load —
third-party network egress and visitor data collection that nobody asked the game to do, and an
attacker-visible dependency on cloud.unity3d.com availability/privacy posture. Crash reporting is also
on. `com.unity.ads 2.0.8` and `com.unity.purchasing 2.0.3` remain in the manifest though their settings
are disabled, so they are dead SDK surface compiled against the project. `cloudProjectId`/`organizationId`
in the settings are identifiers, not credentials — no secret is exposed here.

**Fix.** In `UnityConnectSettings.asset` set `UnityAnalyticsSettings.m_Enabled: 0`,
`m_InitializeOnStartup: 0`, and `CrashReportingSettings.m_Enabled: 0` (or remove the packages), then
remove `com.unity.ads` and `com.unity.purchasing` from `Packages/manifest.json` since no code uses them.
Rebuild the WebGL output and re-commit it. If analytics is actually wanted, it needs to be a conscious
decision with a privacy notice.

---

### L-1 — LOW — Developer identity and local absolute paths committed

**Files:** `…/bin/Debug/netcoreapp2.1/PersonalProjectSite.runtimeconfig.dev.json:4–8` (`C:\\Users\\musta\\.nuget\\packages`, `C:\\Program Files\\dotnet\\sdk\\NuGetFallbackFolder`), `…/obj/project.assets.json` (NuGet root paths), `…/PersonalProjectSite.csproj.user` (scaffolder state), `…/obj/**/PersonalProjectSite.csproj.FileListAbsolute.txt` (absolute paths).

**What is wrong.** Usernames and workstation layout are disclosed to anyone browsing the repo, and the
files are Windows-specific noise that breaks cross-platform builds. No credentials are present in these
files (credential/secret review belongs to the sibling card).

**Fix.** Untrack per M-5, and add `*.csproj.user` plus `*.FileListAbsolute.txt` to `.gitignore`.

---

### L-2 — LOW — 2018-era test toolchain (`Microsoft.NET.Test.Sdk` 15.9.0, MSTest 1.3.2)

**File:** `Website/PersonalProjectSite/PersonalProjectSiteTests/PersonalProjectSiteTests.csproj:10–13`.

No advisories are published against these versions, so this is not a vulnerability finding. Both are
seven years behind (`Microsoft.NET.Test.Sdk` 17.x, MSTest 3.x) and on unsupported branches; the
`netcoreapp2.1` target means the test project also cannot run on a modern runner.

**Fix.** On retarget: `Microsoft.NET.Test.Sdk` 17.x, `MSTest.TestAdapter`/`MSTest.TestFramework` 3.x
(or move to `xunit`), target `net10.0`.

---

### L-3 — LOW — No CI at all: dependency drift and tampering are never caught automatically

**Files:** none (absence). Verified: no `.github/workflows/**`, `.gitlab-ci.yml`, `azure-pipelines.yml`,
`Jenkinsfile`, `Dockerfile`, `Makefile`, `*.sh`, `*.ps1`, `*.bat` in the tree.

**What is wrong.** With no pipeline there is no automated SCA (this report is a point-in-time manual
sweep), no "rebuild and compare against the committed binaries" check, and no locked restore. The
positive side of the same fact — worth recording so the root card does not look for it — is that the
repo therefore **contains none of the classic CI abuse vectors**: no CI secrets, no `curl … | bash`
installers, no build scripts that execute third-party code, no `npm`/`postinstall` hooks (there is no
`package.json` at all).

**Fix.** Add a minimal workflow: `dotnet restore --locked-mode` → `dotnet build -c Release` →
`dotnet test` → `dotnet list package --vulnerable --include-transitive` (or Dependabot/nuget-audit),
plus a job that fails if `bin/`/`obj/` are tracked.

---

### L-4 — LOW — Third-party art/audio/font assets with unverified license provenance

**Files:** `Unity/FlappyBird/Assets/FlappyCopters/_ReadMe/_READ_ME.txt:5–9` (font "zorque" — "download … from the internet. Make sure you adhere to license terms"), `:13–15` (sound effects from `sounds-resource.com` — "make sure you adhere to license terms"); `Unity/FlappyBird/Assets/Birds Pack/**` (no license file); `Illustrator Files/*.ai` (6 files, ~6.9 MB, design sources).

**What is wrong.** The asset pack's own readme declines to state a license and points at third-party
sites; the bird sprite pack ships no license at all. That is a redistribution/legal risk, not a code
execution risk, but it is squarely "vendored third-party material of unknown provenance", and the assets
are embedded into the shipped WebGL build. The `.ai` sources are also heavyweight binary blobs in VCS.

**Fix.** Record explicit license/attribution for the font, sounds and sprite pack in a `THIRD-PARTY.md`,
or replace them with CC0/original assets; consider moving `.ai` sources out of the repo.

---

### L-5 — LOW — Unity 2018.2.2f1: EOL editor/toolchain, no patch path for the committed build

**File:** `Unity/FlappyBird/ProjectSettings/ProjectVersion.txt:1` (`m_EditorVersion: 2018.2.2f1`).

**What is wrong.** The editor line is long out of support, so the committed WebGL payload cannot be
rebuilt with security fixes, and anyone opening the project is asked to install an unpatched 2018 editor.
For accuracy: `CVE-2025-59489` (the Unity Runtime vulnerability, Oct 2025) affects builds for
Android/Windows/Linux/macOS and **does not apply to this WebGL-only build** — I mention it only because it
is the kind of issue an EOL toolchain can never be patched for. No Unity package here has a published
advisory of its own (`com.unity.ads`, `com.unity.analytics`, `com.unity.purchasing` 2.x are simply
abandoned lines).

**Fix.** No action needed if the game is frozen — document that the WebGL artifact is a 2019 release
artifact and is not rebuildable with modern tooling. If it is maintained, port to Unity 6 LTS and
re-export; TextMeshPro 3.x and the modern `com.unity.modules.*` set are drop-in equivalents for what this
project uses.

---

## 4. Positive controls observed (no finding)

Recorded so the consolidated review does not over-state the picture:

- **Vendored JS looks genuine.** jQuery/Bootstrap/Validation files carry intact upstream banners and
  standard UMD footers; no appended payload, no `atob(`/`fromCharCode` obfuscation, no `eval(` outside
  upstream code (the only `eval(` hit in `jquery.js` is the substring inside the word "retrieval("). The
  committed libraries are old, not tampered with.
- **`WebGL.json` performs no remote fetch.** It contains only relative asset URLs
  (`WebGL.data.unityweb`, `WebGL.wasm.code.unityweb`, `WebGL.wasm.framework.unityweb`), so the loader
  does not pull code from a third-party origin at runtime.
- **The two copies of the WebGL build are in sync.** `Unity/FlappyBird/Builds/WebGL/` and
  `wwwroot/FlappyBirdGame/` share identical blob SHAs for `UnityLoader.js`, `WebGL.wasm.code.unityweb`,
  `WebGL.data.unityweb`, `WebGL.wasm.framework.unityweb`, `WebGL.json` — no silent drift between the
  source-of-truth build and what the site serves.
- **Unity manifest is fully pinned** (no `^`/`*`/`latest`), and the game's score JSON parsing was
  hand-written (`Utilities/JSONParser.cs`) rather than pulled from a third-party parser.
- **SRI is present** on all four CDN `<script>` tags (`_Layout.cshtml:60–61`, `:66–67`;
  `_ValidationScriptsPartial.cshtml:10`, `:14`) — only the CDN stylesheet link (M-2) lacks it.
- **No build-time remote code fetch anywhere**: no `curl | bash`, no pre/post-install hooks, no
  `NuGet.config` pointing at a non-default feed, no custom MSBuild `.targets` downloaded at build time.

---

## 5. Appendix — OSV.dev advisory matches for the resolved graph

Query: `POST https://api.osv.dev/v1/querybatch` over all 295 NuGet packages resolved in
`obj/project.assets.json` (both projects) plus the 4 vendored npm libraries; details from
`GET /v1/vulns/{id}`. Result: **55 package↔advisory pairs, 45 distinct advisories, 24 affected packages.**
Severity is the GHSA database label.

| Package | Version | Advisories | Max severity (GHSA) | Fixed in |
|---|---|---|---|---|
| `Microsoft.AspNetCore.App` | 2.1.1 | CVE-2018-8409 (high), CVE-2019-0564 (high), CVE-2019-1075 (moderate), CVE-2020-0602 (moderate), CVE-2020-0603 (high), CVE-2020-1045 (high), CVE-2020-1597 (high), GHSA-cgpw-2gph-2r9g (moderate) | HIGH | 2.1.12, 2.1.15, 2.1.2, 2.1.21, 2.1.22, 2.1.4, 2.1.7, 2.2.1, 2.2.6, 3.0.1, 3.1.1 |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 2.1.1 | CVE-2021-34532 (moderate) | MODERATE | 2.1.29, 3.1.18, 5.0.9 |
| `Microsoft.AspNetCore.Http` | 2.1.1 | CVE-2020-1045 (high) | HIGH | 2.1.22 |
| `Microsoft.AspNetCore.Http.Connections` | 1.0.1 | CVE-2020-0602 (moderate), CVE-2020-0603 (high) | HIGH | 1.0.15 |
| `Microsoft.AspNetCore.Identity` | 2.1.1 | CVE-2018-8171 (high), CVE-2023-33170 (high) | HIGH | 1.0.6, 1.1.6, 2.0.4, 2.1.2, 2.1.39 |
| `Microsoft.AspNetCore.Server.HttpSys` | 2.1.1 | CVE-2019-1075 (moderate) | MODERATE | 2.1.12, 2.2.6 |
| `Microsoft.AspNetCore.Server.Kestrel.Core` | 2.1.1 | CVE-2019-0564 (high), CVE-2021-1723 (high), **CVE-2025-55315 (critical)**, GHSA-cgpw-2gph-2r9g (moderate) | **CRITICAL** | 2.0.4, 2.1.2, 2.1.25, 2.1.7, 2.3.6 |
| `Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets` | 2.1.1 | CVE-2023-38180 (high) | HIGH | 2.1.40 |
| `Microsoft.AspNetCore.SignalR.Core` | 1.0.1 | CVE-2026-70159 (high) | HIGH | 1.2.14 |
| `Microsoft.AspNetCore.SpaServices` | 2.1.1 | CVE-2019-1302 (moderate) | MODERATE | 2.1.13, 2.2.7 |
| `Microsoft.AspNetCore.WebSockets` | 2.1.1 | CVE-2019-0564 (high) | HIGH | 2.1.7, 2.2.1 |
| `Microsoft.NETCore.App` | 2.1.0 | CVE-2018-8416 (moderate), CVE-2019-0545 (high), CVE-2019-0564 (high), CVE-2019-0657 (moderate), CVE-2020-1108 (high), CVE-2020-1147 (high), CVE-2021-1721 (moderate), CVE-2021-34485 (moderate) | HIGH | 2.1.18, 2.1.20, 2.1.25, 2.1.29, 2.1.7, 2.1.8, 2.2.1, 2.2.2 |
| `Newtonsoft.Json` | 11.0.2 | CVE-2024-21907 (high) | HIGH | 13.0.1 |
| `System.Data.SqlClient` | 4.5.1 | CVE-2022-41064 (moderate), CVE-2024-0056 (high) | HIGH | 4.8.5, 4.8.6 |
| `System.IO.Pipelines` | 4.5.0 | CVE-2018-8409 (high) | HIGH | 4.5.1 |
| `System.IdentityModel.Tokens.Jwt` | 5.2.0 | CVE-2024-21319 (moderate) | MODERATE | 5.7.0, 6.34.0, 7.1.2 |
| `System.Net.Http` | 4.3.0 | CVE-2018-8292 (high) | HIGH | 4.3.4 |
| `System.Net.WebSockets.WebSocketProtocol` | 4.5.1 | CVE-2019-0564 (high) | HIGH | 4.5.3 |
| `System.Security.Cryptography.Xml` | 4.5.0 | CVE-2022-34716 (moderate) | MODERATE | 4.7.1, 6.0.1 |
| `System.Text.Encodings.Web` | 4.5.0 | **CVE-2021-26701 (critical)** | **CRITICAL** | 4.5.1, 4.7.2, 5.0.1 |
| `System.Text.RegularExpressions` | 4.3.0 | CVE-2019-0820 (high) | HIGH | 4.3.1 |
| `bootstrap` | 3.3.7 | CVE-2016-10735, CVE-2018-14040, CVE-2018-14042, CVE-2018-20676, CVE-2018-20677, CVE-2019-8331, CVE-2024-6485 (all moderate) | MODERATE | 3.4.0, 3.4.1, 4.0.0-beta.2, 4.1.2, 4.3.1 |
| `jquery` | 3.3.1 | CVE-2019-11358, CVE-2020-11022, CVE-2020-11023 (all moderate) | MODERATE | 3.4.0, 3.5.0 |
| `jquery-validation` | 1.17.0 | CVE-2021-21252 (high), CVE-2021-43306 (low), CVE-2022-31147 (high), CVE-2025-3573 (moderate) | HIGH | 1.19.3, 1.19.4, 1.19.5, 1.20.0 |

Unity packages (`com.unity.*` 2.x) and `com.unity.modules.*` are not covered by OSV or the GHSA database;
they were assessed manually (M-7, L-5). `Microsoft.NET.Test.Sdk 15.9.0`, `MSTest 1.3.2/1.3.2`,
`Microsoft.VisualStudio.Web.CodeGeneration.* 2.1.6` and `jquery-validation-unobtrusive 3.2.9` returned
**no** advisories.

---

## 6. Scope covered / not covered

**Covered by this card:**
- Manifest and lockfile inventory for every build system present (Unity `manifest.json`, two `.csproj`,
  Bower `.bower.json`); absence of `global.json`/`packages.lock.json`/`NuGet.config` confirmed.
- Full resolved .NET dependency graph (295 packages) extracted from the committed `project.assets.json`
  and `deps.json`, and every package checked against OSV.dev; 45 advisories reported with fixed versions.
- Vendored third-party code: Bootstrap/jQuery/Validation versions, integrity heuristics (tamper check),
  SRI coverage of the CDN references.
- Committed binaries and generated artifacts: what exists, why, provenance gaps, size, LFS status.
- Build/CI configuration: absence of any CI pipeline or build script, checked explicitly for the classic
  abuse vectors (untrusted installers, CI secrets, post-install hooks) — none present.
- Unity-side dependency posture: package pins, unused/telemetry SDKs, EOL toolchain, asset licensing.

**Not covered (owned by sibling cards in this review family):**
- Hardcoded secrets, connection strings, credentials and sensitive-data exposure (including the
  committed `Database/PersonalGameSite.sql`, `appsettings*.json` and `launchSettings.json`).
- Injection flaws (SQL/command/path traversal) in `HighScoresDAL`/`GamesDAL`/`SQLUtilities` and unsafe
  deserialization in the Unity `JSONParser`/`ScoreAccess` score-submission path.
- Authentication/authorization behaviour, session/cookie configuration, and insecure framework defaults
  (`AllowedHosts "*"`, developer-exception pages, `ASPNETCORE_ENVIRONMENT=Development` in
  `launchSettings.json`).
- Client-side/XSS exploitability of the vendored libraries in this specific app (i.e. whether untrusted
  data reaches `html()`/`append()`); M-1/M-3 report version-level risk only.
- Runtime/deployment configuration (web server, TLS termination, SQL Server hardening) — not visible
  from the repository.
- No project code was built or executed, and no LFS/asset binaries were analyzed beyond magic bytes and
  gzip expansion, so a compromised payload inside the committed `*.unityweb` blobs cannot be ruled out
  by this card.

**Recommended remediation order for the root card:** C-1 (retarget to .NET 10 LTS) subsumes C-2 and most
of the framework advisories in one change; then H-1/H-2 (or the same retarget), M-6 (lockfile + SDK pin so
fixes stay fixed), M-5/L-1 (untrack build output), M-1/M-2/M-3 (front-end upgrades), M-4, M-7.
