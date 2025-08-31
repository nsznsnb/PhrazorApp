// SSRページ共通: パスワード一時表示 + 二重送信ガード + 「直前Enterでsubmitへフォーカス」 + 初期フォーカス
(function () {
    'use strict';

    // --- パスワード一時表示（Mudの AdornmentClickFunction="showPassword" 前提） ---
    let __pwdTimer;
    window.showPassword = function (inputElement /*, button */) {
        if (!inputElement) return;
        const isPwd = inputElement.type === 'password';
        inputElement.type = isPwd ? 'text' : 'password';
        clearTimeout(__pwdTimer);
        if (isPwd) __pwdTimer = setTimeout(() => { inputElement.type = 'password'; }, 5000);
    };

    // --- 仕掛け本体 ---
    function armForm(formId = 'myForm', btnId = 'btnSubmit') {
        const form = document.getElementById(formId);
        if (!form) return;

        // 送信ボタン（ID優先、なければ form 内の最初の submit）
        const submitBtn =
            document.getElementById(btnId) ||
            form.querySelector('button[type="submit"], input[type="submit"]');
        if (!submitBtn) return;

        // 二重送信ガード
        let isSubmitting = false;
        form.addEventListener('submit', function (e) {
            if (isSubmitting) { e.preventDefault(); return; }
            isSubmitting = true;
            if (submitBtn instanceof HTMLElement) {
                submitBtn.disabled = true;
                const tag = submitBtn.tagName.toLowerCase();
                if (tag === 'input') submitBtn.value = '処理中...';
                else submitBtn.innerText = '処理中...';
                submitBtn.setAttribute('aria-busy', 'true');
            }
        });

        // 「submit のひとつ前」から Enter で submit へフォーカス
        const isFocusable = (el) =>
            el && el.tabIndex >= 0 && !el.disabled && !el.readOnly && el.offsetParent !== null;

        const candidates = Array.from(form.querySelectorAll(`
      input:not([type="hidden"]),
      select,
      textarea,
      button,
      [tabindex]:not([tabindex="-1"])
    `)).filter(isFocusable);

        const submitIndex = candidates.indexOf(submitBtn);
        if (submitIndex <= 0) return; // 先頭がsubmit or 見つからない → 何もしない

        const prev = candidates[submitIndex - 1];
        if (!prev || prev.dataset.enterSubmitArmed === '1') return;

        prev.dataset.enterSubmitArmed = '1'; // 多重バインド防止
        prev.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter' || e.isComposing) return;
            e.preventDefault();
            e.stopPropagation();       // ← この要素の Enter だけはグローバル処理を抑止
            submitBtn.focus();         // 送信はしない（必要なら .click() / form.requestSubmit() へ）
        }, { capture: true });         // ← この要素限定で先取り
    }

    // --- 追加：最初の入力欄へ初期フォーカス（ダイアログ/既存フォーカスを尊重） ---
    function focusFirstInput() {
        // 既にフォーカスがある/ダイアログ表示中なら触らない
        if (document.activeElement && document.activeElement !== document.body) return;
        if (document.querySelector('[role="dialog"], .mud-dialog, .mud-overlay')) return;

        // main/.mud-main-content を優先して探索（無ければdocument全体）
        const scope = document.querySelector('main, .mud-main-content') || document;

        const first = Array.from(scope.querySelectorAll(`
          input:not([type="hidden"]):not([disabled]):not([readonly]),
          select:not([disabled]):not([readonly]),
          textarea:not([disabled]):not([readonly]),
          [role="combobox"]
        `)).find(el => el instanceof HTMLElement && el.tabIndex >= 0 && el.offsetParent !== null);

        if (first) {
            try { first.focus({ preventScroll: true }); } catch { first.focus(); }
        }
    }

    // DOM 構築後に 1 回だけアーム & 初期フォーカス
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', () => { armForm(); focusFirstInput(); }, { once: true });
    } else {
        armForm();
        focusFirstInput();
    }
})();