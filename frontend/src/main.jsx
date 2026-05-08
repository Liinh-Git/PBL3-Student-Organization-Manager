import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'

// TODO Phase 3D:
// Implement router setup with React Router v6+
// Set up AuthContext provider wrapping the app
// Do not implement real routing in Phase 3A

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
