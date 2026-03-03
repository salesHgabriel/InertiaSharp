<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { AvatarFallback, type AvatarFallbackProps, useForwardProps } from 'reka-ui'
import { cn } from '@/lib/utils'

const props = defineProps<AvatarFallbackProps & { class?: HTMLAttributes['class'] }>()
const forwarded = useForwardProps(props)
</script>

<template>
  <AvatarFallback
    v-bind="forwarded"
    :class="cn(
      'flex h-full w-full items-center justify-center rounded-full bg-muted text-sm font-medium',
      props.class,
    )"
  >
    <slot />
  </AvatarFallback>
</template>
