# HermesReviews — `mustangs1552/FlappyBirdGame` — 2026-10-19_1026Z

Security review of **mustangs1552/FlappyBirdGame**, run automatically by Hermes Agent.

| Field | Value |
|---|---|
| Repository | `mustangs1552/FlappyBirdGame` (public) |
| Pinned commit | `4a5d09c0c6daa13fc84eb05aed706c23845f04e6` |
| Commit subject | "Scores are now using own JSONParser." |
| Commit date / author | 2019-03-13T01:06:25Z (2019-03-12 21:06:25 −0400) / Matthew George |
| Review date | 2026-10-19 |
| Review stamp | `2026-10-19_1026Z` |
| Scope | Whole repository at the pinned commit (353 tracked files) |

## Findings

**2 Critical · 3 High · 10 Medium · 8 Low — 23 total.**

| Severity | Headline |
|---|---|
| Critical | ASP.NET Core 2.1.1 is EOL (2021-08-21) and carries `CVE-2025-55315` (Kestrel request smuggling, CVSS 9.9) with **no 2.1 fix in existence**. |
| Critical | `System.Text.Encodings.Web` 4.5.0 — `CVE-2021-26701` RCE. |
| High | `POST /HighScores/SaveNewScore` is fully unauthenticated and unvalidated — anyone can write any score under any name. |
| High | `System.Data.SqlClient` 4.5.1 — `CVE-2024-0056` TLS protection bypass. |
| High | `Newtonsoft.Json` 11.0.2 — `CVE-2024-21907` process-killing DoS. |

Short verdict: **no committed secrets, no SQL injection, no command injection, no path traversal, no unsafe deserialization.** The hand-written code is small and clean; the risk is the EOL ASP.NET Core website (2 Critical + framework CVEs) fronting a wide-open write endpoint.

## Files in this folder

| File | What it is | Source card |
|---|---|---|
| `consolidated-review.md` | **Authoritative** merged security review: all 23 findings with severity, `file:line`, explanation and concrete fix, plus the explicit clean list, remediation order and scope limits. | `t_5cd2d674` (root) |
| `injection-auth-review.md` | Lane report — injection (SQL/command/path traversal), unsafe deserialization, auth/authz, insecure defaults. 0C / 1H / 3M / 2L. | `t_af68f023` |
| `dependency-supply-chain.md` | Lane report — dependency inventory, unpinned/floating versions, OSV sweep of the 295-package .NET graph + 4 vendored JS libraries, committed binaries, CI/build abuse paths. 2C / 2H / 7M / 5L. | `t_a0c435e2` |
| `secrets-sensitive-data.md` | Lane report — hardcoded secrets/credentials/tokens and sensitive-data exposure across all 353 tracked files. No secrets exposed; 3 Low environment-residue findings. | `t_b8e39de8` |
| `notes.md` | Family-level notes: provenance, verification performed by the root card, hashes, and findings that lived only in card summaries. | `t_5cd2d674` (root) |

Start with `consolidated-review.md`. The three lane reports are reproduced **verbatim** and deliberately overlap each other (e.g. the dev-endpoint residue appears in both the secrets and injection lanes); the consolidated file de-duplicates them and is the authoritative view.

## Review family

| Card | Role | Status at close |
|---|---|---|
| `t_5cd2d674` | Root: consolidation, independent verification, docs commit | completed |
| `t_af68f023` | Injection / auth / deserialization lane | completed |
| `t_a0c435e2` | Dependency / supply-chain lane | completed |
| `t_b8e39de8` | Secrets / sensitive-data lane | completed |

## Method

Static, read-only review. Shallow blobless clones at the pinned SHA (no LFS smudge). No project code was modified, executed or built; no requests were sent against any live host; no issues, PRs, branches or tags were touched. The single permitted remote write was the commit that added this folder.

The root card independently re-verified every headline citation at the pin before grading: `HEAD` matched the pinned SHA, the 353-file / 75-`bin|obj` counts reproduced, and each `file:line` in the consolidated report was read directly from the checkout.

## Document integrity

| File | Size | SHA-256 |
|---|---|---|
| `consolidated-review.md` | 34753 | `f3c68b4e673fb1dd84849b25934be9b02fd26004c803d3fcee7e1163298c3061` |
| `dependency-supply-chain.md` | 38212 | `25716196ba525d073410dec7270831719096c244b44e145072341de29fd731f7` |
| `injection-auth-review.md` | 13383 | `38a547a0862d7126efc9dbb3642242ddafc4185fe3d6619bec381af3f4071b81` |
| `secrets-sensitive-data.md` | 8752 | `bd5b35ab7f20f6a76de4c6f83f71a5854a0a5442c60ee62115e7ec8db8f9b6bd` |

`injection-auth-review.md` is the lane's declared completion artifact (ASCII-ified variant of its UTF-8 original `b6ea0e6f576e1ad1fe4f76ca593ec3f1fe3f80f27a61e4e5883d30269fb68256`; the two differ only in em-dash/arrow punctuation — see `notes.md`).
