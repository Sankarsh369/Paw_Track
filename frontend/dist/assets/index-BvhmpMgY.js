(function(){const t=document.createElement("link").relList;if(t&&t.supports&&t.supports("modulepreload"))return;for(const n of document.querySelectorAll('link[rel="modulepreload"]'))a(n);new MutationObserver(n=>{for(const r of n)if(r.type==="childList")for(const d of r.addedNodes)d.tagName==="LINK"&&d.rel==="modulepreload"&&a(d)}).observe(document,{childList:!0,subtree:!0});function s(n){const r={};return n.integrity&&(r.integrity=n.integrity),n.referrerPolicy&&(r.referrerPolicy=n.referrerPolicy),n.crossOrigin==="use-credentials"?r.credentials="include":n.crossOrigin==="anonymous"?r.credentials="omit":r.credentials="same-origin",r}function a(n){if(n.ep)return;n.ep=!0;const r=s(n);fetch(n.href,r)}})();const S="http://localhost:5236/api";let o={token:localStorage.getItem("pawtrack_token")||null,user:JSON.parse(localStorage.getItem("pawtrack_user")||"null"),animals:[],aiDescriptionsMap:{},medicalRecordsMap:{},activeTab:"animals",theme:localStorage.getItem("pawtrack_theme")||"dark"};window.togglePasswordVisibility=U;window.fillDemoCredentials=F;window.closeModal=A;document.addEventListener("DOMContentLoaded",()=>{D(),R(),B()});function D(){document.documentElement.setAttribute("data-theme",o.theme);const e=document.getElementById("btn-theme-toggle");e&&(e.textContent=o.theme==="dark"?"🌙":"☀️")}function P(){o.theme=o.theme==="dark"?"light":"dark",localStorage.setItem("pawtrack_theme",o.theme),D()}async function u(e,t={}){const s=e.startsWith("http")?e:`${S}${e}`,a={"Content-Type":"application/json",...t.headers||{}};o.token&&(a.Authorization=`Bearer ${o.token}`);try{const n=await fetch(s,{...t,headers:a});if(n.status===401)throw l("Unauthorized (401): Session expired or invalid token. Please sign in.","error"),T(),new Error("Unauthorized (401)");if(n.status===403){const c=await n.text()||"Forbidden (403): Only authorized staff (OrgAdmin, BranchAdmin, RescueStaff) can perform this action.";throw new Error(c)}if(n.status===404){const c=await n.text()||"Not Found (404): Requested record or animal not found.";throw new Error(c)}if(n.status>=500){const c=await n.text()||`Server Error (${n.status}): Failed to generate AI description on the server.`;throw new Error(c)}if(!n.ok){const d=await n.text();throw new Error(d||`API error ${n.status}`)}const r=n.headers.get("content-type");return r&&r.includes("application/json")?await n.json():null}catch(n){throw console.error(`API Fetch Error [${e}]:`,n),n}}function B(){const e=document.getElementById("auth-view"),t=document.getElementById("dashboard-view");o.token&&o.user?(e.classList.add("hidden"),t.classList.remove("hidden"),document.getElementById("display-user-name").textContent=o.user.name||"Staff User",document.getElementById("display-user-role").textContent=o.user.role||"Staff",document.getElementById("user-avatar-initials").textContent=(o.user.name||"U").charAt(0).toUpperCase(),H(),J()):(t.classList.add("hidden"),e.classList.remove("hidden"))}async function H(){const e=document.getElementById("api-status-dot"),t=document.getElementById("api-status-text");try{await fetch(`${S}/Animal`,{method:"GET",headers:{Authorization:`Bearer ${o.token}`}}),e.className="status-dot online",t.textContent="http://localhost:5236 (Connected)"}catch{e.className="status-dot",t.textContent="http://localhost:5236 (Offline)"}}function T(){o.token=null,o.user=null,localStorage.removeItem("pawtrack_token"),localStorage.removeItem("pawtrack_user"),l("Logged out successfully","info"),B()}function R(){var e,t,s,a,n,r,d,c,i,p,v,m,f,L,$;(e=document.getElementById("login-form"))==null||e.addEventListener("submit",V),(t=document.getElementById("register-form"))==null||t.addEventListener("submit",z),(s=document.getElementById("link-show-register"))==null||s.addEventListener("click",g=>{g.preventDefault(),document.getElementById("login-form").classList.add("hidden"),document.getElementById("register-form").classList.remove("hidden"),document.getElementById("auth-title").textContent="Create Staff Account",document.getElementById("auth-subtitle").textContent="Register a new account on PawTrack",document.getElementById("toggle-auth-text").innerHTML='Already have an account? <a href="#" id="link-show-login">Sign in here</a>',document.getElementById("link-show-login").addEventListener("click",x)}),(a=document.getElementById("btn-logout"))==null||a.addEventListener("click",T),(n=document.getElementById("btn-theme-toggle"))==null||n.addEventListener("click",P),document.querySelectorAll(".nav-tab").forEach(g=>{g.addEventListener("click",()=>{const b=g.getAttribute("data-tab");j(b)})}),(r=document.getElementById("search-animal"))==null||r.addEventListener("input",y),(d=document.getElementById("filter-status"))==null||d.addEventListener("change",y),(c=document.getElementById("filter-species"))==null||c.addEventListener("change",y),(i=document.getElementById("filter-ai-status"))==null||i.addEventListener("change",E),(p=document.getElementById("select-medical-animal"))==null||p.addEventListener("change",g=>{const b=g.target.value,k=document.getElementById("btn-open-add-medical");b?(k.disabled=!1,N(parseInt(b))):(k.disabled=!0,document.getElementById("medical-records-list").innerHTML=`
        <div class="empty-state">
          <span class="empty-icon">🩺</span>
          <p>Select an animal from the dropdown above to view medical records.</p>
        </div>`)}),(v=document.getElementById("btn-open-add-animal"))==null||v.addEventListener("click",()=>{w("modal-add-animal")}),(m=document.getElementById("btn-open-add-medical"))==null||m.addEventListener("click",()=>{const g=document.getElementById("select-medical-animal").value;g&&(document.getElementById("med-animal-id").value=g,document.getElementById("med-checkup-date").value=new Date().toISOString().split("T")[0],w("modal-add-medical"))}),(f=document.getElementById("form-add-animal"))==null||f.addEventListener("submit",X),(L=document.getElementById("form-edit-ai"))==null||L.addEventListener("submit",W),($=document.getElementById("form-add-medical"))==null||$.addEventListener("submit",K)}function x(e){e&&e.preventDefault(),document.getElementById("register-form").classList.add("hidden"),document.getElementById("login-form").classList.remove("hidden"),document.getElementById("auth-title").textContent="Welcome Back",document.getElementById("auth-subtitle").textContent="Sign in to manage rescue animals, AI descriptions & medical records.",document.getElementById("toggle-auth-text").innerHTML=`Don't have an account? <a href="#" id="link-show-register">Register here</a>`,document.getElementById("link-show-register").addEventListener("click",x)}function j(e){o.activeTab=e,document.querySelectorAll(".nav-tab").forEach(t=>{t.classList.toggle("active",t.getAttribute("data-tab")===e)}),document.querySelectorAll(".tab-pane").forEach(t=>{t.classList.toggle("active",t.id===`tab-${e}`)}),e==="ai-studio"?E():e==="medical"&&C()}function U(e){const t=document.getElementById(e);t&&(t.type=t.type==="password"?"text":"password")}function F(e,t){document.getElementById("login-email").value=e,document.getElementById("login-password").value=t,l(`Autofilled demo credentials for ${e}`,"info")}async function V(e){e.preventDefault();const t=document.getElementById("login-email").value.trim(),s=document.getElementById("login-password").value.trim(),a=document.getElementById("login-spinner");a==null||a.classList.remove("hidden");try{const n=await u("/Auth/login",{method:"POST",body:JSON.stringify({email:t,password:s})});o.token=n.token,o.user={name:n.name,role:n.role},localStorage.setItem("pawtrack_token",o.token),localStorage.setItem("pawtrack_user",JSON.stringify(o.user)),l(`Welcome back, ${n.name}! Signed in as ${n.role}.`,"success"),B()}catch(n){l(`Login failed: ${n.message}`,"error")}finally{a==null||a.classList.add("hidden")}}async function z(e){e.preventDefault();const t=document.getElementById("reg-name").value.trim(),s=document.getElementById("reg-email").value.trim(),a=document.getElementById("reg-password").value.trim(),n=document.getElementById("reg-role").value,r=document.getElementById("reg-branch").value,d=r?parseInt(r):null,c=document.getElementById("reg-spinner");c==null||c.classList.remove("hidden");try{const i=await u("/Auth/register",{method:"POST",body:JSON.stringify({name:t,email:s,password:a,role:n,branchId:d})});o.token=i.token,o.user={name:i.name,role:i.role},localStorage.setItem("pawtrack_token",o.token),localStorage.setItem("pawtrack_user",JSON.stringify(o.user)),l(`Account created! Welcome, ${i.name}.`,"success"),B()}catch(i){l(`Registration failed: ${i.message}`,"error")}finally{c==null||c.classList.add("hidden")}}async function J(){await M(),await G()}async function M(){const e=document.getElementById("animal-grid");e.innerHTML=`
    <div class="loading-state">
      <span class="spinner spinner-lg"></span>
      <p>Fetching rescue animals from API...</p>
    </div>`;try{const t=await u("/Animal");o.animals=t||[],_(),y(),C()}catch{e.innerHTML=`
      <div class="empty-state">
        <span class="empty-icon">⚠️</span>
        <p>Could not connect to PawTrack API at <code>http://localhost:5236</code>.</p>
        <p class="text-muted">Ensure your ASP.NET Core backend is running (e.g. <code>dotnet run</code>).</p>
        <button class="btn btn-secondary btn-sm" onclick="loadAnimals()">🔄 Retry Connection</button>
      </div>`}}function _(){const e=o.animals.length,t=o.animals.filter(a=>a.status==="Available").length,s=o.animals.filter(a=>a.status==="UnderAssessment").length;document.getElementById("stat-total-animals").textContent=e,document.getElementById("cnt-animals").textContent=e,document.getElementById("stat-available").textContent=t,document.getElementById("stat-assessment").textContent=s}function y(){var r,d,c;const e=document.getElementById("animal-grid"),t=(((r=document.getElementById("search-animal"))==null?void 0:r.value)||"").toLowerCase(),s=((d=document.getElementById("filter-status"))==null?void 0:d.value)||"ALL",a=((c=document.getElementById("filter-species"))==null?void 0:c.value)||"ALL",n=o.animals.filter(i=>{const p=i.name.toLowerCase().includes(t)||i.species.toLowerCase().includes(t)||i.breed.toLowerCase().includes(t),v=s==="ALL"||i.status===s;let m=!0;return a==="Dog"?m=i.species.toLowerCase().includes("dog"):a==="Cat"?m=i.species.toLowerCase().includes("cat"):a==="Other"&&(m=!i.species.toLowerCase().includes("dog")&&!i.species.toLowerCase().includes("cat")),p&&v&&m});if(n.length===0){e.innerHTML=`
      <div class="empty-state" style="grid-column: 1 / -1;">
        <span class="empty-icon">🔍</span>
        <p>No animals match your search and filter criteria.</p>
      </div>`;return}e.innerHTML=n.map(i=>{const p=q(i.species),v=i.photoUrl||p,m=o.aiDescriptionsMap[i.id];let f="badge-assessment";return i.status==="Available"?f="badge-available":i.status==="Pending"?f="badge-pending":i.status==="Adopted"&&(f="badge-adopted"),`
      <div class="animal-card glass-panel">
        <div class="card-image-wrapper">
          <img src="${v}" alt="${i.name}" class="card-image" onerror="this.src='${p}'" />
          <span class="card-badge ${f}">${Q(i.status)}</span>
        </div>
        <div class="card-body">
          <div class="animal-title">
            <span>${i.name}</span>
            <span style="font-size: 0.9rem; opacity: 0.8;">${i.gender==="Male"?"♂️":"♀️"}</span>
          </div>
          <div class="animal-species">${i.breed} (${i.species})</div>

          <div class="animal-meta-list">
            <div class="meta-item">🎂 <strong>Age:</strong> ${i.age} ${i.age===1?"year":"years"} old</div>
            <div class="meta-item">📍 <strong>Rescued:</strong> ${i.rescueLocation} (${h(i.rescueDate)})</div>
            <div class="meta-item">🏢 <strong>Branch:</strong> ${i.branchName||"Shelter Branch #"+i.branchId}</div>
          </div>

          ${m?`
            <div style="margin-bottom: 1rem; padding: 0.6rem 0.8rem; background: rgba(20, 184, 166, 0.1); border-radius: var(--radius-sm); font-size: 0.82rem;">
              <strong>✨ AI Bio Status:</strong> 
              <span class="status-badge ${m.status==="Approved"?"status-approved":m.status==="Draft"?"status-draft":"status-rejected"}">
                ${m.status}
              </span>
            </div>
          `:""}

          <div class="card-actions">
            <button class="btn btn-primary btn-sm btn-block" onclick="generateAiBio(${i.id})">
              ✨ Generate AI Bio
            </button>
            <button class="btn btn-secondary btn-sm" onclick="viewHistory(${i.id}, '${O(i.name)}')">
              📜 History
            </button>
          </div>
        </div>
      </div>
    `}).join("")}function q(e){const t=(e||"").toLowerCase();return t.includes("dog")?"https://images.unsplash.com/photo-1543466835-00a7907e9de1?auto=format&fit=crop&w=600&q=80":t.includes("cat")?"https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?auto=format&fit=crop&w=600&q=80":"https://images.unsplash.com/photo-1585110396000-c9ffd4e4b308?auto=format&fit=crop&w=600&q=80"}async function G(){o.aiDescriptionsMap={};for(const t of o.animals)try{const s=await u(`/AiDescription/animal/${t.id}`);s&&(o.aiDescriptionsMap[t.id]=s)}catch{}const e=Object.keys(o.aiDescriptionsMap).length;document.getElementById("stat-ai-count").textContent=e}window.generateAiBio=async function(e){if(!e)return;const t=o.animals.find(a=>a.id===e),s=t?t.name:`Animal #${e}`;l(`Generating AI adoption bio for ${s} via backend...`,"info");try{const a=await u(`/AiDescription/generate/${e}`,{method:"POST"});a&&a.generatedText&&(o.aiDescriptionsMap[e]=a,l(`✨ Generated AI Bio for ${a.animalName||s}!`,"success"),y(),E(),document.getElementById("stat-ai-count").textContent=Object.keys(o.aiDescriptionsMap).length)}catch(a){l(`Generation Error: ${a.message}`,"error")}};function E(){var a;const e=document.getElementById("ai-descriptions-list"),t=((a=document.getElementById("filter-ai-status"))==null?void 0:a.value)||"ALL",s=Object.values(o.aiDescriptionsMap).filter(n=>t==="ALL"||n.status===t);if(s.length===0){e.innerHTML=`
      <div class="empty-state">
        <span class="empty-icon">✨</span>
        <p>No AI descriptions found. Click "Generate AI Bio" on any animal in the Animals tab!</p>
      </div>`;return}e.innerHTML=s.map(n=>{const r=o.animals.find(d=>d.id===n.animalId);return`
      <div class="ai-card glass-panel">
        <div class="ai-card-header">
          <div class="ai-animal-info">
            <span class="ai-animal-name">🐾 ${n.animalName||(r?r.name:"Animal #"+n.animalId)}</span>
            <span class="model-tag">🤖 ${n.modelVersion}</span>
          </div>
          <span class="status-badge ${n.status==="Approved"?"status-approved":n.status==="Draft"?"status-draft":"status-rejected"}">
            ${n.status}
          </span>
        </div>

        <div class="ai-text-content">${I(n.generatedText)}</div>

        <div class="ai-card-footer">
          <div>
            🕒 Generated ${h(n.createdAt)}
            ${n.reviewerName?` | 👤 Reviewed by <strong>${n.reviewerName}</strong>`:""}
            ${n.reviewNotes?`<br/><em>Notes: "${I(n.reviewNotes)}"</em>`:""}
          </div>

          <div class="ai-card-actions">
            <button class="btn btn-secondary btn-sm" onclick="openEditAiModal(${n.id}, '${O(n.generatedText)}')">
              ✏️ Edit Draft
            </button>
            <button class="btn btn-success btn-sm" onclick="approveAiBio(${n.id}, true)">
              ✅ Approve
            </button>
            <button class="btn btn-danger btn-sm" onclick="approveAiBio(${n.id}, false)">
              ❌ Reject
            </button>
          </div>
        </div>
      </div>
    `}).join("")}window.approveAiBio=async function(e,t){const s=t?"Staff approved for public adoption listing.":"Rejected for revision.";try{const a=await u(`/AiDescription/${e}/approve`,{method:"POST",body:JSON.stringify({isApproved:t,reviewNotes:s})});o.aiDescriptionsMap[a.animalId]=a,l(`Description marked as ${a.status}! Reviewer recorded from JWT.`,"success"),E(),y()}catch(a){l(`Approval error: ${a.message}`,"error")}};window.openEditAiModal=function(e,t){document.getElementById("edit-ai-id").value=e,document.getElementById("edit-ai-text").value=t,w("modal-edit-ai")};async function W(e){e.preventDefault();const t=document.getElementById("edit-ai-id").value,s=document.getElementById("edit-ai-text").value.trim();try{const a=await u(`/AiDescription/${t}`,{method:"PUT",body:JSON.stringify({generatedText:s})});o.aiDescriptionsMap[a.animalId]=a,l("AI description updated successfully","success"),A("modal-edit-ai"),E(),y()}catch(a){l(`Update failed: ${a.message}`,"error")}}window.viewHistory=async function(e,t){document.getElementById("history-animal-name").textContent=t;const s=document.getElementById("history-list");s.innerHTML='<div class="loading-state"><span class="spinner"></span><p>Loading history...</p></div>',w("modal-ai-history");try{const a=await u(`/AiDescription/animal/${e}/history`);if(!a||a.length===0){s.innerHTML='<p class="empty-state">No description history recorded yet.</p>';return}s.innerHTML=a.map(n=>`
      <div style="padding: 1rem; border-bottom: 1px solid var(--border-color); margin-bottom: 0.8rem;">
        <div style="display: flex; justify-content: space-between; margin-bottom: 0.4rem;">
          <span class="status-badge ${n.status==="Approved"?"status-approved":n.status==="Draft"?"status-draft":"status-rejected"}">${n.status}</span>
          <span style="font-size: 0.8rem; color: var(--text-muted);">${h(n.createdAt)}</span>
        </div>
        <p style="font-size: 0.92rem; line-height: 1.5; margin-bottom: 0.4rem;">${I(n.generatedText)}</p>
        <div style="font-size: 0.78rem; color: var(--text-dim);">Model: ${n.modelVersion} ${n.reviewerName?`| Reviewer: ${n.reviewerName}`:""}</div>
      </div>
    `).join("")}catch(a){s.innerHTML=`<p class="text-danger">Failed loading history: ${a.message}</p>`}};function C(){const e=document.getElementById("select-medical-animal");if(!e)return;const t=e.value;e.innerHTML='<option value="">-- Choose an Animal --</option>'+o.animals.map(s=>`<option value="${s.id}">${s.name} (${s.species} - ${s.breed})</option>`).join(""),t&&(e.value=t)}async function N(e){const t=document.getElementById("medical-records-list");t.innerHTML='<div class="loading-state"><span class="spinner spinner-lg"></span><p>Loading medical records...</p></div>';try{const s=await u(`/Medical/animal/${e}`);if(!s||s.length===0){t.innerHTML=`
        <div class="empty-state">
          <span class="empty-icon">🩺</span>
          <p>No medical records on file for this animal.</p>
        </div>`;return}t.innerHTML=s.map(a=>`
      <div class="medical-card glass-panel">
        <div class="med-header">
          <span class="med-diagnosis">🩺 ${I(a.diagnosis)}</span>
          <span style="font-size: 0.85rem; color: var(--text-muted);">Checkup Date: ${h(a.checkupDate)}</span>
        </div>
        <div class="med-details">
          <div>💊 <strong>Medication:</strong> ${a.medication||"None"}</div>
          <div>🩹 <strong>Treatment:</strong> ${a.treatment||"Standard checkup"}</div>
          <div>💉 <strong>Vaccination Date:</strong> ${a.vaccinationDate?h(a.vaccinationDate):"N/A"}</div>
          <div>👨‍⚕️ <strong>Veterinarian:</strong> ${a.veterinarianName||"Dr. Assigned"}</div>
        </div>
      </div>
    `).join("")}catch(s){t.innerHTML=`<p class="empty-state">Error loading medical records: ${s.message}</p>`}}async function K(e){e.preventDefault();const t=parseInt(document.getElementById("med-animal-id").value),s=document.getElementById("med-diagnosis").value.trim(),a=document.getElementById("med-treatment").value.trim(),n=document.getElementById("med-medication").value.trim(),r=document.getElementById("med-vaccination-date").value,d=document.getElementById("med-checkup-date").value;try{await u("/Medical",{method:"POST",body:JSON.stringify({animalId:t,diagnosis:s,treatment:a||null,medication:n||null,vaccinationDate:r?new Date(r).toISOString():null,checkupDate:new Date(d).toISOString()})}),l("Medical record saved successfully!","success"),A("modal-add-medical"),N(t)}catch(c){l(`Medical record error: ${c.message}`,"error")}}async function X(e){e.preventDefault();const t={name:document.getElementById("new-animal-name").value.trim(),species:document.getElementById("new-animal-species").value.trim(),breed:document.getElementById("new-animal-breed").value.trim(),age:parseInt(document.getElementById("new-animal-age").value),gender:document.getElementById("new-animal-gender").value,categoryId:parseInt(document.getElementById("new-animal-category").value),branchId:parseInt(document.getElementById("new-animal-branch").value),rescueLocation:document.getElementById("new-animal-location").value.trim(),photoUrl:document.getElementById("new-animal-photo").value.trim()||null,rescueDate:new Date().toISOString()};try{const s=await u("/Animal",{method:"POST",body:JSON.stringify(t)});l(`Registered animal ${s.name}!`,"success"),A("modal-add-animal"),await M()}catch(s){l(`Add animal failed: ${s.message}`,"error")}}function w(e){var t;(t=document.getElementById(e))==null||t.classList.remove("hidden")}function A(e){var t;(t=document.getElementById(e))==null||t.classList.add("hidden")}function l(e,t="info"){const s=document.getElementById("toast-container");if(!s)return;const a=document.createElement("div");a.className=`toast toast-${t}`,a.innerHTML=`
    <span>${t==="success"?"✅":t==="error"?"❌":"ℹ️"}</span>
    <div>${I(e)}</div>
  `,s.appendChild(a),setTimeout(()=>{a.style.opacity="0",a.style.transform="translateX(100%)",setTimeout(()=>a.remove(),300)},4e3)}function Q(e){return e==="UnderAssessment"?"Under Assessment":e}function h(e){return e?new Date(e).toLocaleDateString(void 0,{month:"short",day:"numeric",year:"numeric"}):"N/A"}function I(e){return e?e.replace(/[&<>"']/g,function(t){return{"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#039;"}[t]}):""}function O(e){return e?e.replace(/'/g,"\\'").replace(/"/g,'\\"'):""}
