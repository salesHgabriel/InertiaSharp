<script setup lang="ts">
import { useForm, Link } from '@inertiajs/vue3'
import GuestLayout from '@/Layouts/GuestLayout.vue'
import { Button } from '@/components/ui/button'
import { Input }  from '@/components/ui/input'
import { Label }  from '@/components/ui/label'
import {
  Card, CardContent, CardDescription,
  CardFooter, CardHeader, CardTitle,
} from '@/components/ui/card'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Loader2, AlertCircle } from 'lucide-vue-next'

defineOptions({ layout: GuestLayout })

const props = defineProps<{ errors?: Record<string, string> }>()

const form = useForm({
  firstName: '', lastName: '', email: '',
  password: '', passwordConfirmation: '',
})

const csrf = () =>
  document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content ?? ''

function submit() {
  form.post('/register', { headers: { 'X-CSRF-TOKEN': csrf() }, preserveScroll: true })
}

function err(field: string) {
  return (form.errors as Record<string, string>)[field] ?? props.errors?.[field]
}
</script>

<template>
  <Card class="w-full">
    <CardHeader class="space-y-1">
      <CardTitle class="text-2xl">Create an account</CardTitle>
      <CardDescription>Fill in the details below to get started</CardDescription>
    </CardHeader>

    <CardContent>
      <Alert v-if="err('email')" variant="destructive" class="mb-4">
        <AlertCircle class="h-4 w-4" />
        <AlertDescription>{{ err('email') }}</AlertDescription>
      </Alert>

      <form @submit.prevent="submit" class="space-y-4">
        <div class="grid grid-cols-2 gap-3">
          <div class="space-y-2">
            <Label for="firstName">First name</Label>
            <Input id="firstName" v-model="form.firstName" autocomplete="given-name"
              :class="err('firstName') ? 'border-destructive' : ''" />
            <p v-if="err('firstName')" class="text-xs text-destructive">{{ err('firstName') }}</p>
          </div>
          <div class="space-y-2">
            <Label for="lastName">Last name</Label>
            <Input id="lastName" v-model="form.lastName" autocomplete="family-name" />
          </div>
        </div>

        <div class="space-y-2">
          <Label for="email">Email</Label>
          <Input id="email" v-model="form.email" type="email"
            placeholder="you@example.com" autocomplete="email"
            :class="err('email') ? 'border-destructive' : ''" />
        </div>

        <div class="space-y-2">
          <Label for="password">Password</Label>
          <Input id="password" v-model="form.password" type="password"
            placeholder="Min. 8 characters" autocomplete="new-password"
            :class="err('password') ? 'border-destructive' : ''" />
          <p v-if="err('password')" class="text-xs text-destructive">{{ err('password') }}</p>
        </div>

        <div class="space-y-2">
          <Label for="passwordConfirmation">Confirm password</Label>
          <Input id="passwordConfirmation" v-model="form.passwordConfirmation"
            type="password" autocomplete="new-password"
            :class="err('passwordConfirmation') ? 'border-destructive' : ''" />
          <p v-if="err('passwordConfirmation')" class="text-xs text-destructive">{{ err('passwordConfirmation') }}</p>
        </div>

        <Button type="submit" class="w-full" :disabled="form.processing">
          <Loader2 v-if="form.processing" class="mr-2 h-4 w-4 animate-spin" />
          Create account
        </Button>
      </form>
    </CardContent>

    <CardFooter class="justify-center">
      <p class="text-sm text-muted-foreground">
        Already have an account?
        <Link href="/login" class="text-primary font-medium hover:underline underline-offset-4">Sign in</Link>
      </p>
    </CardFooter>
  </Card>
</template>
