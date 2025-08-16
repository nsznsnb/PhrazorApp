# PhrazorApp

## 開発者向けドキュメント

このプロジェクトの実装方針・命名・画面とサービスの役割・バリデーション方針などは  
**[Development Guide](./DEVELOPMENT.md)** にまとめています。

## ディレクトリ概要

- **Components/** … Razor コンポーネント
  - **Account/** … Identity UI 用のコンポーネント
    - **Pages/** … サインイン/登録/管理などのページ
    - **Shared/** … Identity 専用の共有部品（レイアウト/メニュー等）
  - **Layout/** … MainLayout, NavMenu など
  - **Pages/** … ルーティングされるページ
  - **Shared/** … 再利用コンポーネント
    - **Controls/** … 見た目中心（Presentational）
    - **Containers/** … サービスを利用した処理まで担う（Smart/Container）
    - **Dialogs/** … モーダル（*Dialog / *DialogHost）
  - **App.razor / Routes.razor / _Imports.razor** … ルート/ルーティング/共通 using

- **UI/** … UI 制御・UI依存資産
  - **Managers/** … ローディング表示・UI操作のオーケストレーション
  - **Interop/** … JS 連携（IJSRuntime ラッパー等）
  - **State/** … 画面間で保持する状態（Scoped）
  - **Themes/** … MudTheme などのテーマ実体
  - **Rendering/** … RenderModes（Server/Wasm/Auto のプリセット）

- **Services/** … 業務サービス（UI 非依存）

- **Data/** … データアクセス
  - **ApplicationDbContext.cs** … Identity（Code-first）
  - **EngDbContext.cs** … 業務（DB-first, Scaffold 生成物を使用）
  - **Entities/** … Scaffold 生成物
  - **Repositories/** … リポジトリ
  - **Migrations/** … Identity のマイグレーション

- **Infrastructure/** … 外部 I/O（Blob/Email/OpenAI 等）と Options バインド

- **Extensions/** … 拡張メソッド（ServiceCollection/WebApplication など）

- **Common/** … 横断“定義物”（処理なし）
  - **Constants/** … 定数
  - **Enums/** … 列挙
  - **Options/** … 設定の POCO
  - **Results/** … 結果型（ServiceResult など）
  - **Validation/** … バリデーション定義

- **tools/** … 開発用スクリプト（PowerShell 等）  
  ※ JavaScript は **wwwroot/js/** に配置

- **wwwroot/** … 静的ファイル（js/css/画像 など）

- **GlobalUsings.cs** … グローバル using 定義

