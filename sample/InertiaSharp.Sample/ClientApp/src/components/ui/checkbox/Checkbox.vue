<script setup lang="ts">
import { computed } from 'vue'
import {
  CheckboxIndicator,
  CheckboxRoot,
  type CheckboxRootEmits,
  type CheckboxRootProps,
} from 'reka-ui'
import { Check, Minus } from 'lucide-vue-next'
import { cn } from '@/lib/utils'

interface Props extends CheckboxRootProps {
  class?: any
}

const props = withDefaults(defineProps<Props>(), {
  as: 'button',
  asChild: false,
})

// Mudamos para usar update:modelValue que é o evento correto da reka-ui
const emits = defineEmits<{
  'update:modelValue': [value: boolean | 'indeterminate']
}>()

const delegatedProps = computed(() => {
  const { class: _, ...delegated } = props
  return delegated
})
</script>

<template>
  <CheckboxRoot
    v-bind="delegatedProps"
    :class="cn(
      'peer h-4 w-4 shrink-0 rounded-sm border border-primary ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 data-[state=checked]:bg-primary data-[state=checked]:text-primary-foreground',
      props.class,
    )"
    @update:modelValue="emits('update:modelValue', $event)"
  >
    <CheckboxIndicator class="flex items-center justify-center text-current">
      <Check v-if="$attrs.modelValue === true" class="h-3.5 w-3.5" />
      <Minus v-else-if="$attrs.modelValue === 'indeterminate'" class="h-3.5 w-3.5" />
    </CheckboxIndicator>
  </CheckboxRoot>
</template>