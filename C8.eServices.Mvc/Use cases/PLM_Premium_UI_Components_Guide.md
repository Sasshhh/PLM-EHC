# PLM Premium UI Components Guide

> **Purpose:** Reference guide for creating visually stunning, interactive UI components within the PLM MVC 5 / Razor framework. These patterns were developed for the Complaint Scheduling workflow and can be reused across any module.

---

## Table of Contents
1. [Razor Gotchas (MUST READ)](#razor-gotchas)
2. [Premium Appointment/Date Selector Modal](#premium-modal-selector)
3. [Dark Glass Card with State Transitions](#dark-glass-card)
4. [Animated Slot/Item Cards](#animated-slot-cards)
5. [CSS Animation Patterns](#css-animations)
6. [General Design Principles](#design-principles)

---

## Razor Gotchas (MUST READ) <a name="razor-gotchas"></a>

### 1. `@@keyframes` — Escaping `@` in CSS
Razor interprets `@` as a code delimiter. In `<style>` blocks inside `.cshtml` files, you MUST write CSS keyframes as:
```css
@@keyframes myAnimation {
    from { opacity: 0; }
    to { opacity: 1; }
}
```
NOT `@keyframes` — that will throw `CS0103: The name 'keyframes' does not exist`.

### 2. No Null-Conditional Operator (`?.`) in Razor Expressions
The Razor view compiler in MVC 5 targets C# 5, which does NOT support `?.`. Always use ternary checks:
```csharp
/* WRONG — will crash */
@(sched.DateToSchedule?.ShecduleDate?.Day)

/* CORRECT */
@(sched.DateToSchedule != null && sched.DateToSchedule.ShecduleDate.HasValue 
    ? sched.DateToSchedule.ShecduleDate.Value.Day.ToString() : "")
```

### 3. `@if` vs `if` — Context Awareness
- **After HTML content** (e.g., after a `<button>` tag): Use `@if` — Razor needs the `@` to switch back to code.
- **Inside an existing C# block** (e.g., inside `@using (Html.BeginForm(...)) {` with no preceding HTML): Use `if` without `@` — you're already in code.
- **Rule of thumb:** If the previous line was HTML markup, use `@if`. If the previous line was C# code, use `if`.

### 4. Nested Forms
HTML does NOT support nested `<form>` elements. If you place a `<form>` inside another `<form>`, the inner form won't submit correctly. The outer form's validation will fire instead.
- **Solution:** Move secondary forms outside the main `@using (Html.BeginForm(...))` block.

---

## Premium Modal Selector <a name="premium-modal-selector"></a>

### Architecture
The modal selector uses three layers:
1. **Trigger Card** — A dark, animated card that shows "No Selection" or the selected item
2. **Modal Overlay** — Blurred backdrop with centered modal panel
3. **Slot Cards** — Individual selectable items inside the modal

### Trigger Card Pattern
```html
<div class="appt-selected-display" id="apptSelectorTrigger" onclick="openApptModal()">
    <div class="appt-placeholder-icon">
        <i class="ion ion-ios-calendar"></i>
    </div>
    <!-- No selection state -->
    <div class="appt-no-selection">
        <div class="appt-placeholder-text">
            <strong>No Appointment Selected</strong>
            Choose a date &amp; time that works best for you
        </div>
        <span class="appt-change-btn">Select Appointment</span>
    </div>
    <!-- Has selection state (hidden by default, shown via JS) -->
    <div class="appt-selected-info">
        <div class="appt-sel-date" id="displaySelDate"></div>
        <div class="appt-sel-time" id="displaySelTime"></div>
        <span class="appt-change-btn">Change Selection</span>
    </div>
</div>
```

### State Toggle (JavaScript)
```javascript
function confirmSlotSelection() {
    var $trigger = $('#apptSelectorTrigger');
    $trigger.addClass('has-selection');       // Toggles card from "empty" to "selected"
    $('#displaySelDate').text(selectedDate);   // Inject selected values
    $('#displaySelTime').html(selectedTime);
    $('#hiddenFieldId').val(selectedId);       // Set hidden form field
    closeApptModal();
}
```

### CSS State Toggle
```css
.appt-selected-display.has-selection {
    border: 2px solid #22c55e;
    background: linear-gradient(135deg, #052e16 0%, #14532d 100%);
}
.appt-selected-display.has-selection .appt-no-selection {
    display: none;   /* Hide the placeholder */
}
.appt-selected-display.has-selection .appt-selected-info {
    display: block;  /* Show the selected values */
}
```

---

## Dark Glass Card <a name="dark-glass-card"></a>

### Core CSS
```css
.dark-glass-card {
    background: linear-gradient(135deg, #0f172a 0%, #1e293b 100%);
    border: 2px dashed rgba(99, 102, 241, 0.4);
    border-radius: 16px;
    padding: 40px 30px;
    text-align: center;
    cursor: pointer;
    transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
    position: relative;
    overflow: hidden;
}
.dark-glass-card:hover {
    border-color: rgba(99, 102, 241, 0.8);
    transform: translateY(-2px);
    box-shadow: 0 20px 60px rgba(99, 102, 241, 0.15);
}
```

### Ambient Glow Effect (Radial Pulse)
```css
.dark-glass-card::before {
    content: '';
    position: absolute;
    top: -50%; left: -50%;
    width: 200%; height: 200%;
    background: radial-gradient(circle, rgba(99, 102, 241, 0.05) 0%, transparent 70%);
    animation: pulse-bg 4s ease-in-out infinite;
}
```

---

## Animated Slot Cards <a name="animated-slot-cards"></a>

### Selectable Card Pattern
```css
.slot-card {
    background: rgba(255, 255, 255, 0.03);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 16px;
    padding: 20px 24px;
    cursor: pointer;
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    display: flex;
    align-items: center;
    gap: 16px;
}
.slot-card:hover {
    border-color: rgba(99, 102, 241, 0.4);
    transform: translateX(4px);
}
.slot-card.selected {
    border-color: #6366f1;
    background: rgba(99, 102, 241, 0.1);
    box-shadow: 0 0 0 1px rgba(99, 102, 241, 0.3), 0 8px 30px rgba(99, 102, 241, 0.15);
}
```

### Animated Checkmark on Selection
```css
.slot-card.selected::after {
    content: '\2713';
    position: absolute;
    top: 12px; right: 16px;
    width: 28px; height: 28px;
    background: linear-gradient(135deg, #6366f1, #8b5cf6);
    border-radius: 50%;
    color: white;
    font-size: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    animation: checkPop 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
```

### Date Block (Mini Calendar Look)
```css
.date-block {
    width: 64px; height: 72px;
    background: linear-gradient(135deg, #312e81, #4338ca);
    border-radius: 14px;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
}
.date-block .day { font-size: 26px; font-weight: 800; color: #fff; }
.date-block .month { font-size: 11px; color: #c7d2fe; text-transform: uppercase; letter-spacing: 1px; }
```

---

## CSS Animation Patterns <a name="css-animations"></a>

### Float Animation (for icons)
```css
@@keyframes float {
    0%, 100% { transform: translateY(0); }
    50% { transform: translateY(-8px); }
}
.floating-icon { animation: float 3s ease-in-out infinite; }
```

### Modal Slide Up
```css
@@keyframes modalSlideUp {
    from { opacity: 0; transform: translateY(40px) scale(0.95); }
    to { opacity: 1; transform: translateY(0) scale(1); }
}
```

### Checkmark Pop
```css
@@keyframes checkPop {
    from { transform: scale(0); }
    to { transform: scale(1); }
}
```

### Backdrop Blur (Modal Overlay)
```css
.modal-overlay {
    position: fixed;
    top: 0; left: 0; right: 0; bottom: 0;
    background: rgba(0, 0, 0, 0.6);
    backdrop-filter: blur(8px);
    -webkit-backdrop-filter: blur(8px);
    z-index: 99999;
    display: flex;
    justify-content: center;
    align-items: center;
}
```

---

## Design Principles <a name="design-principles"></a>

### Color Palette
| Purpose | Color | Hex |
|---------|-------|-----|
| Primary (Indigo) | Buttons, borders, accents | `#6366f1` |
| Primary Dark | Date blocks, deep backgrounds | `#312e81` / `#4338ca` |
| Success (Green) | Confirmed states | `#22c55e` / `#16a34a` |
| Card Background | Dark panels | `#0f172a` → `#1e293b` |
| Text Primary | Headings on dark | `#f1f5f9` / `#e2e8f0` |
| Text Muted | Subtitles on dark | `#94a3b8` / `#64748b` |
| Accent Light | Time badges, tags | `#a5b4fc` / `#c7d2fe` |

### Key UX Rules
1. **Never nest forms** — secondary actions go outside the main form
2. **Always add `z-index: 1`** to content over `::before` pseudo-element overlays
3. **Use `cubic-bezier(0.4, 0, 0.2, 1)`** for smooth, professional easing
4. **Modal close** — support both overlay click AND Escape key
5. **Progressive disclosure** — show a simple trigger, reveal complexity in a modal
6. **State feedback** — change colors/borders/icons when user makes a selection

### Integration with PLM Panel Layout
These premium components sit INSIDE the standard PLM panel structure:
```html
<div class="panel-group">
    <div class="panel panel-default">
        <div class="panel-heading" align="center">Page Title</div>
        <div class="panel-body">
            <!-- Premium components go here -->
        </div>
    </div>
</div>
```

---

## Action Button Patterns <a name="action-buttons"></a>

> **Problem:** Bootstrap's `btn-default` becomes invisible on hover (white text on white bg) and `btn-success[disabled]` shows a confusing washed-out green. Always use custom button classes.

### Primary Action (Confirm/Submit) — Disabled/Enabled Toggle
```css
/* Disabled state: grey, not-allowed cursor */
.btn-confirm-appt {
    background-color: #b0b0b0 !important;
    border-color: #a0a0a0 !important;
    color: #fff !important;
    cursor: not-allowed;
    opacity: 0.7;
}
/* Enabled state: vibrant green */
.btn-confirm-appt.enabled {
    background-color: #28a745 !important;
    border-color: #28a745 !important;
    color: #fff !important;
    cursor: pointer;
    opacity: 1;
}
/* Hover (only when enabled) */
.btn-confirm-appt.enabled:hover {
    background-color: #218838 !important;
    border-color: #1e7e34 !important;
    box-shadow: 0 4px 12px rgba(40, 167, 69, 0.3);
    transform: translateY(-1px);
}
```

### Secondary Action (Back/Cancel) — Outlined Style
```css
.btn-back-action {
    background-color: #fff !important;
    border: 2px solid #337ab7 !important;
    color: #337ab7 !important;
    margin-left: 10px;
}
.btn-back-action:hover {
    background-color: #337ab7 !important;
    color: #fff !important;
}
```

### JavaScript: Toggle `.enabled` Class
```javascript
function updateConfirmBtnState() {
    var canConfirm = /* your condition */;
    $('#confirmBtn').prop('disabled', !canConfirm);
    if (canConfirm) {
        $('#confirmBtn').addClass('enabled');
    } else {
        $('#confirmBtn').removeClass('enabled');
    }
}
```

> **Rule:** NEVER rely solely on Bootstrap's `:disabled` pseudo-class for visual feedback. Always pair `disabled` attribute with a custom CSS class toggle for full control.

---

## Reference Implementation
- **File:** `Views/Complaints/ConfirmAppointment.cshtml`
- **Created:** April 2026
- **Pattern:** Modal-based date/time selector with dark glass cards
- **Used For:** Tenant selects investigation appointment slot from CSO-proposed options
