// ==========================================================================
// PAWTRACK FRONTEND CLIENT APPLICATION
// ==========================================================================

const API_BASE = 'http://localhost:5236/api';

// App State
let state = {
  token: localStorage.getItem('pawtrack_token') || null,
  user: JSON.parse(localStorage.getItem('pawtrack_user') || 'null'),
  animals: [],
  aiDescriptionsMap: {}, // animalId -> description object
  medicalRecordsMap: {}, // animalId -> array of records
  activeTab: 'animals',
  theme: localStorage.getItem('pawtrack_theme') || 'dark'
};

// Global helper bindings for window scope (onclicks)
window.togglePasswordVisibility = togglePasswordVisibility;
window.fillDemoCredentials = fillDemoCredentials;
window.closeModal = closeModal;

// Initialize App on DOM Load
document.addEventListener('DOMContentLoaded', () => {
  initTheme();
  setupEventListeners();
  checkAuthAndRender();
});

// ==========================================================================
// THEME MANAGEMENT
// ==========================================================================
function initTheme() {
  document.documentElement.setAttribute('data-theme', state.theme);
  const themeBtn = document.getElementById('btn-theme-toggle');
  if (themeBtn) themeBtn.textContent = state.theme === 'dark' ? '🌙' : '☀️';
}

function toggleTheme() {
  state.theme = state.theme === 'dark' ? 'light' : 'dark';
  localStorage.setItem('pawtrack_theme', state.theme);
  initTheme();
}

// ==========================================================================
// AUTHENTICATION & API UTILITIES
// ==========================================================================
async function fetchApi(endpoint, options = {}) {
  const url = endpoint.startsWith('http') ? endpoint : `${API_BASE}${endpoint}`;
  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {})
  };

  if (state.token) {
    headers['Authorization'] = `Bearer ${state.token}`;
  }

  try {
    const response = await fetch(url, { ...options, headers });
    
    if (response.status === 401) {
      showToast('Unauthorized (401): Session expired or invalid token. Please sign in.', 'error');
      logout();
      throw new Error('Unauthorized (401)');
    }

    if (response.status === 403) {
      const errText = await response.text();
      const msg = errText || 'Forbidden (403): Only authorized staff (OrgAdmin, BranchAdmin, RescueStaff) can perform this action.';
      throw new Error(msg);
    }

    if (response.status === 404) {
      const errText = await response.text();
      const msg = errText || 'Not Found (404): Requested record or animal not found.';
      throw new Error(msg);
    }

    if (response.status >= 500) {
      const errText = await response.text();
      const msg = errText || `Server Error (${response.status}): Failed to generate AI description on the server.`;
      throw new Error(msg);
    }

    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `API error ${response.status}`);
    }

    // Return JSON if present
    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
      return await response.json();
    }
    return null;
  } catch (err) {
    console.error(`API Fetch Error [${endpoint}]:`, err);
    throw err;
  }
}

function checkAuthAndRender() {
  const authView = document.getElementById('auth-view');
  const dashboardView = document.getElementById('dashboard-view');

  if (state.token && state.user) {
    authView.classList.add('hidden');
    dashboardView.classList.remove('hidden');
    
    // Update User Profile Header
    document.getElementById('display-user-name').textContent = state.user.name || 'Staff User';
    document.getElementById('display-user-role').textContent = state.user.role || 'Staff';
    document.getElementById('user-avatar-initials').textContent = (state.user.name || 'U').charAt(0).toUpperCase();

    // Check API Connection Status & Load Data
    pingApiBackend();
    loadDashboardData();
  } else {
    dashboardView.classList.add('hidden');
    authView.classList.remove('hidden');
  }
}

async function pingApiBackend() {
  const dot = document.getElementById('api-status-dot');
  const text = document.getElementById('api-status-text');
  try {
    // Attempt fetch
    await fetch(`${API_BASE}/Animal`, { method: 'GET', headers: { 'Authorization': `Bearer ${state.token}` } });
    dot.className = 'status-dot online';
    text.textContent = 'http://localhost:5236 (Connected)';
  } catch (e) {
    dot.className = 'status-dot';
    text.textContent = 'http://localhost:5236 (Offline)';
  }
}

function logout() {
  state.token = null;
  state.user = null;
  localStorage.removeItem('pawtrack_token');
  localStorage.removeItem('pawtrack_user');
  showToast('Logged out successfully', 'info');
  checkAuthAndRender();
}

// ==========================================================================
// EVENT LISTENERS & NAVIGATION
// ==========================================================================
function setupEventListeners() {
  // Login Form
  document.getElementById('login-form')?.addEventListener('submit', handleLogin);
  
  // Register Form
  document.getElementById('register-form')?.addEventListener('submit', handleRegister);
  
  // Auth Form Toggle Links
  document.getElementById('link-show-register')?.addEventListener('click', (e) => {
    e.preventDefault();
    document.getElementById('login-form').classList.add('hidden');
    document.getElementById('register-form').classList.remove('hidden');
    document.getElementById('auth-title').textContent = 'Create Staff Account';
    document.getElementById('auth-subtitle').textContent = 'Register a new account on PawTrack';
    document.getElementById('toggle-auth-text').innerHTML = `Already have an account? <a href="#" id="link-show-login">Sign in here</a>`;
    document.getElementById('link-show-login').addEventListener('click', showLoginForm);
  });

  // Logout & Theme Buttons
  document.getElementById('btn-logout')?.addEventListener('click', logout);
  document.getElementById('btn-theme-toggle')?.addEventListener('click', toggleTheme);

  // Tab Switching
  document.querySelectorAll('.nav-tab').forEach(tab => {
    tab.addEventListener('click', () => {
      const tabName = tab.getAttribute('data-tab');
      switchTab(tabName);
    });
  });

  // Filters & Search
  document.getElementById('search-animal')?.addEventListener('input', renderAnimals);
  document.getElementById('filter-status')?.addEventListener('change', renderAnimals);
  document.getElementById('filter-species')?.addEventListener('change', renderAnimals);
  document.getElementById('filter-ai-status')?.addEventListener('change', renderAiStudio);

  // Medical Animal Selector
  document.getElementById('select-medical-animal')?.addEventListener('change', (e) => {
    const animalId = e.target.value;
    const btnAddMed = document.getElementById('btn-open-add-medical');
    if (animalId) {
      btnAddMed.disabled = false;
      loadMedicalRecordsForAnimal(parseInt(animalId));
    } else {
      btnAddMed.disabled = true;
      document.getElementById('medical-records-list').innerHTML = `
        <div class="empty-state">
          <span class="empty-icon">🩺</span>
          <p>Select an animal from the dropdown above to view medical records.</p>
        </div>`;
    }
  });

  // Modals Trigger
  document.getElementById('btn-open-add-animal')?.addEventListener('click', () => {
    openModal('modal-add-animal');
  });

  document.getElementById('btn-open-add-medical')?.addEventListener('click', () => {
    const animalId = document.getElementById('select-medical-animal').value;
    if (!animalId) return;
    document.getElementById('med-animal-id').value = animalId;
    document.getElementById('med-checkup-date').value = new Date().toISOString().split('T')[0];
    openModal('modal-add-medical');
  });

  // Modal Forms Submissions
  document.getElementById('form-add-animal')?.addEventListener('submit', handleAddAnimal);
  document.getElementById('form-edit-ai')?.addEventListener('submit', handleUpdateAiDescription);
  document.getElementById('form-add-medical')?.addEventListener('submit', handleAddMedicalRecord);
}

function showLoginForm(e) {
  if (e) e.preventDefault();
  document.getElementById('register-form').classList.add('hidden');
  document.getElementById('login-form').classList.remove('hidden');
  document.getElementById('auth-title').textContent = 'Welcome Back';
  document.getElementById('auth-subtitle').textContent = 'Sign in to manage rescue animals, AI descriptions & medical records.';
  document.getElementById('toggle-auth-text').innerHTML = `Don't have an account? <a href="#" id="link-show-register">Register here</a>`;
  document.getElementById('link-show-register').addEventListener('click', showLoginForm);
}

function switchTab(tabName) {
  state.activeTab = tabName;
  document.querySelectorAll('.nav-tab').forEach(t => {
    t.classList.toggle('active', t.getAttribute('data-tab') === tabName);
  });

  document.querySelectorAll('.tab-pane').forEach(p => {
    p.classList.toggle('active', p.id === `tab-${tabName}`);
  });

  if (tabName === 'ai-studio') {
    renderAiStudio();
  } else if (tabName === 'medical') {
    populateMedicalAnimalDropdown();
  }
}

// Password visibility helper
function togglePasswordVisibility(inputId) {
  const input = document.getElementById(inputId);
  if (input) {
    input.type = input.type === 'password' ? 'text' : 'password';
  }
}

function fillDemoCredentials(email, password) {
  document.getElementById('login-email').value = email;
  document.getElementById('login-password').value = password;
  showToast(`Autofilled demo credentials for ${email}`, 'info');
}

// ==========================================================================
// AUTH HANDLERS
// ==========================================================================
async function handleLogin(e) {
  e.preventDefault();
  const email = document.getElementById('login-email').value.trim();
  const password = document.getElementById('login-password').value.trim();

  const spinner = document.getElementById('login-spinner');
  spinner?.classList.remove('hidden');

  try {
    const data = await fetchApi('/Auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password })
    });

    state.token = data.token;
    state.user = { name: data.name, role: data.role };

    localStorage.setItem('pawtrack_token', state.token);
    localStorage.setItem('pawtrack_user', JSON.stringify(state.user));

    showToast(`Welcome back, ${data.name}! Signed in as ${data.role}.`, 'success');
    checkAuthAndRender();
  } catch (err) {
    showToast(`Login failed: ${err.message}`, 'error');
  } finally {
    spinner?.classList.add('hidden');
  }
}

async function handleRegister(e) {
  e.preventDefault();
  const name = document.getElementById('reg-name').value.trim();
  const email = document.getElementById('reg-email').value.trim();
  const password = document.getElementById('reg-password').value.trim();
  const role = document.getElementById('reg-role').value;
  const branchIdVal = document.getElementById('reg-branch').value;
  const branchId = branchIdVal ? parseInt(branchIdVal) : null;

  const spinner = document.getElementById('reg-spinner');
  spinner?.classList.remove('hidden');

  try {
    const data = await fetchApi('/Auth/register', {
      method: 'POST',
      body: JSON.stringify({ name, email, password, role, branchId })
    });

    state.token = data.token;
    state.user = { name: data.name, role: data.role };

    localStorage.setItem('pawtrack_token', state.token);
    localStorage.setItem('pawtrack_user', JSON.stringify(state.user));

    showToast(`Account created! Welcome, ${data.name}.`, 'success');
    checkAuthAndRender();
  } catch (err) {
    showToast(`Registration failed: ${err.message}`, 'error');
  } finally {
    spinner?.classList.add('hidden');
  }
}

// ==========================================================================
// DASHBOARD & DATA LOADING
// ==========================================================================
async function loadDashboardData() {
  await loadAnimals();
  await loadAllAiDescriptions();
}

async function loadAnimals() {
  const grid = document.getElementById('animal-grid');
  grid.innerHTML = `
    <div class="loading-state">
      <span class="spinner spinner-lg"></span>
      <p>Fetching rescue animals from API...</p>
    </div>`;

  try {
    const animals = await fetchApi('/Animal');
    state.animals = animals || [];
    
    updateStatsCounters();
    renderAnimals();
    populateMedicalAnimalDropdown();
  } catch (err) {
    grid.innerHTML = `
      <div class="empty-state">
        <span class="empty-icon">⚠️</span>
        <p>Could not connect to PawTrack API at <code>http://localhost:5236</code>.</p>
        <p class="text-muted">Ensure your ASP.NET Core backend is running (e.g. <code>dotnet run</code>).</p>
        <button class="btn btn-secondary btn-sm" onclick="loadAnimals()">🔄 Retry Connection</button>
      </div>`;
  }
}

function updateStatsCounters() {
  const total = state.animals.length;
  const available = state.animals.filter(a => a.status === 'Available').length;
  const assessment = state.animals.filter(a => a.status === 'UnderAssessment').length;

  document.getElementById('stat-total-animals').textContent = total;
  document.getElementById('cnt-animals').textContent = total;
  document.getElementById('stat-available').textContent = available;
  document.getElementById('stat-assessment').textContent = assessment;
}

function renderAnimals() {
  const grid = document.getElementById('animal-grid');
  const search = (document.getElementById('search-animal')?.value || '').toLowerCase();
  const statusFilter = document.getElementById('filter-status')?.value || 'ALL';
  const speciesFilter = document.getElementById('filter-species')?.value || 'ALL';

  const filtered = state.animals.filter(animal => {
    const matchesSearch = animal.name.toLowerCase().includes(search) ||
                          animal.species.toLowerCase().includes(search) ||
                          animal.breed.toLowerCase().includes(search);

    const matchesStatus = statusFilter === 'ALL' || animal.status === statusFilter;
    
    let matchesSpecies = true;
    if (speciesFilter === 'Dog') matchesSpecies = animal.species.toLowerCase().includes('dog');
    else if (speciesFilter === 'Cat') matchesSpecies = animal.species.toLowerCase().includes('cat');
    else if (speciesFilter === 'Other') matchesSpecies = !animal.species.toLowerCase().includes('dog') && !animal.species.toLowerCase().includes('cat');

    return matchesSearch && matchesStatus && matchesSpecies;
  });

  if (filtered.length === 0) {
    grid.innerHTML = `
      <div class="empty-state" style="grid-column: 1 / -1;">
        <span class="empty-icon">🔍</span>
        <p>No animals match your search and filter criteria.</p>
      </div>`;
    return;
  }

  grid.innerHTML = filtered.map(animal => {
    const defaultPhoto = getFallbackPhoto(animal.species);
    const photoUrl = animal.photoUrl || defaultPhoto;
    const aiDesc = state.aiDescriptionsMap[animal.id];

    let badgeClass = 'badge-assessment';
    if (animal.status === 'Available') badgeClass = 'badge-available';
    else if (animal.status === 'Pending') badgeClass = 'badge-pending';
    else if (animal.status === 'Adopted') badgeClass = 'badge-adopted';

    return `
      <div class="animal-card glass-panel">
        <div class="card-image-wrapper">
          <img src="${photoUrl}" alt="${animal.name}" class="card-image" onerror="this.src='${defaultPhoto}'" />
          <span class="card-badge ${badgeClass}">${formatStatus(animal.status)}</span>
        </div>
        <div class="card-body">
          <div class="animal-title">
            <span>${animal.name}</span>
            <span style="font-size: 0.9rem; opacity: 0.8;">${animal.gender === 'Male' ? '♂️' : '♀️'}</span>
          </div>
          <div class="animal-species">${animal.breed} (${animal.species})</div>

          <div class="animal-meta-list">
            <div class="meta-item">🎂 <strong>Age:</strong> ${animal.age} ${animal.age === 1 ? 'year' : 'years'} old</div>
            <div class="meta-item">📍 <strong>Rescued:</strong> ${animal.rescueLocation} (${formatDate(animal.rescueDate)})</div>
            <div class="meta-item">🏢 <strong>Branch:</strong> ${animal.branchName || 'Shelter Branch #' + animal.branchId}</div>
          </div>

          ${aiDesc ? `
            <div style="margin-bottom: 1rem; padding: 0.6rem 0.8rem; background: rgba(20, 184, 166, 0.1); border-radius: var(--radius-sm); font-size: 0.82rem;">
              <strong>✨ AI Bio Status:</strong> 
              <span class="status-badge ${aiDesc.status === 'Approved' ? 'status-approved' : aiDesc.status === 'Draft' ? 'status-draft' : 'status-rejected'}">
                ${aiDesc.status}
              </span>
            </div>
          ` : ''}

          <div class="card-actions">
            <button class="btn btn-primary btn-sm btn-block" onclick="generateAiBio(${animal.id})">
              ✨ Generate AI Bio
            </button>
            <button class="btn btn-secondary btn-sm" onclick="viewHistory(${animal.id}, '${escapeJs(animal.name)}')">
              📜 History
            </button>
          </div>
        </div>
      </div>
    `;
  }).join('');
}

function getFallbackPhoto(species) {
  const s = (species || '').toLowerCase();
  if (s.includes('dog')) return 'https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=600&q=80';
  if (s.includes('cat')) return 'https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=600&q=80';
  return 'https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?auto=format&fit=crop&w=600&q=80';
}

// ==========================================================================
// AI DESCRIPTIONS STUDIO & API INTEGRATION
// ==========================================================================
async function loadAllAiDescriptions() {
  state.aiDescriptionsMap = {};
  
  for (const animal of state.animals) {
    try {
      const desc = await fetchApi(`/AiDescription/animal/${animal.id}`);
      if (desc) {
        state.aiDescriptionsMap[animal.id] = desc;
      }
    } catch (e) {
      // No description generated yet for this animal
    }
  }

  const generatedCount = Object.keys(state.aiDescriptionsMap).length;
  document.getElementById('stat-ai-count').textContent = generatedCount;
}

window.generateAiBio = async function(animalId) {
  if (!animalId) return;

  const animal = state.animals.find(a => a.id === animalId);
  const animalName = animal ? animal.name : `Animal #${animalId}`;

  showToast(`Generating AI adoption bio for ${animalName} via backend...`, 'info');

  try {
    // Sends POST http://localhost:5236/api/AiDescription/generate/{animalId} with Authorization: Bearer <token>
    const desc = await fetchApi(`/AiDescription/generate/${animalId}`, {
      method: 'POST'
    });

    if (desc && desc.generatedText) {
      state.aiDescriptionsMap[animalId] = desc;
      
      showToast(`✨ Generated AI Bio for ${desc.animalName || animalName}!`, 'success');
      renderAnimals();
      renderAiStudio();
      document.getElementById('stat-ai-count').textContent = Object.keys(state.aiDescriptionsMap).length;
    }
  } catch (err) {
    showToast(`Generation Error: ${err.message}`, 'error');
  }
};

function renderAiStudio() {
  const container = document.getElementById('ai-descriptions-list');
  const filter = document.getElementById('filter-ai-status')?.value || 'ALL';

  const list = Object.values(state.aiDescriptionsMap).filter(d => {
    return filter === 'ALL' || d.status === filter;
  });

  if (list.length === 0) {
    container.innerHTML = `
      <div class="empty-state">
        <span class="empty-icon">✨</span>
        <p>No AI descriptions found. Click "Generate AI Bio" on any animal in the Animals tab!</p>
      </div>`;
    return;
  }

  container.innerHTML = list.map(desc => {
    const animal = state.animals.find(a => a.id === desc.animalId);
    return `
      <div class="ai-card glass-panel">
        <div class="ai-card-header">
          <div class="ai-animal-info">
            <span class="ai-animal-name">🐾 ${desc.animalName || (animal ? animal.name : 'Animal #' + desc.animalId)}</span>
            <span class="model-tag">🤖 ${desc.modelVersion}</span>
          </div>
          <span class="status-badge ${desc.status === 'Approved' ? 'status-approved' : desc.status === 'Draft' ? 'status-draft' : 'status-rejected'}">
            ${desc.status}
          </span>
        </div>

        <div class="ai-text-content">${escapeHtml(desc.generatedText)}</div>

        <div class="ai-card-footer">
          <div>
            🕒 Generated ${formatDate(desc.createdAt)}
            ${desc.reviewerName ? ` | 👤 Reviewed by <strong>${desc.reviewerName}</strong>` : ''}
            ${desc.reviewNotes ? `<br/><em>Notes: "${escapeHtml(desc.reviewNotes)}"</em>` : ''}
          </div>

          <div class="ai-card-actions">
            <button class="btn btn-secondary btn-sm" onclick="openEditAiModal(${desc.id}, '${escapeJs(desc.generatedText)}')">
              ✏️ Edit Draft
            </button>
            <button class="btn btn-success btn-sm" onclick="approveAiBio(${desc.id}, true)">
              ✅ Approve
            </button>
            <button class="btn btn-danger btn-sm" onclick="approveAiBio(${desc.id}, false)">
              ❌ Reject
            </button>
          </div>
        </div>
      </div>
    `;
  }).join('');
}

window.approveAiBio = async function(id, isApproved) {
  const notes = isApproved ? 'Staff approved for public adoption listing.' : 'Rejected for revision.';
  try {
    const updated = await fetchApi(`/AiDescription/${id}/approve`, {
      method: 'POST',
      body: JSON.stringify({ isApproved, reviewNotes: notes })
    });

    state.aiDescriptionsMap[updated.animalId] = updated;
    showToast(`Description marked as ${updated.status}! Reviewer recorded from JWT.`, 'success');
    renderAiStudio();
    renderAnimals();
  } catch (err) {
    showToast(`Approval error: ${err.message}`, 'error');
  }
};

window.openEditAiModal = function(id, text) {
  document.getElementById('edit-ai-id').value = id;
  document.getElementById('edit-ai-text').value = text;
  openModal('modal-edit-ai');
};

async function handleUpdateAiDescription(e) {
  e.preventDefault();
  const id = document.getElementById('edit-ai-id').value;
  const text = document.getElementById('edit-ai-text').value.trim();

  try {
    const updated = await fetchApi(`/AiDescription/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ generatedText: text })
    });

    state.aiDescriptionsMap[updated.animalId] = updated;
    showToast('AI description updated successfully', 'success');
    closeModal('modal-edit-ai');
    renderAiStudio();
    renderAnimals();
  } catch (err) {
    showToast(`Update failed: ${err.message}`, 'error');
  }
}

window.viewHistory = async function(animalId, animalName) {
  document.getElementById('history-animal-name').textContent = animalName;
  const historyList = document.getElementById('history-list');
  historyList.innerHTML = `<div class="loading-state"><span class="spinner"></span><p>Loading history...</p></div>`;
  openModal('modal-ai-history');

  try {
    const history = await fetchApi(`/AiDescription/animal/${animalId}/history`);
    if (!history || history.length === 0) {
      historyList.innerHTML = `<p class="empty-state">No description history recorded yet.</p>`;
      return;
    }

    historyList.innerHTML = history.map(item => `
      <div style="padding: 1rem; border-bottom: 1px solid var(--border-color); margin-bottom: 0.8rem;">
        <div style="display: flex; justify-content: space-between; margin-bottom: 0.4rem;">
          <span class="status-badge ${item.status === 'Approved' ? 'status-approved' : item.status === 'Draft' ? 'status-draft' : 'status-rejected'}">${item.status}</span>
          <span style="font-size: 0.8rem; color: var(--text-muted);">${formatDate(item.createdAt)}</span>
        </div>
        <p style="font-size: 0.92rem; line-height: 1.5; margin-bottom: 0.4rem;">${escapeHtml(item.generatedText)}</p>
        <div style="font-size: 0.78rem; color: var(--text-dim);">Model: ${item.modelVersion} ${item.reviewerName ? `| Reviewer: ${item.reviewerName}` : ''}</div>
      </div>
    `).join('');
  } catch (err) {
    historyList.innerHTML = `<p class="text-danger">Failed loading history: ${err.message}</p>`;
  }
};

// ==========================================================================
// MEDICAL RECORDS MANAGEMENT
// ==========================================================================
function populateMedicalAnimalDropdown() {
  const select = document.getElementById('select-medical-animal');
  if (!select) return;

  const currentVal = select.value;
  select.innerHTML = `<option value="">-- Choose an Animal --</option>` +
    state.animals.map(a => `<option value="${a.id}">${a.name} (${a.species} - ${a.breed})</option>`).join('');
  
  if (currentVal) select.value = currentVal;
}

async function loadMedicalRecordsForAnimal(animalId) {
  const container = document.getElementById('medical-records-list');
  container.innerHTML = `<div class="loading-state"><span class="spinner spinner-lg"></span><p>Loading medical records...</p></div>`;

  try {
    const records = await fetchApi(`/Medical/animal/${animalId}`);
    if (!records || records.length === 0) {
      container.innerHTML = `
        <div class="empty-state">
          <span class="empty-icon">🩺</span>
          <p>No medical records on file for this animal.</p>
        </div>`;
      return;
    }

    container.innerHTML = records.map(rec => `
      <div class="medical-card glass-panel">
        <div class="med-header">
          <span class="med-diagnosis">🩺 ${escapeHtml(rec.diagnosis)}</span>
          <span style="font-size: 0.85rem; color: var(--text-muted);">Checkup Date: ${formatDate(rec.checkupDate)}</span>
        </div>
        <div class="med-details">
          <div>💊 <strong>Medication:</strong> ${rec.medication || 'None'}</div>
          <div>🩹 <strong>Treatment:</strong> ${rec.treatment || 'Standard checkup'}</div>
          <div>💉 <strong>Vaccination Date:</strong> ${rec.vaccinationDate ? formatDate(rec.vaccinationDate) : 'N/A'}</div>
          <div>👨‍⚕️ <strong>Veterinarian:</strong> ${rec.veterinarianName || 'Dr. Assigned'}</div>
        </div>
      </div>
    `).join('');
  } catch (err) {
    container.innerHTML = `<p class="empty-state">Error loading medical records: ${err.message}</p>`;
  }
}

async function handleAddMedicalRecord(e) {
  e.preventDefault();
  const animalId = parseInt(document.getElementById('med-animal-id').value);
  const diagnosis = document.getElementById('med-diagnosis').value.trim();
  const treatment = document.getElementById('med-treatment').value.trim();
  const medication = document.getElementById('med-medication').value.trim();
  const vaccinationDateVal = document.getElementById('med-vaccination-date').value;
  const checkupDateVal = document.getElementById('med-checkup-date').value;

  try {
    await fetchApi('/Medical', {
      method: 'POST',
      body: JSON.stringify({
        animalId,
        diagnosis,
        treatment: treatment || null,
        medication: medication || null,
        vaccinationDate: vaccinationDateVal ? new Date(vaccinationDateVal).toISOString() : null,
        checkupDate: new Date(checkupDateVal).toISOString()
      })
    });

    showToast('Medical record saved successfully!', 'success');
    closeModal('modal-add-medical');
    loadMedicalRecordsForAnimal(animalId);
  } catch (err) {
    showToast(`Medical record error: ${err.message}`, 'error');
  }
}

// ==========================================================================
// ANIMAL CREATION HANDLER
// ==========================================================================
async function handleAddAnimal(e) {
  e.preventDefault();
  const dto = {
    name: document.getElementById('new-animal-name').value.trim(),
    species: document.getElementById('new-animal-species').value.trim(),
    breed: document.getElementById('new-animal-breed').value.trim(),
    age: parseInt(document.getElementById('new-animal-age').value),
    gender: document.getElementById('new-animal-gender').value,
    categoryId: parseInt(document.getElementById('new-animal-category').value),
    branchId: parseInt(document.getElementById('new-animal-branch').value),
    rescueLocation: document.getElementById('new-animal-location').value.trim(),
    photoUrl: document.getElementById('new-animal-photo').value.trim() || null,
    rescueDate: new Date().toISOString()
  };

  try {
    const created = await fetchApi('/Animal', {
      method: 'POST',
      body: JSON.stringify(dto)
    });

    showToast(`Registered animal ${created.name}!`, 'success');
    closeModal('modal-add-animal');
    await loadAnimals();
  } catch (err) {
    showToast(`Add animal failed: ${err.message}`, 'error');
  }
}

// ==========================================================================
// MODAL & UTILITY FUNCTIONS
// ==========================================================================
function openModal(id) {
  document.getElementById(id)?.classList.remove('hidden');
}

function closeModal(id) {
  document.getElementById(id)?.classList.add('hidden');
}

function showToast(message, type = 'info') {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  toast.innerHTML = `
    <span>${type === 'success' ? '✅' : type === 'error' ? '❌' : 'ℹ️'}</span>
    <div>${escapeHtml(message)}</div>
  `;

  container.appendChild(toast);

  setTimeout(() => {
    toast.style.opacity = '0';
    toast.style.transform = 'translateX(100%)';
    setTimeout(() => toast.remove(), 300);
  }, 4000);
}

function formatStatus(status) {
  if (status === 'UnderAssessment') return 'Under Assessment';
  return status;
}

function formatDate(dateStr) {
  if (!dateStr) return 'N/A';
  return new Date(dateStr).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  });
}

function escapeHtml(str) {
  if (!str) return '';
  return str.replace(/[&<>"']/g, function(m) {
    return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#039;' }[m];
  });
}

function escapeJs(str) {
  if (!str) return '';
  return str.replace(/'/g, "\\'").replace(/"/g, '\\"');
}
