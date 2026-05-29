# View Spec — Advisors (`AdvisorsPage`)

> **Visual references** — screenshots located at:
> `/figma_advisors`
>
> | File | Shows |
> |------|-------|
> | `advisor_view.png` | Main table — default state |
> | `add_advisor_popup.png` | "Add Advisor" modal — empty state |
> | `edit_advisor_view.png` | "Edit Advisor" modal — pre-populated |
> | `delete_advisor_view.png` | Delete confirmation dialog |

---

## 1. Overall page structure

```
AdvisorsPage
├── Sidebar                         ← expanded navigation (see section 2)
├── TopBar                          ← title + avatar
├── TabBar                          ← horizontally scrollable workshop/brand tabs
├── AddAdvisorButton
└── AdvisorsTable
    ├── TableHeader (labels + filter inputs)
    ├── TableBody
    │   └── AdvisorRow × n
    │       ├── Toggle (Status)
    │       └── ActionButtons (Edit · Schedule · Calendar · Delete)
    └── Pagination
```

---

## 2. Sidebar

> **Reference:** `advisor_view.png` — left column

The sidebar in this view is **expanded** and shows more items than the Users view.

- Background: `#0f2033` (dark navy), width `220px`, height `100vh`
- **Logo** — Inchcape (top-left, white)
- **Navigation items** (each with an icon):

| Icon | Label | State |
|------|-------|-------|
| Grid | Dashboard | Default |
| Calendar | Scheduling | Default |
| Chart | Reports | Default |
| Pencil | **Management** | Expanded (arrow up) |
| — | ↳ Workshops | Sub-item, default |
| — | ↳ **Advisors** | Sub-item, **active** (white bold, highlighted) |
| — | ↳ Customer database | Sub-item, default |
| — | ↳ Gestión de Roles y Permisos | Sub-item, default |

- **Logout** button at the bottom, with exit icon (`→|`), white

---

## 3. TopBar

> **Reference:** `advisor_view.png` — top bar

| Element | Detail |
|---------|--------|
| Left | Pencil icon + title **"Management - Advisors"** in `#00b4d8` (cyan), `font-size: var(--font-xl)`, bold |
| Right | "User name" text (grey) + circular avatar **"NU"**, background `#00b4d8`, white text |
| Background | White `#ffffff`, `box-shadow: var(--shadow-sm)` |

---

## 4. Tab bar

> **Reference:** `advisor_view.png` — below the TopBar, above the table card

A horizontally scrollable row of tabs used to filter the advisor list (e.g. by workshop or brand).

- Left arrow `<` and right arrow `>` for overflow scrolling, color `#00b4d8`
- Each tab: label **"TAB"** (replaced with real workshop/brand names at runtime)
- **Active tab** style: text `#00b4d8` (cyan), bold, bottom border `2px solid #00b4d8`
- **Inactive tab** style: text `#6b7280` (grey), no underline
- Up to 2 tabs can be active simultaneously (multi-selection)
- `background: #ffffff`, `border-bottom: 1px solid #e5e7eb`, `padding: 0 32px`

---

## 5. `+ Add advisor` button

> **Reference:** `advisor_view.png` — top-left of the content card

- Label: **`+ Add advisor`**
- Background: `#00b4d8` · Text: white, bold
- `border-radius: var(--radius-md)` · `padding: 8px 20px`
- On click → opens **Add Advisor modal** (section 7)

---

## 6. Advisors table

> **Reference:** `advisor_view.png`

White card, `border-radius: var(--radius-lg)`, `box-shadow: var(--shadow-sm)`.  
No vertical borders. `border-bottom: 1px solid #f0f2f5` between rows.

### 6.1 Columns

| # | Header label | Data type | Has filter | Notes |
|---|-------------|-----------|------------|-------|
| 1 | **Name** | `string` | Yes | Advisor full name, center-aligned |
| 2 | **Status** | `boolean` | No | Toggle + "Active" / "Inactive" label |
| 3 | **Workshop** | `string` | Yes | Can be multi-line (e.g. "Motor Hino / MotorK DFSK") |
| 4 | **Brand** | `string` | Yes | Single or multiple brands (e.g. "HINO", "Subaru") |
| 5 | **Type of service** | `string[]` | Yes | Comma-separated active services (e.g. "Mechanical repairs, Recalls") |
| 6 | **Actions** | — | No | 4 icon buttons — see 6.4 |

### 6.2 Column filter inputs

- Rendered in a row **below** the header labels
- Columns with filter: **Name**, **Workshop**, **Brand**, **Type of service**
- Columns without filter: **Status**, **Actions**
- `<input type="text" placeholder="Filter" />`
- Style: `background: #e8e8e8`, `border: none`, `border-radius: var(--radius-sm)`, `padding: 4px 8px`, `font-size: var(--font-sm)`
- Real-time, case-insensitive

### 6.3 Status toggle

| State | Track color | Knob | Label |
|-------|-------------|------|-------|
| Active (`true`) | `#00b4d8` | Right | "Active" |
| Inactive (`false`) | `#cccccc` | Left | "Inactive" |

- Clicking opens a **status confirmation dialog** (same pattern as Users view)

### 6.4 Action buttons — 4 per row

> **Reference:** `advisor_view.png` — Acciones column

Each row has **four** square icon buttons in this exact order:

| # | Background | Icon | Action |
|---|-----------|------|--------|
| 1 | `#f5c800` (yellow) | Pencil | Opens **Edit Advisor modal** (section 8) |
| 2 | `#00b4d8` (cyan) | Clock | Opens advisor's **schedule / availability view** |
| 3 | `#00b4d8` (cyan) | Calendar / bookmark | Opens advisor's **bookings / appointments view** |
| 4 | `#9b3fa0` (purple) | Trash | Opens **Delete confirmation dialog** (section 9) |

All buttons: `border-radius: var(--radius-md)`, `border: none`, `padding: 8px`, `width: 36px`, `height: 36px`.

### 6.5 Pagination

Centered below the table body.

| Element | Detail |
|---------|--------|
| Rows per page | Dropdown: `5 · 10 · 25 · 50`, default `10` |
| Range text | Dynamic — e.g. `"1-5 of 13"` |
| Navigation | `⏮` First · `◄` Prev · `►` Next · `⏭` Last |
| Active color | `#00b4d8` |
| Disabled color | `#cccccc` |

---

## 7. Add Advisor modal

> **Reference:** `add_advisor_popup.png`

Centered on screen, dark overlay `rgba(0,0,0,0.45)`.  
White card, `border-radius: var(--radius-lg)`, `padding: 40px 36px`, `max-width: 540px`.

### 7.1 Modal header

- Title: **"Add advisor"** · `color: #00b4d8` · `font-size: var(--font-xl)` · bold · centered

### 7.2 Form fields

**Floating label / underline style** (same as the rest of the design system):
- Default: grey label, underline-only border
- Focused / filled: label and underline turn `#00b4d8`
- Error: label and underline turn `#e53e3e`, error text below field

Layout: **2-column grid**, `gap: 24px`.

| Field | Type | Required | Col span | Notes |
|-------|------|----------|----------|-------|
| **Advisor name** | `<input text>` | Yes | 1 | Full name |
| **Phone** | `<input tel>` | Yes | 1 | Numeric, e.g. "562302145" |
| **Workshop** | `<select>` | Yes | 1 | Single selection dropdown |
| **Select brand(s)** | `<select multiple>` | Yes | 1 | Multi-select dropdown; shows selected values comma-separated |

### 7.3 "Activate workshop service" section

Below the form fields, full-width:

- Section label: **"Activate workshop service"** · `color: #00b4d8` · bold · `font-size: var(--font-base)`
- **5 independent toggle switches** arranged in a **2-column grid**:

| Toggle label | Default state (Add) | Column |
|---|---|---|
| Mechanical repairs | ON (cyan) | Left |
| Preventive maintenance | ON (cyan) | Left |
| Dent removal and painting | ON (cyan) | Left |
| Express | OFF (grey) | Right |
| Recalls | OFF (grey) | Right |

Each toggle is a `<Toggle />` atom with its label to the right.  
Each toggle is fully independent — toggling one does not affect the others.  
The active services selected here become the **Type of service** values shown in the table row.

### 7.4 Modal action buttons

| Button | Style | Action |
|--------|-------|--------|
| **Cancel** | White bg · `border: 1.5px solid #00b4d8` · cyan text | Closes modal, discards data |
| **Accept** | `background: #00b4d8` · white text | Validates → calls `POST /advisors` → closes modal + success banner |

---

## 8. Edit Advisor modal

> **Reference:** `edit_advisor_view.png`

Same structure and styles as the Add modal with these differences:

### 8.1 Modal header

- Title: **"Edit advisor"** · same cyan style

### 8.2 Form fields — pre-populated

All fields are pre-filled with the selected advisor's current data:

| Field | Example value | Notes |
|-------|---------------|-------|
| Advisor name * | "Javier Torres" | Editable |
| Phone * | "562302145" | Editable |
| Workshop * | "ST Power" | Dropdown pre-selected |
| Select brand(s) * | "Subaru, BMW" | Multi-select, multiple items pre-selected; dropdown shown open in reference |

When the Brand dropdown is open it shows the full list with already-selected items visually marked (checkmark or highlight).

### 8.3 Service toggles — pre-populated

Toggles reflect the advisor's current service configuration:

| Toggle | State in reference |
|--------|--------------------|
| Mechanical repairs | ON |
| Preventive maintenance | ON |
| Dent removal and painting | ON |
| Express | OFF |
| Recalls | OFF |

### 8.4 Action buttons

Same as Add modal. **Accept** calls `PUT /advisors/:id`.

---

## 9. Delete confirmation dialog

> **Reference:** `delete_advisor_view.png`

Small centered modal, `max-width: 460px`, `padding: 40px 32px`, `border-radius: var(--radius-lg)`.

| Element | Detail |
|---------|--------|
| Title | **"Delete advisor"** · `#00b4d8` · bold · centered |
| Body text | `"Are you sure you want to delete the consultant's information **[Full Name]** for good?"` · name in bold · centered |
| **Cancel** | White bg · cyan border + cyan text |
| **Accept** | `background: #00b4d8` · white text |

- **Cancel** → closes dialog, no action
- **Accept** → calls `DELETE /advisors/:id` → removes row + shows success banner

---

## 10. Advisor data model

```ts
// src/types/advisor.types.ts

export type ServiceType =
  | "Mechanical repairs"
  | "Preventive maintenance"
  | "Dent removal and painting"
  | "Express"
  | "Recalls";

export interface Advisor {
  id:             number;
  name:           string;           // full name
  phone:          string;
  workshopId:     number;
  workshopName:   string;           // denormalized for table display
  brands:         string[];         // e.g. ["Subaru", "BMW"]
  services:       ServiceType[];    // active service types
  status:         boolean;          // true = Active
}

export type CreateAdvisorPayload = Omit<Advisor, "id" | "workshopName">;
export type UpdateAdvisorPayload = Partial<CreateAdvisorPayload>;
```

---

## 11. Mock data

```ts
// src/mocks/advisors.mock.ts
import type { Advisor } from "@/types/advisor.types";

export const initialAdvisors: Advisor[] = [
  { id: 1,  name: "Javier Torres",   phone: "562302145", workshopId: 1, workshopName: "Industrias Mussgo Bogotá",   brands: ["HINO"],           services: ["Mechanical repairs"],                              status: true  },
  { id: 2,  name: "Camilo Torres",   phone: "312000001", workshopId: 2, workshopName: "Proautos Barranquilla",      brands: ["Subaru"],          services: ["Mechanical repairs", "Preventive maintenance"],    status: true  },
  { id: 3,  name: "Iván Gracia",     phone: "312000002", workshopId: 3, workshopName: "STK Power",                 brands: ["BMW"],             services: ["Recalls"],                                        status: false },
  { id: 4,  name: "Daniel Diaz",     phone: "312000003", workshopId: 4, workshopName: "Rasautos Manizales",        brands: ["BMW"],             services: ["Dent removal and painting"],                       status: true  },
  { id: 5,  name: "Jose Pérez",      phone: "312000004", workshopId: 5, workshopName: "Carrazos",                  brands: ["DFSK"],            services: ["Express", "Recalls"],                              status: false },
  { id: 6,  name: "Camilo Sánchez",  phone: "312000005", workshopId: 6, workshopName: "Seikou Bogotá",             brands: ["BMW"],             services: ["Mechanical repairs", "Recalls"],                   status: false },
  { id: 7,  name: "Manuel Ortíz",    phone: "312000006", workshopId: 3, workshopName: "STK Power",                 brands: ["Subaru"],          services: ["Mechanical repairs", "Recalls"],                   status: true  },
  { id: 8,  name: "Andres Álvarez",  phone: "312000007", workshopId: 7, workshopName: "Uno A Automotriz",          brands: ["HINO"],            services: ["Express", "Recalls"],                              status: true  },
  { id: 9,  name: "Álvaro Gómez",    phone: "312000008", workshopId: 8, workshopName: "Motor Hino / MotorK DFSK",  brands: ["HINO"],            services: ["Dent removal and painting"],                       status: false },
  { id: 10, name: "Nicolas Rodríguez",phone:"312000009", workshopId: 3, workshopName: "STK Power",                 brands: ["Subaru"],          services: ["Mechanical repairs", "Preventive maintenance"],    status: true  },
];
```

---

## 12. API calls (`src/services/advisors.service.ts`)

| User action | HTTP call |
|-------------|-----------|
| Page loads | `GET /advisors` |
| Click Accept (Add modal) | `POST /advisors` |
| Click Accept (Edit modal) | `PUT /advisors/:id` |
| Click Accept (Status dialog) | `PATCH /advisors/:id/status` |
| Click Accept (Delete dialog) | `DELETE /advisors/:id` |

---

## 13. Loading & feedback states

| Trigger | UI behaviour |
|---------|-------------|
| Page load | Spinner centered in the table body |
| Create / Edit / Delete in progress | Accept button shows spinner and is disabled |
| Successful create | Cyan success banner: `"Advisor [Name] has been successfully added."` — auto-dismisses after 3 s |
| Successful edit | Cyan success banner: `"Advisor [Name] has been successfully updated."` |
| Successful delete | Cyan success banner: `"Advisor [Name] has been successfully deleted."` |
| API error | Red error banner with the API message or generic fallback |

---

## 14. Component mapping (from `project-structure.md`)

| UI element | Component file |
|-----------|----------------|
| Page wrapper | `src/pages/AdvisorsPage/AdvisorsPage.tsx` |
| Sidebar | `src/components/organisms/Sidebar/Sidebar.tsx` |
| TopBar | `src/components/organisms/TopBar/TopBar.tsx` |
| Tab bar | `src/components/molecules/TabBar/TabBar.tsx` *(new)* |
| `+ Add advisor` button | `src/components/atoms/Button/Button.tsx` |
| Full table | `src/components/organisms/AdvisorsTable/AdvisorsTable.tsx` *(new)* |
| Single table row | `src/components/organisms/AdvisorsTable/AdvisorRow.tsx` *(new)* |
| Filter input | `src/components/molecules/ColumnFilter/ColumnFilter.tsx` |
| Status toggle | `src/components/atoms/Toggle/Toggle.tsx` |
| 4-button action group | `src/components/molecules/AdvisorActionButtons/AdvisorActionButtons.tsx` *(new)* |
| Pagination | `src/components/organisms/Pagination/Pagination.tsx` |
| Add / Edit modal | `src/components/organisms/AdvisorModal/AdvisorModal.tsx` *(new)* |
| Service toggles section | `src/components/molecules/ServiceToggles/ServiceToggles.tsx` *(new)* |
| Multi-select brand field | `src/components/molecules/MultiSelect/MultiSelect.tsx` *(new)* |
| Delete dialog | `src/components/organisms/DeleteConfirmDialog/DeleteConfirmDialog.tsx` |
| Success / Error banner | `src/components/molecules/SuccessBanner` · `ErrorBanner` |
| Spinner | `src/components/atoms/Spinner/Spinner.tsx` |

---

## 15. New additions to `project-structure.md` folder tree

```
src/
├── components/
│   ├── molecules/
│   │   ├── TabBar/
│   │   │   ├── TabBar.tsx              ← scrollable tab row with < > arrows
│   │   │   └── TabBar.module.css
│   │   ├── AdvisorActionButtons/
│   │   │   ├── AdvisorActionButtons.tsx  ← Edit · Schedule · Calendar · Delete
│   │   │   └── AdvisorActionButtons.module.css
│   │   ├── ServiceToggles/
│   │   │   ├── ServiceToggles.tsx      ← 5 independent Toggle atoms in 2-col grid
│   │   │   └── ServiceToggles.module.css
│   │   └── MultiSelect/
│   │       ├── MultiSelect.tsx         ← multi-selection dropdown for brands
│   │       └── MultiSelect.module.css
│   └── organisms/
│       └── AdvisorsTable/
│           ├── AdvisorsTable.tsx
│           ├── AdvisorsTable.module.css
│           ├── AdvisorRow.tsx
│           └── AdvisorRow.module.css
│       └── AdvisorModal/
│           ├── AdvisorModal.tsx        ← shared modal for Add and Edit
│           └── AdvisorModal.module.css
├── pages/
│   └── AdvisorsPage/
│       ├── AdvisorsPage.tsx
│       └── AdvisorsPage.module.css
├── types/
│   └── advisor.types.ts
├── services/
│   └── advisors.service.ts
└── mocks/
    └── advisors.mock.ts
```
