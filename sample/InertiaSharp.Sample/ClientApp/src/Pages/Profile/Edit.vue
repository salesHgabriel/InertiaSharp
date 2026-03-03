<script setup lang="ts">
import { useForm } from '@inertiajs/vue3'
import { Button } from '@/components/ui/button'
import { Input }  from '@/components/ui/input'
import { Label }  from '@/components/ui/label'
import { AlertCircle } from 'lucide-vue-next'
import {
  Card, CardContent, CardDescription, CardHeader, CardTitle,
} from '@/components/ui/card'

import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { CardFooter } from '@/components/ui/card'

import { ref } from 'vue'
import { Loader2, CheckCircle2, X,ImageIcon } from 'lucide-vue-next'

const props = defineProps<{
  user: {
    firstName: string; 
    lastName: string
    email: string; 
    bio?: string;
    phoneNumber?: string,
    avatarUrl?: string | null,
    hasAvatar: boolean,
  }
  errors?: Record<string, string>
  flash?: string
}>()

const csrf = () =>
  document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content ?? ''

// ── Profile form ──────────────────────────────────────────────────────────────
const profileForm = useForm({
  firstName:   props.user.firstName,
  lastName:    props.user.lastName,
  email:       props.user.email,
  bio:         props.user.bio ?? '',
  phoneNumber: props.user.phoneNumber ?? '',
})

function updateProfile() {
  profileForm.post('/profile', { headers: { 'X-CSRF-TOKEN': csrf() }, preserveScroll: true })
}

// ── Password form ─────────────────────────────────────────────────────────────
const passwordForm = useForm({
  currentPassword: '', newPassword: '', newPasswordConfirmation: '',
})

// ── Upload avatar form ─────────────────────────────────────────────────────────────

function initials(name?: string) {
  if (!name) return '?'
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}


const preview    = ref<string | null>(props.user.avatarUrl ?? null)
const isDragging = ref(false)
const inputRef   = ref<HTMLInputElement>()

const formAvatar = useForm({ avatar: null as File | null })
function onFileSelected(file: File) {
  if (!file.type.startsWith('image/')) return
  if (file.size > 2 * 1024 * 1024) return // 2MB
  formAvatar.avatar   = file
  preview.value = URL.createObjectURL(file)
}

function onInputChange(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (file) onFileSelected(file)
}

function onDrop(e: DragEvent) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  if (file) onFileSelected(file)
}

function remove() {
  formAvatar.avatar   = null
  preview.value = props.user.avatarUrl ?? null
  if (inputRef.value) inputRef.value.value = ''
}

function submit() {
  formAvatar.post('/profile/avatar', { forceFormData: true })
}

function updatePassword() {
  passwordForm.post('/profile/password', {
    headers: { 'X-CSRF-TOKEN': csrf() },
    preserveScroll: true,
    onSuccess: () => passwordForm.reset(),
  })
}

function err(form: typeof profileForm | typeof passwordForm, field: string) {
  return (form.errors as Record<string, string>)[field] ?? props.errors?.[field]
}
</script>

<template>
  <div class="flex-1 space-y-6 p-8 max-w-2xl">
    <div>
      <h1 class="text-3xl font-bold tracking-tight">Profile settings</h1>
      <p class="text-muted-foreground mt-1">Manage your personal information and security.</p>
    </div>

    <!-- Flash message -->
    <Alert v-if="flash" class="border-green-500/50 bg-green-50 text-green-900 dark:bg-green-950 dark:text-green-100">
      <CheckCircle2 class="h-4 w-4 !text-green-600" />
      <AlertTitle>Success</AlertTitle>
      <AlertDescription>{{ flash }}</AlertDescription>
    </Alert>

    <!-- Personal info -->
    <Card>
      <CardHeader>
        <CardTitle>Personal information</CardTitle>
        <CardDescription>Update your name, email, and contact details.</CardDescription>
      </CardHeader>
      <CardContent>
        <form @submit.prevent="updateProfile" class="space-y-4">
          <div class="grid grid-cols-2 gap-3">
            <div class="space-y-2">
              <Label for="firstName">First name</Label>
              <Input id="firstName" v-model="profileForm.firstName"
                :class="err(profileForm, 'firstName') ? 'border-destructive' : ''" />
              <p v-if="err(profileForm, 'firstName')" class="text-xs text-destructive">
                {{ err(profileForm, 'firstName') }}
              </p>
            </div>
            <div class="space-y-2">
              <Label for="lastName">Last name</Label>
              <Input id="lastName" v-model="profileForm.lastName" />
            </div>
          </div>

          <div class="space-y-2">
            <Label for="email">Email address</Label>
            <Input id="email" v-model="profileForm.email" type="email"
              :class="err(profileForm, 'email') ? 'border-destructive' : ''" />
            <p v-if="err(profileForm, 'email')" class="text-xs text-destructive">
              {{ err(profileForm, 'email') }}
            </p>
          </div>

          <div class="space-y-2">
            <Label for="phone">Phone number</Label>
            <Input id="phone" v-model="profileForm.phoneNumber"
              type="tel" placeholder="+55 11 99999-9999" />
          </div>

          <div class="space-y-2">
            <Label for="bio">Bio</Label>
            <textarea
              id="bio" v-model="profileForm.bio" rows="3"
              placeholder="Tell us a bit about yourself…"
              class="flex min-h-[80px] w-full rounded-md border border-input bg-background
                     px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground
                     focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring
                     focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 resize-none"
            />
          </div>

          <div class="flex justify-end">
            <Button type="submit" :disabled="profileForm.processing">
              <Loader2 v-if="profileForm.processing" class="mr-2 h-4 w-4 animate-spin" />
              Save changes
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>

    <!-- upload arquivo avatar  -->

    <Card class="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Foto de perfil</CardTitle>
        <CardDescription>JPG, PNG ou WEBP • máx. 2 MB</CardDescription>
      </CardHeader>

      <CardContent class="space-y-5">

        <!-- Avatar preview -->
        <div class="flex justify-center">
          <Avatar class="h-24 w-24 text-xl ring-2 ring-border ring-offset-2 ring-offset-background">
            <AvatarImage v-if="preview" :src="preview" alt="Avatar" />
            <AvatarFallback>{{ initials(user.firstName) }}</AvatarFallback>
          </Avatar>
        </div>

        <!-- Drop zone -->
        <div
            role="button"
            tabindex="0"
            class="relative flex flex-col items-center gap-3 rounded-lg border-2 border-dashed p-6 text-center transition-colors cursor-pointer focus-visible:outline-none focus-visible:ring-1 focus-visible:ring-ring"
            :class="isDragging
          ? 'border-primary bg-primary/5'
          : 'border-muted-foreground/30 hover:border-primary/60 hover:bg-muted/40'"
            @click="inputRef?.click()"
            @keydown.enter="inputRef?.click()"
            @keydown.space.prevent="inputRef?.click()"
            @dragover.prevent="isDragging = true"
            @dragleave.prevent="isDragging = false"
            @drop.prevent="onDrop"
        >
          <div class="flex h-10 w-10 items-center justify-center rounded-full bg-muted">
            <ImageIcon class="h-5 w-5 text-muted-foreground" />
          </div>

          <div class="space-y-1">
            <p class="text-sm font-medium">
              Arraste uma imagem ou
              <span class="text-primary underline underline-offset-2">clique aqui</span>
            </p>
            <p class="text-xs text-muted-foreground">
              {{ formAvatar.avatar ? formAvatar.avatar.name : 'Nenhum arquivo selecionado' }}
            </p>
          </div>

          <!-- Botão remover arquivo -->
          <button
              v-if="formAvatar.avatar"
              type="button"
              class="absolute top-2 right-2 flex h-6 w-6 items-center justify-center rounded-full
                 bg-destructive/10 text-destructive hover:bg-destructive/20 transition-colors"
              @click.stop="remove"
          >
            <X class="h-3.5 w-3.5" />
          </button>

          <input
              ref="inputRef"
              type="file"
              accept="image/jpeg,image/png,image/webp"
              class="hidden"
              @change="onInputChange"
          />
        </div>

        <!-- Erro -->
        <Alert v-if="formAvatar.errors.avatar || errors?.avatar" variant="destructive">
          <AlertCircle class="h-4 w-4" />
          <AlertDescription>
            {{ formAvatar.errors.avatar ?? errors?.avatar }}
          </AlertDescription>
        </Alert>

      </CardContent>

      <CardFooter class="justify-end gap-2">
        <Button variant="outline" :disabled="!formAvatar.avatar || formAvatar.processing" @click="remove">
          Cancelar
        </Button>
        <Button :disabled="!formAvatar.avatar || formAvatar.processing" @click="submit">
          <Loader2 v-if="formAvatar.processing" class="h-4 w-4 animate-spin" />
          Salvar foto
        </Button>
      </CardFooter>
    </Card>
    
    
    <!-- Change password -->
    <Card>
      <CardHeader>
        <CardTitle>Change password</CardTitle>
        <CardDescription>Use a strong password of at least 8 characters.</CardDescription>
      </CardHeader>
      <CardContent>
        <form @submit.prevent="updatePassword" class="space-y-4">
          <div class="space-y-2">
            <Label for="currentPassword">Current password</Label>
            <Input id="currentPassword" v-model="passwordForm.currentPassword"
              type="password" autocomplete="current-password"
              :class="err(passwordForm, 'PasswordMismatch') ? 'border-destructive' : ''" />
            <p v-if="err(passwordForm, 'PasswordMismatch')" class="text-xs text-destructive">
              {{ err(passwordForm, 'PasswordMismatch') }}
            </p>
          </div>

          <div class="space-y-2">
            <Label for="newPassword">New password</Label>
            <Input id="newPassword" v-model="passwordForm.newPassword"
              type="password" autocomplete="new-password" />
          </div>

          <div class="space-y-2">
            <Label for="confirmPassword">Confirm new password</Label>
            <Input id="confirmPassword" v-model="passwordForm.newPasswordConfirmation"
              type="password" autocomplete="new-password"
              :class="err(passwordForm, 'newPasswordConfirmation') ? 'border-destructive' : ''" />
            <p v-if="err(passwordForm, 'newPasswordConfirmation')" class="text-xs text-destructive">
              {{ err(passwordForm, 'newPasswordConfirmation') }}
            </p>
          </div>

          <div class="flex justify-end">
            <Button type="submit" variant="outline" :disabled="passwordForm.processing">
              <Loader2 v-if="passwordForm.processing" class="mr-2 h-4 w-4 animate-spin" />
              Update password
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  </div>
</template>
