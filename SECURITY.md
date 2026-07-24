# Security Policy

## Supported Versions

Security fixes are applied to the current `main` branch. This repository does not
currently publish versioned releases or NuGet packages, so older commits and local
install artifacts are not maintained as supported versions.

## Reporting A Vulnerability

Use [GitHub private vulnerability reporting](https://github.com/fyziktom/CanDoItAll.Mcp/security/advisories/new).
Do not publish exploit details, credentials, private data, host information, or
sensitive proof in a public issue. If the private form is unavailable, contact the
repository owner to establish a private channel before sharing details.

Include the affected server or tool, commit, reproduction steps, expected impact, and
any safe mitigation already tested.

## Scope

Security-sensitive areas in this repository include MCP stdio boundaries, command and
process supervision, SSH host verification and remote-path guards, settings and secret
resolution, workspace ownership, and generated runtime artifacts.

Vulnerabilities in the main application, CodeAnalytics libraries, or Components
libraries should be reported to their owning repositories. Cross-repository issues may
reference this repository privately without copying credentials or sensitive logs.
