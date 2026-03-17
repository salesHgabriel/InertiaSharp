using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Views;

public static class SvelteGenerator
{
    public static Dictionary<string, string> Generate(ProjectOptions opts)
    {
        var files = new Dictionary<string, string>
        {
            ["ClientApp/package.json"]         = PackageJson(opts),
            ["ClientApp/vite.config.ts"]       = ViteConfig(opts),
            ["ClientApp/tsconfig.json"]        = TsConfig(),
            ["ClientApp/tailwind.config.js"]   = TailwindConfig(),
            ["ClientApp/postcss.config.js"]    = PostCssConfig(),
            ["ClientApp/components.json"]      = ComponentsJson(),
            ["ClientApp/src/app.ts"]           = AppTs(opts),
            ["ClientApp/src/assets/app.css"]   = AppCss(),
            ["ClientApp/src/lib/utils.ts"]     = UtilsTs(),

            // shadcn-svelte UI components
            ["ClientApp/src/lib/components/ui/button/index.ts"]      = ButtonIndex(),
            ["ClientApp/src/lib/components/ui/button/button.svelte"]  = ButtonSvelte(),
            ["ClientApp/src/lib/components/ui/card/index.ts"]         = CardIndex(),
            ["ClientApp/src/lib/components/ui/card/card.svelte"]      = CardSvelte(),
            ["ClientApp/src/lib/components/ui/input/index.ts"]        = InputIndex(),
            ["ClientApp/src/lib/components/ui/input/input.svelte"]    = InputSvelte(),
            ["ClientApp/src/lib/components/ui/label/index.ts"]        = LabelIndex(),
            ["ClientApp/src/lib/components/ui/label/label.svelte"]    = LabelSvelte(),
            ["ClientApp/src/lib/components/ui/badge/index.ts"]        = BadgeIndex(),
            ["ClientApp/src/lib/components/ui/badge/badge.svelte"]    = BadgeSvelte(),
            ["ClientApp/src/lib/components/ui/separator/index.ts"]    = SeparatorIndex(),
            ["ClientApp/src/lib/components/ui/separator/separator.svelte"] = SeparatorSvelte(),
        };

        if (opts.IncludeAuth)
        {
            files["ClientApp/src/layouts/AppLayout.svelte"]          = AppLayout(opts);
            files["ClientApp/src/layouts/GuestLayout.svelte"]        = GuestLayout(opts);
            files["ClientApp/src/Pages/Auth/Login.svelte"]           = LoginPage(opts);
            files["ClientApp/src/Pages/Auth/Register.svelte"]        = RegisterPage(opts);
            files["ClientApp/src/Pages/Dashboard.svelte"]            = DashboardPage(opts);
            files["ClientApp/src/Pages/Profile/Edit.svelte"]         = ProfileEditPage(opts);
            files["ClientApp/src/Pages/Admin/Users.svelte"]          = AdminUsersPage(opts);
            files["ClientApp/src/Pages/Errors/Forbidden.svelte"]     = ForbiddenPage();
            files["ClientApp/src/Pages/Errors/ServerError.svelte"]   = ServerErrorPage();
        }
        else
        {
            files["ClientApp/src/layouts/AppLayout.svelte"] = SimpleAppLayout(opts);
            files["ClientApp/src/Pages/Home.svelte"]        = HomePage(opts);
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
    "build": "vite build",
    "preview": "vite preview"
  },
  "dependencies": {
    "@inertiajs/svelte": "^2.0.0",
    "bits-ui": "^1.3.4",
    "clsx": "^2.1.1",
    "lucide-svelte": "^0.468.0",
    "svelte": "^5.0.0",
    "tailwind-merge": "^2.6.1",
    "tailwindcss-animate": "^1.0.7"
  },
  "devDependencies": {
    "@sveltejs/vite-plugin-svelte": "^5.0.0",
    "@types/node": "^22.0.0",
    "autoprefixer": "^10.4.20",
    "postcss": "^8.5.1",
    "svelte-check": "^4.0.0",
    "tailwindcss": "^3.4.17",
    "typescript": "^5.7.2",
    "vite": "^6.0.6"
  }
}
""";

    private static string ViteConfig(ProjectOptions opts) => $$"""
import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import { svelte } from '@sveltejs/vite-plugin-svelte'
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
  plugins: [svelte()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
      '$lib': fileURLToPath(new URL('./src/lib', import.meta.url)),
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
      input: path.resolve(__dirname, 'src/app.ts'),
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
  "compilerOptions": {
    "target": "ES2022",
    "useDefineForClassFields": true,
    "module": "ESNext",
    "lib": ["ES2022", "DOM", "DOM.Iterable"],
    "skipLibCheck": true,
    "moduleResolution": "Bundler",
    "allowImportingTsExtensions": true,
    "isolatedModules": true,
    "noEmit": true,
    "strict": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"],
      "$lib/*": ["./src/lib/*"]
    }
  },
  "include": ["src/**/*.ts", "src/**/*.svelte"]
}
""";

    private static string TailwindConfig() => """
/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,svelte}'],
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
  "$schema": "https://shadcn-svelte.com/schema.json",
  "style": "default",
  "tailwind": {
    "config": "tailwind.config.js",
    "css": "src/assets/app.css",
    "baseColor": "slate",
    "cssVariables": true
  },
  "aliases": {
    "components": "$lib/components",
    "utils": "$lib/utils"
  }
}
""";

    private static string AppTs(ProjectOptions opts) => $$"""
import './assets/app.css'
import { mount } from 'svelte'
import { createInertiaApp } from '@inertiajs/svelte'
import AppLayout from './layouts/AppLayout.svelte'

createInertiaApp({
  resolve: (name: string) => {
    const pages = import.meta.glob('./Pages/**/*.svelte', { eager: true }) as Record<string, { default: object }>
    const page = pages[`./Pages/${name}.svelte`]

    if (!page)
      throw new Error(`Inertia page not found: ./Pages/${name}.svelte`)

    return { default: page.default, layout: (page as any).layout ?? AppLayout }
  },

  title: (title) => (title ? `${title} – {{opts.ProjectName}}` : '{{opts.ProjectName}}'),

  setup({ el, App, props }) {
    mount(App, { target: el, props })
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

    // ── shadcn-svelte UI components ───────────────────────────────────────────

    private static string ButtonIndex() => """
export { default as Button } from './button.svelte'
""";

    private static string ButtonSvelte() => """
<script lang="ts">
  import type { HTMLButtonAttributes } from 'svelte/elements'
  import { cn } from '$lib/utils'

  interface Props extends HTMLButtonAttributes {
    variant?: 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost' | 'link'
    size?: 'default' | 'sm' | 'lg' | 'icon'
  }

  let { variant = 'default', size = 'default', class: className, children, ...restProps }: Props = $props()

  const variants: Record<string, string> = {
    default: 'bg-primary text-primary-foreground hover:bg-primary/90',
    destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
    outline: 'border border-input bg-background hover:bg-accent hover:text-accent-foreground',
    secondary: 'bg-secondary text-secondary-foreground hover:bg-secondary/80',
    ghost: 'hover:bg-accent hover:text-accent-foreground',
    link: 'text-primary underline-offset-4 hover:underline',
  }

  const sizes: Record<string, string> = {
    default: 'h-10 px-4 py-2',
    sm: 'h-9 rounded-md px-3',
    lg: 'h-11 rounded-md px-8',
    icon: 'h-10 w-10',
  }
</script>

<button
  class={cn(
    'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
    variants[variant],
    sizes[size],
    className
  )}
  {...restProps}
>
  {@render children?.()}
</button>
""";

    private static string CardIndex() => """
export { default as Card } from './card.svelte'
""";

    private static string CardSvelte() => """
<script lang="ts">
  import { cn } from '$lib/utils'
  let { class: className, children, ...restProps } = $props()
</script>

<div class={cn('rounded-lg border bg-card text-card-foreground shadow-sm', className)} {...restProps}>
  {@render children?.()}
</div>
""";

    private static string InputIndex() => """
export { default as Input } from './input.svelte'
""";

    private static string InputSvelte() => """
<script lang="ts">
  import type { HTMLInputAttributes } from 'svelte/elements'
  import { cn } from '$lib/utils'
  let { class: className, value = $bindable(''), ...restProps }: HTMLInputAttributes = $props()
</script>

<input
  bind:value
  class={cn(
    'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
    className
  )}
  {...restProps}
/>
""";

    private static string LabelIndex() => """
export { default as Label } from './label.svelte'
""";

    private static string LabelSvelte() => """
<script lang="ts">
  import type { HTMLLabelAttributes } from 'svelte/elements'
  import { cn } from '$lib/utils'
  let { class: className, children, ...restProps }: HTMLLabelAttributes = $props()
</script>

<label
  class={cn('text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70', className)}
  {...restProps}
>
  {@render children?.()}
</label>
""";

    private static string BadgeIndex() => """
export { default as Badge } from './badge.svelte'
""";

    private static string BadgeSvelte() => """
<script lang="ts">
  import { cn } from '$lib/utils'
  interface Props {
    variant?: 'default' | 'secondary' | 'destructive' | 'outline'
    class?: string
    children?: any
  }
  let { variant = 'default', class: className, children }: Props = $props()

  const variants: Record<string, string> = {
    default: 'border-transparent bg-primary text-primary-foreground hover:bg-primary/80',
    secondary: 'border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80',
    destructive: 'border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80',
    outline: 'text-foreground',
  }
</script>

<div class={cn('inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors', variants[variant], className)}>
  {@render children?.()}
</div>
""";

    private static string SeparatorIndex() => """
export { default as Separator } from './separator.svelte'
""";

    private static string SeparatorSvelte() => """
<script lang="ts">
  import { cn } from '$lib/utils'
  let { orientation = 'horizontal', class: className } = $props()
</script>

<div
  role="separator"
  class={cn(
    'shrink-0 bg-border',
    orientation === 'horizontal' ? 'h-[1px] w-full' : 'h-full w-[1px]',
    className
  )}
/>
""";

    // ── Layouts ──────────────────────────────────────────────────────────────

    private static string AppLayout(ProjectOptions opts) => $$"""
<script lang="ts">
  import { router, usePage } from '@inertiajs/svelte'
  import { LayoutDashboard, LogOut, Shield, User } from 'lucide-svelte'
  import { Button } from '$lib/components/ui/button'
  import { Separator } from '$lib/components/ui/separator'

  let { children } = $props()

  const page = usePage()
  const auth = $derived((page.props as any)?.auth)
  const user = $derived(auth?.user)
  const permissions = $derived(auth?.permissions)
  const initials = $derived(user?.fullName?.split(' ').map((n: string) => n[0]).join('').toUpperCase() ?? '?')

  function logout() {
    const token = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''
    router.post('/logout', {}, { headers: { 'X-CSRF-TOKEN': token } })
  }
</script>

<div class="flex h-screen overflow-hidden bg-background">
  <!-- Sidebar -->
  <aside class="flex w-64 flex-col border-r bg-card">
    <div class="flex h-16 items-center gap-2 border-b px-6">
      <span class="text-lg font-bold tracking-tight">{{opts.ProjectName}}</span>
    </div>

    <nav class="flex flex-1 flex-col gap-1 p-3 text-sm">
      <a
        href="/dashboard"
        class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
      >
        <LayoutDashboard class="h-4 w-4" />
        Dashboard
      </a>

      <a
        href="/profile"
        class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
      >
        <User class="h-4 w-4" />
        Profile
      </a>

      {#if permissions?.canManageUsers}
        <a
          href="/admin/users"
          class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <Shield class="h-4 w-4" />
          Users
        </a>
      {/if}
    </nav>

    <Separator />

    <div class="flex items-center gap-3 p-4">
      <div class="relative flex h-9 w-9 shrink-0 overflow-hidden rounded-full bg-muted">
        {#if user?.hasAvatar}
          <img src="/{user.avatarPathFile}" alt={user.fullName} class="h-full w-full object-cover" />
        {:else}
          <span class="flex h-full w-full items-center justify-center text-sm font-medium uppercase">
            {initials}
          </span>
        {/if}
      </div>
      <div class="flex min-w-0 flex-1 flex-col">
        <span class="truncate text-sm font-medium">{user?.fullName}</span>
        <span class="truncate text-xs text-muted-foreground">{user?.email}</span>
      </div>
      <Button variant="ghost" size="icon" class="h-8 w-8 shrink-0" onclick={logout}>
        <LogOut class="h-4 w-4" />
      </Button>
    </div>
  </aside>

  <!-- Main content -->
  <main class="flex flex-1 flex-col overflow-auto">
    <div class="flex-1 p-8">
      {@render children()}
    </div>
  </main>
</div>
""";

    private static string GuestLayout(ProjectOptions opts) => $$"""
<script lang="ts">
  let { children } = $props()
</script>

<div class="flex min-h-screen items-center justify-center bg-muted/40 p-4">
  <div class="w-full max-w-sm">
    <div class="mb-8 text-center">
      <h1 class="text-2xl font-bold tracking-tight">{{opts.ProjectName}}</h1>
      <p class="mt-1 text-sm text-muted-foreground">Welcome back</p>
    </div>
    {@render children()}
  </div>
</div>
""";

    private static string SimpleAppLayout(ProjectOptions opts) => $$"""
<script lang="ts">
  let { children } = $props()
</script>

<div class="min-h-screen bg-background">
  <header class="border-b">
    <div class="mx-auto flex h-14 max-w-5xl items-center px-4">
      <span class="text-lg font-semibold">{{opts.ProjectName}}</span>
    </div>
  </header>
  <main class="mx-auto max-w-5xl p-6">
    {@render children()}
  </main>
</div>
""";

    // ── Pages ─────────────────────────────────────────────────────────────────

    private static string LoginPage(ProjectOptions opts) => $$"""
<script module>
  import GuestLayout from '@/layouts/GuestLayout.svelte'
  export const layout = GuestLayout
</script>

<script lang="ts">
  import { router } from '@inertiajs/svelte'
  import { Button } from '$lib/components/ui/button'
  import { Card } from '$lib/components/ui/card'
  import { Input } from '$lib/components/ui/input'
  import { Label } from '$lib/components/ui/label'

  let { errors = {} }: { errors?: Record<string, string> } = $props()

  let email = $state('')
  let password = $state('')
  let processing = $state(false)

  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

  function submit(e: Event) {
    e.preventDefault()
    processing = true
    router.post('/login', { email, password }, {
      headers: { 'X-CSRF-TOKEN': csrf },
      preserveState: true,
      onFinish: () => { processing = false },
    })
  }
</script>

<Card class="p-6">
  <h2 class="mb-6 text-xl font-semibold">Sign in to your account</h2>

  <form onsubmit={submit} class="space-y-4">
    <div class="space-y-2">
      <Label for="email">Email</Label>
      <Input id="email" type="email" bind:value={email} placeholder="admin@demo.com" autocomplete="email" />
      {#if errors.email}
        <p class="text-sm text-destructive">{errors.email}</p>
      {/if}
    </div>

    <div class="space-y-2">
      <Label for="password">Password</Label>
      <Input id="password" type="password" bind:value={password} placeholder="••••••••" autocomplete="current-password" />
    </div>

    <Button type="submit" class="w-full" disabled={processing}>
      {processing ? 'Signing in…' : 'Sign in'}
    </Button>
  </form>

  <p class="mt-4 text-center text-sm text-muted-foreground">
    Don't have an account?
    <a href="/register" class="font-medium text-primary hover:underline">Register</a>
  </p>

  <p class="mt-3 text-center text-xs text-muted-foreground">
    Demo: <code>admin@demo.com</code> / <code>Password123!</code>
  </p>
</Card>
""";

    private static string RegisterPage(ProjectOptions opts) => $$"""
<script module>
  import GuestLayout from '@/layouts/GuestLayout.svelte'
  export const layout = GuestLayout
</script>

<script lang="ts">
  import { router } from '@inertiajs/svelte'
  import { Button } from '$lib/components/ui/button'
  import { Card } from '$lib/components/ui/card'
  import { Input } from '$lib/components/ui/input'
  import { Label } from '$lib/components/ui/label'

  let { errors = {} }: { errors?: Record<string, string> } = $props()

  let firstName = $state('')
  let lastName  = $state('')
  let email     = $state('')
  let password  = $state('')
  let passwordConfirmation = $state('')
  let processing = $state(false)

  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

  function submit(e: Event) {
    e.preventDefault()
    processing = true
    router.post('/register', { firstName, lastName, email, password, passwordConfirmation }, {
      headers: { 'X-CSRF-TOKEN': csrf },
      preserveState: true,
      onFinish: () => { processing = false },
    })
  }
</script>

<Card class="p-6">
  <h2 class="mb-6 text-xl font-semibold">Create an account</h2>

  <form onsubmit={submit} class="space-y-4">
    <div class="grid grid-cols-2 gap-4">
      <div class="space-y-2">
        <Label for="firstName">First name</Label>
        <Input id="firstName" bind:value={firstName} placeholder="John" />
      </div>
      <div class="space-y-2">
        <Label for="lastName">Last name</Label>
        <Input id="lastName" bind:value={lastName} placeholder="Doe" />
      </div>
    </div>

    <div class="space-y-2">
      <Label for="email">Email</Label>
      <Input id="email" type="email" bind:value={email} placeholder="john@example.com" />
      {#if errors.email}
        <p class="text-sm text-destructive">{errors.email}</p>
      {/if}
    </div>

    <div class="space-y-2">
      <Label for="password">Password</Label>
      <Input id="password" type="password" bind:value={password} placeholder="Min. 8 characters" />
    </div>

    <div class="space-y-2">
      <Label for="passwordConfirmation">Confirm password</Label>
      <Input id="passwordConfirmation" type="password" bind:value={passwordConfirmation} placeholder="••••••••" />
      {#if errors.passwordConfirmation}
        <p class="text-sm text-destructive">{errors.passwordConfirmation}</p>
      {/if}
    </div>

    <Button type="submit" class="w-full" disabled={processing}>
      {processing ? 'Creating account…' : 'Create account'}
    </Button>
  </form>

  <p class="mt-4 text-center text-sm text-muted-foreground">
    Already have an account?
    <a href="/login" class="font-medium text-primary hover:underline">Sign in</a>
  </p>
</Card>
""";

    private static string DashboardPage(ProjectOptions opts) => $$"""
<script lang="ts">
  import { Activity, CheckSquare, Users } from 'lucide-svelte'
  import { Badge } from '$lib/components/ui/badge'
  import { Card } from '$lib/components/ui/card'

  let { user, stats, permissions }: {
    user: { id: string; fullName: string; email: string; createdAt: string; roles: string[] }
    stats: { totalUsers: number; activeToday: number; pendingTasks: number }
    permissions: { canEditContent: boolean; canManageUsers: boolean; canViewReports: boolean }
  } = $props()
</script>

<div>
  <div class="mb-6">
    <h1 class="text-2xl font-bold">Dashboard</h1>
    <p class="text-muted-foreground">Welcome back, {user.fullName}</p>
  </div>

  <div class="mb-6 grid gap-4 sm:grid-cols-3">
    <Card class="p-4">
      <div class="flex items-center gap-3">
        <div class="rounded-md bg-primary/10 p-2">
          <Users class="h-5 w-5 text-primary" />
        </div>
        <div>
          <p class="text-sm text-muted-foreground">Total Users</p>
          <p class="text-2xl font-bold">{stats.totalUsers}</p>
        </div>
      </div>
    </Card>

    <Card class="p-4">
      <div class="flex items-center gap-3">
        <div class="rounded-md bg-green-500/10 p-2">
          <Activity class="h-5 w-5 text-green-500" />
        </div>
        <div>
          <p class="text-sm text-muted-foreground">Active Today</p>
          <p class="text-2xl font-bold">{stats.activeToday}</p>
        </div>
      </div>
    </Card>

    <Card class="p-4">
      <div class="flex items-center gap-3">
        <div class="rounded-md bg-orange-500/10 p-2">
          <CheckSquare class="h-5 w-5 text-orange-500" />
        </div>
        <div>
          <p class="text-sm text-muted-foreground">Pending Tasks</p>
          <p class="text-2xl font-bold">{stats.pendingTasks}</p>
        </div>
      </div>
    </Card>
  </div>

  <Card class="p-6">
    <h2 class="mb-4 font-semibold">Your Account</h2>
    <dl class="space-y-3 text-sm">
      <div class="flex justify-between">
        <dt class="text-muted-foreground">Name</dt>
        <dd class="font-medium">{user.fullName}</dd>
      </div>
      <div class="flex justify-between">
        <dt class="text-muted-foreground">Email</dt>
        <dd class="font-medium">{user.email}</dd>
      </div>
      <div class="flex justify-between">
        <dt class="text-muted-foreground">Roles</dt>
        <dd class="flex gap-1">
          {#each user.roles as role}
            <Badge variant="secondary">{role}</Badge>
          {/each}
        </dd>
      </div>
    </dl>
  </Card>

  {#if permissions.canEditContent}
    <div class="mt-4">
      <Card class="border-green-200 bg-green-50 p-4 dark:border-green-900 dark:bg-green-950/20">
        <p class="text-sm font-medium text-green-800 dark:text-green-400">
          You can edit content (Admin / Editor role)
        </p>
      </Card>
    </div>
  {/if}

  {#if permissions.canManageUsers}
    <div class="mt-4">
      <Card class="border-blue-200 bg-blue-50 p-4 dark:border-blue-900 dark:bg-blue-950/20">
        <p class="text-sm font-medium text-blue-800 dark:text-blue-400">
          You can manage users (Admin role) —
          <a href="/admin/users" class="underline">View all users</a>
        </p>
      </Card>
    </div>
  {/if}
</div>
""";

    private static string ProfileEditPage(ProjectOptions opts) => $$"""
<script lang="ts">
  import { router } from '@inertiajs/svelte'
  import { Button } from '$lib/components/ui/button'
  import { Card } from '$lib/components/ui/card'
  import { Input } from '$lib/components/ui/input'
  import { Label } from '$lib/components/ui/label'
  import { Separator } from '$lib/components/ui/separator'

  let { user, flash, errors = {} }: {
    user: { firstName: string; lastName: string; email: string; bio?: string; phoneNumber?: string; avatarPathFile?: string; hasAvatar: boolean }
    flash?: string
    errors?: Record<string, string>
  } = $props()

  const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

  let firstName   = $state(user.firstName)
  let lastName    = $state(user.lastName)
  let email       = $state(user.email)
  let bio         = $state(user.bio ?? '')
  let phoneNumber = $state(user.phoneNumber ?? '')

  let currentPassword        = $state('')
  let newPassword            = $state('')
  let newPasswordConfirmation = $state('')
  let fileInput: HTMLInputElement

  function updateProfile(e: Event) {
    e.preventDefault()
    router.post('/profile', { firstName, lastName, email, bio, phoneNumber }, {
      headers: { 'X-CSRF-TOKEN': csrf }
    })
  }

  function changePassword(e: Event) {
    e.preventDefault()
    router.post('/profile/password', { currentPassword, newPassword, newPasswordConfirmation }, {
      headers: { 'X-CSRF-TOKEN': csrf }
    })
  }

  function submitAvatar(e: Event) {
    const file = (e.target as HTMLInputElement).files?.[0]
    if (!file) return
    const data = new FormData()
    data.append('avatar', file)
    fetch('/profile/avatar', { method: 'POST', body: data })
      .then(() => window.location.reload())
  }
</script>

<div class="max-w-2xl space-y-8">
  <div>
    <h1 class="text-2xl font-bold">Profile</h1>
    <p class="text-muted-foreground">Manage your account settings</p>
  </div>

  {#if flash}
    <div class="rounded-lg border border-green-200 bg-green-50 p-4 text-sm font-medium text-green-800 dark:border-green-900 dark:bg-green-950/20 dark:text-green-400">
      {flash}
    </div>
  {/if}

  <Card class="p-6">
    <h2 class="mb-4 font-semibold">Avatar</h2>
    <div class="flex items-center gap-4">
      <div class="relative flex h-16 w-16 shrink-0 overflow-hidden rounded-full bg-muted">
        {#if user.hasAvatar}
          <img src="/{user.avatarPathFile}" alt={user.firstName} class="h-full w-full object-cover" />
        {:else}
          <span class="flex h-full w-full items-center justify-center text-lg font-medium uppercase">
            {user.firstName[0]}{user.lastName[0]}
          </span>
        {/if}
      </div>
      <div>
        <input bind:this={fileInput} type="file" accept="image/*" class="hidden" onchange={submitAvatar} />
        <Button variant="outline" size="sm" onclick={() => fileInput.click()}>Change photo</Button>
      </div>
    </div>
  </Card>

  <Card class="p-6">
    <h2 class="mb-4 font-semibold">Personal information</h2>
    <form onsubmit={updateProfile} class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div class="space-y-2">
          <Label for="firstName">First name</Label>
          <Input id="firstName" bind:value={firstName} />
        </div>
        <div class="space-y-2">
          <Label for="lastName">Last name</Label>
          <Input id="lastName" bind:value={lastName} />
        </div>
      </div>

      <div class="space-y-2">
        <Label for="email">Email</Label>
        <Input id="email" type="email" bind:value={email} />
        {#if errors.email}
          <p class="text-sm text-destructive">{errors.email}</p>
        {/if}
      </div>

      <div class="space-y-2">
        <Label for="bio">Bio</Label>
        <Input id="bio" bind:value={bio} placeholder="Tell us about yourself" />
      </div>

      <div class="space-y-2">
        <Label for="phone">Phone</Label>
        <Input id="phone" type="tel" bind:value={phoneNumber} placeholder="+1 (555) 000-0000" />
      </div>

      <Button type="submit">Save changes</Button>
    </form>
  </Card>

  <Separator />

  <Card class="p-6">
    <h2 class="mb-4 font-semibold">Change password</h2>
    <form onsubmit={changePassword} class="space-y-4">
      <div class="space-y-2">
        <Label for="currentPassword">Current password</Label>
        <Input id="currentPassword" type="password" bind:value={currentPassword} />
      </div>
      <div class="space-y-2">
        <Label for="newPassword">New password</Label>
        <Input id="newPassword" type="password" bind:value={newPassword} />
      </div>
      <div class="space-y-2">
        <Label for="newPasswordConfirmation">Confirm new password</Label>
        <Input id="newPasswordConfirmation" type="password" bind:value={newPasswordConfirmation} />
        {#if errors.newPasswordConfirmation}
          <p class="text-sm text-destructive">{errors.newPasswordConfirmation}</p>
        {/if}
      </div>
      <Button type="submit" variant="outline">Update password</Button>
    </form>
  </Card>
</div>
""";

    private static string AdminUsersPage(ProjectOptions opts) => $$"""
<script lang="ts">
  import { Card } from '$lib/components/ui/card'

  let { users }: {
    users: Array<{ id: string; fullName: string; email: string; createdAt: string }>
  } = $props()

  function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  }
</script>

<div>
  <div class="mb-6">
    <h1 class="text-2xl font-bold">Users</h1>
    <p class="text-muted-foreground">{users.length} registered users</p>
  </div>

  <Card>
    <div class="overflow-x-auto">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b bg-muted/50">
            <th class="px-4 py-3 text-left font-medium text-muted-foreground">Name</th>
            <th class="px-4 py-3 text-left font-medium text-muted-foreground">Email</th>
            <th class="px-4 py-3 text-left font-medium text-muted-foreground">Joined</th>
          </tr>
        </thead>
        <tbody>
          {#each users as user}
            <tr class="border-b last:border-0 hover:bg-muted/25">
              <td class="px-4 py-3 font-medium">{user.fullName}</td>
              <td class="px-4 py-3 text-muted-foreground">{user.email}</td>
              <td class="px-4 py-3 text-muted-foreground">{formatDate(user.createdAt)}</td>
            </tr>
          {/each}
        </tbody>
      </table>
    </div>
  </Card>
</div>
""";

    private static string HomePage(ProjectOptions opts) => $$"""
<script lang="ts">
  import { Badge } from '$lib/components/ui/badge'
  import { Card } from '$lib/components/ui/card'

  let { items }: {
    items: Array<{ id: number; name: string; description?: string; createdAt: string }>
  } = $props()

  function formatDate(date: string) {
    return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  }
</script>

<div>
  <div class="mb-6">
    <h1 class="text-2xl font-bold">{{opts.ProjectName}}</h1>
    <p class="text-muted-foreground">Items from the database</p>
  </div>

  <div class="space-y-3">
    {#each items as item}
      <Card class="flex items-center justify-between p-4">
        <div>
          <p class="font-medium">{item.name}</p>
          {#if item.description}
            <p class="text-sm text-muted-foreground">{item.description}</p>
          {/if}
        </div>
        <Badge variant="outline">{formatDate(item.createdAt)}</Badge>
      </Card>
    {/each}
  </div>

  {#if items.length === 0}
    <p class="mt-8 text-center text-muted-foreground">
      No items yet. Add some to the database!
    </p>
  {/if}
</div>
""";

    private static string ForbiddenPage() => """
<script lang="ts">
  import { Button } from '$lib/components/ui/button'
</script>

<div class="flex min-h-screen flex-col items-center justify-center text-center">
  <p class="text-6xl font-bold text-muted-foreground/20">403</p>
  <h1 class="mt-4 text-2xl font-bold">Access Denied</h1>
  <p class="mt-2 text-muted-foreground">You don't have permission to view this page.</p>
  <Button class="mt-6" onclick={() => window.location.href = '/dashboard'}>
    Go to Dashboard
  </Button>
</div>
""";

    private static string ServerErrorPage() => """
<script lang="ts">
  import { Button } from '$lib/components/ui/button'
</script>

<div class="flex min-h-screen flex-col items-center justify-center text-center">
  <p class="text-6xl font-bold text-muted-foreground/20">500</p>
  <h1 class="mt-4 text-2xl font-bold">Server Error</h1>
  <p class="mt-2 text-muted-foreground">Something went wrong on our end.</p>
  <Button class="mt-6" onclick={() => window.location.href = '/'}>
    Go Home
  </Button>
</div>
""";
}
