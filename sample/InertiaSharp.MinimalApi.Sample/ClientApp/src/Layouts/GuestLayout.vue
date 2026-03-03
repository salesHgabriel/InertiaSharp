<script setup lang="ts">
// Guest layout — used by Login and Register pages
</script>

<template>
  <div class="min-h-screen grid lg:grid-cols-2">
    <!-- Left: brand panel (hidden on mobile) -->
    <div class="hidden lg:flex flex-col bg-zinc-900 text-white p-10 justify-between">
      <div class="flex items-center gap-2 font-semibold">
        <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-white text-zinc-900">
          <span class="text-xs font-bold">IS</span>
        </div>
        InertiaSharp
      </div>

      <blockquote class="space-y-2">
        <p class="text-lg leading-relaxed text-zinc-300">
          "Build modern SPAs with ASP.NET Core and Vue — without the API overhead.
          Full hot reload, cookie auth, and shadcn-vue out of the box."
        </p>
        <footer class="text-sm text-zinc-500">InertiaSharp — .NET adapter for Inertia.js</footer>
      </blockquote>
    </div>

    <!-- Right: form area -->
    <div class="flex items-center justify-center p-6 sm:p-12">
      <div class="w-full max-w-sm space-y-4">
        <!-- Mobile logo -->
        <div class="flex items-center gap-2 font-semibold lg:hidden mb-6">
          <div class="flex h-8 w-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <span class="text-xs font-bold">IS</span>
          </div>
          InertiaSharp
        </div>

        <slot />
      </div>
    </div>
  </div>
</template>
