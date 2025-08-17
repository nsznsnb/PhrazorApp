
---

Development.md

# Development Guide

> このドキュメントは **ChatGPT に実装方針を学習させるためのドキュメント**です。
> 本ドキュメント自体もChatGPTで作成されています。

---

## 用語（このプロジェクトでの意味）

* **Entity**
  DB のテーブルと 1 対 1 で対応する型。**UI には出しません。**

* **Model（画面モデル / DTO）**
  画面でバインドして使う型。**UI では Entity ではなく Model を使います。**

* **ModelMapper（マッピングファイル）**
  Entity ⇔ Model の変換や、取得時に必要な項目だけ取り出す投影式をまとめたクラス。

* **Validator（バリデーションファイル）**
  **FluentValidation を用いて、Model の「単項目」および「項目間の関連」バリデーションを定義する場所**です。
  例：必須・文字数・形式・リスト重複など。**DB を参照するチェックは扱いません**（サービス側で行います）。

* **PageMessageStore**
  **ページ上部へのメッセージ表示**を担います。ページ全体（モデル全体）に関わるエラーをまとめて見せたいときに使います。

* **UiOperationRunner**
  画面の **読み込み / 書き込み / 再読み込み** を、進捗表示（オーバーレイ）・Snackbar・例外処理付きで実行するヘルパーです。
  **サービスを呼び出してアプリ固有の処理を実行**し、メッセージ表示や例外処理まで面倒を見ます。

* **Service（アプリケーションサービス）**
  **アプリ固有の処理**（保存・取得・外部 API 連携・業務ルール）を実装する層。**戻り値は ServiceResult** に揃えます。
  DB 操作は **主に UnitOfWork** を使ってまとめます。

* **ServiceResult**
  成功 / 警告 / 失敗 とメッセージ、必要ならデータを返す共通型。

* **UnitOfWork**
  1 回の保存処理をひとまとめにして **トランザクション管理**を行い、**複数の Repository を自由に取り出して操作**できます。

* **Repository**
  単一テーブル（Entity）を扱う読み書きユーティリティ。

---

# 1) UI

## 1-1. UI は Model を使う

* 画面の双方向バインドは **Model** で行います。
* DB の Entity は直接使わず、**ModelMapper** で変換してから扱います。

## 1-2. 確認ダイアログ

```csharp
var ok = await DialogService.ShowConfirmAsync(
    DialogConfirmType.RegisterConfirm,
    "この操作を実行します。よろしいですか？"
);
if (!ok) return;
```

## 1-3. バリデーション（EditContext + FluentValidation + PageMessageStore）

* **役割分担**

  * **Validator（バリデーションファイル）**：フィールド単位・項目間の関連のルールを定義
  * **ページ側**：送信時に検証を実行し、失敗時は **PageMessageStore** にページ全体のメッセージを出す
  * **DB が必要なチェック**（重複・整合性など）は **サービス内で実施**（後述の Service 参照）

**① Validator ファイル（例: `XxxModelValidator.cs`）**

```csharp
// using FluentValidation;

public sealed class XxxModelValidator : AbstractValidator<XxxModel>
{
    public XxxModelValidator()
    {
        // 単項目・関連の例
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);

        // ページ全体へ出したいエラー（ValidationSummary / PageMessageStore に載せる）
        RuleFor(x => x.SubGenres).Custom((list, ctx) =>
        {
            if (list is null || list.Count == 0) return;

            if (list.Count > 3)
                ctx.AddFailure(string.Empty, "サブジャンルの追加は3個までです。");

            var defCount = list.Count(s => s.IsDefault);
            if (defCount != 1)
                ctx.AddFailure(string.Empty, "サブジャンルを指定する場合、既定は1件だけにしてください。");

            var hasDup = list
                .Select(s => (s.Name ?? string.Empty).Trim())
                .Where(n => n.Length > 0)
                .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);
            if (hasDup)
                ctx.AddFailure(string.Empty, "サブジャンル名が重複しています。");
        });
    }
}
```

**② ページ側（例: `Edit.razor` / コードビハインド）**

```csharp
private EditContext? _editCtx;

protected override void OnInitialized()
{
    _editCtx = new EditContext(_model); // _model はページの Model
}

public async ValueTask<bool> SubmitAsync(bool validate = true)
{
    if (_editCtx is null) return false;

    if (validate && !_editCtx.Validate())
    {
        // 失敗時だけページ上部にメッセージを出す
        _editCtx.PublishPageLevelErrors(ServiceProvider);
        return false;
    }

    await SubmitCoreAsync();
    return true;
}

private void OnInvalidSubmit(EditContext editContext)
{
    editContext.PublishPageLevelErrors(ServiceProvider);
}
```

## 1-4. UiOperationRunner（4 つの API）

> **サービスを用いてアプリ固有の処理を実行**しながら、**進捗（オーバーレイ）・Snackbar・例外処理**まで担当します。
> 画面側は最小限のコードで結果の反映に集中できます。

```csharp
// 読み込み（オーバーレイなし）
var dto = await UiOperationRunner.ReadAsync(() => Service.GetAsync(args));

// 読み込み（オーバーレイあり）
var dto2 = await UiOperationRunner.ReadWithOverlayAsync(
    () => Service.GetAsync(args),
    message: "読込中…"
);

// 書き込み（ServiceResult<T> を受け取る）
var op = await UiOperationRunner.WriteAsync(
    () => Service.SaveAsync(model),
    message: "保存しています…"
);
if (op.IsSuccess && op.Data is not null)
{
    // 画面に反映
}

// 書き込み → 再読み込み
var op2 = await UiOperationRunner.WriteThenReloadAsync(
    write:  () => Service.SaveAsync(model),
    reload: () => Service.GetAsync(args),
    message: "更新しています…"
);
```

---

# 2) Service

## 2-1. 役割

* **アプリ固有の処理**（保存・取得・外部 API・業務ルール）を実装します。UI には依存しません。
* DB 操作は **主に UnitOfWork** を使ってまとめます（次章）。

## 2-2. 戻り値は ServiceResult に統一

```csharp
// 成功（データあり）
return ServiceResult.Success(data, "保存しました");

// 警告（ユーザーに修正を促す）
return ServiceResult.Warning(data, "同じ内容のデータがあります。内容を見直してください。");

// 失敗
return ServiceResult.Error<T>("保存に失敗しました");

// 戻り値なし（履歴記録など）
return ServiceResult.None.Success();
// 失敗（戻り値なし）
return ServiceResult.None.Error("記録に失敗しました");
```

## 2-3. DB を見るチェック（重複・整合性など）

* **DB を参照しないと判断できない確認**は、**サービス内の保存処理などで行います**。
* 問題が見つかったら **`ServiceResult.Warning(...)`** を返し、UI 側で Snackbar(Warning) による案内を行います。

---

# 3) UnitOfWork（と Repository）

## 3-1. 何ができるか

* **トランザクション管理**を行い、1 回の保存処理をまとめて成功/失敗させられます。
* **複数の Repository を自由に取り出して操作**できます（例：A を保存しつつ B も更新）。

## 3-2. よく使う呼び方

* **読み込み**は `ReadAsync(...)`。
* **保存**は `ExecuteInTransactionAsync(...)`。この中で個別に `SaveChanges` は呼びません。

```csharp
// 読み取り（必要な列だけを投影して最小限取得）
var list = await _uow.ReadAsync(async repos =>
{
    return await repos.MyEntities
        .Queryable(true) // AsNoTracking
        .Select(MyModelMapper.ListProjection) // 投影式で必要な列だけ
        .ToListAsync();
});

// 保存（Upsert の一例：複数 Repository を扱ってもOK）
var saved = await _uow.ExecuteInTransactionAsync(async repos =>
{
    var entity = await repos.MyEntities
        .Queryable(false) // Tracking
        .FirstOrDefaultAsync(x => x.Id == model.Id);

    if (entity is null)
    {
        var eNew = model.ToEntity(userId);
        await repos.MyEntities.AddAsync(eNew);

        // 例：別テーブルも同時に
        // await repos.OtherEntities.AddAsync(...);

        return eNew.ToModel();
    }
    else
    {
        model.ApplyTo(entity);
        await repos.MyEntities.UpdateAsync(entity);
        return entity.ToModel();
    }
});
```

## 3-3. DbContext の共通設定

* **ユーザーごとの出し分けが必要なエンティティは、読み込み時に自動で現在のユーザーにフィルタ**されます。
  そのため、通常は `UserId` 条件を追加しません（管理用途で全件が必要なときのみフィルタを外します）。
* **`CreatedAt` / `UpdatedAt` は保存時に自動設定**されます。
  画面やサービスで個別に設定する必要はありません。

## 3-4. Repository とは

* 単一テーブル（Entity）を扱うための小さなユーティリティです。
  代表メソッド：`Queryable(asNoTracking)`, `AddAsync`, `UpdateAsync`, `DeleteAsync` など。
  UnitOfWork から複数の Repository を取り出して連携させます。

## 3-5. マッピングファイルの書き方（ModelMapper）

* 目的：Entity⇔Model の変換を一箇所に集め、**UI と DB を分離**します。
* 最低限そろえるもの：

  * `ToModel(this Entity)`：Entity → Model
  * `ToEntity(this Model, string userId)`：Model → 新規 Entity
  * `ApplyTo(this Model, Entity)`：既存 Entity へ反映
  * **投影用の式**：`Expression<Func<TEntity, TModel>>`（必要な列だけ選ぶため）

```csharp
public static class MyModelMapper
{
    public static MyModel ToModel(this DMyEntity e) => new()
    {
        // プロパティ写像
    };

    public static DMyEntity ToEntity(this MyModel m, string userId)
    {
        // 採番・初期値・子要素の作成など
    }

    public static void ApplyTo(this MyModel m, DMyEntity e)
    {
        // 値の反映、子要素の同期（全置換/差分は要件に合わせて）
    }

    public static readonly Expression<Func<DMyEntity, MyModel>> ListProjection
        = e => new MyModel
        {
            // 必要な列だけ
        };
}
```

---

# 付記（細目）

* **Common フォルダの役割**
  Common フォルダには、アプリ全体で横断的に使用する定義（定数・Enum・設定）を格納します。**既存の定義をなるべく再利用**してください。

* **Common の名前空間規約**
  Common に作成するすべてのファイルの名前空間は **`PhrazorApp.Common`** とします。`GlobalUsings.cs` に登録済みのため、**各ファイルで個別の `using` を記述する必要はありません**。

* **Extensions の活用**
  拡張メソッドは **Extensions** フォルダにまとまっています。特に **ダイアログ起動を簡略化する拡張メソッド（例：`DialogServiceExtensions` の各種 `Show*` 系）** を積極的に利用し、ページ側の記述量を削減してください。

* **Components/Shared の共通 UI**
  **Components/Shared** には共通 UI コンポーネントが配置されています。ページ実装ではこれらを活用し、**個別ページの重複コードを減らす**方針です（例：`SectionTitle`、`ActionCard`、`TableWithToolbar` など）。

* **共通処理・コンポーネント作成の第一原則**
  **シンプルに利用できることを最優先**とします。**外部に公開する API は極力絞り（引数は少なく）**、必要になった時点で段階的に公開・拡張する方針とします。

---

#### <small>ChatGptへのプロンプト</small>

<small>

* Zipファイル「**PhrazorApp.zip**」を展開し、本アプリのソースコードを学習してください。
* プロジェクトルートにある **README.md** と **DEVELOPMENT.md** にはフォルダ構成と実装方針が記載されています。**コード提供の際にはその方針に従ってください**。
* 以降のチャットは、**ソースコードと実装方針が学習済み**であることを前提に行います。

</small>
