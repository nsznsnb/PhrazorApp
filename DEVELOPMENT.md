
---

Development.md

# Development Guide

> ���̃h�L�������g�� **ChatGPT �Ɏ������j���w�K�����邽�߂̃h�L�������g**�ł��B
> �{�h�L�������g���̂�ChatGPT�ō쐬����Ă��܂��B

---

## �p��i���̃v���W�F�N�g�ł̈Ӗ��j

* **Entity**
  DB �̃e�[�u���� 1 �� 1 �őΉ�����^�B**UI �ɂ͏o���܂���B**

* **Model�i��ʃ��f�� / DTO�j**
  ��ʂŃo�C���h���Ďg���^�B**UI �ł� Entity �ł͂Ȃ� Model ���g���܂��B**

* **ModelMapper�i�}�b�s���O�t�@�C���j**
  Entity �� Model �̕ϊ���A�擾���ɕK�v�ȍ��ڂ������o�����e�����܂Ƃ߂��N���X�B

* **Validator�i�o���f�[�V�����t�@�C���j**
  **FluentValidation ��p���āAModel �́u�P���ځv����сu���ڊԂ̊֘A�v�o���f�[�V�������`����ꏊ**�ł��B
  ��F�K�{�E�������E�`���E���X�g�d���ȂǁB**DB ���Q�Ƃ���`�F�b�N�͈����܂���**�i�T�[�r�X���ōs���܂��j�B

* **PageMessageStore**
  **�y�[�W�㕔�ւ̃��b�Z�[�W�\��**��S���܂��B�y�[�W�S�́i���f���S�́j�Ɋւ��G���[���܂Ƃ߂Č��������Ƃ��Ɏg���܂��B

* **UiOperationRunner**
  ��ʂ� **�ǂݍ��� / �������� / �ēǂݍ���** ���A�i���\���i�I�[�o�[���C�j�ESnackbar�E��O�����t���Ŏ��s����w���p�[�ł��B
  **�T�[�r�X���Ăяo���ăA�v���ŗL�̏��������s**���A���b�Z�[�W�\�����O�����܂Ŗʓ|�����܂��B

* **Service�i�A�v���P�[�V�����T�[�r�X�j**
  **�A�v���ŗL�̏���**�i�ۑ��E�擾�E�O�� API �A�g�E�Ɩ����[���j����������w�B**�߂�l�� ServiceResult** �ɑ����܂��B
  DB ����� **��� UnitOfWork** ���g���Ă܂Ƃ߂܂��B

* **ServiceResult**
  ���� / �x�� / ���s �ƃ��b�Z�[�W�A�K�v�Ȃ�f�[�^��Ԃ����ʌ^�B

* **UnitOfWork**
  1 ��̕ۑ��������ЂƂ܂Ƃ߂ɂ��� **�g�����U�N�V�����Ǘ�**���s���A**������ Repository �����R�Ɏ��o���đ���**�ł��܂��B

* **Repository**
  �P��e�[�u���iEntity�j�������ǂݏ������[�e�B���e�B�B

---

# 1) UI

## 1-1. UI �� Model ���g��

* ��ʂ̑o�����o�C���h�� **Model** �ōs���܂��B
* DB �� Entity �͒��ڎg�킸�A**ModelMapper** �ŕϊ����Ă��爵���܂��B

## 1-2. �m�F�_�C�A���O

```csharp
var ok = await DialogService.ShowConfirmAsync(
    DialogConfirmType.RegisterConfirm,
    "���̑�������s���܂��B��낵���ł����H"
);
if (!ok) return;
```

## 1-3. �o���f�[�V�����iEditContext + FluentValidation + PageMessageStore�j

* **�������S**

  * **Validator�i�o���f�[�V�����t�@�C���j**�F�t�B�[���h�P�ʁE���ڊԂ̊֘A�̃��[�����`
  * **�y�[�W��**�F���M���Ɍ��؂����s���A���s���� **PageMessageStore** �Ƀy�[�W�S�̂̃��b�Z�[�W���o��
  * **DB ���K�v�ȃ`�F�b�N**�i�d���E�������Ȃǁj�� **�T�[�r�X���Ŏ��{**�i��q�� Service �Q�Ɓj

**�@ Validator �t�@�C���i��: `XxxModelValidator.cs`�j**

```csharp
// using FluentValidation;

public sealed class XxxModelValidator : AbstractValidator<XxxModel>
{
    public XxxModelValidator()
    {
        // �P���ځE�֘A�̗�
        RuleFor(x => x.Title).NotEmpty().MaximumLength(100);

        // �y�[�W�S�̂֏o�������G���[�iValidationSummary / PageMessageStore �ɍڂ���j
        RuleFor(x => x.SubGenres).Custom((list, ctx) =>
        {
            if (list is null || list.Count == 0) return;

            if (list.Count > 3)
                ctx.AddFailure(string.Empty, "�T�u�W�������̒ǉ���3�܂łł��B");

            var defCount = list.Count(s => s.IsDefault);
            if (defCount != 1)
                ctx.AddFailure(string.Empty, "�T�u�W���������w�肷��ꍇ�A�����1�������ɂ��Ă��������B");

            var hasDup = list
                .Select(s => (s.Name ?? string.Empty).Trim())
                .Where(n => n.Length > 0)
                .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
                .Any(g => g.Count() > 1);
            if (hasDup)
                ctx.AddFailure(string.Empty, "�T�u�W�����������d�����Ă��܂��B");
        });
    }
}
```

**�A �y�[�W���i��: `Edit.razor` / �R�[�h�r�n�C���h�j**

```csharp
private EditContext? _editCtx;

protected override void OnInitialized()
{
    _editCtx = new EditContext(_model); // _model �̓y�[�W�� Model
}

public async ValueTask<bool> SubmitAsync(bool validate = true)
{
    if (_editCtx is null) return false;

    if (validate && !_editCtx.Validate())
    {
        // ���s�������y�[�W�㕔�Ƀ��b�Z�[�W���o��
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

## 1-4. UiOperationRunner�i4 �� API�j

> **�T�[�r�X��p���ăA�v���ŗL�̏��������s**���Ȃ���A**�i���i�I�[�o�[���C�j�ESnackbar�E��O����**�܂ŒS�����܂��B
> ��ʑ��͍ŏ����̃R�[�h�Ō��ʂ̔��f�ɏW���ł��܂��B

```csharp
// �ǂݍ��݁i�I�[�o�[���C�Ȃ��j
var dto = await UiOperationRunner.ReadAsync(() => Service.GetAsync(args));

// �ǂݍ��݁i�I�[�o�[���C����j
var dto2 = await UiOperationRunner.ReadWithOverlayAsync(
    () => Service.GetAsync(args),
    message: "�Ǎ����c"
);

// �������݁iServiceResult<T> ���󂯎��j
var op = await UiOperationRunner.WriteAsync(
    () => Service.SaveAsync(model),
    message: "�ۑ����Ă��܂��c"
);
if (op.IsSuccess && op.Data is not null)
{
    // ��ʂɔ��f
}

// �������� �� �ēǂݍ���
var op2 = await UiOperationRunner.WriteThenReloadAsync(
    write:  () => Service.SaveAsync(model),
    reload: () => Service.GetAsync(args),
    message: "�X�V���Ă��܂��c"
);
```

---

# 2) Service

## 2-1. ����

* **�A�v���ŗL�̏���**�i�ۑ��E�擾�E�O�� API�E�Ɩ����[���j���������܂��BUI �ɂ͈ˑ����܂���B
* DB ����� **��� UnitOfWork** ���g���Ă܂Ƃ߂܂��i���́j�B

## 2-2. �߂�l�� ServiceResult �ɓ���

```csharp
// �����i�f�[�^����j
return ServiceResult.Success(data, "�ۑ����܂���");

// �x���i���[�U�[�ɏC���𑣂��j
return ServiceResult.Warning(data, "�������e�̃f�[�^������܂��B���e���������Ă��������B");

// ���s
return ServiceResult.Error<T>("�ۑ��Ɏ��s���܂���");

// �߂�l�Ȃ��i�����L�^�Ȃǁj
return ServiceResult.None.Success();
// ���s�i�߂�l�Ȃ��j
return ServiceResult.None.Error("�L�^�Ɏ��s���܂���");
```

## 2-3. DB ������`�F�b�N�i�d���E�������Ȃǁj

* **DB ���Q�Ƃ��Ȃ��Ɣ��f�ł��Ȃ��m�F**�́A**�T�[�r�X���̕ۑ������Ȃǂōs���܂�**�B
* ��肪���������� **`ServiceResult.Warning(...)`** ��Ԃ��AUI ���� Snackbar(Warning) �ɂ��ē����s���܂��B

---

# 3) UnitOfWork�i�� Repository�j

## 3-1. �����ł��邩

* **�g�����U�N�V�����Ǘ�**���s���A1 ��̕ۑ��������܂Ƃ߂Đ���/���s�������܂��B
* **������ Repository �����R�Ɏ��o���đ���**�ł��܂��i��FA ��ۑ����� B ���X�V�j�B

## 3-2. �悭�g���Ăѕ�

* **�ǂݍ���**�� `ReadAsync(...)`�B
* **�ۑ�**�� `ExecuteInTransactionAsync(...)`�B���̒��Ōʂ� `SaveChanges` �͌Ăт܂���B

```csharp
// �ǂݎ��i�K�v�ȗ񂾂��𓊉e���čŏ����擾�j
var list = await _uow.ReadAsync(async repos =>
{
    return await repos.MyEntities
        .Queryable(true) // AsNoTracking
        .Select(MyModelMapper.ListProjection) // ���e���ŕK�v�ȗ񂾂�
        .ToListAsync();
});

// �ۑ��iUpsert �̈��F���� Repository �������Ă�OK�j
var saved = await _uow.ExecuteInTransactionAsync(async repos =>
{
    var entity = await repos.MyEntities
        .Queryable(false) // Tracking
        .FirstOrDefaultAsync(x => x.Id == model.Id);

    if (entity is null)
    {
        var eNew = model.ToEntity(userId);
        await repos.MyEntities.AddAsync(eNew);

        // ��F�ʃe�[�u����������
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

## 3-3. DbContext �̋��ʐݒ�

* **���[�U�[���Ƃ̏o���������K�v�ȃG���e�B�e�B�́A�ǂݍ��ݎ��Ɏ����Ō��݂̃��[�U�[�Ƀt�B���^**����܂��B
  ���̂��߁A�ʏ�� `UserId` ������ǉ����܂���i�Ǘ��p�r�őS�����K�v�ȂƂ��̂݃t�B���^���O���܂��j�B
* **`CreatedAt` / `UpdatedAt` �͕ۑ����Ɏ����ݒ�**����܂��B
  ��ʂ�T�[�r�X�Ōʂɐݒ肷��K�v�͂���܂���B

## 3-4. Repository �Ƃ�

* �P��e�[�u���iEntity�j���������߂̏����ȃ��[�e�B���e�B�ł��B
  ��\���\�b�h�F`Queryable(asNoTracking)`, `AddAsync`, `UpdateAsync`, `DeleteAsync` �ȂǁB
  UnitOfWork ���畡���� Repository �����o���ĘA�g�����܂��B

## 3-5. �}�b�s���O�t�@�C���̏������iModelMapper�j

* �ړI�FEntity��Model �̕ϊ�����ӏ��ɏW�߁A**UI �� DB �𕪗�**���܂��B
* �Œ�����낦����́F

  * `ToModel(this Entity)`�FEntity �� Model
  * `ToEntity(this Model, string userId)`�FModel �� �V�K Entity
  * `ApplyTo(this Model, Entity)`�F���� Entity �֔��f
  * **���e�p�̎�**�F`Expression<Func<TEntity, TModel>>`�i�K�v�ȗ񂾂��I�Ԃ��߁j

```csharp
public static class MyModelMapper
{
    public static MyModel ToModel(this DMyEntity e) => new()
    {
        // �v���p�e�B�ʑ�
    };

    public static DMyEntity ToEntity(this MyModel m, string userId)
    {
        // �̔ԁE�����l�E�q�v�f�̍쐬�Ȃ�
    }

    public static void ApplyTo(this MyModel m, DMyEntity e)
    {
        // �l�̔��f�A�q�v�f�̓����i�S�u��/�����͗v���ɍ��킹�āj
    }

    public static readonly Expression<Func<DMyEntity, MyModel>> ListProjection
        = e => new MyModel
        {
            // �K�v�ȗ񂾂�
        };
}
```

---

# �t�L�i�זځj

* **Common �t�H���_�̖���**
  Common �t�H���_�ɂ́A�A�v���S�̂ŉ��f�I�Ɏg�p�����`�i�萔�EEnum�E�ݒ�j���i�[���܂��B**�����̒�`���Ȃ�ׂ��ė��p**���Ă��������B

* **Common �̖��O��ԋK��**
  Common �ɍ쐬���邷�ׂẴt�@�C���̖��O��Ԃ� **`PhrazorApp.Common`** �Ƃ��܂��B`GlobalUsings.cs` �ɓo�^�ς݂̂��߁A**�e�t�@�C���Ōʂ� `using` ���L�q����K�v�͂���܂���**�B

* **Extensions �̊��p**
  �g�����\�b�h�� **Extensions** �t�H���_�ɂ܂Ƃ܂��Ă��܂��B���� **�_�C�A���O�N�����ȗ�������g�����\�b�h�i��F`DialogServiceExtensions` �̊e�� `Show*` �n�j** ��ϋɓI�ɗ��p���A�y�[�W���̋L�q�ʂ��팸���Ă��������B

* **Components/Shared �̋��� UI**
  **Components/Shared** �ɂ͋��� UI �R���|�[�l���g���z�u����Ă��܂��B�y�[�W�����ł͂��������p���A**�ʃy�[�W�̏d���R�[�h�����炷**���j�ł��i��F`SectionTitle`�A`ActionCard`�A`TableWithToolbar` �Ȃǁj�B

* **���ʏ����E�R���|�[�l���g�쐬�̑�ꌴ��**
  **�V���v���ɗ��p�ł��邱�Ƃ��ŗD��**�Ƃ��܂��B**�O���Ɍ��J���� API �͋ɗ͍i��i�����͏��Ȃ��j**�A�K�v�ɂȂ������_�Œi�K�I�Ɍ��J�E�g��������j�Ƃ��܂��B

---

#### <small>ChatGpt�ւ̃v�����v�g</small>

<small>

* Zip�t�@�C���u**PhrazorApp.zip**�v��W�J���A�{�A�v���̃\�[�X�R�[�h���w�K���Ă��������B
* �v���W�F�N�g���[�g�ɂ��� **README.md** �� **DEVELOPMENT.md** �ɂ̓t�H���_�\���Ǝ������j���L�ڂ���Ă��܂��B**�R�[�h�񋟂̍ۂɂ͂��̕��j�ɏ]���Ă�������**�B
* �ȍ~�̃`���b�g�́A**�\�[�X�R�[�h�Ǝ������j���w�K�ς�**�ł��邱�Ƃ�O��ɍs���܂��B

</small>
