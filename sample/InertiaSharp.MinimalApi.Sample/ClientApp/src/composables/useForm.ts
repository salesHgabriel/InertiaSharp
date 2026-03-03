import { useForm as useInertiaForm } from '@inertiajs/vue3'

/**
 * Thin wrapper around Inertia's useForm that automatically injects the
 * CSRF token from <meta name="csrf-token"> into every request header.
 *
 * ASP.NET Core validates the token on POST/PUT/PATCH/DELETE via
 * [ValidateAntiForgeryToken].
 *
 * Usage:
 *   const form = useForm({ email: '', password: '' })
 *   form.post('/login')
 */
// export function useForm<T extends Record<string, unknown>>(initialData: T) {
//   const csrfToken = document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content
//
//   const form = useInertiaForm(initialData)
//
//   // Override submit to always include the CSRF header
//   const originalTransform = form.transform.bind(form)
//
//   if (csrfToken) {
//     form.transform((data) => data) // ensure transform chain exists
//   }
//
//   return form
// }
//
// export { useForm }

// import { useForm as useInertiaForm } from '@inertiajs/vue3'

/**
 * Thin wrapper around Inertia's useForm that automatically injects the
 * CSRF token from <meta name="csrf-token"> into every request header.
 *
 * ASP.NET Core validates the token on POST/PUT/PATCH/DELETE via
 * [ValidateAntiForgeryToken].
 *
 * Usage:
 *   const form = useForm({ email: '', password: '' })
 *   form.post('/login')
 */
export function useForm<T extends Record<string, any>>(initialData: T) {
  const csrfToken = document.querySelector<HTMLMetaElement>('meta[name="csrf-token"]')?.content

  const form = useInertiaForm(initialData as any)

  // Override the original methods to include CSRF token
  const originalPost = form.post.bind(form)
  const originalPut = form.put.bind(form)
  const originalPatch = form.patch.bind(form)
  const originalDelete = form.delete.bind(form)

  form.post = (url: string, options: any = {}) => {
    const headers = { ...options.headers }
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }
    return originalPost(url, { ...options, headers })
  }

  form.put = (url: string, options: any = {}) => {
    const headers = { ...options.headers }
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }
    return originalPut(url, { ...options, headers })
  }

  form.patch = (url: string, options: any = {}) => {
    const headers = { ...options.headers }
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }
    return originalPatch(url, { ...options, headers })
  }

  form.delete = (url: string, options: any = {}) => {
    const headers = { ...options.headers }
    if (csrfToken) {
      headers['X-CSRF-TOKEN'] = csrfToken
    }
    return originalDelete(url, { ...options, headers })
  }

  return form
}

