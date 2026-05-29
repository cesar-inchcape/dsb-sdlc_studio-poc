# Frontend Project Structure — User Management (React + TypeScript + Atomic Design)

## Project initialization

```bash
npm create vite@latest 3pd_booking_fe -- --template react-ts
cd 3pd_booking_fe
npm install
```

---

## Dependencies

### Production

```json
{
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "axios": "^1.7.2"
  }
}
```

### Development

```json
{
  "devDependencies": {
    "@types/react": "^18.3.1",
    "@types/react-dom": "^18.3.1",
    "@vitejs/plugin-react": "^4.3.1",
    "typescript": "^5.5.3",
    "vite": "^5.3.4",
    "eslint": "^9.7.0",
    "@eslint/js": "^9.7.0",
    "eslint-plugin-react-hooks": "^5.1.0",
    "eslint-plugin-react-refresh": "^0.4.9",
    "globals": "^15.8.0",
    "typescript-eslint": "^8.0.0"
  }
}
```

---

## Configuration files

### `vite.config.ts`

```ts
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
});
```

### `tsconfig.json`

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["src/*"]
    }
  },
  "include": ["src"]
}
```

### `.env.example`

```env
VITE_API_BASE_URL=http://localhost:3000
```

---

## Folder structure (Atomic Design)

```
user-management/
├── public/
│   └── favicon.ico
│
├── src/
│   │
│   ├── assets/                         # Static assets
│   │   └── logo.svg
│   │
│   ├── styles/                         # Global styles and design tokens
│   │   ├── tokens.css                  # CSS custom properties (colors, spacing, etc.)
│   │   ├── reset.css                   # CSS reset / normalize
│   │   └── global.css                  # Body, typography, base rules
│   │
│   ├── types/                          # Shared TypeScript interfaces and types
│   │   ├── user.types.ts
│   │   └── api.types.ts
│   │
│   ├── services/                       # HTTP layer — all API calls live here
│   │   ├── api.ts                      # Axios instance with base URL and interceptors
│   │   └── users.service.ts            # CRUD functions for the /users endpoint
│   │
│   ├── hooks/                          # Custom reusable hooks
│   │   ├── useUsers.ts                 # Fetches and manages the users list
│   │   └── usePagination.ts            # Pagination logic (page, rowsPerPage, slice)
│   │
│   ├── components/                     # Atomic Design hierarchy
│   │   │
│   │   ├── atoms/                      # Smallest indivisible UI elements
│   │   │   ├── Button/
│   │   │   │   ├── Button.tsx
│   │   │   │   └── Button.module.css
│   │   │   ├── Input/
│   │   │   │   ├── Input.tsx
│   │   │   │   └── Input.module.css
│   │   │   ├── Toggle/
│   │   │   │   ├── Toggle.tsx          # Custom toggle switch (no external lib)
│   │   │   │   └── Toggle.module.css
│   │   │   ├── Badge/
│   │   │   │   ├── Badge.tsx           # "Active" / "Inactive" status label
│   │   │   │   └── Badge.module.css
│   │   │   ├── Avatar/
│   │   │   │   ├── Avatar.tsx          # Circular avatar with initials
│   │   │   │   └── Avatar.module.css
│   │   │   ├── Icon/
│   │   │   │   ├── Icon.tsx            # Wrapper for SVG icons
│   │   │   │   └── Icon.module.css
│   │   │   └── Spinner/
│   │   │       ├── Spinner.tsx         # Loading indicator
│   │   │       └── Spinner.module.css
│   │   │
│   │   ├── molecules/                  # Combinations of atoms with a single responsibility
│   │   │   ├── FormField/
│   │   │   │   ├── FormField.tsx       # Label + Input + error message
│   │   │   │   └── FormField.module.css
│   │   │   ├── FormSelect/
│   │   │   │   ├── FormSelect.tsx      # Label + Select + error message
│   │   │   │   └── FormSelect.module.css
│   │   │   ├── ColumnFilter/
│   │   │   │   ├── ColumnFilter.tsx    # Column header + filter input
│   │   │   │   └── ColumnFilter.module.css
│   │   │   ├── ActionButtons/
│   │   │   │   ├── ActionButtons.tsx   # Edit (yellow) + Delete (purple) buttons per row
│   │   │   │   └── ActionButtons.module.css
│   │   │   ├── NavItem/
│   │   │   │   ├── NavItem.tsx         # Sidebar nav item with optional sub-items
│   │   │   │   └── NavItem.module.css
│   │   │   ├── SuccessBanner/
│   │   │   │   ├── SuccessBanner.tsx   # Cyan success toast banner
│   │   │   │   └── SuccessBanner.module.css
│   │   │   └── ErrorBanner/
│   │   │       ├── ErrorBanner.tsx     # Red error toast banner
│   │   │       └── ErrorBanner.module.css
│   │   │
│   │   ├── organisms/                  # Complex sections composed of molecules and atoms
│   │   │   ├── Sidebar/
│   │   │   │   ├── Sidebar.tsx         # Logo + NavMenu + LogoutButton
│   │   │   │   └── Sidebar.module.css
│   │   │   ├── TopBar/
│   │   │   │   ├── TopBar.tsx          # Page title + user avatar
│   │   │   │   └── TopBar.module.css
│   │   │   ├── UsersTable/
│   │   │   │   ├── UsersTable.tsx      # Full table with filters, rows, and pagination
│   │   │   │   ├── UsersTable.module.css
│   │   │   │   ├── UserRow.tsx         # Single table row
│   │   │   │   └── UserRow.module.css
│   │   │   ├── UserModal/
│   │   │   │   ├── UserModal.tsx       # Create / edit modal with form
│   │   │   │   └── UserModal.module.css
│   │   │   ├── DeleteConfirmDialog/
│   │   │   │   ├── DeleteConfirmDialog.tsx
│   │   │   │   └── DeleteConfirmDialog.module.css
│   │   │   └── Pagination/
│   │   │       ├── Pagination.tsx      # Rows per page + range text + nav buttons
│   │   │       └── Pagination.module.css
│   │   │
│   │   └── templates/                  # Page skeletons — layout without real data
│   │       └── MainLayout/
│   │           ├── MainLayout.tsx      # Sidebar + main content area slot
│   │           └── MainLayout.module.css
│   │
│   ├── pages/                          # Route-level components — wire templates with data
│   │   └── UsersPage/
│   │       ├── UsersPage.tsx
│   │       └── UsersPage.module.css
│   │
│   ├── mocks/                          # Development fallback data
│   │   └── users.mock.ts
│   │
│   ├── App.tsx                         # Root component — renders MainLayout + UsersPage
│   ├── App.module.css
│   └── main.tsx                        # Vite entry point
│
├── .env.example
├── .gitignore
├── eslint.config.js
├── index.html
├── package.json
├── tsconfig.json
└── vite.config.ts
```

---

## Design tokens (`src/styles/tokens.css`)

```css
:root {
  /* Colors */
  --color-primary:      #00b4d8;
  --color-primary-dark: #0096b7;
  --color-sidebar:      #0f2033;
  --color-edit:         #f5c800;
  --color-delete:       #9b3fa0;
  --color-bg:           #f0f2f5;
  --color-surface:      #ffffff;
  --color-text:         #1a1a2e;
  --color-text-muted:   #6b7280;
  --color-inactive:     #cccccc;
  --color-filter-bg:    #e8e8e8;
  --color-error:        #e53e3e;
  --color-success:      #00b4d8;

  /* Spacing */
  --spacing-xs:  4px;
  --spacing-sm:  8px;
  --spacing-md:  16px;
  --spacing-lg:  24px;
  --spacing-xl:  32px;

  /* Border radius */
  --radius-sm:   4px;
  --radius-md:   6px;
  --radius-lg:   12px;
  --radius-full: 9999px;

  /* Typography */
  --font-family: "Inter", "Segoe UI", sans-serif;
  --font-sm:     0.875rem;
  --font-base:   1rem;
  --font-lg:     1.25rem;
  --font-xl:     1.5rem;

  /* Shadows */
  --shadow-sm:   0 1px 3px rgba(0, 0, 0, 0.08);
  --shadow-md:   0 4px 12px rgba(0, 0, 0, 0.12);

  /* Transitions */
  --transition-fast: 150ms ease;
  --transition-base: 250ms ease;

  /* Layout */
  --sidebar-width: 220px;
}
```

---

## Shared types (`src/types/`)

### `user.types.ts`

```ts
export interface User {
  id: number;
  email: string;
  username: string;
  country: string;
  role: string;
  status: boolean;   // true = Active, false = Inactive
  incadea: boolean;  // true = Yes, false = No
}

export type CreateUserPayload = Omit<User, "id">;
export type UpdateUserPayload = Partial<CreateUserPayload>;
```

### `api.types.ts`

```ts
export interface ApiResponse<T> {
  data: T;
  message?: string;
}

export interface ApiError {
  message: string;
  statusCode: number;
}
```

---

## Service layer (`src/services/`)

### `api.ts`

```ts
import axios from "axios";

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? "http://localhost:3000",
  headers: { "Content-Type": "application/json" },
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message = error.response?.data?.message ?? "Unexpected error";
    return Promise.reject(new Error(message));
  }
);

export default api;
```

### `users.service.ts`

```ts
import api from "./api";
import type { User, CreateUserPayload, UpdateUserPayload } from "@/types/user.types";
import { initialUsers } from "@/mocks/users.mock";

const USE_MOCK = !import.meta.env.VITE_API_BASE_URL;

export async function getUsers(): Promise<User[]> {
  if (USE_MOCK) return initialUsers;
  const { data } = await api.get<User[]>("/users");
  return data;
}

export async function createUser(payload: CreateUserPayload): Promise<User> {
  if (USE_MOCK) { /* mock logic */ }
  const { data } = await api.post<User>("/users", payload);
  return data;
}

export async function updateUser(id: number, payload: UpdateUserPayload): Promise<User> {
  if (USE_MOCK) { /* mock logic */ }
  const { data } = await api.put<User>(`/users/${id}`, payload);
  return data;
}

export async function toggleUserStatus(id: number, status: boolean): Promise<User> {
  if (USE_MOCK) { /* mock logic */ }
  const { data } = await api.patch<User>(`/users/${id}/status`, { status });
  return data;
}

export async function deleteUser(id: number): Promise<void> {
  if (USE_MOCK) { /* mock logic */ }
  await api.delete(`/users/${id}`);
}
```

---

## Custom hooks (`src/hooks/`)

### `useUsers.ts`

```ts
import { useState, useEffect, useCallback } from "react";
import type { User, CreateUserPayload, UpdateUserPayload } from "@/types/user.types";
import * as usersService from "@/services/users.service";

export function useUsers() {
  const [users, setUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const fetchUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await usersService.getUsers();
      setUsers(data);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { fetchUsers(); }, [fetchUsers]);

  const createUser  = async (payload: CreateUserPayload) => { /* ... */ };
  const updateUser  = async (id: number, payload: UpdateUserPayload) => { /* ... */ };
  const toggleStatus = async (id: number, status: boolean) => { /* ... */ };
  const removeUser  = async (id: number) => { /* ... */ };

  return { users, loading, error, successMessage, createUser, updateUser, toggleStatus, removeUser };
}
```

### `usePagination.ts`

```ts
import { useState } from "react";

export function usePagination<T>(items: T[], defaultRowsPerPage = 10) {
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(defaultRowsPerPage);

  const totalPages = Math.ceil(items.length / rowsPerPage);
  const paginatedItems = items.slice(page * rowsPerPage, (page + 1) * rowsPerPage);

  const goToFirst = () => setPage(0);
  const goToPrev  = () => setPage((p) => Math.max(0, p - 1));
  const goToNext  = () => setPage((p) => Math.min(totalPages - 1, p + 1));
  const goToLast  = () => setPage(totalPages - 1);

  return { page, rowsPerPage, setRowsPerPage, paginatedItems, totalPages, goToFirst, goToPrev, goToNext, goToLast };
}
```

---

## NPM scripts (`package.json`)

```json
{
  "scripts": {
    "dev":     "vite",
    "build":   "tsc -b && vite build",
    "preview": "vite preview",
    "lint":    "eslint src"
  }
}
```
