/*
 * EHC upload guard - loaded once by the layouts, applies to every document upload screen.
 *
 * The 40+ upload partials each post files with their own XMLHttpRequest code. Most only handle
 * a 200 answer, so a server error, a timeout or a dropped connection left the "Uploading..."
 * overlay on screen forever. This file fixes that in one place without touching each partial:
 *   - shows real upload progress (percent) in the overlay
 *   - stops waiting after 5 minutes
 *   - on any failure closes the overlay and tells the user what happened
 *   - treats a {"status":"Failure"} answer as a failure instead of "Document(s) Uploaded"
 *   - checks file size and empty files as soon as a file is picked
 */
(function (window, $) {
    'use strict';
    if (!window.XMLHttpRequest || window.__ehcUploadGuard) return;
    window.__ehcUploadGuard = true;

    var UPLOAD_URL = /\/File\/(AddDocument\w*|AddWarningDocument|ManualAddDocument|Upload\w*)/i;
    var TIMEOUT_MS = 5 * 60 * 1000;
    var MAX_MB = 50;
    var BLOCKED_EXT = /\.(exe|bat|cmd|msi|js|vbs|ps1|dll|scr|com)$/i;

    function closeOverlay() {
        try { if (window.HoldOn) window.HoldOn.close(); } catch (e) { }
    }

    function setOverlayText(text) {
        var el = document.getElementById('holdon-message');
        if (el) el.innerHTML = text;
    }

    function tell(title, text, type) {
        if (window.swal) { try { window.swal(title, text, type || 'error'); return; } catch (e) { } }
        if (window.bootbox) { try { window.bootbox.alert('<b>' + title + '</b><br/>' + text); return; } catch (e) { } }
        window.alert(title + '\n\n' + text);
    }

    function resetPendingIds() {
        // Several partials keep the selected checklist ids in a page level variable; clear it so a retry
        // does not send the same ids twice.
        try { if (typeof window.documentCheckListIds === 'string') window.documentCheckListIds = ''; } catch (e) { }
    }

    function failureFromBody(xhr) {
        try {
            var type = xhr.getResponseHeader('Content-Type') || '';
            if (type.indexOf('json') < 0) return null;
            var body = JSON.parse(xhr.responseText);
            if (body && typeof body.status === 'string' && body.status.toLowerCase() === 'failure') {
                return body.message || 'The document could not be saved. Please check the file and try again.';
            }
        } catch (e) { }
        return null;
    }

    function statusMessage(xhr) {
        if (xhr.status === 0) return 'The connection to the server was lost. Please check your network and try again.';
        if (xhr.status === 401 || xhr.status === 403) return 'Your session has expired or you do not have access. Please log in again.';
        if (xhr.status === 404 || xhr.status === 413) return 'The file is too large for the server. Please upload a smaller file (under ' + MAX_MB + ' MB).';
        return 'The server could not save the document (error ' + xhr.status + '). Please try again. If it keeps failing, report the time and file name to the administrator.';
    }

    var origOpen = window.XMLHttpRequest.prototype.open;
    var origSend = window.XMLHttpRequest.prototype.send;

    window.XMLHttpRequest.prototype.open = function (method, url) {
        try {
            this.__ehcUpload = String(method).toUpperCase() === 'POST' && UPLOAD_URL.test(String(url));
        } catch (e) { this.__ehcUpload = false; }
        return origOpen.apply(this, arguments);
    };

    window.XMLHttpRequest.prototype.send = function (body) {
        var xhr = this;
        if (!xhr.__ehcUpload) return origSend.apply(xhr, arguments);

        var handled = false;
        try { if (!xhr.timeout) xhr.timeout = TIMEOUT_MS; } catch (e) { }

        try {
            if (xhr.upload) {
                xhr.upload.addEventListener('progress', function (e) {
                    if (!e.lengthComputable) return;
                    var pct = Math.round(e.loaded * 100 / e.total);
                    setOverlayText(pct < 100
                        ? 'Uploading document/s... ' + pct + '%'
                        : 'Saving document/s... Please wait');
                });
            }
        } catch (e) { }

        function fail(title, text) {
            if (handled) return;
            handled = true;
            resetPendingIds();
            closeOverlay();
            tell(title, text, 'error');
        }

        // Wrap the page's own handler so a failure is reported clearly instead of "Document(s) Uploaded".
        var pageHandler = xhr.onreadystatechange;
        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    var problem = failureFromBody(xhr);
                    if (problem) { fail('Upload failed', problem); return; }
                    closeOverlay();
                } else if (xhr.status !== 0) {
                    // Let the page clean up first (some reset their own fields), then make sure the
                    // overlay is gone and the user knows why.
                    try { if (pageHandler) pageHandler.apply(xhr, arguments); } catch (e) { }
                    fail('Upload failed', statusMessage(xhr));
                    return;
                }
            }
            if (pageHandler) return pageHandler.apply(xhr, arguments);
        };

        xhr.addEventListener('timeout', function () {
            fail('Upload is taking too long', 'The server did not answer within 5 minutes. Refresh the page to check whether the document was saved before uploading again.');
        });
        xhr.addEventListener('error', function () { fail('Upload failed', statusMessage(xhr)); });
        xhr.addEventListener('abort', function () { fail('Upload cancelled', 'The upload was cancelled. Please try again.'); });

        return origSend.apply(xhr, arguments);
    };

    // Check files as soon as they are picked, on every upload screen.
    if ($) {
        $(document).on('change', 'input[type=file]', function () {
            var input = this, files = input.files || [], problems = [];
            for (var i = 0; i < files.length; i++) {
                var f = files[i];
                if (f.size === 0) problems.push(f.name + ' is empty.');
                else if (f.size > MAX_MB * 1024 * 1024) problems.push(f.name + ' is ' + (f.size / 1048576).toFixed(1) + ' MB. The limit is ' + MAX_MB + ' MB.');
                else if (BLOCKED_EXT.test(f.name)) problems.push(f.name + ' is not an allowed file type.');
            }
            if (problems.length) {
                try { $(input).val(''); } catch (e) { }
                tell('File not accepted', problems.join('\n') + '\n\nPlease choose a PDF, image or Office document.', 'warning');
            }
        });
    }
})(window, window.jQuery);
