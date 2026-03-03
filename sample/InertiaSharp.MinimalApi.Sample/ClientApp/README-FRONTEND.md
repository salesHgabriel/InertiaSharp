# Frontend — shadcn-vue + reka-ui + Vue 3 + Vite

## Stack

| Lib | Papel |
|-----|-------|
| **Vue 3** | Framework |
| **Vite** | Dev server + build |
| **Inertia.js** (`@inertiajs/vue3`) | Bridge ASP.NET Core ↔ Vue (sem API REST) |
| **shadcn-vue** | Componentes UI (copy-paste, não um npm package) |
| **reka-ui** | Primitivos acessíveis headless (sucessor do radix-vue) |
| **Tailwind CSS v3** | Estilos utilitários |
| **@vueuse/core** | Utilitários Vue reativos |
| **lucide-vue-next** | Ícones |

## Por que reka-ui e não radix-vue?

O `shadcn-vue@latest` migrou de `radix-vue` → **`reka-ui`** (o mesmo projeto, renomeado e melhorado).
- `reka-ui` é o sucessor direto, mesma API, mesma equipe
- `npx shadcn-vue@latest add ...` agora instala `reka-ui` automaticamente
- Não use `radix-vue` em projetos novos

## Adicionar mais componentes

Os componentes são **copy-paste** — o CLI copia o código-fonte para o seu projeto,
dando-lhe total controle. Não é uma lib npm que você atualiza.

```bash
cd ClientApp

# Adicionar componentes individualmente
npx shadcn-vue@latest add dialog
npx shadcn-vue@latest add select
npx shadcn-vue@latest add dropdown-menu
npx shadcn-vue@latest add tabs
npx shadcn-vue@latest add toast
npx shadcn-vue@latest add table
npx shadcn-vue@latest add form          # integra com vee-validate/zod
npx shadcn-vue@latest add date-picker
npx shadcn-vue@latest add sidebar       # componente sidebar completo

# Adicionar vários de uma vez
npx shadcn-vue@latest add dialog select dropdown-menu tabs

# Listar todos disponíveis
npx shadcn-vue@latest add --help
```

O CLI usa `components.json` para saber onde colocar os arquivos.

## Estrutura dos componentes

```
src/components/ui/
├── button/
│   ├── Button.vue       ← componente (usa reka-ui Primitive)
│   └── index.ts         ← exports + CVA variants
├── input/
│   ├── Input.vue        ← usa @vueuse/core useVModel
│   └── index.ts
├── card/
│   ├── Card.vue
│   ├── CardHeader.vue
│   ├── CardTitle.vue
│   ├── CardDescription.vue
│   ├── CardContent.vue
│   ├── CardFooter.vue
│   └── index.ts
├── checkbox/
│   ├── Checkbox.vue     ← usa reka-ui CheckboxRoot + CheckboxIndicator
│   └── index.ts
├── label/
│   ├── Label.vue        ← usa reka-ui Label
│   └── index.ts
├── badge/
│   ├── Badge.vue        ← apenas CVA, sem primitivos
│   └── index.ts
├── separator/
│   ├── Separator.vue    ← usa reka-ui Separator
│   └── index.ts
├── avatar/
│   ├── Avatar.vue       ← usa reka-ui AvatarRoot
│   ├── AvatarImage.vue  ← usa reka-ui AvatarImage
│   ├── AvatarFallback.vue
│   └── index.ts
└── alert/
    ├── Alert.vue        ← apenas CVA
    ├── AlertTitle.vue
    ├── AlertDescription.vue
    └── index.ts
```

## Padrão de importação

```vue
<script setup lang="ts">
// Sempre importar do barril index.ts da pasta
import { Button }   from '@/components/ui/button'
import { Input }    from '@/components/ui/input'
import { Label }    from '@/components/ui/label'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from '@/components/ui/card'
import { Badge }     from '@/components/ui/badge'
import { Separator } from '@/components/ui/separator'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import { Alert, AlertTitle, AlertDescription } from '@/components/ui/alert'
</script>
```

## Customizar o tema

Edite as variáveis CSS em `src/assets/app.css`:

```css
:root {
  --primary:   240 5.9% 10%;    /* cor principal */
  --radius:    0.5rem;           /* border-radius global */
  /* ... demais tokens */
}
```

Ou use o [theme generator](https://ui.shadcn.com/themes) do shadcn para gerar um tema customizado.

## Hot reload

```bash
# Na raiz do projeto InertiaSharp
./run-dev.sh          # MVC (porta 5001, Vite 5173)
./run-dev.sh minimal  # Minimal API (porta 5002, Vite 5174)
```

- Mudanças em `.vue` → HMR instantâneo (sem reload de página)
- Mudanças em `.cs` → `dotnet watch` reinicia o backend
