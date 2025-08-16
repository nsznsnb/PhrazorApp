<#
 .SYNOPSIS
   EngDbContext（DB-first）の再スキャフォールディングと、
   ApplicationDbContext（Identity）のマイグレーション適用を一括実行します。

 .FEATURES（安全弁）
   - Data\Entities の未コミット差分があれば警告＆確認（-Force で抑止）
   - 実行前バックアップ（zip）オプション（-BackupEntities）
   - Dry-Run（コマンド表示のみ、実行しない）（-DryRun）
   - secrets のログ出力抑止（値は画面に出しません）

 .REQUIREMENTS
   - dotnet SDK / EF Core Tools（`dotnet ef`）
   - プロジェクトが UserSecrets を設定済み（UserSecretsId）
   - secrets.json or 環境変数に接続文字列が設定済み
       ConnectionStrings:EngDbContext / ConnectionStrings:DefaultConnection

 .USAGE（例）
   ./scripts/ef-refresh.ps1 -Project .\PhrazorApp.csproj
   ./scripts/ef-refresh.ps1 -Project .\PhrazorApp.csproj -Tables MGenres,MDiaryTags -BackupEntities
   ./scripts/ef-refresh.ps1 -Project .\PhrazorApp.csproj -DryRun
#>

[CmdletBinding()]
param(
  # 対象の csproj パス
  [string]$Project = "$PSScriptRoot\..\PhrazorApp.csproj",

  # DbContext 名
  [string]$EngContext = "EngDbContext",
  [string]$IdentityContext = "ApplicationDbContext",

  # ConnectionStrings のキー名（secrets 参照）
  [string]$ConnKeyEng = "ConnectionStrings:EngDbContext",
  [string]$ConnKeyIdentity = "ConnectionStrings:DefaultConnection",

  # 出力フォルダ（従来どおり）
  [string]$ContextDir = "Data",
  [string]$EntitiesDir = "Data\Entities",
  [string]$MigrationsDir = "Data\Migrations",

  # スキーマ／対象テーブル（Identity の AspNet* は含めない）
  [string]$Schema = "dbo",
  [string[]]$Tables = @("MGenres","MSubGenres","MPhrases","MDiaryTags"),

  # 初回 Identity マイグレーション名（存在しないときのみ追加）
  [string]$MigrationName = "IdentityInitial",

  # オプション
  [switch]$SkipScaffold,       # スキャフォールディングをスキップ
  [switch]$SkipMigrate,        # database update をスキップ
  [switch]$EmitIdempotentSql,  # 冪等スクリプトを出力（.\deploy\Identity.sql）
  [switch]$BackupEntities,     # スキャフォールディング前に Data\Entities を zip 退避
  [switch]$DryRun,             # コマンドを表示のみ（実行しない）
  [switch]$Force               # 確認プロンプトを出さずに続行
)

# ─────────────────────────────────────────────────────────────────────────────
# 基本設定
# ─────────────────────────────────────────────────────────────────────────────
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Info ($m){ Write-Host "[INFO] $m" -ForegroundColor Cyan }
function Warn ($m){ Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Ok   ($m){ Write-Host "[OK]   $m" -ForegroundColor Green }
function Err  ($m){ Write-Host "[ERR]  $m" -ForegroundColor Red }

function Exec([string]$cmd, [string[]]$args) {
  $show = $cmd + ' ' + ($args -join ' ')
  Info "EXEC: $show"
  if ($DryRun) { return }
  & $cmd @args
}

# ─────────────────────────────────────────────────────────────────────────────
# プロジェクト・前提チェック
# ─────────────────────────────────────────────────────────────────────────────
$projFull = Resolve-Path $Project
$projDir  = Split-Path $projFull
Push-Location $projDir
Info "Project: $projFull"
Info " Dir   : $projDir"

# dotnet ef
try { & dotnet ef --version | Out-Null } catch {
  Err "dotnet-ef が見つかりません。'dotnet tool install -g dotnet-ef' を実行してください。"
  exit 1
}

# Git リポジトリかどうか
$gitRoot = (git rev-parse --show-toplevel) 2>$null
$inGit = $LASTEXITCODE -eq 0

# Entities の未コミット差分チェック（安全弁）
if ($inGit -and -not $Force -and -not $SkipScaffold) {
  $dirty = (git status --porcelain -- $EntitiesDir) 2>$null
  if ($dirty) {
    Warn "$EntitiesDir に未コミットの変更があります。--force で確認をスキップできます。"
    Write-Host $dirty
    $ans = Read-Host "続行しますか？(y/N)"
    if ($ans -ne 'y') { Pop-Location; exit 1 }
  }
}

# secrets が設定されていそうか（値は画面に表示しない）
function Has-Secret($key) {
  try {
    $v = (& dotnet user-secrets get $key) 2>$null
    if ($v -and $v.Trim().Length -gt 0) { return $true }
  } catch { }
  return $false
}
if (-not (Has-Secret $ConnKeyEng)) {
  Warn "UserSecrets '$ConnKeyEng' が見つかりません（appsettings/環境変数で供給されていれば問題ありません）。"
}
if (-not (Has-Secret $ConnKeyIdentity)) {
  Warn "UserSecrets '$ConnKeyIdentity' が見つかりません（appsettings/環境変数で供給されていれば問題ありません）。"
}

# テーブル列挙の簡易チェック
if (-not $SkipScaffold -and ($Tables.Count -eq 0)) {
  Err "スキャフォールディング対象のテーブルが 0 件です。-Tables で指定してください。"
  Pop-Location; exit 1
}

# バックアップ（任意）
if ($BackupEntities -and -not $SkipScaffold) {
  if (Test-Path $EntitiesDir) {
    $stamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupDir = Join-Path $PSScriptRoot "..\backup"
    if (-not (Test-Path $backupDir)) { New-Item -ItemType Directory -Path $backupDir | Out-Null }
    $zip = Join-Path $backupDir "Entities_$stamp.zip"
    Info "バックアップ作成: $zip"
    if (-not $DryRun) { Compress-Archive -Path "$EntitiesDir\*" -DestinationPath $zip -Force }
    Ok "バックアップ完了。"
  } else {
    Warn "$EntitiesDir が見つかりません。バックアップはスキップします。"
  }
}

# ─────────────────────────────────────────────────────────────────────────────
# 1) EngDbContext のスキャフォールディング（DB-first）
# ─────────────────────────────────────────────────────────────────────────────
if (-not $SkipScaffold) {
  Info "スキャフォールディング開始（出力: $EntitiesDir, コンテキスト: $EngContext）"
  $tableArgs = @()
  foreach ($t in $Tables) { $tableArgs += @("--table", $t) }

  $args = @(
    "dbcontext","scaffold", "Name=$ConnKeyEng", "Microsoft.EntityFrameworkCore.SqlServer",
    "--context", $EngContext,
    "--context-dir", $ContextDir,
    "--output-dir", $EntitiesDir,
    "--schema", $Schema,
    "--use-database-names",
    "--data-annotations",
    "--no-onconfiguring",
    "--force"
  ) + $tableArgs

  Exec "dotnet" $args
  Ok "スキャフォールディング完了。"
} else {
  Warn "スキャフォールディングをスキップしました。"
}

# ─────────────────────────────────────────────────────────────────────────────
# 2) Identity マイグレーション（存在しなければ追加）→ 適用
# ─────────────────────────────────────────────────────────────────────────────
# 既存マイグレーション判定
Info "Identity 既存マイグレーションを取得中…"
$migsText = ""
try { $migsText = (& dotnet ef migrations list --context $IdentityContext) -join "`n" } catch { $migsText = "" }

$needAdd = $true
if ($migsText -match [regex]::Escape($MigrationName)) {
  Info "マイグレーション '$MigrationName' は既に存在します。add はスキップします。"
  $needAdd = $false
}

if ($needAdd) {
  Info "Identity にマイグレーション '$MigrationName' を追加します。"
  Exec "dotnet" @("ef","migrations","add",$MigrationName,"--context",$IdentityContext,"--output-dir",$MigrationsDir)
  Ok "マイグレーション追加完了。"
}

# database update
if (-not $SkipMigrate) {
  Info "Identity の database update を実行します。"
  Exec "dotnet" @("ef","database","update","--context",$IdentityContext)
  Ok "database update 完了。"
} else {
  Warn "database update をスキップしました。"
}

# 冪等スクリプト
if ($EmitIdempotentSql) {
  $outDir = Resolve-Path (Join-Path $PSScriptRoot "..\deploy") -ErrorAction SilentlyContinue
  if (-not $outDir) { $outDir = New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot "..\deploy") }
  $scriptPath = Join-Path $outDir "Identity.sql"
  Info "冪等スクリプトを出力: $scriptPath"
  Exec "dotnet" @("ef","migrations","script","-c",$IdentityContext,"--idempotent","-o",$scriptPath)
  Ok "冪等スクリプト出力完了。"
}

Pop-Location
Ok "すべての処理が完了しました。"
