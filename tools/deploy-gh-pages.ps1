<#
.SYNOPSIS
    Publishes the web build to the gh-pages branch, which GitHub Pages serves.

.DESCRIPTION
    Copies the build (Builds/WebGL by default) into a temporary worktree of the gh-pages branch,
    replacing what was there, commits it and pushes it. The working tree of the current branch is not
    touched. The first run creates gh-pages as an orphan branch, with no history from main.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/deploy-gh-pages.ps1

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools/deploy-gh-pages.ps1 -NoPush
    Commits to the local gh-pages branch only; push it later with: git push origin gh-pages
#>
param(
    [string]$BuildPath = "Builds/WebGL",
    [string]$Branch = "gh-pages",
    [string]$Remote = "origin",
    [switch]$NoPush
)

$ErrorActionPreference = "Stop"

function Invoke-Git {
    & git @args
    if ($LASTEXITCODE -ne 0) {
        throw "git $($args -join ' ') failed with exit code $LASTEXITCODE."
    }
}

function Test-GitRef([string]$Ref) {
    & git show-ref --verify --quiet $Ref
    return $LASTEXITCODE -eq 0
}

$projectRoot = Split-Path -Parent $PSScriptRoot
$build = Join-Path $projectRoot $BuildPath
if (-not (Test-Path (Join-Path $build "index.html"))) {
    throw "No web build in '$build'. Build it first: SelfBulletting > Build > Web (GitHub Pages)."
}

$worktree = Join-Path ([System.IO.Path]::GetTempPath()) ("selfbulletting-$Branch-" + [guid]::NewGuid().ToString("N").Substring(0, 8))
Push-Location $projectRoot

try {
    $source = (& git rev-parse --short HEAD).Trim()

    # On the first deploy the branch exists nowhere yet.
    & git ls-remote --exit-code --heads $Remote $Branch | Out-Null
    $onRemote = $LASTEXITCODE -eq 0
    if ($onRemote) {
        Invoke-Git fetch --quiet $Remote $Branch
    }

    $local = Test-GitRef "refs/heads/$Branch"
    $localIsBehind = $false
    if ($local -and $onRemote) {
        & git merge-base --is-ancestor $Branch "$Remote/$Branch"
        $localIsBehind = $LASTEXITCODE -eq 0
    }

    if ($onRemote -and (-not $local -or $localIsBehind)) {
        Invoke-Git worktree add -B $Branch $worktree "$Remote/$Branch"
    }
    elseif ($local) {
        # Keeps deploys committed with -NoPush and not pushed yet.
        Invoke-Git worktree add $worktree $Branch
    }
    else {
        Invoke-Git worktree add --orphan -b $Branch $worktree
    }

    # Replace the whole site: hashed build files from older deploys must not pile up.
    Get-ChildItem -Force $worktree | Where-Object { $_.Name -ne ".git" } | Remove-Item -Recurse -Force
    Copy-Item -Path (Join-Path $build "*") -Destination $worktree -Recurse -Force

    # Serve the files as they are, without the Jekyll pass.
    New-Item -ItemType File -Path (Join-Path $worktree ".nojekyll") -Force | Out-Null

    Invoke-Git -C $worktree add --all
    & git -C $worktree diff --cached --quiet
    if ($LASTEXITCODE -eq 0) {
        Write-Host "The build is the same as on $Branch, nothing to deploy."
        return
    }

    Invoke-Git -C $worktree commit --quiet -m "Deploy web build from $source"
    Write-Host "Committed the web build from $source to $Branch."

    if ($NoPush) {
        Write-Host "Not pushed. Push it with: git push $Remote $Branch"
    }
    else {
        Invoke-Git -C $worktree push $Remote $Branch
        Write-Host "Pushed $Branch. GitHub Pages updates in a minute or two."
    }
}
finally {
    if (Test-Path $worktree) {
        & git worktree remove --force $worktree
    }
    Pop-Location
}
