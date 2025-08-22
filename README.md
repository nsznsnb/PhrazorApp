# Phrazor
## �{�A�v���P�[�V�����̊T�v
�{�A�v���P�[�V�����͉p�앶���K�Ɖp����L�Y������C���@�\�Ƃ��āA�p��\���͂̋������s���܂��B

## �J���Ҍ����h�L�������g

���̃v���W�F�N�g�̗p��E�������j�� **[Development Guide](./DEVELOPMENT.md)** �ɂ܂Ƃ߂Ă��܂��B

## �V�X�e���\���}

```mermaid
flowchart TB

  Client["���[�U�[�̃u���E�U�iHTTPS�j"]

  subgraph Azure["Azure App Service"]
    direction TB

    subgraph Blazor["Blazor Server"]
      direction TB
     UI["UI�iMudBlazor�j"]
     SVC["Services"]
     INF["Infrastructure"]
     DATA["Repositories"]
     UI -->|UIOperationRunner| SVC
     SVC -->|UnitOfWork| DATA
     SVC --> INF
    end
  end

  Client --> UI

  DATA --> SQL["Azure SQL Database"]
  INF --> RESEND["Resend�i���[�����M�j"]
  INF --> OPENAI["OpenAI�i�摜�����^���L�Y��j"]
  INF -.�\��/������.- BLOB["Azure Blob Storage�i�摜�j"]


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

- **Services/** �c �A�v���ŗL�̏���(�T�[�r�X)

- **Data/** �c �f�[�^�x�[�X�A�N�Z�X
  - **ApplicationDbContext.cs** �c ASP.NET Identity (Code-first)
  - **EngDbContext.cs** �c EFCore�iDB-first�j
  - **Entities/** �c �e�[�u����\������N���X�Q(�G���e�B�e�B)
  - **Repositories/** �c �e�[�u�������S��(���|�W�g��)
  - **Migrations/** �c Identity �̃}�C�O���[�V����

- **Downloads/** �c �O���z�M����t�@�C���Q

- **Endpoints/** �c Minimal API �Q�i�F�t���t�@�C���z�M�pAPI ��)

- **Infrastructure/** �c �O�� I/O�iBlob/Email/OpenAI ���j

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

