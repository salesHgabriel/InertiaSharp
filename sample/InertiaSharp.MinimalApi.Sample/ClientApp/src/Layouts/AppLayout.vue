<script setup lang="ts">
import { Link, router, usePage } from '@inertiajs/vue3'
import { Button }    from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import {Avatar, AvatarFallback, AvatarImage} from '@/components/ui/avatar'
import { LayoutDashboard, User, LogOut, Zap } from 'lucide-vue-next'

const page = usePage<{
  auth?: {
    user: { fullName: string; email: string, avatarPathFile: string | null, hasAvatar : boolean }
    permissions: Record<string, boolean>
  }
}>()

function initials(name?: string) {
  if (!name) return '?'
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}

function logout() {
  const token = document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content
  router.post('/logout', {}, { headers: { 'X-CSRF-TOKEN': token ?? '' } })
}

const navItems = [
  { href: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/profile',   label: 'Profile',   icon: User },
]
</script>

<template>
  <div class="flex h-screen overflow-hidden">
    <!-- Sidebar -->
    <aside class="flex flex-col w-64 border-r bg-sidebar text-sidebar-foreground shrink-0">
      <!-- Brand -->
      <div class="flex items-center gap-2 h-16 px-6 border-b border-sidebar-border">
        <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
          <Zap class="h-4 w-4" />
        </div>
        <span class="font-semibold text-sm">InertiaSharp</span>
      </div>

      <!-- Nav -->
      <nav class="flex-1 p-3 space-y-1">
        <Button
          v-for="item in navItems"
          :key="item.href"
          variant="ghost"
          class="w-full justify-start gap-3 text-sidebar-foreground hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
          :class="{ 'bg-sidebar-accent text-sidebar-accent-foreground': page.url.startsWith(item.href) }"
          as-child
        >
          <Link :href="item.href">
            <component :is="item.icon" class="h-4 w-4 shrink-0" />
            {{ item.label }}
          </Link>
        </Button>
      </nav>

      <Separator class="bg-sidebar-border" />

      <!-- User -->
      <div class="p-3 space-y-1">
        <div class="flex items-center gap-3 px-2 py-2 rounded-md">
          <Avatar v-if="page.props.auth?.user?.hasAvatar" class="h-8 w-8 text-xs">
            <AvatarImage v-if="page.props.auth?.user?.hasAvatar" :src="page.props.auth?.user?.avatarPathFile" alt="Avatar" />
          </Avatar>
          <Avatar v-else class="h-8 w-8 text-xs">
            <AvatarFallback>{{ initials(page.props.auth?.user?.fullName) }}</AvatarFallback>
          </Avatar>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-medium truncate">{{ page.props.auth?.user?.fullName }}</p>
            <p class="text-xs text-muted-foreground truncate">{{ page.props.auth?.user?.email }}</p>
          </div>
        </div>

        <Button
          variant="ghost"
          class="w-full justify-start gap-3 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
          @click="logout"
        >
          <LogOut class="h-4 w-4" />
          Sign out
        </Button>
      </div>
    </aside>

    <!-- Main -->
    <main class="flex-1 overflow-auto">
      <slot />
    </main>
  </div>
</template>
