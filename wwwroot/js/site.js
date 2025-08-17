// --- Enter フォーカス移動（全体適用） ------------------------------

// 一度だけ初期化
(function enableEnterFocusNav() {
    if (window.__phrazor_enter_nav__) return;
    window.__phrazor_enter_nav__ = true;

    const isFocusable = (el) =>
        el &&
        el.tabIndex >= 0 &&
        !el.disabled &&
        !el.readOnly &&
        el.offsetParent !== null;

    const getFocusableElements = () => {
        return Array.from(document.querySelectorAll(`
      input:not([type="hidden"]),
      select,
      [tabindex]:not([tabindex="-1"])
    `)).filter(isFocusable);
    };

    const moveFocus = (current, reverse = false) => {
        const focusable = getFocusableElements();
        const index = focusable.indexOf(current);
        if (index === -1) return;
        const nextIndex = reverse ? index - 1 : index + 1;
        if (focusable[nextIndex]) focusable[nextIndex].focus();
    };

    document.addEventListener('keydown', (e) => {
        // 他で preventDefault 済みなら何もしない（Blazor側で制御可能に）
        if (e.defaultPrevented) return;

        const tag = e.target.tagName.toLowerCase();
        const type = e.target.getAttribute('type');

        // Enter: submit以外の input/select でフォーカス移動
        if (e.key === 'Enter') {
            if (['input', 'select'].includes(tag) && type !== 'submit') {
                e.preventDefault();
                moveFocus(e.target);
            }
        }
    });
})();

// --- JsInteropManager から呼ぶユーティリティ（ES Module） ---------

export function focusElementById(id) {
    try {
        const el = id && document.getElementById(id);
        if (el) el.focus({ preventScroll: true });
    } catch { /* no-op */ }
}

export function scrollToId(id, smooth = true) {
    try {
        const el = id && document.getElementById(id);
        if (!el) return;
        el.scrollIntoView({ behavior: smooth ? 'smooth' : 'auto', block: 'start', inline: 'nearest' });
    } catch { /* no-op */ }
}
