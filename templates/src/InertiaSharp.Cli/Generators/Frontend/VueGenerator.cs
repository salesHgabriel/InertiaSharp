using InertiaSharp.Cli.Models;

namespace InertiaSharp.Cli.Generators.Views;

public static class VueGenerator
{
    public static Dictionary<string, string> Generate(ProjectOptions opts)
    {
        var files = new Dictionary<string, string>
        {
            ["ClientApp/package.json"]          = PackageJson(opts),
            ["ClientApp/vite.config.ts"]        = ViteConfig(opts),
            ["ClientApp/tsconfig.json"]         = TsConfig(),
            ["ClientApp/tailwind.config.js"]    = TailwindConfig(),
            ["ClientApp/postcss.config.js"]     = PostCssConfig(),
            ["ClientApp/components.json"]       = ComponentsJson(),
            ["ClientApp/tsconfig.node.json"]     = TsConfigNode(),
            ["ClientApp/src/app.ts"]            = AppTs(opts),
            ["ClientApp/src/assets/app.css"]    = AppCss(),
            ["ClientApp/src/lib/utils.ts"]      = UtilsTs(),

            // UI primitives (reka-ui / shadcn-vue pattern)
            ["ClientApp/src/components/ui/button/index.ts"]          = ButtonIndex(),
            ["ClientApp/src/components/ui/button/Button.vue"]        = ButtonVue(),
            ["ClientApp/src/components/ui/card/index.ts"]            = CardIndex(),
            ["ClientApp/src/components/ui/card/Card.vue"]            = CardVue(),
            ["ClientApp/src/components/ui/input/index.ts"]           = InputIndex(),
            ["ClientApp/src/components/ui/input/Input.vue"]          = InputVue(),
            ["ClientApp/src/components/ui/label/index.ts"]           = LabelIndex(),
            ["ClientApp/src/components/ui/label/Label.vue"]          = LabelVue(),
            ["ClientApp/src/components/ui/badge/index.ts"]           = BadgeIndex(),
            ["ClientApp/src/components/ui/badge/Badge.vue"]          = BadgeVue(),
            ["ClientApp/src/components/ui/alert/index.ts"]           = AlertIndex(),
            ["ClientApp/src/components/ui/alert/Alert.vue"]          = AlertVue(),
            ["ClientApp/src/components/ui/separator/index.ts"]       = SeparatorIndex(),
            ["ClientApp/src/components/ui/separator/Separator.vue"]  = SeparatorVue(),
            ["ClientApp/src/components/ui/avatar/index.ts"]          = AvatarIndex(),
            ["ClientApp/src/components/ui/avatar/Avatar.vue"]        = AvatarVue(),
        };

        if (opts.IncludeAuth)
        {
            files["ClientApp/src/Layouts/AppLayout.vue"]    = AppLayout(opts);
            files["ClientApp/src/Layouts/GuestLayout.vue"]  = GuestLayout(opts);
            files["ClientApp/src/Pages/Auth/Login.vue"]     = LoginPage(opts);
            files["ClientApp/src/Pages/Auth/Register.vue"]  = RegisterPage(opts);
            files["ClientApp/src/Pages/Dashboard.vue"]      = DashboardPage(opts);
            files["ClientApp/src/Pages/Profile/Edit.vue"]   = ProfileEditPage(opts);
            files["ClientApp/src/Pages/Admin/Users.vue"]    = AdminUsersPage(opts);
            files["ClientApp/src/Pages/Errors/Forbidden.vue"]    = ForbiddenPage();
            files["ClientApp/src/Pages/Errors/ServerError.vue"]  = ServerErrorPage();
        }
        else
        {
            files["ClientApp/src/Layouts/AppLayout.vue"]  = SimpleAppLayout(opts);
            files["ClientApp/src/Pages/Home.vue"]         = HomePage(opts);
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
    "build": "vue-tsc --noEmit && vite build",
    "preview": "vite preview",
    "ui:add": "npx shadcn-vue@latest add"
  },
  "dependencies": {
    "@inertiajs/vue3": "^2.0.0",
    "@vueuse/core": "^12.0.0",
    "class-variance-authority": "^0.7.1",
    "clsx": "^2.1.1",
    "lucide-vue-next": "^0.468.0",
    "reka-ui": "^2.2.0",
    "tailwind-merge": "^2.6.1",
    "tailwindcss-animate": "^1.0.7",
    "vue": "^3.5.13"
  },
  "devDependencies": {
    "@types/node": "^22.0.0",
    "@vitejs/plugin-vue": "^5.2.1",
    "autoprefixer": "^10.4.20",
    "postcss": "^8.5.1",
    "tailwindcss": "^3.4.17",
    "typescript": "^5.7.2",
    "vite": "^6.0.6",
    "vue-tsc": "^2.2.0"
  }
}
""";

    private static string ViteConfig(ProjectOptions opts) => $$"""
import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'
import vue from '@vitejs/plugin-vue'
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
  plugins: [vue()],
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
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "preserve",
    "strict": true,
    "noUnusedLocals": false,
    "noUnusedParameters": false,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src/**/*.ts", "src/**/*.d.ts", "src/**/*.tsx", "src/**/*.vue"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
""";

    private static string TsConfigNode() => """
{
  "compilerOptions": {
    "composite": true,
    "skipLibCheck": true,
    "module": "ESNext",
    "moduleResolution": "Bundler",
    "allowSyntheticDefaultImports": true
  },
  "include": ["vite.config.ts"]
}
""";

    private static string TailwindConfig() => """
/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{ts,tsx,vue}'],
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
  "$schema": "https://shadcn-vue.com/schema.json",
  "style": "default",
  "typescript": true,
  "tsConfigPath": "./tsconfig.json",
  "framework": "vite",
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

    private static string AppTs(ProjectOptions opts) => $$"""
import './assets/app.css'

import { createApp, h, type DefineComponent } from 'vue'
import { createInertiaApp } from '@inertiajs/vue3'
import AppLayout from '@/Layouts/AppLayout.vue'

createInertiaApp({
  resolve: (name: string) => {
    const pages = import.meta.glob<DefineComponent>('./Pages/**/*.vue', { eager: true })
    const page  = pages[`./Pages/${name}.vue`]

    if (!page)
      throw new Error(`Inertia page not found: ./Pages/${name}.vue`)

    if (!page.default.layout)
      page.default.layout = AppLayout

    return page
  },

  title: (title) => (title ? `${title} – {{opts.ProjectName}}` : '{{opts.ProjectName}}'),

  setup({ el, App, props, plugin }) {
    createApp({ render: () => h(App, props) })
      .use(plugin)
      .mount(el)
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
    --popover: 0 0% 100%;
    --popover-foreground: 222.2 84% 4.9%;
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

    // ── UI components ────────────────────────────────────────────────────────

    private static string ButtonIndex() => """
export { default as Button } from './Button.vue'
""";

    private static string ButtonVue() => """
<script setup lang="ts">
import { Primitive, type PrimitiveProps } from 'reka-ui'
import { type HTMLAttributes, computed } from 'vue'
import { cn } from '@/lib/utils'

const variants = {
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
}

interface Props extends PrimitiveProps {
  variant?: keyof typeof variants.variant
  size?: keyof typeof variants.size
  class?: HTMLAttributes['class']
}

const props = withDefaults(defineProps<Props>(), {
  as: 'button',
  variant: 'default',
  size: 'default',
})

const classes = computed(() =>
  cn(
    'inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-md text-sm font-medium ring-offset-background transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:pointer-events-none disabled:opacity-50',
    variants.variant[props.variant],
    variants.size[props.size],
    props.class,
  ),
)
</script>

<template>
  <Primitive :as="as" :as-child="asChild" :class="classes">
    <slot />
  </Primitive>
</template>
""";

    private static string CardIndex() => """
export { default as Card } from './Card.vue'
""";

    private static string CardVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  class?: HTMLAttributes['class']
}
const props = defineProps<Props>()
</script>

<template>
  <div :class="cn('rounded-lg border bg-card text-card-foreground shadow-sm', props.class)">
    <slot />
  </div>
</template>
""";

    private static string InputIndex() => """
export { default as Input } from './Input.vue'
""";

    private static string InputVue() => """
<script setup lang="ts">
import { type HTMLAttributes, useAttrs } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  class?: HTMLAttributes['class']
  type?: string
}

const props = withDefaults(defineProps<Props>(), { type: 'text' })
</script>

<template>
  <input
    v-bind="$attrs"
    :type="props.type"
    :class="cn(
      'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50',
      props.class
    )"
  />
</template>
""";

    private static string LabelIndex() => """
export { default as Label } from './Label.vue'
""";

    private static string LabelVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  class?: HTMLAttributes['class']
  for?: string
}
const props = defineProps<Props>()
</script>

<template>
  <label
    :for="props.for"
    :class="cn('text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70', props.class)"
  >
    <slot />
  </label>
</template>
""";

    private static string BadgeIndex() => """
export { default as Badge } from './Badge.vue'
""";

    private static string BadgeVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

type Variant = 'default' | 'secondary' | 'destructive' | 'outline'

interface Props {
  variant?: Variant
  class?: HTMLAttributes['class']
}

const variants: Record<Variant, string> = {
  default: 'border-transparent bg-primary text-primary-foreground hover:bg-primary/80',
  secondary: 'border-transparent bg-secondary text-secondary-foreground hover:bg-secondary/80',
  destructive: 'border-transparent bg-destructive text-destructive-foreground hover:bg-destructive/80',
  outline: 'text-foreground',
}

const props = withDefaults(defineProps<Props>(), { variant: 'default' })
</script>

<template>
  <div :class="cn('inline-flex items-center rounded-full border px-2.5 py-0.5 text-xs font-semibold transition-colors', variants[props.variant], props.class)">
    <slot />
  </div>
</template>
""";

    private static string AlertIndex() => """
export { default as Alert } from './Alert.vue'
""";

    private static string AlertVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  variant?: 'default' | 'destructive'
  class?: HTMLAttributes['class']
}

const variants = {
  default: 'bg-background text-foreground',
  destructive: 'border-destructive/50 text-destructive dark:border-destructive [&>svg]:text-destructive',
}

const props = withDefaults(defineProps<Props>(), { variant: 'default' })
</script>

<template>
  <div role="alert" :class="cn('relative w-full rounded-lg border p-4 [&>svg~*]:pl-7 [&>svg+div]:translate-y-[-3px] [&>svg]:absolute [&>svg]:left-4 [&>svg]:top-4 [&>svg]:text-foreground', variants[props.variant], props.class)">
    <slot />
  </div>
</template>
""";

    private static string SeparatorIndex() => """
export { default as Separator } from './Separator.vue'
""";

    private static string SeparatorVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  orientation?: 'horizontal' | 'vertical'
  class?: HTMLAttributes['class']
}
const props = withDefaults(defineProps<Props>(), { orientation: 'horizontal' })
</script>

<template>
  <div
    :class="cn(
      'shrink-0 bg-border',
      props.orientation === 'horizontal' ? 'h-[1px] w-full' : 'h-full w-[1px]',
      props.class
    )"
  />
</template>
""";

    private static string AvatarIndex() => """
export { default as Avatar } from './Avatar.vue'
""";

    private static string AvatarVue() => """
<script setup lang="ts">
import { type HTMLAttributes } from 'vue'
import { cn } from '@/lib/utils'

interface Props {
  src?: string
  alt?: string
  fallback?: string
  class?: HTMLAttributes['class']
}
const props = defineProps<Props>()
</script>

<template>
  <div :class="cn('relative flex h-10 w-10 shrink-0 overflow-hidden rounded-full', props.class)">
    <img v-if="props.src" :src="props.src" :alt="props.alt" class="aspect-square h-full w-full object-cover" />
    <div v-else class="flex h-full w-full items-center justify-center rounded-full bg-muted text-sm font-medium uppercase">
      {{ props.fallback ?? props.alt?.slice(0, 2) ?? '??' }}
    </div>
  </div>
</template>
""";

    // ── Layouts ──────────────────────────────────────────────────────────────

    private static string AppLayout(ProjectOptions opts) => """
<script setup lang="ts">
import { Link, usePage, router } from '@inertiajs/vue3'
import { computed } from 'vue'
import { LayoutDashboard, User, Shield, LogOut } from 'lucide-vue-next'
import { Avatar } from '@/components/ui/avatar'
import { Button } from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'

const page = usePage<{
  auth?: {
    user: { id: string; fullName: string; email: string; avatarPathFile?: string; hasAvatar: boolean }
    permissions: { canEditContent: boolean; canManageUsers: boolean; canViewReports: boolean }
  }
}>()

const user        = computed(() => page.props.auth?.user)
const permissions = computed(() => page.props.auth?.permissions)
const initials    = computed(() => user.value?.fullName?.split(' ').map(n => n[0]).join('').toUpperCase() ?? '?')

function logout() {
  const token = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''
  router.post('/logout', {}, { headers: { 'X-CSRF-TOKEN': token } })
}
</script>

<template>
  <div class="flex h-screen overflow-hidden bg-background">
    <!-- Sidebar -->
    <aside class="flex w-64 flex-col border-r bg-card">
      <div class="flex h-16 items-center gap-2 border-b px-6">
        <span class="text-lg font-bold tracking-tight">__PROJECT_NAME__</span>
      </div>

      <nav class="flex flex-1 flex-col gap-1 p-3 text-sm">
        <Link
          href="/dashboard"
          class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <LayoutDashboard class="h-4 w-4" />
          Dashboard
        </Link>

        <Link
          href="/profile"
          class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <User class="h-4 w-4" />
          Profile
        </Link>

        <Link
          v-if="permissions?.canManageUsers"
          href="/admin/users"
          class="flex items-center gap-3 rounded-md px-3 py-2 text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground"
        >
          <Shield class="h-4 w-4" />
          Users
        </Link>
      </nav>

      <Separator />

      <div class="flex items-center gap-3 p-4">
        <Avatar
          :src="user?.hasAvatar ? `/${user.avatarPathFile}` : undefined"
          :fallback="initials"
          :alt="user?.fullName"
        />
        <div class="flex min-w-0 flex-1 flex-col">
          <span class="truncate text-sm font-medium">{{ user?.fullName }}</span>
          <span class="truncate text-xs text-muted-foreground">{{ user?.email }}</span>
        </div>
        <Button variant="ghost" size="icon" class="h-8 w-8 shrink-0" @click="logout">
          <LogOut class="h-4 w-4" />
        </Button>
      </div>
    </aside>

    <!-- Main content -->
    <main class="flex flex-1 flex-col overflow-auto">
      <div class="flex-1 p-8">
        <slot />
      </div>
    </main>
  </div>
</template>
""".Replace("__PROJECT_NAME__", opts.ProjectName);

    private static string GuestLayout(ProjectOptions opts) => """
<template>
  <div class="flex min-h-screen items-center justify-center bg-muted/40 p-4">
    <div class="w-full max-w-sm">
      <div class="mb-8 text-center">
        <h1 class="text-2xl font-bold tracking-tight">__PROJECT_NAME__</h1>
        <p class="mt-1 text-sm text-muted-foreground">Welcome back</p>
      </div>
      <slot />
    </div>
  </div>
</template>
""".Replace("__PROJECT_NAME__", opts.ProjectName);

    private static string SimpleAppLayout(ProjectOptions opts) => """
<template>
  <div class="min-h-screen bg-background">
    <header class="border-b">
      <div class="mx-auto flex h-14 max-w-5xl items-center px-4">
        <span class="text-lg font-semibold">__PROJECT_NAME__</span>
      </div>
    </header>
    <main class="mx-auto max-w-5xl p-6">
      <slot />
    </main>
  </div>
</template>
""".Replace("__PROJECT_NAME__", opts.ProjectName);

    // ── Pages ─────────────────────────────────────────────────────────────────

    private static string LoginPage(ProjectOptions opts) => """
<script setup lang="ts">
import { useForm } from '@inertiajs/vue3'
import GuestLayout from '@/Layouts/GuestLayout.vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card } from '@/components/ui/card'

defineOptions({ layout: GuestLayout })

const props = defineProps<{
  errors?: Record<string, string>
}>()

const form = useForm({
  email: '',
  password: '',
  remember: false,
})

const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

function submit() {
  form.post('/login', {
    headers: { 'X-CSRF-TOKEN': csrf },
    preserveState: true,
  })
}
</script>

<template>
  <Card class="p-6">
    <h2 class="mb-6 text-xl font-semibold">Sign in to your account</h2>

    <form @submit.prevent="submit" class="space-y-4">
      <div class="space-y-2">
        <Label for="email">Email</Label>
        <Input id="email" v-model="form.email" type="email" placeholder="admin@demo.com" autocomplete="email" />
        <p v-if="props.errors?.email" class="text-sm text-destructive">{{ props.errors.email }}</p>
      </div>

      <div class="space-y-2">
        <Label for="password">Password</Label>
        <Input id="password" v-model="form.password" type="password" placeholder="••••••••" autocomplete="current-password" />
      </div>

      <Button type="submit" class="w-full" :disabled="form.processing">
        {{ form.processing ? 'Signing in…' : 'Sign in' }}
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
</template>
""";

    private static string RegisterPage(ProjectOptions opts) => """
<script setup lang="ts">
import { useForm } from '@inertiajs/vue3'
import GuestLayout from '@/Layouts/GuestLayout.vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card } from '@/components/ui/card'

defineOptions({ layout: GuestLayout })

const props = defineProps<{
  errors?: Record<string, string>
}>()

const form = useForm({
  firstName: '',
  lastName: '',
  email: '',
  password: '',
  passwordConfirmation: '',
})

const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

function submit() {
  form.post('/register', {
    headers: { 'X-CSRF-TOKEN': csrf },
    preserveState: true,
  })
}
</script>

<template>
  <Card class="p-6">
    <h2 class="mb-6 text-xl font-semibold">Create an account</h2>

    <form @submit.prevent="submit" class="space-y-4">
      <div class="grid grid-cols-2 gap-4">
        <div class="space-y-2">
          <Label for="firstName">First name</Label>
          <Input id="firstName" v-model="form.firstName" placeholder="John" />
          <p v-if="props.errors?.firstName" class="text-sm text-destructive">{{ props.errors.firstName }}</p>
        </div>
        <div class="space-y-2">
          <Label for="lastName">Last name</Label>
          <Input id="lastName" v-model="form.lastName" placeholder="Doe" />
        </div>
      </div>

      <div class="space-y-2">
        <Label for="email">Email</Label>
        <Input id="email" v-model="form.email" type="email" placeholder="john@example.com" />
        <p v-if="props.errors?.email" class="text-sm text-destructive">{{ props.errors.email }}</p>
      </div>

      <div class="space-y-2">
        <Label for="password">Password</Label>
        <Input id="password" v-model="form.password" type="password" placeholder="Min. 8 characters" />
      </div>

      <div class="space-y-2">
        <Label for="passwordConfirmation">Confirm password</Label>
        <Input id="passwordConfirmation" v-model="form.passwordConfirmation" type="password" placeholder="••••••••" />
        <p v-if="props.errors?.passwordConfirmation" class="text-sm text-destructive">{{ props.errors.passwordConfirmation }}</p>
      </div>

      <Button type="submit" class="w-full" :disabled="form.processing">
        {{ form.processing ? 'Creating account…' : 'Create account' }}
      </Button>
    </form>

    <p class="mt-4 text-center text-sm text-muted-foreground">
      Already have an account?
      <a href="/login" class="font-medium text-primary hover:underline">Sign in</a>
    </p>
  </Card>
</template>
""";

    private static string DashboardPage(ProjectOptions opts) => """
<script setup lang="ts">
import { Badge } from '@/components/ui/badge'
import { Card } from '@/components/ui/card'
import { Users, Activity, CheckSquare } from 'lucide-vue-next'

const props = defineProps<{
  user: {
    id: string
    fullName: string
    email: string
    createdAt: string
    roles: string[]
  }
  stats: { totalUsers: number; activeToday: number; pendingTasks: number }
  permissions: { canEditContent: boolean; canManageUsers: boolean; canViewReports: boolean }
}>()
</script>

<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Dashboard</h1>
      <p class="text-muted-foreground">Welcome back, {{ props.user.fullName }}</p>
    </div>

    <!-- Stat cards -->
    <div class="mb-6 grid gap-4 sm:grid-cols-3">
      <Card class="p-4">
        <div class="flex items-center gap-3">
          <div class="rounded-md bg-primary/10 p-2">
            <Users class="h-5 w-5 text-primary" />
          </div>
          <div>
            <p class="text-sm text-muted-foreground">Total Users</p>
            <p class="text-2xl font-bold">{{ props.stats.totalUsers }}</p>
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
            <p class="text-2xl font-bold">{{ props.stats.activeToday }}</p>
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
            <p class="text-2xl font-bold">{{ props.stats.pendingTasks }}</p>
          </div>
        </div>
      </Card>
    </div>

    <!-- User info -->
    <Card class="p-6">
      <h2 class="mb-4 font-semibold">Your Account</h2>
      <dl class="space-y-3 text-sm">
        <div class="flex justify-between">
          <dt class="text-muted-foreground">Name</dt>
          <dd class="font-medium">{{ props.user.fullName }}</dd>
        </div>
        <div class="flex justify-between">
          <dt class="text-muted-foreground">Email</dt>
          <dd class="font-medium">{{ props.user.email }}</dd>
        </div>
        <div class="flex justify-between">
          <dt class="text-muted-foreground">Roles</dt>
          <dd class="flex gap-1">
            <Badge v-for="role in props.user.roles" :key="role" variant="secondary">{{ role }}</Badge>
          </dd>
        </div>
      </dl>
    </Card>

    <!-- Permission sections -->
    <div v-if="props.permissions.canEditContent" class="mt-4">
      <Card class="border-green-200 bg-green-50 p-4 dark:border-green-900 dark:bg-green-950/20">
        <p class="text-sm font-medium text-green-800 dark:text-green-400">
          You can edit content (Admin / Editor role)
        </p>
      </Card>
    </div>

    <div v-if="props.permissions.canManageUsers" class="mt-4">
      <Card class="border-blue-200 bg-blue-50 p-4 dark:border-blue-900 dark:bg-blue-950/20">
        <p class="text-sm font-medium text-blue-800 dark:text-blue-400">
          You can manage users (Admin role) —
          <a href="/admin/users" class="underline">View all users</a>
        </p>
      </Card>
    </div>
  </div>
</template>
""";

    private static string ProfileEditPage(ProjectOptions opts) => """
<script setup lang="ts">
import { useForm } from '@inertiajs/vue3'
import { ref } from 'vue'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Card } from '@/components/ui/card'
import { Alert } from '@/components/ui/alert'
import { Separator } from '@/components/ui/separator'
import { Avatar } from '@/components/ui/avatar'

const props = defineProps<{
  user: {
    firstName: string
    lastName: string
    email: string
    bio?: string
    phoneNumber?: string
    avatarPathFile?: string
    hasAvatar: boolean
  }
  flash?: string
  errors?: Record<string, string>
}>()

const csrf = document.querySelector('meta[name="csrf-token"]')?.getAttribute('content') ?? ''

const profileForm = useForm({
  firstName: props.user.firstName,
  lastName:  props.user.lastName,
  email:     props.user.email,
  bio:       props.user.bio ?? '',
  phoneNumber: props.user.phoneNumber ?? '',
})

function updateProfile() {
  profileForm.post('/profile', { headers: { 'X-CSRF-TOKEN': csrf } })
}

const passwordForm = useForm({
  currentPassword: '',
  newPassword: '',
  newPasswordConfirmation: '',
})

function changePassword() {
  passwordForm.post('/profile/password', { headers: { 'X-CSRF-TOKEN': csrf } })
}

const fileInput = ref<HTMLInputElement | null>(null)

function submitAvatar(event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return

  const data = new FormData()
  data.append('avatar', file)

  fetch('/profile/avatar', {
    method: 'POST',
    body: data,
  }).then(() => window.location.reload())
}
</script>

<template>
  <div class="max-w-2xl space-y-8">
    <div>
      <h1 class="text-2xl font-bold">Profile</h1>
      <p class="text-muted-foreground">Manage your account settings</p>
    </div>

    <Alert v-if="props.flash" class="border-green-200 bg-green-50 text-green-800 dark:border-green-900 dark:bg-green-950/20 dark:text-green-400">
      {{ props.flash }}
    </Alert>

    <!-- Avatar -->
    <Card class="p-6">
      <h2 class="mb-4 font-semibold">Avatar</h2>
      <div class="flex items-center gap-4">
        <Avatar
          :src="props.user.hasAvatar ? `/${props.user.avatarPathFile}` : undefined"
          :fallback="`${props.user.firstName[0]}${props.user.lastName[0]}`"
          class="h-16 w-16 text-lg"
        />
        <div>
          <input ref="fileInput" type="file" accept="image/*" class="hidden" @change="submitAvatar" />
          <Button variant="outline" size="sm" @click="fileInput?.click()">Change photo</Button>
        </div>
      </div>
    </Card>

    <!-- Profile info -->
    <Card class="p-6">
      <h2 class="mb-4 font-semibold">Personal information</h2>
      <form @submit.prevent="updateProfile" class="space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label for="firstName">First name</Label>
            <Input id="firstName" v-model="profileForm.firstName" />
            <p v-if="props.errors?.firstName" class="text-sm text-destructive">{{ props.errors.firstName }}</p>
          </div>
          <div class="space-y-2">
            <Label for="lastName">Last name</Label>
            <Input id="lastName" v-model="profileForm.lastName" />
          </div>
        </div>

        <div class="space-y-2">
          <Label for="email">Email</Label>
          <Input id="email" v-model="profileForm.email" type="email" />
          <p v-if="props.errors?.email" class="text-sm text-destructive">{{ props.errors.email }}</p>
        </div>

        <div class="space-y-2">
          <Label for="bio">Bio</Label>
          <Input id="bio" v-model="profileForm.bio" placeholder="Tell us about yourself" />
        </div>

        <div class="space-y-2">
          <Label for="phone">Phone</Label>
          <Input id="phone" v-model="profileForm.phoneNumber" type="tel" placeholder="+1 (555) 000-0000" />
        </div>

        <Button type="submit" :disabled="profileForm.processing">Save changes</Button>
      </form>
    </Card>

    <Separator />

    <!-- Change password -->
    <Card class="p-6">
      <h2 class="mb-4 font-semibold">Change password</h2>
      <form @submit.prevent="changePassword" class="space-y-4">
        <div class="space-y-2">
          <Label for="currentPassword">Current password</Label>
          <Input id="currentPassword" v-model="passwordForm.currentPassword" type="password" />
        </div>

        <div class="space-y-2">
          <Label for="newPassword">New password</Label>
          <Input id="newPassword" v-model="passwordForm.newPassword" type="password" />
        </div>

        <div class="space-y-2">
          <Label for="newPasswordConfirmation">Confirm new password</Label>
          <Input id="newPasswordConfirmation" v-model="passwordForm.newPasswordConfirmation" type="password" />
          <p v-if="props.errors?.newPasswordConfirmation" class="text-sm text-destructive">{{ props.errors.newPasswordConfirmation }}</p>
        </div>

        <Button type="submit" variant="outline" :disabled="passwordForm.processing">Update password</Button>
      </form>
    </Card>
  </div>
</template>
""";

    private static string AdminUsersPage(ProjectOptions opts) => """
<script setup lang="ts">
import { Card } from '@/components/ui/card'

const props = defineProps<{
  users: Array<{ id: string; fullName: string; email: string; createdAt: string }>
}>()

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>

<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">Users</h1>
      <p class="text-muted-foreground">{{ props.users.length }} registered users</p>
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
            <tr v-for="user in props.users" :key="user.id" class="border-b last:border-0 hover:bg-muted/25">
              <td class="px-4 py-3 font-medium">{{ user.fullName }}</td>
              <td class="px-4 py-3 text-muted-foreground">{{ user.email }}</td>
              <td class="px-4 py-3 text-muted-foreground">{{ formatDate(user.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </Card>
  </div>
</template>
""";

    private static string HomePage(ProjectOptions opts) => """
<script setup lang="ts">
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'

const props = defineProps<{
  items: Array<{ id: number; name: string; description?: string; createdAt: string }>
}>()

function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>

<template>
  <div>
    <div class="mb-6">
      <h1 class="text-2xl font-bold">__PROJECT_NAME__</h1>
      <p class="text-muted-foreground">Items from the database</p>
    </div>

    <div class="space-y-3">
      <Card v-for="item in props.items" :key="item.id" class="flex items-center justify-between p-4">
        <div>
          <p class="font-medium">{{ item.name }}</p>
          <p v-if="item.description" class="text-sm text-muted-foreground">{{ item.description }}</p>
        </div>
        <Badge variant="outline">{{ formatDate(item.createdAt) }}</Badge>
      </Card>
    </div>

    <p v-if="!props.items.length" class="mt-8 text-center text-muted-foreground">
      No items yet. Add some to the database!
    </p>
  </div>
</template>
""".Replace("__PROJECT_NAME__", opts.ProjectName);

    private static string ForbiddenPage() => """
<script setup lang="ts">
import { Link } from '@inertiajs/vue3'
import { Button } from '@/components/ui/button'
</script>

<template>
  <div class="flex min-h-screen flex-col items-center justify-center text-center">
    <p class="text-6xl font-bold text-muted-foreground/20">403</p>
    <h1 class="mt-4 text-2xl font-bold">Access Denied</h1>
    <p class="mt-2 text-muted-foreground">You don't have permission to view this page.</p>
    <Button as-child class="mt-6">
      <Link href="/dashboard">Go to Dashboard</Link>
    </Button>
  </div>
</template>
""";

    private static string ServerErrorPage() => """
<script setup lang="ts">
import { Link } from '@inertiajs/vue3'
import { Button } from '@/components/ui/button'
</script>

<template>
  <div class="flex min-h-screen flex-col items-center justify-center text-center">
    <p class="text-6xl font-bold text-muted-foreground/20">500</p>
    <h1 class="mt-4 text-2xl font-bold">Server Error</h1>
    <p class="mt-2 text-muted-foreground">Something went wrong on our end.</p>
    <Button as-child class="mt-6">
      <Link href="/">Go Home</Link>
    </Button>
  </div>
</template>
""";
}
