# PLM-EHC: Property Lease Management System

This repository contains the Property Lease Management (PLM) system. 

## 📌 Architectural Isolation Rules

To prevent conflict between subsystems in the shared repository:
1. **Table Prefix:** Only use tables starting with the `RE_` prefix (e.g. `RE_Applications`, `RE_Facilities`, `RE_FacilityCategories`, `RE_FacilityUnits`).
2. **C# Code Casing:** Controllers, ViewModels, and models must start with `RE_` or be inside the `RealEstate` folder structure.
3. **Configuration Namespace:** Real Estate application settings and keys are prefixed with `re_` or `red_`.
4. **Isolated Views:** Contain views strictly in `Views/RealEstate` and `Views/RealEstateAdmin`.
5. **Visual Styling / Theme Isolation:** The system's layout files (like `Views/Shared/RCS_Layout.cshtml`) and global sidebars / navigation headers are shared with EHC and HSD and **MUST NOT** be modified. All custom brand stylings (specifically the Onyx Obsidian theme) must be declared and scoped locally via `<style>` blocks or inline styles inside the individual Real Estate views (`Views/RealEstate/*` and `Views/RealEstateAdmin/*`).

---

## 🎨 Premium Color Scheme & Visual Design Tokens

Ensure the Real Estate branch uses the following gold/amber and dark slate color token palette for all UI components, buttons, and alert states (locally scoped inside our views only):

| UI Component | Color Role | CSS Custom Property | Hex Code | HSL Representation | CSS Usage Guideline |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Brand Accent** | Dominant Theme | `--re-gold` | `#c59b27` | `hsl(44, 67%, 46%)` | Main highlights, active tab borders, focus states |
| **Brand Hover** | Interaction Accent | `--re-gold-hover` | `#a37f1c` | `hsl(44, 71%, 38%)` | Hover states for primary gold buttons |
| **Theme Light** | Background Tint | `--re-gold-light` | `#fafaf9` | `hsl(60, 9%, 98%)` | Soft card background fills, selected tab fill |
| **Heading/Title** | Typography Primary | `--re-text-dark` | `#1c1917` | `hsl(24, 10%, 10%)` | Primary page headings, strong text (`<h3>`, `<h4>`) |
| **Primary Button** | Success / Submit Action | `--re-btn-success` | `#c59b27` | `hsl(44, 67%, 46%)` | Positive click actions ("Submit", "Save", "Add") |
| **Secondary Button** | Neutral / Cancel Action | `--re-btn-cancel` | `#fafaf9` | `hsl(60, 9%, 98%)` | Back-out actions, Close, Reset (Border: `#cbd5e1`) |
| **Cancel Hover** | Neutral Hover | `--re-btn-cancel-hover` | `#f5f5f4` | `hsl(60, 5%, 96%)` | Hover state for Cancel / Back actions |
| **Destructive Button**| Delete / Remove Action | `--re-btn-delete` | `#fff1f2` | `hsl(350, 100%, 97%)` | Soft red/rose button bg with `#e11d48` text |
| **Destructive Hover** | Delete Hover | `--re-btn-delete-hover` | `#ffe4e6` | `hsl(350, 100%, 95%)` | Hover state for Delete / Remove actions |
| **Primary Banner** | Header Cards bg | `--re-banner-grad` | `linear-gradient` | `135deg, #2e2a27 to #1c1917` | Premium headers and dashboard panels |

### Premium Button CSS Guidelines:
* **Primary / Positive Button:**
  ```css
  .btn-submit-gold {
      background-color: var(--re-gold) !important;
      border-color: var(--re-gold) !important;
      color: #ffffff !important;
      border-radius: 8px !important;
      padding: 10px 24px !important;
      font-weight: 600 !important;
  }
  ```
* **Secondary / Cancel Button:**
  ```css
  .btn-cancel-warm {
      background-color: var(--re-btn-cancel) !important;
      border-color: #cbd5e1 !important;
      color: #475569 !important;
      border-radius: 8px !important;
      padding: 10px 24px !important;
      font-weight: 600 !important;
  }
  ```
* **Destructive / Delete Button:**
  ```css
  .btn-delete-rose {
      background-color: var(--re-btn-delete) !important;
      border-color: #fecdd3 !important;
      color: #e11d48 !important;
      border-radius: 6px !important;
      padding: 6px 12px !important;
      font-weight: 600 !important;
  }
  ```


For detailed session history, progress status, and completed features, please refer to:
* [where last stopped.md](file:///c:/REPO/PLM%20V1/where%20last%20stopped.md)
