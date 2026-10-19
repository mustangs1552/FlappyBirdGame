# Notes — family `t_5cd2d674` (mustangs1552/FlappyBirdGame @ 4a5d09c0c6)

Every card in this review family produced its own document, so this file contains **no
summary-only findings** — all four lanes' findings are in their named `.md` files. What follows is
provenance and cross-card context that does not belong in a findings report.

## Doc-less cards

None. All three children (`t_af68f023`, `t_a0c435e2`, `t_b8e39de8`) attached a report; the root
(`t_5cd2d674`) authored `consolidated-review.md`.

## Root's independent verification of lane claims

The consolidated report was not a concatenation. Before grading, the root re-cloned at the pin and
confirmed:

- `git rev-parse HEAD` = `4a5d09c0c6daa13fc84eb05aed706c23845f04e6`, subject "Scores are now using
  own JSONParser.", author Matthew George — matches the pin, and this commit **is** the remote head
  at review time (i.e. this folder is the first `HermesReviews/` commit for this repo; no prior
  review docs were present in the tree, which is why 353 tracked files were reviewed, not more).
- `git ls-tree -r --name-only HEAD` → 353 files; `grep -Ec '(^|/)(bin|obj)/'` → **75**. Both figures
  quoted by the secrets and dependency lanes reproduce exactly.
- Read in full from the checkout: `HighScoresController.cs` (26 lines; `[HttpPost]` at 11, action at
  12, `int.Parse` at 17/19), `Startup.cs` (63 lines; `AddMvc()` at 35, no `AddAntiforgery`),
  `PersonalProjectSite.csproj` (lines 4/8/9/10 exactly as cited), `Views/Game/Index.cshtml`
  (`<iframe src="@Model.GameSrc">` at line 7), `HighScoresDAL.AddHighScore` (username check at 78,
  bound parameters only), `SQLUtilities.PerformSQL` (line 45 signature, `Substring(0, 6)` select-sniff
  at 61, `AddWithValue` loop at 56-58), `_Layout.cshtml` (bootstrap CSS link at 13-15 with **no**
  `integrity`; jQuery CDN script at 57-61 with `crossorigin` + `integrity`), `UIManager.SaveHighScore`
  (`||` bug at line 36), `JSONParser` (`ParseJSON` at 222, recursive `ReadObject` at 132),
  `.gitignore` (8 lines; line 4 ignores `Unity/FlappyBird/Builds/`), `UnityConnectSettings.asset`
  (analytics `m_Enabled: 1`, `m_InitializeOnStartup: 1`, `m_TestMode: 1` at 20-22; crash reporting
  `m_Enabled: 1` at 14).
- Runtime manifest versions in `bin/Debug/netcoreapp2.1/PersonalProjectSite.deps.json`:
  `Microsoft.AspNetCore.App/2.1.1`, `System.Text.Encodings.Web/4.5.0`, `Newtonsoft.Json/11.0.2`.
- Absence of `[Authorize]`, `[ValidateAntiForgeryToken]`, `AddAntiforgery`, `AddIdentity`,
  `AddAuthentication` in **all hand-written source** (the only matches are inside `obj/`/`bin/` build
  artifacts, i.e. the framework's own DLL manifests).
- No `.gitattributes` / no `.git-lfs` config → the `*.unityweb` blobs are real multi-MB objects, not
  LFS pointers, consistent with the dependency lane's stated clone strategy being unnecessary.

## One thing the lanes did not catch, checked and cleared by the root

`Unity/FlappyBird/ProjectSettings/ProjectSettings.asset:495` contains
`psp2PackagePassword: F69A…exui (32 chars)`, which any credential scanner will flag. It
is **not a secret**: it sits in an otherwise entirely blank PS Vita platform block
(`psp2NPCommsPassphrase`, `psp2PatchOriginalPackage`, `psp2KeystoneFile` all empty), alongside
`metroCertificatePassword:` (empty) and `metroPackageName: Template_2D`. It is the stock Unity 2D
template's shipped default placeholder for the Vita package password. No console/mobile signing
material exists in the project at all. Recorded here so a future scan does not open it as a finding
or, worse, rotate a "credential" that was never real.

## Cross-lane overlaps (deliberately kept in both lane docs)

- **Dev endpoints** — `ScoreAccess.cs:37`, `SampleScene.unity:238`, `Player.prefab:113` appear in
  both the secrets lane (as environment residue) and the injection lane (as the transport/TLS
  finding). Consolidated as M-2 (transport) + L-1 (residue).
- **Committed build output** — the dependency lane grades `bin`/`obj` provenance (2.9 MB, third-party
  MSTest DLLs) while the secrets lane grades the same trees as a path-disclosure surface, and adds
  the 4.2 MB Unity WebGL build committed twice under a `.gitignore` rule that says otherwise.
  Consolidated as M-8 (provenance) + L-2 (WebGL artifacts) + L-1 (machine paths).
- **`JSONParser.cs`** — the injection lane grades it Low (unbounded recursion / O(n²)); the dependency
  lane cites it as a *positive* ("hand-written rather than a third-party parser, no external parser
  dependency"). Both are true; the consolidated report keeps the Low and the positive.
- **jQuery / Bootstrap / jquery-validation** — the dependency lane reports version-level CVEs and
  grades jquery-validation's upstream HIGHs down to Medium for lack of a server-side regex sink. The
  injection lane independently confirmed no untrusted data reaches `html()`/`append()` in this app's
  views (all model output goes through Razor encoding), which is what justifies keeping those at
  Medium rather than raising them.

## Grading decisions made at consolidation

- **C-1 kept at Critical** with an explicit conditional: it is Critical for a deployed,
  internet-reachable site (the committed `wwwroot/FlappyBirdGame/` deployment payload implies that)
  and degrades to High for an archive-only repo. The report states the condition rather than
  picking silently.
- **H-1 kept at High.** The injection lane already graded it High with the same reasoning the root
  would use: it is a real integrity compromise of the site's only user-facing data, but not RCE or
  credential theft.
- **C-2 (`System.Text.Encodings.Web` RCE) kept at Critical** on the dependency lane's grading.
  Reachability from untrusted input was not proven in this app; the component is on the hot path of
  every rendered page and the fix is one line.
- **M-6 (jquery-validation) kept at Medium** despite three upstream HIGH advisories, because the
  vulnerable validators are client-side only and no server-side regex consumes the same input.
- Nothing was graded downward without the reason being written into `consolidated-review.md`.

## Document integrity / transport notes

- `injection-auth-review.md` is the lane's declared completion artifact, sha256
  `38a547a0862d7126efc9dbb3642242ddafc4185fe3d6619bec381af3f4071b81` (13383 bytes). The lane also
  holds a UTF-8 original, sha256
  `b6ea0e6f576e1ad1fe4f76ca593ec3f1fe3f80f27a61e4e5883d30269fb68256` (13441 bytes). A diff of the two
  shows **punctuation only**: em dashes, en dashes and `→` were ASCII-fied to `--`, `-` and `->`
  during the lane's attachment repair. No wording, finding, severity or citation differs. The
  committed copy is the declared artifact so that the file in this folder matches the sha the card
  recorded.
- `dependency-supply-chain.md` sha256 `25716196…` and `secrets-sensitive-data-audit.md` /
  `secrets-sensitive-data.md` sha256 `bd5b35ab…` both match the sha256 values their cards declared
  at completion.
- The secrets lane's card note records a prior failed inline-attachment attempt (a 32-byte
  `placeholder` attachment, id 80) before it delivered via completion artifacts. That is a transport
  artifact, not a deliverable; it is not reproduced here.
- The dependency lane reported its clone was `--depth 1 --filter=blob:none` with
  `GIT_LFS_SKIP_SMUDGE=1`; the repo has no LFS, so no binaries were skipped. The lane did not
  decompile or deep-audit the committed `*.unityweb` payloads (gzip magic + header checks only), so
  a compromised payload inside them remains unexcluded — stated in the consolidated scope limits.

## Cleanup

All three lane workdirs were deleted after their reports were delivered. The root's `repo-checkout`
was deleted after the docs commit was pushed and verified against `git ls-remote`.
