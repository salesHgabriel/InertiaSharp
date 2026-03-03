<script setup lang="ts">
import type { HTMLAttributes } from 'vue'
import { Separator, type SeparatorProps, useForwardProps } from 'reka-ui'
import { cn } from '@/lib/utils'

const props = defineProps<SeparatorProps & { class?: HTMLAttributes['class'] }>()
const forwarded = useForwardProps(props)
</script>

<template>
  <Separator
    v-bind="forwarded"
    :class="cn(
      'shrink-0 bg-border',
      props.orientation === 'vertical' ? 'h-full w-[1px]' : 'h-[1px] w-full',
      props.class,
    )"
  />
</template>
