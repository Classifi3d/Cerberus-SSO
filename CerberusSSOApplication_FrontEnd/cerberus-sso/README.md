# React + TypeScript + Vite

## API and login

Axios uses `VITE_API_BASE_URL` as its base URL. For local development, set
`VITE_API_BASE_URL=http://localhost:5211` in `.env.local` and restart Vite.
If unset, requests use the current origin; configure your server or reverse
proxy to route `/User/login` to the API. The API must allow the frontend's
origin when using a different origin.

`useLogin` in `src/features/login/hooks/useLogin.ts` provides a React Query
mutation. Call `mutate` or `mutateAsync` with `{ email, password, requestId? }`;
`requestId` is the OAuth request ID from the login URL when present. Its data
is one of `{ token }`, `{ message, challengeId }` (MFA required), or
`{ redirectUrl }`. Handle these outcomes in the calling UI; this setup does
not store tokens, complete MFA, or navigate automatically. Failed requests
are exposed through the mutation's error state. The existing login form is not
wired to the hook yet.

## UI components

The `@` alias points to `src`. Shadcn and React Bits components both live in
`src/shared/ui`. The shadcn
configuration uses `src/shared/styles/tailwind.css` for Tailwind imports and
theme tokens and `src/lib/utils.ts` for class merging. Tailwind v4 cannot run
through Sass, so `tailwind.css` stays plain CSS and is imported in
`src/main.tsx` alongside `globals.scss` (your global SCSS entry).

The layout renders the shared toaster. To display a notification, import its
manager and add a toast:

```tsx
import { toast } from "@/shared/ui/toast";

toast.add({ title: "Saved", type: "success" });
```

This template provides a minimal setup to get React working in Vite with HMR and some ESLint rules.

Currently, two official plugins are available:

- [@vitejs/plugin-react](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react) uses [Oxc](https://oxc.rs)
- [@vitejs/plugin-react-swc](https://github.com/vitejs/vite-plugin-react/blob/main/packages/plugin-react-swc) uses [SWC](https://swc.rs/)

## React Compiler

The React Compiler is not enabled on this template because of its impact on dev & build performances. To add it, see [this documentation](https://react.dev/learn/react-compiler/installation).

## Expanding the ESLint configuration

If you are developing a production application, we recommend updating the configuration to enable type-aware lint rules:

```js
export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...

      // Remove tseslint.configs.recommended and replace with this
      tseslint.configs.recommendedTypeChecked,
      // Alternatively, use this for stricter rules
      tseslint.configs.strictTypeChecked,
      // Optionally, add this for stylistic rules
      tseslint.configs.stylisticTypeChecked,

      // Other configs...
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```

You can also install [eslint-plugin-react-x](https://npmx.dev/package/eslint-plugin-react-x) and [eslint-plugin-react-dom](https://npmx.dev/package/eslint-plugin-react-dom) for React-specific lint rules:

```js
// eslint.config.js
import reactX from 'eslint-plugin-react-x'
import reactDom from 'eslint-plugin-react-dom'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      // Other configs...
      // Enable lint rules for React
      reactX.configs['recommended-typescript'],
      // Enable lint rules for React DOM
      reactDom.configs.recommended,
    ],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.node.json', './tsconfig.app.json'],
        tsconfigRootDir: import.meta.dirname,
      },
      // other options...
    },
  },
])

```
