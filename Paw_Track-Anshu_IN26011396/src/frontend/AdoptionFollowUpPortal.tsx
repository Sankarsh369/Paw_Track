import React, { useState } from 'react';
import { User } from '../data/schema';
import { followUpController } from '../controllers/followUpController';
import { Calendar, CheckCircle2, Clock, Plus, UserCheck } from 'lucide-react';

interface AdoptionFollowUpPortalProps {
  currentUser: User;
}

export const AdoptionFollowUpPortal: React.FC<AdoptionFollowUpPortalProps> = ({ currentUser }) => {
  const [selectedAdoptionId, setSelectedAdoptionId] = useState<number | null>(null);
  
  // Schedule Form
  const [showScheduleModal, setShowScheduleModal] = useState(false);
  const [followUpDate, setFollowUpDate] = useState<string>('');
  const [followUpNotes, setFollowUpNotes] = useState<string>('');

  // Feedback
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const adoptionsRes = followUpController.getAdoptions(currentUser);
  const adoptions: any[] = adoptionsRes.success && adoptionsRes.data ? adoptionsRes.data : [];

  const selectedAdoption = adoptions.find((a) => a.id === selectedAdoptionId) || adoptions[0];

  const handleScheduleFollowUp = () => {
    if (!selectedAdoption || !followUpDate || !followUpNotes.trim()) return;

    const res = followUpController.createFollowUp(
      {
        adoptionId: selectedAdoption.id,
        followUpDate: followUpDate,
        notes: followUpNotes
      },
      currentUser
    );

    if (res.success) {
      setFeedback({ type: 'success', text: 'Post-adoption follow-up scheduled successfully.' });
      setShowScheduleModal(false);
      setFollowUpDate('');
      setFollowUpNotes('');
      setRefreshKey((k) => k + 1);
    } else {
      setFeedback({ type: 'error', text: res.error || 'Failed to schedule follow-up.' });
    }
  };

  const handleCompleteFollowUp = (followUpId: number) => {
    const res = followUpController.completeFollowUp(followUpId, 'Check-in verified and completed.', currentUser);
    if (res.success) {
      setFeedback({ type: 'success', text: 'Follow-up marked as Completed.' });
      setRefreshKey((k) => k + 1);
    } else {
      setFeedback({ type: 'error', text: res.error || 'Failed to update follow-up.' });
    }
  };

  return (
    <div key={refreshKey}>
      {feedback && (
        <div className={`alert-banner ${feedback.type === 'success' ? 'alert-info' : 'alert-error'}`}>
          <CheckCircle2 size={18} />
          <span>{feedback.text}</span>
          <button
            onClick={() => setFeedback(null)}
            style={{ marginLeft: 'auto', background: 'none', border: 'none', color: 'inherit', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>
      )}

      {adoptions.length === 0 ? (
        <div className="glass-panel" style={{ textAlign: 'center', padding: 40, color: '#94a3b8' }}>
          No adoptions recorded for your branch yet.
        </div>
      ) : (
        <div style={{ display: 'grid', gridTemplateColumns: '320px 1fr', gap: 24 }}>
          {/* Left Column: Adoption Records List */}
          <div className="glass-panel" style={{ padding: 16 }}>
            <h3 style={{ marginBottom: 16 }}>Adoptions ({adoptions.length})</h3>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {adoptions.map((adoption) => (
                <div
                  key={adoption.id}
                  onClick={() => setSelectedAdoptionId(adoption.id)}
                  style={{
                    padding: 14,
                    borderRadius: 10,
                    background: selectedAdoption?.id === adoption.id ? 'rgba(99, 102, 241, 0.25)' : 'rgba(15, 23, 42, 0.5)',
                    border: selectedAdoption?.id === adoption.id ? '1px solid #6366f1' : '1px solid rgba(255, 255, 255, 0.08)',
                    cursor: 'pointer',
                    transition: 'all 0.2s ease'
                  }}
                >
                  <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 4 }}>
                    <strong style={{ fontSize: '1.05rem' }}>{adoption.animalName}</strong>
                    <span style={{ fontSize: '0.8rem', color: '#a5b4fc' }}>#{adoption.id}</span>
                  </div>
                  <div style={{ fontSize: '0.85rem', color: '#94a3b8' }}>
                    Adopter: {adoption.adopterName}
                  </div>
                  <div style={{ fontSize: '0.8rem', color: '#64748b', marginTop: 4 }}>
                    Date: {adoption.adoptionDate} • {adoption.followUpCount} Check-ins
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* Right Column: Selected Adoption Details & Follow-up History */}
          {selectedAdoption && (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 24 }}>
              {/* Adoption Summary Card */}
              <div className="glass-panel">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 16 }}>
                  <div>
                    <h2>Adoption Details: {selectedAdoption.animalName}</h2>
                    <p style={{ color: '#94a3b8', fontSize: '0.9rem' }}>
                      Branch: {selectedAdoption.branchName}
                    </p>
                  </div>
                  {(currentUser.role !== 'Adopter') && (
                    <button className="btn btn-primary" onClick={() => setShowScheduleModal(true)}>
                      <Plus size={16} /> Schedule Check-in
                    </button>
                  )}
                </div>

                <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: 16, background: 'rgba(15, 23, 42, 0.6)', padding: 16, borderRadius: 12 }}>
                  <div>
                    <div style={{ fontSize: '0.8rem', color: '#64748b' }}>Adopter</div>
                    <div style={{ fontWeight: 600 }}>{selectedAdoption.adopterName}</div>
                    <div style={{ fontSize: '0.8rem', color: '#94a3b8' }}>{selectedAdoption.adopterEmail}</div>
                  </div>
                  <div>
                    <div style={{ fontSize: '0.8rem', color: '#64748b' }}>Approved By</div>
                    <div style={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 4 }}>
                      <UserCheck size={14} color="#34d399" /> {selectedAdoption.approvedByName}
                    </div>
                  </div>
                  <div>
                    <div style={{ fontSize: '0.8rem', color: '#64748b' }}>Adoption Date</div>
                    <div style={{ fontWeight: 600, display: 'flex', alignItems: 'center', gap: 4 }}>
                      <Calendar size={14} color="#6366f1" /> {selectedAdoption.adoptionDate}
                    </div>
                  </div>
                </div>
              </div>

              {/* Follow-up Timeline ($1:N$ Multiple Follow-ups supported) */}
              <div className="glass-panel">
                <h3>Post-Adoption Follow-up History ({selectedAdoption.followUpHistory.length})</h3>
                <p style={{ color: '#94a3b8', fontSize: '0.88rem', marginBottom: 16 }}>
                  Chronological record of post-adoption check-ins, medical updates, and home visits.
                </p>

                {selectedAdoption.followUpHistory.length === 0 ? (
                  <p style={{ color: '#64748b' }}>No follow-up check-ins scheduled yet for this adoption.</p>
                ) : (
                  <div className="timeline">
                    {selectedAdoption.followUpHistory.map((fu: any) => (
                      <div className="timeline-item" key={fu.id}>
                        <div className="timeline-icon">
                          {fu.status === 'Completed' ? (
                            <CheckCircle2 size={20} color="#34d399" />
                          ) : (
                            <Clock size={20} color="#fbbf24" />
                          )}
                        </div>
                        <div style={{ flexGrow: 1 }}>
                          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 4 }}>
                            <strong>Follow-up #{fu.id} ({fu.followUpDate})</strong>
                            <span className={`status-pill status-${fu.status}`}>{fu.status}</span>
                          </div>
                          <div style={{ fontSize: '0.85rem', color: '#94a3b8', marginBottom: 6 }}>
                            Conducted By: {fu.conductedByName} ({fu.conductedByRole})
                          </div>
                          <div style={{ fontSize: '0.92rem', color: '#e2e8f0', background: 'rgba(0,0,0,0.2)', padding: 10, borderRadius: 8 }}>
                            {fu.notes}
                          </div>

                          {fu.status === 'Scheduled' && (currentUser.role !== 'Adopter') && (
                            <div style={{ marginTop: 10, display: 'flex', justifyContent: 'flex-end' }}>
                              <button
                                className="btn btn-secondary"
                                style={{ padding: '4px 12px', fontSize: '0.82rem' }}
                                onClick={() => handleCompleteFollowUp(fu.id)}
                              >
                                <CheckCircle2 size={14} /> Mark Completed
                              </button>
                            </div>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {/* SCHEDULE FOLLOW-UP MODAL */}
      {showScheduleModal && selectedAdoption && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">Schedule Check-in for {selectedAdoption.animalName}</h3>
              <button
                onClick={() => setShowScheduleModal(false)}
                style={{ background: 'none', border: 'none', color: '#fff', fontSize: '1.2rem', cursor: 'pointer' }}
              >
                ✕
              </button>
            </div>

            <div className="form-group">
              <label className="form-label">Follow-up Date:</label>
              <input
                type="date"
                className="form-input"
                value={followUpDate}
                onChange={(e) => setFollowUpDate(e.target.value)}
              />
            </div>

            <div className="form-group">
              <label className="form-label">Check-in Notes / Plan:</label>
              <textarea
                className="form-textarea"
                rows={3}
                placeholder="Enter objectives (e.g. 1-month health check, home visit, vaccination review)..."
                value={followUpNotes}
                onChange={(e) => setFollowUpNotes(e.target.value)}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12, marginTop: 24 }}>
              <button className="btn btn-secondary" onClick={() => setShowScheduleModal(false)}>
                Cancel
              </button>
              <button
                className="btn btn-primary"
                disabled={!followUpDate || !followUpNotes.trim()}
                onClick={handleScheduleFollowUp}
              >
                Save Follow-Up
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
