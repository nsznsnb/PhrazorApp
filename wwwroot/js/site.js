document.addEventListener('DOMContentLoaded', () => {
    const isFocusable = (el) => {
        return el &&
            el.tabIndex >= 0 &&
            !el.disabled &&
            !el.readOnly &&
            el.offsetParent !== null;
    };

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
        if (focusable[nextIndex]) {
            focusable[nextIndex].focus();
        }
    };

    document.addEventListener('keydown', (e) => {
        const tag = e.target.tagName.toLowerCase();
        const type = e.target.getAttribute('type');

        // Enter キー: submit を除く input, select でフォーカス移動
        if (e.key === 'Enter') {
            if (['input', 'select'].includes(tag) && type !== 'submit') {
                e.preventDefault();
                moveFocus(e.target);
            }
        }

    });
});

window.busyGuard = (function () {
    let enabled = false;

    function isAllowed(target) {
        // data-allow-while-busy が付いている祖先要素があれば許可
        return !!target?.closest?.('[data-allow-while-busy]');
    }

    function onClick(e) {
        if (!enabled) return;
        if (isAllowed(e.target)) return;
        e.preventDefault();
        e.stopPropagation();
    }

    function onKeydown(e) {
        if (!enabled) return;
        // ボタン/リンクのキーボード起動もブロック
        if (e.key === 'Enter' || e.key === ' ') {
            if (isAllowed(e.target)) return;
            e.preventDefault();
            e.stopPropagation();
        }
    }

    return {
        init() {
            // キャプチャ段階で先取りブロック
            document.addEventListener('click', onClick, true);
            document.addEventListener('keydown', onKeydown, true);
        },
        setEnabled(v) {
            enabled = !!v;
            // 視覚的なヒント（任意）：マウスカーソル
            document.documentElement.classList.toggle('busy-cursor', enabled);
        }
    };
})();