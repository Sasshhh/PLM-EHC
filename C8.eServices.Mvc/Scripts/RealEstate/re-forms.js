/* Real Estate (DPRE) applicant forms: section stepper, progress, Yes/No pills,
   file names and "which required item is missing" help. No dependencies; loaded with defer. */
(function () {
    'use strict';
    var page = document.querySelector('.re-form-page');
    if (!page) { return; }

    function visible(el) { return !!(el && (el.offsetWidth || el.offsetHeight || el.getClientRects().length)); }
    function closest(el, sel) { while (el && el.nodeType === 1) { if (el.matches(sel)) { return el; } el = el.parentElement; } return null; }

    // ---- Yes/No segmented radios and tick boxes ----
    function syncChecks() {
        page.querySelectorAll('.re-seg label.radio-inline').forEach(function (l) {
            var i = l.querySelector('input'); l.classList.toggle('is-checked', !!(i && i.checked));
        });
        page.querySelectorAll('.re-check').forEach(function (l) {
            var i = l.querySelector('input[type=checkbox]'); l.classList.toggle('is-checked', !!(i && i.checked));
        });
    }
    page.addEventListener('change', function (e) {
        if (e.target.type === 'radio' || e.target.type === 'checkbox') { syncChecks(); }
        if (e.target.type === 'file') { e.target.classList.toggle('has-file', e.target.files && e.target.files.length > 0); var c = closest(e.target, '.doc-upload-card'); if (c) { c.classList.remove('is-missing'); } }
        update();
    });
    syncChecks();

    // ---- stepper: one chip per card that has data-step ----
    var cards = Array.prototype.slice.call(page.querySelectorAll('.re-card[data-step]'));
    var chips = [];
    var bar, progress;
    if (cards.length > 1) {
        bar = document.createElement('nav');
        bar.className = 're-steps';
        bar.setAttribute('aria-label', 'Form sections');
        cards.forEach(function (card, i) {
            if (!card.id) { card.id = 're-sec-' + (i + 1); }
            var a = document.createElement('a');
            a.href = '#' + card.id;
            a.className = 're-step';
            a.innerHTML = '<span class="n"><span>' + (i + 1) + '</span></span>' + card.getAttribute('data-step');
            a.addEventListener('click', function (e) {
                e.preventDefault();
                card.scrollIntoView({ behavior: 'smooth', block: 'start' });
                var f = card.querySelector('input:not([type=hidden]),select,textarea');
                if (f && visible(f)) { setTimeout(function () { try { f.focus({ preventScroll: true }); } catch (x) { } }, 400); }
            });
            bar.appendChild(a); chips.push(a);
        });
        progress = document.createElement('div');
        progress.className = 're-progress';
        progress.innerHTML = '<i></i>';
        var first = cards[0];
        first.parentNode.insertBefore(bar, first);
        first.parentNode.insertBefore(progress, first);
        chips[0].classList.add('is-current');
    }

    function isRequired(el) {
        if (el.required) { return true; }
        var g = closest(el, '.form-group');
        var lab = g && g.querySelector('label.req');
        return !!lab && !closest(el, '.re-seg');
    }
    function filled(el) {
        if (el.type === 'checkbox') { return el.checked; }
        if (el.type === 'radio') { return !!page.querySelector('input[name="' + el.name + '"]:checked'); }
        if (el.type === 'file') { return (el.files && el.files.length > 0) || !!(closest(el, '.form-group') || document).querySelector('.text-success'); }
        return !!(el.value && el.value.trim());
    }
    function cardDone(card) {
        var fields = card.querySelectorAll('input:not([type=hidden]):not([type=button]):not([disabled]),select:not([disabled]),textarea');
        var any = false, ok = true;
        fields.forEach(function (f) {
            if (!visible(f) && f.type !== 'file' && f.type !== 'radio' && f.type !== 'checkbox') { return; }
            var g = closest(f, '.form-group,.doc-upload-card');
            if (g && !visible(g)) { return; }
            if (isRequired(f)) { any = true; if (!filled(f)) { ok = false; } }
        });
        var segs = card.querySelectorAll('.re-seg');
        segs.forEach(function (s) { var g = closest(s, '.form-group'); if (g && !visible(g)) { return; } any = true; if (!s.querySelector('input:checked')) { ok = false; } });
        return any && ok;
    }
    function update() {
        if (!chips.length) { return; }
        var done = 0;
        cards.forEach(function (c, i) { var d = cardDone(c); chips[i].classList.toggle('is-done', d); if (d) { done++; } });
        progress.firstChild.style.width = Math.round(done * 100 / cards.length) + '%';
    }
    page.addEventListener('input', function () { clearTimeout(update.t); update.t = setTimeout(update, 250); });
    update();

    // highlight the section in view
    if (chips.length && 'IntersectionObserver' in window) {
        var io = new IntersectionObserver(function (entries) {
            entries.forEach(function (en) {
                if (!en.isIntersecting) { return; }
                var i = cards.indexOf(en.target);
                chips.forEach(function (c, j) { c.classList.toggle('is-current', i === j); });
                var chip = chips[i];
                if (chip && bar.scrollWidth > bar.clientWidth) { bar.scrollTo({ left: chip.offsetLeft - 12, behavior: 'smooth' }); }
            });
        }, { rootMargin: '-35% 0px -60% 0px' });
        cards.forEach(function (c) { io.observe(c); });
    }

    // ---- show which required item is missing, including hidden file inputs and other tabs ----
    window.reFirstInvalid = function (form) {
        var bad = null;
        Array.prototype.some.call(form.elements, function (el) {
            if (el.willValidate && !el.checkValidity()) {
                var g = closest(el, '.form-group,.doc-upload-card');
                if (g && g.style.display === 'none') { return false; }
                bad = el; return true;
            }
            return false;
        });
        if (!bad) { return null; }
        var pane = closest(bad, '.tab-pane');
        if (pane && !pane.classList.contains('active') && window.jQuery) { jQuery('a[href="#' + pane.id + '"]').tab('show'); }
        var card = closest(bad, '.doc-upload-card');
        var target = card || closest(bad, '.form-group') || bad;
        if (card) { card.classList.add('is-missing'); }
        target.scrollIntoView({ behavior: 'smooth', block: 'center' });
        var label = card ? (card.querySelector('.doc-title') || {}).textContent : ((closest(bad, '.form-group') || {}).querySelector ? (closest(bad, '.form-group').querySelector('label') || {}).textContent : '');
        if (visible(bad)) { setTimeout(function () { try { bad.focus({ preventScroll: true }); bad.reportValidity(); } catch (x) { } }, 350); }
        return (label || bad.name || '').replace(/\*/g, '').trim();
    };
})();
