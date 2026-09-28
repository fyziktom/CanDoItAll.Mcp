[CmdletBinding(SupportsShouldProcess)]
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",

    [switch]$NoRestore,

    [switch]$IncludeIntegrationTests
)

$ErrorActionPreference = "Stop"

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot "..\.."))
$solutionPath = Join-Path $repositoryRoot "CanDoItAll.Mcp.slnx"

$requiredFiles = @(
    ".editorconfig",
    ".gitattributes",
    ".gitignore",
    "AGENTS.md",
    "CONTRIBUTING.md",
    "Directory.Build.props",
    "global.json",
    "LICENSE",
    "NuGet.config",
    "README.md",
    "SECURITY.md"
)

$stableTestProjects = @(
    "tests\CanDoItAll.Mcp.CodeAnalytics.Tests\CanDoItAll.Mcp.CodeAnalytics.Tests.csproj",
    "tests\CanDoItAll.Mcp.Components.Tests\CanDoItAll.Mcp.Components.Tests.csproj",
    "tests\CanDoItAll.Mcp.DotNetWatch.Tests\CanDoItAll.Mcp.DotNetWatch.Tests.csproj",
    "tests\CanDoItAll.Mcp.Mermaid.Tests\CanDoItAll.Mcp.Mermaid.Tests.csproj",
    "tests\CanDoItAll.Mcp.SshOps.Tests\CanDoItAll.Mcp.SshOps.Tests.csproj"
)

$integrationTestProject =
    "tests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests\CanDoItAll.Mcp.DotNetWatch.IntegrationTests.csproj"

function Test-RequiredRepositoryFiles {
    foreach ($relativePath in $requiredFiles) {
        $path = Join-Path $repositoryRoot $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "Required repository file '$relativePath' is missing."
        }
    }

    $licenseText = Get-Content -LiteralPath (Join-Path $repositoryRoot "LICENSE") -Raw
    if (-not $licenseText.Contains("https://aicandoitall.com")) {
        throw "LICENSE does not contain the required CanDoItAll website link."
    }

    Get-Content -LiteralPath (Join-Path $repositoryRoot "global.json") -Raw |
        ConvertFrom-Json |
        Out-Null

    [xml](Get-Content -LiteralPath (Join-Path $repositoryRoot "Directory.Build.props") -Raw) |
        Out-Null
    [xml](Get-Content -LiteralPath (Join-Path $repositoryRoot "NuGet.config") -Raw) |
        Out-Null
}

function Test-LocalMarkdownLinks {
    $ignoredDirectoryNames = @(
        ".git",
        ".artifacts",
        ".mcp-state",
        "artifacts",
        "bin",
        "obj",
        "output"
    )

    $markdownFiles =
        Get-ChildItem -LiteralPath $repositoryRoot -Recurse -File -Filter "*.md" |
        Where-Object {
            $relativePath =
                $_.FullName.Substring($repositoryRoot.Length).TrimStart("\", "/")
            $segments = $relativePath -split "[\\/]"
            -not ($segments | Where-Object { $ignoredDirectoryNames -contains $_ })
        }

    $linkPattern = "\[[^\]]+\]\((?<target>[^)]+)\)"
    foreach ($markdownFile in $markdownFiles) {
        $content = Get-Content -LiteralPath $markdownFile.FullName -Raw
        foreach ($match in [regex]::Matches($content, $linkPattern)) {
            $target = $match.Groups["target"].Value.Trim().Trim("<", ">")
            if (
                [string]::IsNullOrWhiteSpace($target) -or
                $target.StartsWith("#") -or
                $target -match "^[a-z][a-z0-9+.-]*:"
            ) {
                continue
            }

            $pathPart = ($target -split "#", 2)[0]
            if ([string]::IsNullOrWhiteSpace($pathPart)) {
                continue
            }

            $decodedPath = [System.Uri]::UnescapeDataString($pathPart)
            $resolvedPath = [System.IO.Path]::GetFullPath(
                (Join-Path $markdownFile.DirectoryName $decodedPath)
            )
            if (-not (Test-Path -LiteralPath $resolvedPath)) {
                $relativeMarkdownPath =
                    $markdownFile.FullName.Substring($repositoryRoot.Length).TrimStart("\", "/")
                throw "Broken local Markdown link '$target' in '$relativeMarkdownPath'."
            }
        }
    }
}

function Invoke-DotNet {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host ("dotnet " + ($Arguments -join " "))
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments[0]) failed with exit code $LASTEXITCODE."
    }
}

Test-RequiredRepositoryFiles
Test-LocalMarkdownLinks

$validationDescription =
    "restore, build, and run stable tests for CanDoItAll.Mcp"
if ($IncludeIntegrationTests) {
    $validationDescription += ", including non-quarantined integration tests"
}

if (-not $PSCmdlet.ShouldProcess($repositoryRoot, $validationDescription)) {
    return
}

Push-Location $repositoryRoot
try {
    if (-not $NoRestore) {
        Invoke-DotNet -Arguments @("restore", $solutionPath)
    }

    Invoke-DotNet -Arguments @(
        "build",
        $solutionPath,
        "--configuration",
        $Configuration,
        "--no-restore"
    )

    foreach ($relativeProjectPath in $stableTestProjects) {
        Invoke-DotNet -Arguments @(
            "test",
            (Join-Path $repositoryRoot $relativeProjectPath),
            "--configuration",
            $Configuration,
            "--no-build"
        )
    }

    if ($IncludeIntegrationTests) {
        Invoke-DotNet -Arguments @(
            "test",
            (Join-Path $repositoryRoot $integrationTestProject),
            "--configuration",
            $Configuration,
            "--no-build",
            "--filter",
            "Category!=Quarantined"
        )
    }
}
finally {
    Pop-Location
}

[pscustomobject]@{
    RepositoryPath = $repositoryRoot
    Configuration = $Configuration
    Restored = -not $NoRestore
    StableTestProjects = $stableTestProjects.Count
    IntegrationTestsIncluded = $IncludeIntegrationTests.IsPresent
}
