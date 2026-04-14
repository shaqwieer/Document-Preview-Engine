<template>
  <div class="uploader">

    <div
      class="drop-zone"
      :class="{ 'drag-over': isDragging }"
      @dragover.prevent="isDragging = true"
      @dragleave="isDragging = false"
      @drop.prevent="onDrop"
      @click="fileInput.click()"
    >
      <input
        ref="fileInput"
        type="file"
        accept=".pptx,.ppt,.docx,.doc,.xlsx,.xls,.pdf"
        style="display:none"
        @change="onFileSelected"
      />
      <p>اسحب الملف هنا أو انقر للاختيار</p>
      <p class="hint">PPTX · DOCX · XLSX · PDF — حتى 200 ميجابايت</p>
    </div>

    <!-- Upload progress -->
    <div v-if="uploading" class="progress-bar">
      <div class="progress-fill" :style="{ width: progress + '%' }" />
      <span>{{ progress }}%</span>
    </div>

    <!-- Success -->
    <div v-if="uploadedFile" class="upload-success">
      <p>تم الرفع: {{ uploadedFile.originalName }}</p>
      <button @click="$emit('uploaded', uploadedFile)">معاينة الملف</button>
    </div>

    <!-- Error -->
    <p v-if="uploadError" class="upload-error">{{ uploadError }}</p>

  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const emit = defineEmits(['uploaded'])

const isDragging  = ref(false)
const uploading   = ref(false)
const progress    = ref(0)
const uploadedFile = ref(null)
const uploadError  = ref('')
const fileInput    = ref(null)

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer.files[0]
  if (file) uploadFile(file)
}

function onFileSelected(e) {
  const file = e.target.files[0]
  if (file) uploadFile(file)
}

async function uploadFile(file) {
  uploadError.value  = ''
  uploadedFile.value = null
  uploading.value    = true
  progress.value     = 0

  const form = new FormData()
  form.append('file', file)

  try {
    const { data } = await axios.post('/api/files/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: (e) => {
        progress.value = Math.round((e.loaded / e.total) * 100)
      }
    })
    uploadedFile.value = data
  } catch (err) {
    uploadError.value = err.response?.data || 'فشل رفع الملف'
  } finally {
    uploading.value = false
  }
}
</script>

<style scoped>
.uploader { display: flex; flex-direction: column; gap: 16px; }

.drop-zone {
  border: 2px dashed #bbb;
  border-radius: 12px;
  padding: 40px;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.2s, background 0.2s;
}
.drop-zone:hover, .drop-zone.drag-over {
  border-color: #1976d2;
  background: #f0f6ff;
}
.drop-zone .hint { color: #999; font-size: 13px; margin-top: 6px; }

.progress-bar {
  height: 8px;
  background: #e0e0e0;
  border-radius: 4px;
  overflow: hidden;
  position: relative;
}
.progress-fill {
  height: 100%;
  background: #1976d2;
  transition: width 0.2s;
}

.upload-success {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: #e8f5e9;
  border-radius: 8px;
}
.upload-success button {
  margin-right: auto;
  padding: 6px 16px;
  background: #2e7d32;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}
.upload-error { color: #c62828; font-size: 14px; }
</style>
