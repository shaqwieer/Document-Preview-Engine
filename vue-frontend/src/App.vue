<template>
  <div id="app" dir="rtl">
    <header class="app-header">
      <h1>منصة التعليم</h1>
    </header>

    <main class="app-main">

      <!-- Upload panel (instructor side) -->
      <section class="upload-panel">
        <h2>رفع ملف جديد</h2>
        <FileUpload @uploaded="onFileUploaded" />
      </section>

      <!-- File list -->
      <section v-if="files.length" class="file-list">
        <h2>الملفات المتاحة</h2>
        <ul>
          <li
            v-for="f in files"
            :key="f.fileId"
            :class="{ active: selectedFile?.fileId === f.fileId }"
            @click="selectFile(f)"
          >
            <span class="ext-badge">{{ f.extension.replace('.','') }}</span>
            {{ f.fileName }}
          </li>
        </ul>
      </section>

      <!-- Viewer -->
      <section v-if="selectedFile" class="viewer-panel">
        <h2>{{ selectedFile.fileName }}</h2>
        <FileViewer
          :fileId="selectedFile.fileId"
          :originalName="selectedFile.fileName"
        />
      </section>

    </main>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import axios from 'axios'
import FileUpload from './components/FileUpload.vue'
import FileViewer from './components/FileViewer.vue'

const files        = ref([])
const selectedFile = ref(null)

async function loadFiles() {
  const { data } = await axios.get('/api/files')
  files.value = data
}

function onFileUploaded(file) {
  loadFiles()
  selectedFile.value = file
}

function selectFile(file) {
  selectedFile.value = file
}

onMounted(loadFiles)
</script>

<style>
* { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: 'Cairo', 'Segoe UI', sans-serif; background: #f5f5f5; }

.app-header {
  background: #1565c0;
  color: white;
  padding: 16px 24px;
}

.app-main {
  display: grid;
  grid-template-columns: 300px 1fr;
  grid-template-rows: auto 1fr;
  gap: 16px;
  padding: 16px;
  max-width: 1400px;
  margin: 0 auto;
}

.upload-panel {
  grid-column: 1;
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
}

.file-list {
  grid-column: 1;
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
}
.file-list ul { list-style: none; display: flex; flex-direction: column; gap: 6px; }
.file-list li {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px;
  border-radius: 8px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.15s;
}
.file-list li:hover { background: #f0f4ff; }
.file-list li.active { background: #e3f2fd; font-weight: 500; }

.ext-badge {
  background: #1565c0;
  color: white;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 4px;
  text-transform: uppercase;
  min-width: 38px;
  text-align: center;
}

.viewer-panel {
  grid-column: 2;
  grid-row: 1 / 3;
  background: white;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 4px rgba(0,0,0,0.1);
  min-height: 80vh;
}

h2 { font-size: 16px; margin-bottom: 12px; color: #333; }
</style>
