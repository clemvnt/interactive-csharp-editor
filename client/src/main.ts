import { createApp } from 'vue'
import App from './App.vue'
import './index.css'
import Worker from 'monaco-editor/esm/vs/editor/editor.worker.js?worker'

window.MonacoEnvironment = {
  getWorker(workerId) {
    return new Worker()
  },
}

createApp(App).mount('#app')
