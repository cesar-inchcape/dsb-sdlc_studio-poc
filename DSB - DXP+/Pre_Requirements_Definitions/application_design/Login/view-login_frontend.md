# View Spec — Login (`LoginPage`)

> **Visual references** — screenshots located at:
> `/figma_login`
>
> | File | Shows |
> |------|-------|
> | `landing.png` | Full-screen landing page with background image and Login button |

---

## 1. Overall page structure

The login experience has **two layers** rendered on top of the same full-screen background:

1. **Landing screen** — always visible, shows branding + Login button
2. **Login modal** — appears centered on screen when the user clicks Login

```
LoginPage
├── LandingBackground        ← full-screen image + overlay
│   ├── InchcapeLogo
│   ├── HeroText             ← "DIGITAL SERVICE BOOKING"
│   └── LoginButton          ← triggers LoginModal
└── LoginModal (conditional)
    ├── ModalOverlay
    └── LoginForm
        ├── EmailField
        ├── PasswordField
        ├── SubmitButton
        └── ErrorBanner (conditional)
```

---

## 2. Landing screen

> **Reference:** `landing.png`

### 2.1 Background

- Full-viewport image (`width: 100vw`, `height: 100vh`, `object-fit: cover`)
- Dark car driving through a neon-lit tunnel
- Subtle dark gradient overlay on the left half so text remains readable:
  `background: linear-gradient(to right, rgba(0,0,0,0.55) 40%, transparent 100%)`

### 2.2 Inchcape logo

- Top-left corner, `position: absolute`, `top: 24px`, `left: 32px`
- White steering-wheel icon + "Inchcape" wordmark, white
- Height: `~36px`

### 2.3 Hero text

- `position: absolute`, vertically centered on the left third of the screen
- Text: **"DIGITAL SERVICE BOOKING"**
- `font-size: clamp(2.5rem, 5vw, 4rem)` · `font-weight: 900` · `color: #ffffff`
- `text-transform: uppercase` · `line-height: 1.1` · `max-width: 420px`

### 2.4 Login button

- Sits directly below the hero text, `margin-top: 28px`
- Label: **"Login"**
- Background: `#00b4d8` (cyan) · Text: white, bold
- `border-radius: var(--radius-md)` · `padding: 10px 36px` · `font-size: var(--font-base)`
- No border · `cursor: pointer`
- Hover: `background: #0096b7` (darker cyan), smooth transition `var(--transition-fast)`
- On click → opens **Login modal** (section 3)

---

## 3. Login modal

Centered on screen with a dark overlay behind it.

- Overlay: `rgba(0, 0, 0, 0.55)`, full-viewport, `z-index: 100`
- Card: white `#ffffff`, `border-radius: var(--radius-lg)`, `padding: 40px 36px`
- `width: 420px`, `max-width: 90vw`
- Drop shadow: `var(--shadow-md)`

### 3.1 Modal header

| Element | Detail |
|---------|--------|
| Logo | Inchcape steering-wheel icon, centered, `height: 40px`, color `#00b4d8` |
| Title | **"Iniciar sesión"** · `color: #00b4d8` · `font-size: var(--font-xl)` · bold · centered |
| Subtitle | `"Ingresa tus credenciales para continuar"` · `color: var(--color-text-muted)` · `font-size: var(--font-sm)` · centered |

### 3.2 Form fields

Both fields follow the **floating label / underline style** from the design system:

- Default: grey label above an underline-only border
- Focused / filled: label turns `#00b4d8`, underline turns `#00b4d8`
- Error: underline turns `#e53e3e`, label turns `#e53e3e`, error message below field

#### Email field

| Property | Value |
|----------|-------|
| Label | `"Correo institucional *"` |
| Type | `<input type="email" />` |
| Placeholder | `"usuario@inchcape.com"` |
| Validation | Required · valid email format |
| Autocomplete | `email` |

#### Password field

| Property | Value |
|----------|-------|
| Label | `"Contraseña *"` |
| Type | `<input type="password" />` |
| Placeholder | `"••••••••"` |
| Validation | Required · min 6 characters |
| Right icon | Eye toggle to show / hide password text |
| Autocomplete | `current-password` |

### 3.3 Submit button

- Label: **"Ingresar"**
- Full width of the form
- Background: `#00b4d8` · Text: white, bold
- `border-radius: var(--radius-md)` · `padding: 12px`
- Disabled + spinner while request is in progress
- `margin-top: 28px`

### 3.4 Error banner (conditional)

Appears below the submit button when credentials do not match the dummy user:

- Background: `#fef2f2` · Border-left: `4px solid #e53e3e`
- Text: `"Correo o contraseña incorrectos."` · color `#e53e3e` · `font-size: var(--font-sm)`
- Auto-dismisses after 5 s or on any field change

---

## 4. Authentication flow (client-side only)

> No real backend verification. The frontend validates credentials against a hardcoded dummy user and self-generates a JWT.

```
User clicks "Ingresar"
       │
       ▼
Validate fields (client-side)
       │ invalid → show inline errors, stop
       ▼
Compare email + password against DUMMY_USER
       │ no match → show ErrorBanner, stop
       ▼
Generate JWT (see section 5)
       │
       ▼
Store token in localStorage ("dsb_token")
       │
       ▼
Redirect to /admin/users  (UsersPage)
```

---

## 5. JWT generation (no verification)

Install the lightweight encoder:

```bash
npm install jwt-encode
```

### `src/services/auth.service.ts`

```ts
import jwtEncode from "jwt-encode";
import { DUMMY_USER } from "@/mocks/auth.mock";

const JWT_SECRET = "inchcape_dsb_dev_secret"; // dev-only, never expose in production
const TOKEN_KEY  = "dsb_token";

export interface LoginPayload {
  email: string;
  password: string;
}

export interface JwtClaims {
  sub: string;       // user id
  email: string;
  username: string;
  country: string;
  role: string;
  iat: number;       // issued at (Unix timestamp)
  exp: number;       // expiry  (Unix timestamp, iat + 8 h)
}

export function login(payload: LoginPayload): string {
  const { email, password } = payload;

  if (
    email.trim().toLowerCase() !== DUMMY_USER.email.toLowerCase() ||
    password !== DUMMY_USER.password
  ) {
    throw new Error("Invalid credentials");
  }

  const now = Math.floor(Date.now() / 1000);

  const claims: JwtClaims = {
    sub:      String(DUMMY_USER.id),
    email:    DUMMY_USER.email,
    username: DUMMY_USER.username,
    country:  DUMMY_USER.country,
    role:     DUMMY_USER.role,
    iat:      now,
    exp:      now + 8 * 60 * 60,   // 8 hours
  };

  const token = jwtEncode(claims, JWT_SECRET);
  localStorage.setItem(TOKEN_KEY, token);
  return token;
}

export function logout(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function isAuthenticated(): boolean {
  const token = getToken();
  if (!token) return false;

  try {
    // Decode payload without verifying signature (dev-only)
    const [, payloadB64] = token.split(".");
    const claims: JwtClaims = JSON.parse(atob(payloadB64));
    return claims.exp > Math.floor(Date.now() / 1000);
  } catch {
    return false;
  }
}
```

---

## 6. Dummy user credentials

### `src/mocks/auth.mock.ts`

```ts
export const DUMMY_USER = {
  id:       1,
  email:    "admin@inchcape.com",
  password: "Admin1234",        // plain-text, dev-only mock
  username: "Admin Inchcape",
  country:  "Colombia",
  role:     "Workshop Admin",
};
```

| Field | Value |
|-------|-------|
| **Email** | `admin@inchcape.com` |
| **Password** | `Admin1234` |
| **Role** | Workshop Admin |
| **Country** | Colombia |

> These credentials exist only in the frontend mock. They must be replaced with real API authentication before any production deployment.

---

## 7. Route guarding

### `src/router/ProtectedRoute.tsx`

```tsx
import { Navigate, Outlet } from "react-router-dom";
import { isAuthenticated } from "@/services/auth.service";

export function ProtectedRoute() {
  return isAuthenticated() ? <Outlet /> : <Navigate to="/login" replace />;
}
```

### Route setup (`src/App.tsx`)

```tsx
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { LoginPage }      from "@/pages/LoginPage/LoginPage";
import { UsersPage }      from "@/pages/UsersPage/UsersPage";
import { ProtectedRoute } from "@/router/ProtectedRoute";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/admin/users" element={<UsersPage />} />
        </Route>
        <Route path="*" element={<Navigate to="/login" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
```

Add react-router-dom to dependencies:

```bash
npm install react-router-dom
npm install -D @types/react-router-dom
```

---

## 8. Loading & feedback states

| Trigger | UI behaviour |
|---------|-------------|
| Submit with empty fields | Inline field errors (red underline + message), submit blocked |
| Submit with wrong credentials | Red ErrorBanner below the button |
| Submit in progress | Button disabled, spinner inside button, fields read-only |
| Success | Token stored → immediate redirect to `/admin/users` |
| Session expired | `isAuthenticated()` returns `false` → redirect to `/login` |

---

## 9. Component mapping (from `project-structure.md`)

| UI element | Component file |
|-----------|----------------|
| Full page wrapper | `src/pages/LoginPage/LoginPage.tsx` |
| Background + hero text | `src/pages/LoginPage/LandingBackground.tsx` |
| Login trigger button | `src/components/atoms/Button/Button.tsx` |
| Modal overlay + card | `src/components/organisms/LoginModal/LoginModal.tsx` |
| Email field | `src/components/molecules/FormField/FormField.tsx` |
| Password field | `src/components/molecules/FormField/FormField.tsx` (type="password") |
| Submit button | `src/components/atoms/Button/Button.tsx` |
| Error banner | `src/components/molecules/ErrorBanner/ErrorBanner.tsx` |
| Spinner | `src/components/atoms/Spinner/Spinner.tsx` |
| Route guard | `src/router/ProtectedRoute.tsx` |
| Auth service | `src/services/auth.service.ts` |
| Dummy user | `src/mocks/auth.mock.ts` |

---

## 10. Updated folder structure additions

The following files are added to the structure defined in `project-structure.md`:

```
src/
├── pages/
│   ├── LoginPage/
│   │   ├── LoginPage.tsx           ← full-screen landing + modal logic
│   │   ├── LoginPage.module.css
│   │   └── LandingBackground.tsx   ← hero image + text + Login button
│   └── UsersPage/ ...
├── components/
│   └── organisms/
│       └── LoginModal/
│           ├── LoginModal.tsx      ← modal card + form
│           └── LoginModal.module.css
├── services/
│   └── auth.service.ts             ← login(), logout(), getToken(), isAuthenticated()
├── mocks/
│   └── auth.mock.ts                ← DUMMY_USER credentials
└── router/
    └── ProtectedRoute.tsx          ← redirects unauthenticated users to /login
```
