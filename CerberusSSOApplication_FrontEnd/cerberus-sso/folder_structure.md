src/
├── app/
│   ├── api/                # App-level API configurations, interceptors, or clients
│   └── routing/
│       └── Router.tsx      # Main application routing setup
├── assets/                 # Images, icons, and static assets
├── components/
│   └── ui/                 # shadcn/ui components (Button, Input, Dialog, etc.)
├── features/               # Domain-specific modules
│   ├── account/
│   │   ├── components/     # Account-specific UI pieces
│   │   ├── hooks/          # Account-specific custom hooks
│   │   ├── pages/          # Account views/pages
│   │   └── utils/          # Account-related utils & Yup schemas
│   ├── login/
│   │   ├── components/
│   │   ├── hooks/
│   │   ├── pages/
│   │   │   └── LoginPage.tsx
│   │   └── utils/          # e.g., login.schema.ts (Yup validation)
│   └── signup/
│       ├── components/
│       ├── hooks/
│       ├── pages/
│       └── utils/          # e.g., signup.schema.ts (Yup validation)
├── shared/                 # Globally shared application building blocks
│   ├── components/         # Shared app components (Navbar, Footer, etc.)
│   ├── config/             # Environment variables, app constants, routes config
│   ├── hooks/              # Global custom hooks (useAuth, useDebounce, etc.)
│   ├── services/           # Global API service calls (authService, userService, etc.)
│   ├── stores/             # Global state management (Zustand, Redux Toolkit, etc.)
│   ├── types/              # Global TypeScript interfaces/types
│   └── utils/              # Global helper functions (cn, formatters, etc.)
├── styles/
│   └── index.css           # Tailwind v4 stylesheet (@import "tailwindcss";)
├── App.tsx                 # Root application wrapper
└── main.tsx                # Application entry point