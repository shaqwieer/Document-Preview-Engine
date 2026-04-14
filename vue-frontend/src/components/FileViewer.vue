<template>
  <div class="file-viewer">

    <!-- Loading state -->
    <div v-if="state === 'loading'" class="viewer-loading">
      <div class="spinner" />
      <p>جاري تحميل الملف...</p>
    </div>

    <!-- Error state -->
    <div v-else-if="state === 'error'" class="viewer-error">
      <p>{{ errorMessage }}</p>
      <button @click="loadViewer">إعادة المحاولة</button>
    </div>

    <!-- ONLYOFFICE mounts here -->
    <div
      v-show="state === 'ready'"
      id="onlyoffice-container"
      ref="viewerContainer"
      class="viewer-frame"
    />

  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import axios from 'axios'

const props = defineProps({
  fileId:       { type: String, required: true },
  originalName: { type: String, default: '' }
})

const state          = ref('loading')   // 'loading' | 'ready' | 'error'
const errorMessage   = ref('')
const viewerContainer = ref(null)
let   docEditor      = null

// ── Load ONLYOFFICE JS SDK once (injected into <head>) ─────────────────
function loadOnlyOfficeScript() {
  return new Promise((resolve, reject) => {
    if (window.DocsAPI) { resolve(); return }

    // ONLYOFFICE serves its own JS from its container
    const onlyOfficeOrigin = import.meta.env.VITE_ONLYOFFICE_PUBLIC_URL
                             || 'http://localhost:8080'

    const script = document.createElement('script')
    script.src   = `${onlyOfficeOrigin}/web-apps/apps/api/documents/api.js`
    script.onload  = resolve
    script.onerror = () => reject(new Error('ONLYOFFICE SDK failed to load'))
    document.head.appendChild(script)
  })
}

// ── Fetch viewer config (JWT-signed) from .NET API ─────────────────────
async function fetchViewerConfig() {
  const { data } = await axios.get(`/api/files/${props.fileId}/viewer-config`, {
    params: { originalName: props.originalName }
  })
  return data   // { config: {...}, token: "jwt..." }
}

// ── Mount the ONLYOFFICE editor in view-only mode ──────────────────────
async function loadViewer() {
  state.value = 'loading'

  try {
    await loadOnlyOfficeScript()

    const { config, token } = await fetchViewerConfig()

    // Destroy previous instance if switching files
    if (docEditor) {
      docEditor.destroyEditor()
      docEditor = null
    }

    docEditor = new window.DocsAPI.DocEditor('onlyoffice-container', {
      ...config,
      token,
      height: '100%',
      width:  '100%',
      events: {
        onAppReady:  () => { state.value = 'ready' },
        onError:     (e) => {
          const detail = typeof e.data === 'object'
            ? (e.data?.description || e.data?.error || JSON.stringify(e.data))
            : e.data
          errorMessage.value = `خطأ في المعاينة: ${detail}`
          state.value = 'error'
        }
      }
    })

  } catch (err) {
    errorMessage.value = err.message || 'فشل تحميل الملف'
    state.value = 'error'
    console.error('FileViewer error:', err)
  }
}

// ── Reload when fileId changes ─────────────────────────────────────────
watch(() => props.fileId, loadViewer)

onMounted(loadViewer)

onBeforeUnmount(() => {
  if (docEditor) {
    docEditor.destroyEditor()
    docEditor = null
  }
})
</script>

<style scoped>
.file-viewer {
  width: 100%;
  height: 100%;
  min-height: 600px;
  position: relative;
}

.viewer-frame {
  width: 100%;
  height: 100%;
  min-height: 600px;
}

.viewer-loading,
.viewer-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 400px;
  gap: 16px;
  color: #666;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #e0e0e0;
  border-top-color: #1976d2;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin { to { transform: rotate(360deg); } }

.viewer-error button {
  padding: 8px 20px;
  background: #1976d2;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
</style>
