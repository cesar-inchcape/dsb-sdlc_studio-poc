# View Spec — User Management (`UsersPage`)

> **Visual references** — all screenshots are located at:
> `/figma_users`
>
> | File | Shows |
> |------|-------|
> | `Users_view.png` | Main table — default state |
> | `add_user.png` | "Add User" modal — empty state |
> | `add_user_with example_data.png` | "Add User" modal — filled + role dropdown open |
> | `Edit_users.png` | "Edit User" modal — pre-populated |
> | `Delete_User.png` | Delete confirmation dialog |
> | `Change_ user_status.png` | Status change confirmation dialog |

---

## 1. Overall page structure

> **Reference:** `Users_view.png`

The page is rendered inside `MainLayout` (sidebar + content area). The content area contains, top to bottom:

1. **TopBar** — page icon + title + user avatar
2. **`+ New User` button** — triggers the Add modal
3. **UsersTable** — filters + rows + pagination

```
UsersPage
├── TopBar
├── NewUserButton
└── UsersTable
    ├── TableHeader (column labels + filter inputs)
    ├── TableBody
    │   └── UserRow × n (Toggle, text cells, ActionButtons)
    └── Pagination
```

---

## 2. TopBar

> **Reference:** `Users_view.png` — top bar

| Element | Detail |
|---------|--------|
| Left | Person icon (`👤`) + title **"Gestión de Usuarios"** in `#00b4d8` (cyan), `font-size: var(--font-xl)`, bold |
| Right | Text "Nombre de usuario" (grey) + circular avatar with initials **"NU"**, background `#00b4d8`, white text |
| Background | White (`#ffffff`), subtle bottom shadow `var(--shadow-sm)` |

---

## 3. `+ New User` button

> **Reference:** `Users_view.png` — top-left of the content card

- Label: **`+ Nuevo Usuario`**
- Background: `#00b4d8` · Text: white, bold
- `border-radius: var(--radius-md)` · `padding: 8px 20px`
- On click → opens **Add User modal** (section 5)

---

## 4. Users table

> **Reference:** `Users_view.png`

White card (`background: #ffffff`, `border-radius: var(--radius-lg)`, `box-shadow: var(--shadow-sm)`).  
No vertical borders. Horizontal divider between rows (`border-bottom: 1px solid #f0f2f5`).

### 4.1 Columns

| # | Header label | Data type | Has filter input | Notes |
|---|-------------|-----------|-----------------|-------|
| 1 | **Email** | `string` | Yes | Plain text, left-aligned |
| 2 | **Estado** | `boolean` | Yes | Toggle switch + "Activo" / "Inactivo" label |
| 3 | **Nombre usuario** | `string` | Yes | Full name, center-aligned |
| 4 | **País** | `string` | Yes | Center-aligned |
| 5 | **Rol** | `string` | Yes | Center-aligned |
| 6 | **Acciones** | — | No | Edit + Delete buttons, right-aligned |

### 4.2 Column filter inputs

- Rendered in a dedicated row **below** the header labels
- `<input type="text" placeholder="Filter" />`
- Styles: `background: #e8e8e8`, `border: none`, `border-radius: var(--radius-sm)`, `padding: 4px 8px`, `font-size: var(--font-sm)`
- Filter is **real-time** and **case-insensitive**
- Columns **Acciones** has no filter input

### 4.3 Status toggle (Estado column)

> **Reference:** `Users_view.png` — Estado column

Custom `<Toggle />` atom with a text label to its right:

| State | Track color | Knob position | Label text |
|-------|-------------|---------------|------------|
| Active (`true`) | `#00b4d8` (cyan) | Right | "Activo" |
| Inactive (`false`) | `#cccccc` (grey) | Left | "Inactivo" |

Clicking the toggle does **not** change the status immediately.  
It opens the **Status confirmation dialog** (section 8).

### 4.4 Action buttons (Acciones column)

Two square icon buttons per row, side by side:

| Button | Background | Icon | On click |
|--------|-----------|------|----------|
| Edit | `#f5c800` (yellow) | Pencil (white) | Opens Edit User modal (section 6) |
| Delete | `#9b3fa0` (purple) | Trash (white) | Opens Delete confirmation dialog (section 7) |

Both: `border-radius: var(--radius-md)`, `border: none`, `padding: 8px`, `width: 36px`, `height: 36px`.

### 4.5 Pagination

> **Reference:** `Users_view.png` — bottom of the card

Centered below the table body.

| Element | Detail |
|---------|--------|
| Rows per page | Dropdown: options `5 · 10 · 25 · 50`, default `10` |
| Range text | Dynamic — e.g. `"1-10 of 13"` |
| Navigation buttons | `⏮` First · `◄` Prev · `►` Next · `⏭` Last |
| Active / hover color | `#00b4d8` |
| Disabled color | `#cccccc` |

---

## 5. Add User modal

> **References:** `add_user.png` (empty state) · `add_user_with example_data.png` (filled state)

Opens centered on screen with a dark semi-transparent overlay (`rgba(0,0,0,0.45)`).  
White card, `border-radius: var(--radius-lg)`, `padding: 32px`, max-width `520px`.

### 5.1 Modal header

- Title: **"Agregar Usuario"** · color `#00b4d8` · `font-size: var(--font-xl)` · bold · centered
- Below title (read-only context info, cyan label style):
  - `País: Colombia` — inherits from the logged-in user's country context; not editable

### 5.2 Form fields

Fields use a **floating label / underline style**:
- Default: label is gray, sits above an underline-only border
- Focused / filled: label turns `#00b4d8` (cyan) and scales up slightly
- Error state: underline turns `#e53e3e` (red), error message below in red

Layout is a **2-column grid** (`gap: 24px`). Single-column fields span both columns.

| Field | Type | Required | Column span | Notes |
|-------|------|----------|-------------|-------|
| **Nombre** (First name) | `<input text>` | Yes | 1 | — |
| **Apellidos** (Last name) | `<input text>` | Yes | 1 | — |
| **Correo institucional** (Email) | `<input email>` | Yes | 1 | Validated as email format |
| **Idioma** (Language) | `<select>` | Yes | 1 | Dropdown; e.g. "Español" |
| **Rol** (Role) | `<select>` | Yes | 1 | Dropdown; triggers conditional fields (see 5.3) |
| **Taller** (Workshop) | `<select>` | Yes (conditional) | 1 | Appears only when role requires a workshop assignment |
| **Asesor** (Advisor) | `<input text>` | Yes (conditional) | 2 (full row) | Appears only when role requires an advisor |

### 5.3 Conditional fields by role

> **Reference:** `add_user_with example_data.png` — dropdown open shows role options

The role dropdown options visible in the reference are:
- Workshop Admin
- Workshop Assistant *(highlighted — selected)*
- Service Advisor

When a role that implies a physical workshop is selected (e.g. **Workshop Admin**, **Workshop Assistant**), the following extra fields appear below Rol:

- **Taller \*** — `<select>` dropdown for workshop name
- **Asesor \*** — `<input text>` for the advisor's name

These fields must be **hidden** when a role with no workshop context is selected (e.g. Service Advisor with no workshop dependency).

### 5.4 Status section

Below the form fields, full-width:

- Section label: **"Seleccione el estado del usuario"** · color `#00b4d8` · bold
- `<Toggle />` atom + label text "Activo" / "Inactivo"
- Default on open: **Inactive** (toggle off, grey)

### 5.5 Modal action buttons

Full-width row at the bottom, two buttons side by side:

| Button | Style | Action |
|--------|-------|--------|
| **Cancelar** | White bg · `border: 1.5px solid #00b4d8` · cyan text · `border-radius: var(--radius-md)` | Closes modal, discards data |
| **Aceptar** | `background: #00b4d8` · white text · `border-radius: var(--radius-md)` | Validates → calls `POST /users` → closes modal + shows success banner |

---

## 6. Edit User modal

> **Reference:** `Edit_users.png`

Same structure and styles as the Add modal with the following differences:

### 6.1 Modal header

- Title: **"Editar Usuario"** · same cyan style
- Read-only context info block (cyan label, plain value):
  - `Correo institucional: javier.torres@inchcape.com` — email is **not editable** in this view
  - `País: Colombia` — country is **not editable**

### 6.2 Form fields

The email and country fields are removed from the form grid (shown as read-only info above).  
Remaining editable fields:

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| **Nombre** | `<input text>` | Yes | Pre-populated |
| **Apellidos** | `<input text>` | Yes | Pre-populated |
| **Idioma** | `<select>` | Yes | Pre-populated |
| **Rol** | `<select>` | Yes | Pre-populated; changing it may show/hide conditional fields |
| **Workshop name** | `<select>` | Conditional | Same conditional logic as Add modal |

### 6.3 Status section and buttons

Identical to the Add modal. Toggle reflects the user's current status on open.  
**Aceptar** calls `PUT /users/:id`.

---

## 7. Delete confirmation dialog

> **Reference:** `Delete_User.png`

Small centered modal, white card, `max-width: 420px`, `border-radius: var(--radius-lg)`, `padding: 40px 32px`.

| Element | Detail |
|---------|--------|
| Title | **"Eliminar usuario"** · `#00b4d8` · bold · centered |
| Body text | "¿Está seguro de eliminar el usuario **[Full Name]** definitivamente?" · name in bold · centered |
| **Cancelar** | White bg · cyan border + cyan text |
| **Aceptar** | `background: #00b4d8` · white text |

- **Cancelar** → closes dialog, no action
- **Aceptar** → calls `DELETE /users/:id` → removes row from table

---

## 8. Status change confirmation dialog

> **Reference:** `Change_ user_status.png`

Same dimensions and style as the Delete dialog.

| Element | Detail |
|---------|--------|
| Title | **"Estado de usuario"** · `#00b4d8` · bold · centered |
| Body text | "¿Está seguro de Activar/Inactivar al usuario **[Full Name]**?" · name in bold · centered |
| **Cancelar** | White bg · cyan border + cyan text |
| **Aceptar** | `background: #00b4d8` · white text |

- Triggered when the user clicks a **status toggle** in the table row
- **Cancelar** → closes dialog, toggle reverts visually
- **Aceptar** → calls `PATCH /users/:id/status` → toggle updates in table

---

## 9. Form validation rules

| Field | Rule |
|-------|------|
| Nombre / Apellidos | Required, non-empty string |
| Correo institucional | Required, valid email format (`@inchcape.com` recommended) |
| Idioma | Required, must select an option |
| Rol | Required, must select an option |
| Taller | Required when visible (role-dependent) |
| Asesor | Required when visible (role-dependent) |

On submit with empty required fields:
- Underline border turns `#e53e3e`
- Label turns `#e53e3e`
- Small error text appears below the field: `"This field is required"`
- **Aceptar** button remains active but submission is blocked until all errors are resolved

---

## 10. Loading & feedback states

| Trigger | UI behaviour |
|---------|-------------|
| Page load / table fetch | Spinner (`<Spinner />`) centered in the table body area |
| Create / Edit / Delete in progress | **Aceptar** button shows spinner and is disabled |
| Successful create | Cyan success banner below TopBar: `"The user [Name] has been successfully created."` — auto-dismisses after 3 s |
| Successful edit | Cyan success banner: `"The user [Name] has been successfully updated."` |
| Successful delete | Cyan success banner: `"The user [Name] has been successfully deleted."` |
| API error | Red error banner below TopBar with the message from the API response |

---

## 11. Component mapping (from `project-structure.md`)

| UI element | Component file |
|-----------|----------------|
| Page wrapper | `src/pages/UsersPage/UsersPage.tsx` |
| Top bar | `src/components/organisms/TopBar/TopBar.tsx` |
| `+ New User` button | `src/components/atoms/Button/Button.tsx` |
| Table (full) | `src/components/organisms/UsersTable/UsersTable.tsx` |
| Single table row | `src/components/organisms/UsersTable/UserRow.tsx` |
| Filter input | `src/components/molecules/ColumnFilter/ColumnFilter.tsx` |
| Status toggle | `src/components/atoms/Toggle/Toggle.tsx` |
| Edit + Delete buttons | `src/components/molecules/ActionButtons/ActionButtons.tsx` |
| Pagination bar | `src/components/organisms/Pagination/Pagination.tsx` |
| Add / Edit modal | `src/components/organisms/UserModal/UserModal.tsx` |
| Delete dialog | `src/components/organisms/DeleteConfirmDialog/DeleteConfirmDialog.tsx` |
| Status dialog | `src/components/organisms/ChangeStatusDialog/ChangeStatusDialog.tsx` |
| Success banner | `src/components/molecules/SuccessBanner/SuccessBanner.tsx` |
| Error banner | `src/components/molecules/ErrorBanner/ErrorBanner.tsx` |
| Spinner | `src/components/atoms/Spinner/Spinner.tsx` |
| Form text input | `src/components/atoms/Input/Input.tsx` |
| Form select | `src/components/molecules/FormSelect/FormSelect.tsx` |
| Form field wrapper | `src/components/molecules/FormField/FormField.tsx` |

---

## 12. API calls (from `src/services/users.service.ts`)

| User action | HTTP call |
|-------------|-----------|
| Page loads | `GET /users` |
| Click Aceptar (Add modal) | `POST /users` |
| Click Aceptar (Edit modal) | `PUT /users/:id` |
| Click Aceptar (Status dialog) | `PATCH /users/:id/status` |
| Click Aceptar (Delete dialog) | `DELETE /users/:id` |
