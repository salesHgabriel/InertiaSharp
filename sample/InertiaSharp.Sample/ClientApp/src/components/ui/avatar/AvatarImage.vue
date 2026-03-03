<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { AvatarImage, type AvatarImageProps, useForwardProps } from 'reka-ui'
import { cn } from '@/lib/utils'

const props = defineProps<AvatarImageProps & { class?: HTMLAttributes['class'] }>()
const forwarded = useForwardProps(props)
</script>

<template>
  <AvatarImage
    v-bind="forwarded"
    :class="cn('aspect-square h-full w-full', props.class)"
  />
</template>
