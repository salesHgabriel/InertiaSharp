using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Views;

public static class ReactGenerator
{
    public static Dictionary<string, string> Generate(ProjectOptions opts)
    {
        var files = new Dictionary<string, string>
        {
            ["ClientApp/package.json"]         = PackageJson(opts),
            ["ClientApp/vite.config.ts"]       = ViteConfig(opts),
            ["ClientApp/tsconfig.json"]        = TsConfig(),
            ["ClientApp/tsconfig.app.json"]    = TsConfigApp(),
            ["ClientApp/tailwind.config.js"]   = TailwindConfig(),
            ["ClientApp/postcss.config.js"]    = PostCssConfig(),
            ["ClientApp/components.json"]      = ComponentsJson(),
            ["ClientApp/src/app.tsx"]          = AppTsx(opts),
            ["ClientApp/src/assets/app.css"]   = AppCss(),
            ["ClientApp/src/lib/utils.ts"]     = UtilsTs(),

            // shadcn/ui components (hand-crafted, no CLI needed)
            ["ClientApp/src/components/ui/button.tsx"]    = ButtonTsx(),
            ["ClientApp/src/components/ui/card.tsx"]      = CardTsx(),
            ["ClientApp/src/components/ui/input.tsx"]     = InputTsx(),
            ["ClientApp/src/components/ui/label.tsx"]     = LabelTsx(),
            ["ClientApp/src/components/ui/badge.tsx"]     = BadgeTsx(),
            ["ClientApp/src/components/ui/alert.tsx"]     = AlertTsx(),
            ["ClientApp/src/components/ui/separator.tsx"] = SeparatorTsx(),
            ["ClientApp/src/components/ui/avatar.tsx"]    = AvatarTsx(),
        };

        if (opts.IncludeAuth)
        {
            files["ClientApp/src/layouts/AppLayout.tsx"]    = AppLayout(opts);
            files["ClientApp/src/layouts/GuestLayout.tsx"]  = GuestLayout(opts);
            files["ClientApp/src/Pages/Auth/Login.tsx"]     = LoginPage(opts);
            files["ClientApp/src/Pages/Auth/Register.tsx"]  = RegisterPage(opts);
            files["ClientApp/src/Pages/Dashboard.tsx"]      = DashboardPage(opts);
            files["ClientApp/src/Pages/Profile/Edit.tsx"]   = ProfileEditPage(opts);
            files["ClientApp/src/Pages/Admin/Users.tsx"]    = AdminUsersPage(opts);
            files["ClientApp/src/Pages/Errors/Forbidden.tsx"]   = ForbiddenPage();
            files["ClientApp/src/Pages/Errors/ServerError.tsx"]  = ServerErrorPage();
        }
        else
        {
            files["ClientApp/src/layouts/AppLayout.tsx"]  = SimpleAppLayout(opts);
            files["ClientApp/src/Pages/Home.tsx"]         = HomePage(opts);
        }

        return files;
    }

    // ── Config files ─────────────────────────────────────────────────────────

    private static string PackageJson(ProjectOptions opts) => $$"""
{
  "name": "{{opts.ProjectName.ToLower()}}",
  "version": "1.0.0",
  "private": true,
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc -b && vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "@inertiajs/react": "^2.0.0",
    "@radix-ui/react-avatar": "^1.1.2",
    "@radix-ui/react-label": "^2.1.1",
    "@radix-ui/react-separator": "^1.1.0",
    "@radix-ui/react-slot": "^1.1.1",
    "class-variance-authority": "^0.7.1",
    "clsx": "^2.1.1",
    "lucide-react": "^0.468.0",
    "react": "^19.0.0",
    "react-dom": "^19.0.0",
    "tailwind-merge": "^2.6.1",
    "tailwindcss-animate": "^1.0.7"
  },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "@types/react": "^19.0.0",
    "@types/react-dom": "^19.0.0",
    "@vitejs/plugin-react": "^4.3.4",
    "autoprefixer": "^10.4.20",
    "postcss": "^8.5.1",
    "tailwindcss": "^3.4.17",
    "typescript": "^5.7.2",
    "vite": "^6.0.6"
  }
}
""";

    private static string ViteConfig(ProjectOptions opts) => $$"""
import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import react from '@vitejs/plugin-react'
import autoprefixer from 'autoprefixer'
import tailwind from 'tailwindcss'
import { defineConfig } from 'vite'

const target = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
    ? process.env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:5001'

export default defineConfig({
  css: {
    postcss: {
      plugins: [tailwind(), autoprefixer()],
    },
  },
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: {{opts.VitePort}},
    strictPort: true,
    cors: { origin: [target] },
    hmr: { host: 'localhost', port: {{opts.VitePort}} },
  },
  build: {
    outDir: path.resolve(__dirname, '../wwwroot/dist'),
    emptyOutDir: true,
    manifest: true,
    rollupOptions: {
      input: path.resolve(__dirname, 'src/app.tsx'),
      output: {
        entryFileNames: 'app.js',
        chunkFileNames: 'chunks/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          const name = assetInfo.names?.[0] ?? ''
          return name.endsWith('.css') ? 'app.css' : 'assets/[name]-[hash][extname]'
        },
      },
    },
  },
})
""";

    private static string TsConfig() => """
{
  "files": [],
  "references": [
    { "path": "./tsconfig.app.json" }
  ]
}
""";

    private static string TsConfigApp() => """
{
  "compilerOptions": {
    "target": "ES2022",
    "useDefineForClassFields": true,
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "Bundler",
    "allowImportingTsExtensions": true,
    "isolatedModules": true,
    "moduleDetection": "force",
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": false,
    "noUnusedParameters": false,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src"]
}
""";

    private static string TailwindConfig() => """
/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
}
""";

    private static string PostCssConfig() => """
export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
  },
}
""";

    private static string ComponentsJson() => """
{
  "$schema": "https://ui.shadcn.com/schema.json",
  "style": "default",
  "rsc": false,
  "tsx": true,
  "tailwind": {
    "config": "tailwind.config.js",
    "css": "src/assets/app.css",
    "baseColor": "slate",
    "cssVariables": true
  },
  "aliases": {
    "components": "@/components",
    "utils": "@/lib/utils"
  }
}
""";

    private static string AppTsx(ProjectOptions opts) => $$"""
import './assets/app.css'
import { createInertiaApp } from '@inertiajs/react'
import { createRoot } from 'react-dom/client'
import AppLayout from './layouts/AppLayout'

createInertiaApp({
  resolve: (name: string) => {
    const pages = import.meta.glob('./Pages/**/*.tsx', { eager: true }) as Record<string, { default: React.ComponentType }>
    const page = pages[`./Pages/${name}.tsx`]

    if (!page)
      throw new Error(`Inertia page not found: ./Pages/${name}.tsx`)

    if (!(page.default as any).layout)
      (page.default as any).layout = (page: React.ReactNode) => <AppLayout>{page}</AppLayout>

    return page
  },

  title: (title) => (title ? `${title} – {{opts.ProjectName}}` : '{{opts.ProjectName}}'),

  setup({ el, App, props }) {
    createRoot(el).render(<App {...props} />)
  },

  progress: {
    color: '#6366f1',
    includeCSS: true,
    showSpinner: false,
  },
})
""";

    private static string AppCss() => """
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --card: 0 0% 100%;
    --card-foreground: 222.2 84% 4.9%;
    --primary: 222.2 47.4% 11.2%;
    --primary-foreground: 210 40% 98%;
    --secondary: 210 40% 96.1%;
    --secondary-foreground: 222.2 47.4% 11.2%;
    --muted: 210 40% 96.1%;
    --muted-foreground: 215.4 16.3% 46.9%;
    --accent: 210 40% 96.1%;
    --accent-foreground: 222.2 47.4% 11.2%;
    --destructive: 0 84.2% 60.2%;
    --destructive-foreground: 210 40% 98%;
    --border: 214.3 31.8% 91.4%;
    --input: 214.3 31.8% 91.4%;
    --ring: 222.2 84% 4.9%;
    --radius: 0.5rem;
  }
  .dark {
    --background: 222.2 84% 4.9%;
    --foreground: 210 40% 98%;
    --card: 222.2 84% 4.9%;
    --card-foreground: 210 40% 98%;
    --primary: 210 40% 98%;
    --primary-foreground: 222.2 47.4% 11.2%;
    --secondary: 217.2 32.6% 17.5%;
    --secondary-foreground: 210 40% 98%;
    --muted: 217.2 32.6% 17.5%;
    --muted-foreground: 215 20.2% 65.1%;
    --accent: 217.2 32.6% 17.5%;
    --accent-foreground: 210 40% 98%;
    --destructive: 0 62.8% 30.6%;
    --destructive-foreground: 210 40% 98%;
    --border: 217.2 32.6% 17.5%;
    --input: 217.2 32.6% 17.5%;
    --ring: 212.7 26.8% 83.9%;
  }
}

@layer base {
  * { @apply border-border; }
  body { @apply bg-background text-foreground; }
}
""";

    private static string UtilsTs() => """
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
""";

    // ── shadcn/ui components ──────────────────────────────────────────────────

    private static string ButtonTsx() => """
import * as React from 'react'
import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const buttonVariants = cva(
  'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
        outline: 'border border-input bg-background hover:bg-accent hover:text-accent-foreground',
        secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
        link: 'text-primary underline-offset-4 hover:underline',
      },
      size: {
        default: 'h-10 px-4 py-2',
        sm: 'h-9 rounded-md px-3',
        lg: 'h-11 rounded-md px-8',
        icon: 'h-10 w-10',
      },
    },
    defaultVariants: {
      variant: 'default',
      size: 'default',
    },
  },
)

export interface ButtonProps
  extends React.ButtonHTMLAttributes<HTMLButtonElement>,
    VariantProps<typeof buttonVariants> {
  asChild?: boolean
}

const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant, size, asChild = false, ...props }, ref) => {
    const Comp = asChild ? Slot : 'button'
    return (
      <Comp
        className={cn(buttonVariants({ variant, size, className }))}
        ref={ref}
        {...props}
      />
    )
  },
)
Button.displayName = 'Button'

export { Button, buttonVariants }
""";

    private static string CardTsx() => """
import * as React from 'react'
import { cn } from '@/lib/utils'

const Card = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn('rounded-lg border bg-card text-card-foreground shadow-sm', className)} {...props} />
  ),
)
Card.displayName = 'Card'

const CardHeader = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn('flex flex-col space-y-1.5 p-6', className)} {...props} />
  ),
)
CardHeader.displayName = 'CardHeader'

const CardTitle = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLHeadingElement>>(
  ({ className, ...props }, ref) => (
    <h3 ref={ref} className={cn('text-2xl font-semibold leading-none tracking-tight', className)} {...props} />
  ),
)
CardTitle.displayName = 'CardTitle'

const CardContent = React.forwardRef<HTMLDivElement, React.HTMLAttributes<HTMLDivElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn('p-6 pt-0', className)} {...props} />
  ),
)
CardContent.displayName = 'CardContent'

export { Card, CardHeader, CardTitle, CardContent }
""";

    private static string InputTsx() => """
import * as React from 'react'
import { cn } from '@/lib/utils'

export interface InputProps extends React.InputHTMLAttributes<HTMLInputElement> {}

const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, ...props }, ref) => (
    <input
      type={type}
      className={cn(
        'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
        className,
      )}
      ref={ref}
      {...props}
    />
  ),
)
Input.displayName = 'Input'

export { Input }
""";

    private static string LabelTsx() => """
import * as React from 'react'
import * as LabelPrimitive from '@radix-ui/react-label'
import { cn } from '@/lib/utils'

const Label = React.forwardRef<
  React.ElementRef<typeof LabelPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof LabelPrimitive.Root>
>(({ className, ...props }, ref) => (
  <LabelPrimitive.Root
    ref={ref}
    className={cn('text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70', className)}
    {...props}
  />
))
Label.displayName = LabelPrimitive.Root.displayName

export { Label }
""";

    private static string BadgeTsx() => """
import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const badgeVariants = cva(
  'inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors',
  {
    variants: {
      variant: {
        default: 'border-transparent bg-primary text-primary-foreground hover:bg-primary/80',
        secondary: 'border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80',
        destructive: 'border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80',
        outline: 'text-foreground',
      },
    },
    defaultVariants: { variant: 'default' },
  },
)

export interface BadgeProps
  extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return <div className={cn(badgeVariants({ variant }), className)} {...props} />
}

export { Badge, badgeVariants }
""";

    private static string AlertTsx() => """
import * as React from 'react'
import { cva, type VariantProps } from 'class-variance-authority'
import { cn } from '@/lib/utils'

const alertVariants = cva(
  'relative w-full rounded-lg border p-4',
  {
    variants: {
      variant: {
        default: 'bg-background text-foreground',
        destructive: 'border-destructive/50 text-destructive dark:border-destructive',
      },
    },
    defaultVariants: { variant: 'default' },
  },
)

const Alert = React.forwardRef<
  HTMLDivElement,
  React.HTMLAttributes<HTMLDivElement> & VariantProps<typeof alertVariants>
>(({ className, variant, ...props }, ref) => (
  <div ref={ref} role="alert" className={cn(alertVariants({ variant }), className)} {...props} />
))
Alert.displayName = 'Alert'

const AlertDescription = React.forwardRef<HTMLParagraphElement, React.HTMLAttributes<HTMLParagraphElement>>(
  ({ className, ...props }, ref) => (
    <div ref={ref} className={cn('text-sm [&_p]:leading-relaxed', className)} {...props} />
  ),
)
AlertDescription.displayName = 'AlertDescription'

export { Alert, AlertDescription }
""";

    private static string SeparatorTsx() => """
import * as React from 'react'
import * as SeparatorPrimitive from '@radix-ui/react-separator'
import { cn } from '@/lib/utils'

const Separator = React.forwardRef<
  React.ElementRef<typeof SeparatorPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof SeparatorPrimitive.Root>
>(({ className, orientation = 'horizontal', decorative = true, ...props }, ref) => (
  <SeparatorPrimitive.Root
    ref={ref}
    decorative={decorative}
    orientation={orientation}
    className={cn(
      'shrink-0 bg-border',
      orientation === 'horizontal' ? 'h-[1px] w-full' : 'h-full w-[1px]',
      className,
    )}
    {...props}
  />
))
Separator.displayName = SeparatorPrimitive.Root.displayName

export { Separator }
""";

    private static string AvatarTsx() => """
import * as React from 'react'
import * as AvatarPrimitive from '@radix-ui/react-avatar'
import { cn } from '@/lib/utils'

const Avatar = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Root>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Root
    ref={ref}
    className={cn('relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full', className)}
    {...props}
  />
))
Avatar.displayName = AvatarPrimitive.Root.displayName

const AvatarImage = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Image>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Image>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Image ref={ref} className={cn('aspect-square h-full w-full', className)} {...props} />
))
AvatarImage.displayName = AvatarPrimitive.Image.displayName

const AvatarFallback = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Fallback>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Fallback>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Fallback
    ref={ref}
    className={cn('flex h-full w-full items-center justify-center rounded-full bg-muted text-sm font-medium uppercase', className)}
    {...props}
  />
))
AvatarFallback.displayName = AvatarPrimitive.Fallback.displayName

export { Avatar, AvatarImage, AvatarFallback }
""";

    // ── Layouts ──────────────────────────────────────────────────────────────

    private static string AppLayout(ProjectOptions opts) => $$"""
import { Link, router, usePage } from '@inertiajs/react'
import { LayoutDashboard, LogOut, Shield, User } from 'lucide-react'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'

interface AuthProps {
  auth?: {
    user: { id: string; fullName: string; email: string; avatarPathFile?: string; hasAvatar: boolean }
    permissions: { canEditContent: boolean; canManageUsers: boolean; canViewReports: boolean }
  }
}

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { props } = usePage<AuthProps>()
  const user        = props.auth?.user
  const permissions = props.auth?.permissions
  const initials    = user?.fullName?.split(' ').map(n => n[0]).join('').toUpperCase() ?? '?'

  function logout() {
    const token = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''
    router.post('/logout', {}, { headers: { 'X-CSRF-TOKEN': token } })
  }

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      {/* Sidebar */}
      <aside className="flex w-64 flex-col border-r bg-card">
        <div className="flex h-16 items-center gap-2 border-b px-6">
          <span className="text-lg font-bold tracking-tight">{{opts.ProjectName}}</span>
        </div>

        <nav className="flex flex-1 flex-col gap-1 p-3 text-sm">
          <Link
            href="/dashboard"
            className="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
          >
            <LayoutDashboard className="h-4 w-4" />
            Dashboard
          </Link>

          <Link
            href="/profile"
            className="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
          >
            <User className="h-4 w-4" />
            Profile
          </Link>

          {permissions?.canManageUsers && (
            <Link
              href="/admin/users"
              className="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
            >
              <Shield className="h-4 w-4" />
              Users
            </Link>
          )}
        </nav>

        <Separator />

        <div className="flex items-center gap-3 p-4">
          <Avatar className="h-9 w-9">
            {user?.hasAvatar && <AvatarImage src={`/${user.avatarPathFile}`} alt={user.fullName} />}
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
          <div className="flex min-w-0 flex-1 flex-col">
            <span className="truncate text-sm font-medium">{user?.fullName}</span>
            <span className="truncate text-xs text-muted-foreground">{user?.email}</span>
          </div>
          <Button variant="ghost" size="icon" className="h-8 w-8 shrink-0" onClick={logout}>
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex flex-1 flex-col overflow-auto">
        <div className="flex-1 p-8">{children}</div>
      </main>
    </div>
  )
}
""";

    private static string GuestLayout(ProjectOptions opts) => $$"""
export default function GuestLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-muted/40 p-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold tracking-tight">{{opts.ProjectName}}</h1>
          <p className="mt-1 text-sm text-muted-foreground">Welcome back</p>
        </div>
        {children}
      </div>
    </div>
  )
}
""";

    private static string SimpleAppLayout(ProjectOptions opts) => $$"""
export default function AppLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="mx-auto flex h-14 max-w-5xl items-center px-4">
          <span className="text-lg font-semibold">{{opts.ProjectName}}</span>
        </div>
      </header>
      <main className="mx-auto max-w-5xl p-6">{children}</main>
    </div>
  )
}
""";

    // ── Pages ─────────────────────────────────────────────────────────────────

    private static string LoginPage(ProjectOptions opts) => $$"""
import { useForm } from '@inertiajs/react'
import GuestLayout from '@/layouts/GuestLayout'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  errors?: Record<string, string>
}

export default function Login({ errors }: Props) {
  const form = useForm({ email: '', password: '', remember: false })
  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

  function submit(e: React.FormEvent) {
    e.preventDefault()
    form.post('/login', { headers: { 'X-CSRF-TOKEN': csrf }, preserveState: true })
  }

  return (
    <Card>
      <CardContent className="pt-6">
        <h2 className="mb-6 text-xl font-semibold">Sign in to your account</h2>

        <form onSubmit={submit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={form.data.email}
              onChange={e => form.setData('email', e.target.value)}
              placeholder="admin@demo.com"
              autoComplete="email"
            />
            {errors?.email && <p className="text-sm text-destructive">{errors.email}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              value={form.data.password}
              onChange={e => form.setData('password', e.target.value)}
              placeholder="••••••••"
              autoComplete="current-password"
            />
          </div>

          <Button type="submit" className="w-full" disabled={form.processing}>
            {form.processing ? 'Signing in…' : 'Sign in'}
          </Button>
        </form>

        <p className="mt-4 text-center text-sm text-muted-foreground">
          Don't have an account?{' '}
          <a href="/register" className="font-medium text-primary hover:underline">Register</a>
        </p>

        <p className="mt-3 text-center text-xs text-muted-foreground">
          Demo: <code>admin@demo.com</code> / <code>Password123!</code>
        </p>
      </CardContent>
    </Card>
  )
}

Login.layout = (page: React.ReactNode) => <GuestLayout>{page}</GuestLayout>
""";

    private static string RegisterPage(ProjectOptions opts) => $$"""
import { useForm } from '@inertiajs/react'
import GuestLayout from '@/layouts/GuestLayout'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

interface Props {
  errors?: Record<string, string>
}

export default function Register({ errors }: Props) {
  const form = useForm({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    passwordConfirmation: '',
  })

  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

  function submit(e: React.FormEvent) {
    e.preventDefault()
    form.post('/register', { headers: { 'X-CSRF-TOKEN': csrf }, preserveState: true })
  }

  return (
    <Card>
      <CardContent className="pt-6">
        <h2 className="mb-6 text-xl font-semibold">Create an account</h2>

        <form onSubmit={submit} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First name</Label>
              <Input
                id="firstName"
                value={form.data.firstName}
                onChange={e => form.setData('firstName', e.target.value)}
                placeholder="John"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last name</Label>
              <Input
                id="lastName"
                value={form.data.lastName}
                onChange={e => form.setData('lastName', e.target.value)}
                placeholder="Doe"
              />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              value={form.data.email}
              onChange={e => form.setData('email', e.target.value)}
              placeholder="john@example.com"
            />
            {errors?.email && <p className="text-sm text-destructive">{errors.email}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="password">Password</Label>
            <Input
              id="password"
              type="password"
              value={form.data.password}
              onChange={e => form.setData('password', e.target.value)}
              placeholder="Min. 8 characters"
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="passwordConfirmation">Confirm password</Label>
            <Input
              id="passwordConfirmation"
              type="password"
              value={form.data.passwordConfirmation}
              onChange={e => form.setData('passwordConfirmation', e.target.value)}
              placeholder="••••••••"
            />
            {errors?.passwordConfirmation && (
              <p className="text-sm text-destructive">{errors.passwordConfirmation}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={form.processing}>
            {form.processing ? 'Creating account…' : 'Create account'}
          </Button>
        </form>

        <p className="mt-4 text-center text-sm text-muted-foreground">
          Already have an account?{' '}
          <a href="/login" className="font-medium text-primary hover:underline">Sign in</a>
        </p>
      </CardContent>
    </Card>
  )
}

Register.layout = (page: React.ReactNode) => <GuestLayout>{page}</GuestLayout>
""";

    private static string DashboardPage(ProjectOptions opts) => $$"""
import { Activity, CheckSquare, Users } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'

interface Props {
  user: { id: string; fullName: string; email: string; createdAt: string; roles: string[] }
  stats: { totalUsers: number; activeToday: number; pendingTasks: number }
  permissions: { canEditContent: boolean; canManageUsers: boolean; canViewReports: boolean }
}

export default function Dashboard({ user, stats, permissions }: Props) {
  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Welcome back, {user.fullName}</p>
      </div>

      <div className="mb-6 grid gap-4 sm:grid-cols-3">
        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <div className="rounded-md bg-primary/10 p-2">
              <Users className="h-5 w-5 text-primary" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total Users</p>
              <p className="text-2xl font-bold">{stats.totalUsers}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <div className="rounded-md bg-green-500/10 p-2">
              <Activity className="h-5 w-5 text-green-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Active Today</p>
              <p className="text-2xl font-bold">{stats.activeToday}</p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <div className="rounded-md bg-orange-500/10 p-2">
              <CheckSquare className="h-5 w-5 text-orange-500" />
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Pending Tasks</p>
              <p className="text-2xl font-bold">{stats.pendingTasks}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      <Card>
        <CardContent className="pt-6">
          <h2 className="mb-4 font-semibold">Your Account</h2>
          <dl className="space-y-3 text-sm">
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Name</dt>
              <dd className="font-medium">{user.fullName}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Email</dt>
              <dd className="font-medium">{user.email}</dd>
            </div>
            <div className="flex justify-between">
              <dt className="text-muted-foreground">Roles</dt>
              <dd className="flex gap-1">
                {user.roles.map(role => (
                  <Badge key={role} variant="secondary">{role}</Badge>
                ))}
              </dd>
            </div>
          </dl>
        </CardContent>
      </Card>

      {permissions.canEditContent && (
        <div className="mt-4">
          <Card className="border-green-200 bg-green-50 dark:border-green-900 dark:bg-green-950/20">
            <CardContent className="pt-4">
              <p className="text-sm font-medium text-green-800 dark:text-green-400">
                You can edit content (Admin / Editor role)
              </p>
            </CardContent>
          </Card>
        </div>
      )}

      {permissions.canManageUsers && (
        <div className="mt-4">
          <Card className="border-blue-200 bg-blue-50 dark:border-blue-900 dark:bg-blue-950/20">
            <CardContent className="pt-4">
              <p className="text-sm font-medium text-blue-800 dark:text-blue-400">
                You can manage users (Admin role) —{' '}
                <a href="/admin/users" className="underline">View all users</a>
              </p>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
""";

    private static string ProfileEditPage(ProjectOptions opts) => $$"""
import { useForm } from '@inertiajs/react'
import { useRef } from 'react'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Separator } from '@/components/ui/separator'

interface Props {
  user: {
    firstName: string; lastName: string; email: string
    bio?: string; phoneNumber?: string; avatarPathFile?: string; hasAvatar: boolean
  }
  flash?: string
  errors?: Record<string, string>
}

export default function ProfileEdit({ user, flash, errors }: Props) {
  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''
  const fileInput = useRef<HTMLInputElement>(null)

  const profileForm = useForm({
    firstName: user.firstName, lastName: user.lastName,
    email: user.email, bio: user.bio ?? '', phoneNumber: user.phoneNumber ?? '',
  })

  const passwordForm = useForm({
    currentPassword: '', newPassword: '', newPasswordConfirmation: '',
  })

  function updateProfile(e: React.FormEvent) {
    e.preventDefault()
    profileForm.post('/profile', { headers: { 'X-CSRF-TOKEN': csrf } })
  }

  function changePassword(e: React.FormEvent) {
    e.preventDefault()
    passwordForm.post('/profile/password', { headers: { 'X-CSRF-TOKEN': csrf } })
  }

  function submitAvatar(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    const data = new FormData()
    data.append('avatar', file)
    fetch('/profile/avatar', { method: 'POST', body: data })
      .then(() => window.location.reload())
  }

  const initials = `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()

  return (
    <div className="max-w-2xl space-y-8">
      <div>
        <h1 className="text-2xl font-bold">Profile</h1>
        <p className="text-muted-foreground">Manage your account settings</p>
      </div>

      {flash && (
        <Alert className="border-green-200 bg-green-50 text-green-800 dark:border-green-900 dark:bg-green-950/20 dark:text-green-400">
          <AlertDescription>{flash}</AlertDescription>
        </Alert>
      )}

      <Card>
        <CardContent className="pt-6">
          <h2 className="mb-4 font-semibold">Avatar</h2>
          <div className="flex items-center gap-4">
            <Avatar className="h-16 w-16">
              {user.hasAvatar && <AvatarImage src={`/${user.avatarPathFile}`} alt={user.firstName} />}
              <AvatarFallback className="text-lg">{initials}</AvatarFallback>
            </Avatar>
            <div>
              <input ref={fileInput} type="file" accept="image/*" className="hidden" onChange={submitAvatar} />
              <Button variant="outline" size="sm" onClick={() => fileInput.current?.click()}>
                Change photo
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6">
          <h2 className="mb-4 font-semibold">Personal information</h2>
          <form onSubmit={updateProfile} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="firstName">First name</Label>
                <Input id="firstName" value={profileForm.data.firstName}
                  onChange={e => profileForm.setData('firstName', e.target.value)} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Last name</Label>
                <Input id="lastName" value={profileForm.data.lastName}
                  onChange={e => profileForm.setData('lastName', e.target.value)} />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input id="email" type="email" value={profileForm.data.email}
                onChange={e => profileForm.setData('email', e.target.value)} />
              {errors?.email && <p className="text-sm text-destructive">{errors.email}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="bio">Bio</Label>
              <Input id="bio" value={profileForm.data.bio}
                onChange={e => profileForm.setData('bio', e.target.value)}
                placeholder="Tell us about yourself" />
            </div>

            <div className="space-y-2">
              <Label htmlFor="phone">Phone</Label>
              <Input id="phone" type="tel" value={profileForm.data.phoneNumber}
                onChange={e => profileForm.setData('phoneNumber', e.target.value)}
                placeholder="+1 (555) 000-0000" />
            </div>

            <Button type="submit" disabled={profileForm.processing}>Save changes</Button>
          </form>
        </CardContent>
      </Card>

      <Separator />

      <Card>
        <CardContent className="pt-6">
          <h2 className="mb-4 font-semibold">Change password</h2>
          <form onSubmit={changePassword} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="currentPassword">Current password</Label>
              <Input id="currentPassword" type="password" value={passwordForm.data.currentPassword}
                onChange={e => passwordForm.setData('currentPassword', e.target.value)} />
              {errors?.PasswordMismatch && (
                <p className="text-sm text-destructive">{errors.PasswordMismatch}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="newPassword">New password</Label>
              <Input id="newPassword" type="password" value={passwordForm.data.newPassword}
                onChange={e => passwordForm.setData('newPassword', e.target.value)} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="newPasswordConfirmation">Confirm new password</Label>
              <Input id="newPasswordConfirmation" type="password" value={passwordForm.data.newPasswordConfirmation}
                onChange={e => passwordForm.setData('newPasswordConfirmation', e.target.value)} />
              {errors?.newPasswordConfirmation && (
                <p className="text-sm text-destructive">{errors.newPasswordConfirmation}</p>
              )}
            </div>
            <Button type="submit" variant="outline" disabled={passwordForm.processing}>
              Update password
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
""";

    private static string AdminUsersPage(ProjectOptions opts) => $$"""
import { Card } from '@/components/ui/card'

interface Props {
  users: Array<{ id: string; fullName: string; email: string; createdAt: string }>
}

export default function AdminUsers({ users }: Props) {
  function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold">Users</h1>
        <p className="text-muted-foreground">{users.length} registered users</p>
      </div>

      <Card>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b bg-muted/50">
                <th className="px-4 py-3 text-left font-medium text-muted-foreground">Name</th>
                <th className="px-4 py-3 text-left font-medium text-muted-foreground">Email</th>
                <th className="px-4 py-3 text-left font-medium text-muted-foreground">Joined</th>
              </tr>
            </thead>
            <tbody>
              {users.map(user => (
                <tr key={user.id} className="border-b last:border-0 hover:bg-muted/25">
                  <td className="px-4 py-3 font-medium">{user.fullName}</td>
                  <td className="px-4 py-3 text-muted-foreground">{user.email}</td>
                  <td className="px-4 py-3 text-muted-foreground">{formatDate(user.createdAt)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Card>
    </div>
  )
}
""";

    private static string HomePage(ProjectOptions opts) => $$"""
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'

interface Props {
  items: Array<{ id: number; name: string; description?: string; createdAt: string }>
}

export default function Home({ items }: Props) {
  function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  }

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold">{{opts.ProjectName}}</h1>
        <p className="text-muted-foreground">Items from the database</p>
      </div>

      <div className="space-y-3">
        {items.map(item => (
          <Card key={item.id} className="flex items-center justify-between p-4">
            <div>
              <p className="font-medium">{item.name}</p>
              {item.description && (
                <p className="text-sm text-muted-foreground">{item.description}</p>
              )}
            </div>
            <Badge variant="outline">{formatDate(item.createdAt)}</Badge>
          </Card>
        ))}
      </div>

      {items.length === 0 && (
        <p className="mt-8 text-center text-muted-foreground">
          No items yet. Add some to the database!
        </p>
      )}
    </div>
  )
}
""";

    private static string ForbiddenPage() => """
import { Link } from '@inertiajs/react'
import { Button } from '@/components/ui/button'

export default function Forbidden() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center text-center">
      <p className="text-6xl font-bold text-muted-foreground/20">403</p>
      <h1 className="mt-4 text-2xl font-bold">Access Denied</h1>
      <p className="mt-2 text-muted-foreground">You don't have permission to view this page.</p>
      <Button asChild className="mt-6">
        <Link href="/dashboard">Go to Dashboard</Link>
      </Button>
    </div>
  )
}
""";

    private static string ServerErrorPage() => """
import { Link } from '@inertiajs/react'
import { Button } from '@/components/ui/button'

export default function ServerError() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center text-center">
      <p className="text-6xl font-bold text-muted-foreground/20">500</p>
      <h1 className="mt-4 text-2xl font-bold">Server Error</h1>
      <p className="mt-2 text-muted-foreground">Something went wrong on our end.</p>
      <Button asChild className="mt-6">
        <Link href="/">Go Home</Link>
      </Button>
    </div>
  )
}
""";
}
