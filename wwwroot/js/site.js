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
            if (['input', 'select', 'textarea'].includes(tag) && type !== 'submit') {
                e.preventDefault();
                moveFocus(e.target);
            }
        }

    });
});
