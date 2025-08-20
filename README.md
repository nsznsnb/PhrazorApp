# PhrazorApp
## PhrazorApp�̊T�v
**�p�앶���K**��**�p����L�Y��**�����C���@�\�Ƃ��āA�p��\���͂̋������s���A�v���P�[�V�����ł��B

## �J���Ҍ����h�L�������g

���̃v���W�F�N�g�̎������j�E�����E��ʂƃT�[�r�X�̖����E�o���f�[�V�������j�Ȃǂ�  
**[Development Guide](./DEVELOPMENT.md)** �ɂ܂Ƃ߂Ă��܂��B

## �V�X�e���\���}

```mermaid
flowchart TB
  Client["���[�U�[�̃u���E�U�iHTTPS�j"] --> UI

  subgraph App["Azure Web App / Blazor Server"]
    UI["UI�iMudBlazor�j"]
    SVC["Services"]
    INF["Infrastructure"]
    DATA["Repositories"]
    UI -->|UIOperationRunner| SVC
    UI -->|UIOperationRunner| INF
    SVC -->|UnitOfWork| DATA
  end

  DATA --> SQL["Azure SQL Database"]
  INF -.�\��/������.- BLOB["Azure Blob Storage�i�摜�j"]
  INF --> OPENAI["OpenAI�i�摜�����^���L�Y��j"]
  INF --> RESEND["Resend�i���[�����M�j"]
```

## �f�B���N�g���T�v

- **Components/** �c Razor �R���|�[�l���g
  - **Account/** �c Identity UI �p�̃R���|�[�l���g
    - **Pages/** �c �T�C���C��/�o�^/�Ǘ��Ȃǂ̃y�[�W
    - **Shared/** �c Identity ��p�̋��L���i�i���C�A�E�g/���j���[���j
  - **Layout/** �c MainLayout, NavMenu �Ȃ�
  - **Pages/** �c ���[�e�B���O�����y�[�W
  - **Shared/** �c �ė��p�R���|�[�l���g
    - **Controls/** �c ��UI���i(�T�[�r�X�̏������܂܂Ȃ�)
    - **Containers/** �c �T�[�r�X�𗘗p���������܂ŒS��UI���i
    - **Dialogs/** �c ���[�_���i*Dialog / *DialogHost�j
  - **App.razor / Routes.razor / _Imports.razor** �c ���[�g/���[�e�B���O/���� using

- **UI/** �c UI ����EUI�ˑ����Y
  - **Managers/** �c ���[�f�B���O�\���EUI����̃I�[�P�X�g���[�V����
  - **Interop/** �c JS �A�g�iIJSRuntime ���b�p�[���j
  - **State/** �c ��ʊԂŕێ������ԁiScoped�j
  - **Themes/** �c �A�v���S�̂̐F���Ȃǂ̐ݒ�
  - **Rendering/** �c RenderModes�iServer/Wasm/Auto �̃v���Z�b�g�j

- **Services/** �c �Ɩ��T�[�r�X�iUI ��ˑ��j

- **Data/** �c �f�[�^�A�N�Z�X
  - **ApplicationDbContext.cs** �c Identity�iCode-first�j
  - **EngDbContext.cs** �c �Ɩ��iDB-first, Scaffold ���������g�p�j
  - **Entities/** �c Scaffold ������
  - **Repositories/** �c ���|�W�g��
  - **Migrations/** �c Identity �̃}�C�O���[�V����

- **Infrastructure/** �c �O�� I/O�iBlob/Email/OpenAI ���j�� Options �o�C���h

- **Extensions/** �c �g�����\�b�h�i�_�C�A���O�Ăяo���Ȃǂ֗̕����\�b�h)

- **Common/** �c �A�v���S�̂Ŏg�p����g��`�E�ݒ�h
  - **Constants/** �c �萔
  - **Csv/** �c Csv�捞���ɗ��p����X�L�[�}(��̃��^���)
  - **Enums/** �c �񋓑�
  - **Options/** �c appsettings.json�Ǎ��p�N���X��
  - **Results/** �c ���ʌ^�iServiceResult �Ȃǁj
  - **Validation/** �c �o���f�[�V�����̃O���[�o���ݒ�

- **tools/** �c �J���p�X�N���v�g�iPowerShell ���j  
  �� JavaScript �� **wwwroot/js/** �ɔz�u

- **wwwroot/** �c �ÓI�t�@�C���ijs/css/�摜 �Ȃǁj

- **GlobalUsings.cs** �c �O���[�o�� using ��`

