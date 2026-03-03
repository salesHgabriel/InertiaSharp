<script setup lang="ts">
import { Link } from '@inertiajs/vue3'
import {
  Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'
import { Badge }     from '@/components/ui/badge'
import { Button }    from '@/components/ui/button'
import { Separator } from '@/components/ui/separator'
import { Users, Activity, CheckSquare, ChevronRight, Shield } from 'lucide-vue-next'

const props = defineProps<{
  user: {
    id: string
    fullName: string
    email: string
    createdAt: string
    roles: string[]
  }
  permissions: {
    canEditContent: boolean
    canManageUsers: boolean
    canViewReports: boolean
  }
  stats: {
    totalUsers:  number
    activeToday: number
    pendingTasks: number
  }
}>()
</script>

<template>
  <div class="flex-1 space-y-6 p-8">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p class="text-muted-foreground mt-1">
          Welcome back, <strong>{{ user.fullName }}</strong> 👋
        </p>
      </div>
      <div class="flex gap-2">
        <Badge v-for="role in user.roles" :key="role" variant="outline" class="gap-1.5">
          <Shield class="h-3 w-3" />
          {{ role }}
        </Badge>
      </div>
    </div>

    <Separator />

    <!-- Stats -->
    <div class="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Total Users</CardTitle>
          <Users class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ stats.totalUsers }}</div>
          <p class="text-xs text-muted-foreground">All registered accounts</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Active Today</CardTitle>
          <Activity class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ stats.activeToday }}</div>
          <p class="text-xs text-muted-foreground">Sessions in last 24h</p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Pending Tasks</CardTitle>
          <CheckSquare class="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ stats.pendingTasks }}</div>
          <p class="text-xs text-muted-foreground">Awaiting your review</p>
        </CardContent>
      </Card>
    </div>

    <!-- Quick actions -->
    <div class="grid gap-4 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle class="text-base">Quick Actions</CardTitle>
          <CardDescription>Common tasks based on your permissions</CardDescription>
        </CardHeader>
        <CardContent class="space-y-2">
          <Button variant="outline" class="w-full justify-between" as-child>
            <Link href="/profile">
              Edit profile
              <ChevronRight class="h-4 w-4 ml-auto" />
            </Link>
          </Button>

          <Button
            v-if="permissions.canEditContent"
            variant="outline"
            class="w-full justify-between"
          >
            Manage content
            <ChevronRight class="h-4 w-4 ml-auto" />
          </Button>

          <Button
            v-if="permissions.canManageUsers"
            variant="outline"
            class="w-full justify-between"
          >
            User management
            <ChevronRight class="h-4 w-4 ml-auto" />
          </Button>
        </CardContent>
      </Card>

      <!-- Account info -->
      <Card>
        <CardHeader>
          <CardTitle class="text-base">Account Information</CardTitle>
          <CardDescription>Your profile at a glance</CardDescription>
        </CardHeader>
        <CardContent>
          <dl class="space-y-3 text-sm">
            <div class="flex justify-between">
              <dt class="text-muted-foreground">Email</dt>
              <dd class="font-medium">{{ user.email }}</dd>
            </div>
            <Separator />
            <div class="flex justify-between">
              <dt class="text-muted-foreground">Member since</dt>
              <dd class="font-medium">{{ new Date(user.createdAt).toLocaleDateString() }}</dd>
            </div>
            <Separator />
            <div class="flex justify-between">
              <dt class="text-muted-foreground">Roles</dt>
              <dd class="flex gap-1">
                <Badge v-for="role in user.roles" :key="role" variant="secondary" class="text-xs">
                  {{ role }}
                </Badge>
              </dd>
            </div>
          </dl>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
