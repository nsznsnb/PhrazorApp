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

export function historyBack(fallbackUrl) {
    try {
        if (window.history.length > 1) {
            window.history.back();
        } else if (fallbackUrl) {
            window.location.assign(fallbackUrl);
        }
    } catch {
        if (fallbackUrl) window.location.assign(fallbackUrl);
    }
}

// --- Visibility observer (IntersectionObserver) ---
const _ivMap = new Map();
let _ivSeq = 0;

/**
 * 任意の要素IDを監視し、画面に入ったら C# の OnTargetVisibilityChanged(true/false) を呼ぶ
 */
export function observeElementVisibility(elementId, dotnetRef) {
    const el = document.getElementById(elementId);
    if (!el) return null;

    const id = `iv_${++_ivSeq}`;
    const obs = new IntersectionObserver((entries) => {
        // どれか一つでも可視なら true
        const visible = entries.some(e => e.isIntersecting && e.intersectionRatio > 0);
        try { dotnetRef.invokeMethodAsync('OnTargetVisibilityChanged', visible); } catch { }
    }, {
        threshold: 0   // 1ピクセルでも見えたら true（必要なら 0.1 等に調整）
    });

    obs.observe(el);
    _ivMap.set(id, obs);
    return id;
}

export function unobserveElementVisibility(id) {
    const obs = _ivMap.get(id);
    if (!obs) return;
    obs.disconnect();
    _ivMap.delete(id);
}
