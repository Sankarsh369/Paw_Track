import React, { useState } from 'react';
import { User } from '../data/schema';
import { dbContext } from '../data/dbContext';
import { AdopterPortal } from './AdopterPortal';
import { StaffReviewPortal } from './StaffReviewPortal';
import { AdoptionFollowUpPortal } from './AdoptionFollowUpPortal';
import { PawPrint, HeartHandshake, ShieldCheck, History, RotateCcw } from 'lucide-react';

export const App: React.FC = () => {
  // Current active user for testing role-based & branch-scoped authorization
  const [currentUserId, setCurrentUserId] = useState<number>(2); // Default to Branch 1 Admin (John)
  const [activeTab, setActiveTab] = useState<'adopter' | 'review' | 'followup'>('review');
  const [resetCount, setResetCount] = useState(0);

  const currentUser: User = dbContext.users.find((u) => u.id === currentUserId) || dbContext.users[0];

  const handleResetData = () => {
    dbContext.resetToSeed();
    setResetCount((c) => c + 1);
  };

  return (
    <div className="app-container" key={resetCount}>
      {/* Header & User Switcher */}
      <header className="header-bar glass-panel" style={{ padding: '16px 24px', marginBottom: 24 }}>
        <div className="brand-title">
          <PawPrint size={32} color="#6366f1" />
          <span>PawTrack</span>
          <span style={{ fontSize: '0.9rem', color: '#94a3b8', fontWeight: 400 }}>
            Adoption & Follow-up Module
          </span>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 16 }}>
          <div className="user-switcher">
            <span style={{ fontSize: '0.85rem', color: '#94a3b8' }}>Logged in as:</span>
            <select
              value={currentUserId}
              onChange={(e) => setCurrentUserId(Number(e.target.value))}
            >
              <optgroup label="Admins & Staff">
                <option value={1}>Sarah Admin (Org Admin - All Branches)</option>
                <option value={2}>John Manager (Branch 1 Admin - Downtown)</option>
                <option value={3}>Alice Staff (Branch 1 Rescue Staff - Downtown)</option>
                <option value={5}>Elena Manager (Branch 2 Admin - Westside)</option>
                <option value={6}>David Staff (Branch 2 Rescue Staff - Westside)</option>
              </optgroup>
              <optgroup label="Adopters">
                <option value={8}>Mark Adopter (User #8)</option>
                <option value={9}>Emily Adopter (User #9)</option>
                <option value={10}>Michael Adopter (User #10)</option>
              </optgroup>
            </select>
            <span className={`role-badge role-${currentUser.role}`}>
              {currentUser.role} {currentUser.branchId ? `(Branch ${currentUser.branchId})` : '(Org-wide)'}
            </span>
          </div>

          <button
            className="btn btn-secondary"
            onClick={handleResetData}
            title="Reset database state to original sample dataset"
            style={{ padding: '8px 12px', fontSize: '0.85rem' }}
          >
            <RotateCcw size={14} /> Reset Demo Data
          </button>
        </div>
      </header>

      {/* Main Navigation Tabs */}
      <nav className="nav-tabs">
        <button
          className={`tab-btn ${activeTab === 'adopter' ? 'active' : ''}`}
          onClick={() => setActiveTab('adopter')}
        >
          <HeartHandshake size={18} /> Adopter Portal
        </button>
        <button
          className={`tab-btn ${activeTab === 'review' ? 'active' : ''}`}
          onClick={() => setActiveTab('review')}
        >
          <ShieldCheck size={18} /> Application Review Queue
        </button>
        <button
          className={`tab-btn ${activeTab === 'followup' ? 'active' : ''}`}
          onClick={() => setActiveTab('followup')}
        >
          <History size={18} /> Adoption & Follow-Up History
        </button>
      </nav>

      {/* Main View Display */}
      <main>
        {activeTab === 'adopter' && <AdopterPortal currentUser={currentUser} />}
        {activeTab === 'review' && <StaffReviewPortal currentUser={currentUser} />}
        {activeTab === 'followup' && <AdoptionFollowUpPortal currentUser={currentUser} />}
      </main>
    </div>
  );
};
