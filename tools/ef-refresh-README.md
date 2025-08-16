# secrets.json に接続文字列を設定（初回だけ）
dotnet user-secrets set 'ConnectionStrings:EngDbContext'     'Server=<SERVER>;Database=<DB_NAME>;User ID=<USER>;Password=<PASSWORD>;TrustServerCertificate=True'
dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'Server=<SERVER>;Database=<DB_NAME>;User ID=<USER>;Password=<PASSWORD>;TrustServerCertificate=True'

# 既定テーブルを再スキャフォールディング → Identity を update
.\tools\ef-refresh.ps1 -Project .\PhrazorApp.csproj

# バックアップを取りつつ実行
.\tools\ef-refresh.ps1 -Project .\PhrazorApp.csproj -BackupEntities

# Dry-Run（実行しないでコマンドだけ確認）
.\tools\ef-refresh.ps1 -DryRun

# テーブルを限定
.\tools\ef-refresh.ps1 -Tables MGenres,MDiaryTags

# 未コミット確認をスキップ（CIなど非対話）
.\tools\ef-refresh.ps1 -Force