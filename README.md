# PhrazorApp

## �J���Ҍ����h�L�������g

���̃v���W�F�N�g�̎������j�E�����E��ʂƃT�[�r�X�̖����E�o���f�[�V�������j�Ȃǂ�  
**[Development Guide](./DEVELOPMENT.md)** �ɂ܂Ƃ߂Ă��܂��B

## �f�B���N�g���T�v

- **Components/** �c Razor �R���|�[�l���g
  - **Account/** �c Identity UI �p�̃R���|�[�l���g
    - **Pages/** �c �T�C���C��/�o�^/�Ǘ��Ȃǂ̃y�[�W
    - **Shared/** �c Identity ��p�̋��L���i�i���C�A�E�g/���j���[���j
  - **Layout/** �c MainLayout, NavMenu �Ȃ�
  - **Pages/** �c ���[�e�B���O�����y�[�W
  - **Shared/** �c �ė��p�R���|�[�l���g
    - **Controls/** �c �����ڒ��S�iPresentational�j
    - **Containers/** �c �T�[�r�X�𗘗p���������܂ŒS���iSmart/Container�j
    - **Dialogs/** �c ���[�_���i*Dialog / *DialogHost�j
  - **App.razor / Routes.razor / _Imports.razor** �c ���[�g/���[�e�B���O/���� using

- **UI/** �c UI ����EUI�ˑ����Y
  - **Managers/** �c ���[�f�B���O�\���EUI����̃I�[�P�X�g���[�V����
  - **Interop/** �c JS �A�g�iIJSRuntime ���b�p�[���j
  - **State/** �c ��ʊԂŕێ������ԁiScoped�j
  - **Themes/** �c MudTheme �Ȃǂ̃e�[�}����
  - **Rendering/** �c RenderModes�iServer/Wasm/Auto �̃v���Z�b�g�j

- **Services/** �c �Ɩ��T�[�r�X�iUI ��ˑ��j

- **Data/** �c �f�[�^�A�N�Z�X
  - **ApplicationDbContext.cs** �c Identity�iCode-first�j
  - **EngDbContext.cs** �c �Ɩ��iDB-first, Scaffold ���������g�p�j
  - **Entities/** �c Scaffold ������
  - **Repositories/** �c ���|�W�g��
  - **Migrations/** �c Identity �̃}�C�O���[�V����

- **Infrastructure/** �c �O�� I/O�iBlob/Email/OpenAI ���j�� Options �o�C���h

- **Extensions/** �c �g�����\�b�h�iServiceCollection/WebApplication �Ȃǁj

- **Common/** �c ���f�g��`���h�i�����Ȃ��j
  - **Constants/** �c �萔
  - **Enums/** �c ��
  - **Options/** �c �ݒ�� POCO
  - **Results/** �c ���ʌ^�iServiceResult �Ȃǁj
  - **Validation/** �c �o���f�[�V������`

- **tools/** �c �J���p�X�N���v�g�iPowerShell ���j  
  �� JavaScript �� **wwwroot/js/** �ɔz�u

- **wwwroot/** �c �ÓI�t�@�C���ijs/css/�摜 �Ȃǁj

- **GlobalUsings.cs** �c �O���[�o�� using ��`

