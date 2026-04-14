import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  timeout: 30000
})

export const filesApi = {

  // Get all uploaded files
  list() {
    return api.get('/files').then(r => r.data)
  },

  // Upload a new file — onProgress(percent) callback
  upload(file, onProgress) {
    const form = new FormData()
    form.append('file', file)
    return api.post('/files/upload', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress: e => onProgress && onProgress(
        Math.round((e.loaded / e.total) * 100)
      )
    }).then(r => r.data)
  },

  // Get ONLYOFFICE viewer config + JWT for a file
  getViewerConfig(fileId, originalName = '') {
    return api.get(`/files/${fileId}/viewer-config`, {
      params: { originalName }
    }).then(r => r.data)
  },

  // Delete a file
  delete(fileId) {
    return api.delete(`/files/${fileId}`).then(r => r.data)
  }
}
