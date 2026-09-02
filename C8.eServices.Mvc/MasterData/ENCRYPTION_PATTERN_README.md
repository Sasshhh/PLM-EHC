# Encrypted Action Parameter Pattern

## Overview

This project uses **AES-based URL encryption** (`AesCrypto`) to protect route IDs from being visible or tampered with in the browser address bar. This applies to all sensitive GET actions that accept an `id` or similar parameter.

---

## How It Works

### 1. Controller Side — `[DecryptParameter]` Attribute

Decorate any GET action that should receive an encrypted parameter with `[DecryptParameter]`:

```csharp
[DecryptParameter]
[Authorize(Roles = "Client Services Officer")]
public ActionResult Details(int id)
{
    // 'id' is automatically populated from the decrypted 'q' query string
    var record = db.MyTable.Find(id);
    return View(record);
}
```

**How the attribute works:**
- Reads `Request.QueryString["q"]` from the incoming URL
- Decrypts it using `AesCrypto` (passphrase: `"calciummagnesium"`)
- Parses the decrypted string (e.g., `"id=42"`) using `&` as the key-value delimiter
- Injects the values into `filterContext.ActionParameters` before the action executes

**Attribute location:** `C8.eServices.Mvc\Helpers\DecryptParameterAttribute.cs`

---

### 2. View Side — `AesCrypto().Encrypt(...)`

When generating a link to a `[DecryptParameter]` action, always pass `q` as the route value containing the AES-encrypted string.

#### Using `Html.ActionLink`:

```razor
@Html.ActionLink("View", "Details", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + item.Id.ToString()) }, new { @class = "btn btn-primary" })
```

#### Using `Url.Action` (for `<a>` tags):

```razor
<a href="@Url.Action("Details", "Complaints", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + Model.Id) })" class="btn btn-primary">
    View
</a>
```

#### Passing multiple parameters:

```razor
@Html.ActionLink("View", "Details", new { q = new C8.eServices.Mvc.Helpers.AesCrypto().Encrypt("id=" + item.Id + "&categoryId=" + item.CategoryId) }, new { @class = "btn btn-primary" })
```

The delimiter between key-value pairs is `&` (matches how `DecryptParameterAttribute` splits them).

---

### 3. Controller Redirects

When a POST action needs to redirect to a `[DecryptParameter]` GET action, encrypt the `q` parameter in `RedirectToAction`:

```csharp
return RedirectToAction("Details", new { q = new AesCrypto().Encrypt("id=" + record.Id) });
```

> **Important:** Do NOT use `new { id = record.Id }` — this generates a plain `?id=5` URL which `[DecryptParameter]` will reject with "Malicious Activity".

---

## Supported Parameter Types

`DecryptParameterAttribute` automatically parses the following types from the decrypted string:

| Type       | Example decrypted value     |
|------------|-----------------------------|
| `int`      | `"id=42"`                   |
| `int?`     | `"id=42"`                   |
| `long`     | `"id=1234567890"`           |
| `DateTime` | `"date=2024-06-01"`         |
| `decimal`  | `"amount=99.99"`            |
| `double`   | `"score=3.14"`              |
| `float`    | `"rate=1.5"`                |
| `string`   | `"name=John"`               |

---

## Applied Controllers

The following controllers currently use `[DecryptParameter]`:

| Controller           | Action                  | Encrypted param |
|----------------------|-------------------------|-----------------|
| `ComplaintsController` | `Details`             | `id` (TenantComplaint.Id) |
| `ComplaintsController` | `ScheduleAppointment` | `id` (TenantComplaint.Id) |
| `ComplaintsController` | `ConfirmAppointment`  | `id` (TenantComplaint.Id) |
| `ComplaintsController` | `CaptureOutcome`      | `id` (TenantComplaint.Id) |
| `LeaseDetailsController` | `AcceptLeaseRenewal` | `Id`           |
| Various other controllers (PropertyLeaseApplication, HumanSettlement, etc.) | See individual files |

---

## ⚠️ Do NOT Use `[EncryptedActionParameter]`

There is an older `[EncryptedActionParameter]` attribute in `Helpers\EncryptedActionParameterAttribute.cs` that uses **DES** encryption (weak, 56-bit key) with `??` as the delimiter. **Do not use this for new code.** It exists for legacy reasons only.

Use `[DecryptParameter]` + `AesCrypto` for all new actions.

---

## Quick Reference Checklist

When adding a new action that accepts a sensitive `id`:

- [ ] Add `[DecryptParameter]` to the GET action
- [ ] In views, use `new { q = new AesCrypto().Encrypt("id=" + item.Id) }` for all links
- [ ] In POST redirects back to encrypted actions, use `new { q = new AesCrypto().Encrypt("id=" + id) }`
- [ ] Never pass a raw `id` to a `[DecryptParameter]` action
