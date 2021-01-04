<template>
  <div class="w-screen h-screen flex items-center justify-center">
    <div class="w-1/2 h-1/2 p-10 bg-white rounded-md shadow">
      <div ref="editorElement" class="w-full h-full"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { onMounted, ref } from 'vue'
import * as monaco from 'monaco-editor'

async function call<TReq, TRes>(url: string, body: TReq): Promise<TRes> {
  const response = await fetch(url, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  const json = await response.json()

  return json as TRes
}

monaco.languages.registerCompletionItemProvider('csharp', {
  triggerCharacters: ['.', '('],
  provideCompletionItems: async (model, position) => {
    const response: Record<string, unknown> = await call(
      'https://localhost:5001/editor/completions',
      {
        code: model.getValue(),
        lineNumber: position.lineNumber,
        column: position.column,
      }
    )

    return {
      suggestions: (response.completions ??
        []) as monaco.languages.CompletionItem[],
    }
  },
})

monaco.languages.registerHoverProvider('csharp', {
  provideHover: async (model, position) => {
    const response: Record<string, unknown> = await call(
      'https://localhost:5001/editor/hover',
      {
        code: model.getValue(),
        lineNumber: position.lineNumber,
        column: position.column,
      }
    )

    return {
      contents: [{ value: response.text }],
    } as monaco.languages.Hover
  },
})

const editorElement = ref<HTMLElement>()
let editor: monaco.editor.IStandaloneCodeEditor
onMounted(() => {
  if (editorElement.value) {
    editor = monaco.editor.create(editorElement.value, {
      language: 'csharp',
      minimap: { enabled: false },
    })

    editor.onDidChangeModelContent(async (e) => {
      const diagnostics: monaco.editor.IMarkerData[] = await call(
        'https://localhost:5001/editor/diagnostics',
        {
          code: editor!.getValue(),
        }
      )

      monaco.editor.setModelMarkers(editor!.getModel()!, 'csharp', diagnostics)
    })
  }
})
</script>
