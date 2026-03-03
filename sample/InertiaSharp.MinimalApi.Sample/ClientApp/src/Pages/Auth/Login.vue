<script setup lang="ts">
import { ref } from 'vue'
import { useForm, Link } from '@inertiajs/vue3'
import GuestLayout from '@/Layouts/GuestLayout.vue'
import { Button }   from '@/components/ui/button'
import { Input }    from '@/components/ui/input'
import { Label }    from '@/components/ui/label'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Card, CardContent, CardDescription,
  CardFooter, CardHeader, CardTitle,
} from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Eye, EyeOff, Loader2, AlertCircle } from 'lucide-vue-next'

defineOptions({ layout: GuestLayout })

defineProps<{ errors?: Record<string, string> }>()

const showPassword = ref(false)

const form = useForm({ email: '', password: '', remember: false })

const csrf = () =>
  document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content ?? ''

function submit() {
  form.post('/login', { headers: { 'X-CSRF-TOKEN': csrf() }, preserveScroll: true })
}
</script>

<template>
  <Card class="w-full">
    <CardHeader class="space-y-1">
      <CardTitle class="text-2xl">Sign in</CardTitle>
      <CardDescription>Enter your credentials to access your account</CardDescription>
    </CardHeader>

    <CardContent>
      <Alert v-if="form.errors.email || errors?.email" variant="destructive" class="mb-4">
        <AlertCircle class="h-4 w-4" />
        <AlertDescription>{{ form.errors.email ?? errors?.email }}</AlertDescription>
      </Alert>

      <form @submit.prevent="submit" class="space-y-4">
        <div class="space-y-2">
          <Label for="email">Email</Label>
          <Input
            id="email" v-model="form.email" type="email"
            placeholder="you@example.com" autocomplete="email"
            :class="(form.errors.email || errors?.email) ? 'border-destructive' : ''"
          />
        </div>

        <div class="space-y-2">
          <Label for="password">Password</Label>
          <div class="relative">
            <Input
              id="password" v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="••••••••" autocomplete="current-password" class="pr-10"
            />
            <button
              type="button" @click="showPassword = !showPassword"
              class="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
            >
              <EyeOff v-if="showPassword" class="h-4 w-4" />
              <Eye v-else class="h-4 w-4" />
            </button>
          </div>
        </div>

        <div class="flex items-center space-x-2">
          <Checkbox id="remember" v-model="form.remember" />
          <Label for="remember" class="text-sm font-normal cursor-pointer">Remember me for 30 days</Label>
        </div>

        <Button type="submit" class="w-full" :disabled="form.processing">
          <Loader2 v-if="form.processing" class="mr-2 h-4 w-4 animate-spin" />
          Sign in
        </Button>
      </form>
    </CardContent>

    <CardFooter class="justify-center">
      <p class="text-sm text-muted-foreground">
        Don't have an account?
        <Link href="/register" class="text-primary font-medium hover:underline underline-offset-4">Create one</Link>
      </p>
    </CardFooter>
  </Card>
</template>
