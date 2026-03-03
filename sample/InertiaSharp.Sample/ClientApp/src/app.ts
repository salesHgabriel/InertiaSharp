import './assets/app.css'

import { createApp, h, type DefineComponent } from 'vue'
import { createInertiaApp } from '@inertiajs/vue3'
import AppLayout from '@/Layouts/AppLayout.vue'

/**
 * Bootstrap the Inertia + Vue 3 application.
 *
 * Pages are loaded from ./Pages/** using Vite's import.meta.glob.
 * Each file maps to a component name like "Auth/Login" → Pages/Auth/Login.vue.
 */
createInertiaApp({
  // Resolve page components by name (set via controller: this.Inertia("Auth/Login", ...))
  resolve: (name: string) => {
    const pages = import.meta.glob<DefineComponent>('./Pages/**/*.vue', { eager: true })
    const page  = pages[`./Pages/${name}.vue`]

    if (!page)
      throw new Error(`Inertia page not found: ./Pages/${name}.vue`)

    // Attach the default layout to all pages unless they opt out
    if (!page.default.layout)
      page.default.layout = AppLayout

    return page
  },

  // App title: "Dashboard – MyApp"
  title: (title) => (title ? `${title} – InertiaSharp Demo` : 'InertiaSharp Demo'),

  setup({ el, App, props, plugin }) {
    createApp({ render: () => h(App, props) })
      .use(plugin)
      .mount(el)
  },

  // Show a progress bar during page transitions
  progress: {
    color: '#6366f1',
    includeCSS: true,
    showSpinner: false,
  },
})
