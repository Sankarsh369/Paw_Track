import React, { useState } from 'react';
import { User } from '../data/schema';
import { adoptionController } from '../controllers/adoptionController';
import { CheckCircle, XCircle, AlertTriangle, ShieldCheck, Filter } from 'lucide-react';

interface StaffReviewPortalProps {
  currentUser: User;
}

export const StaffReviewPortal: React.FC<StaffReviewPortalProps> = ({ currentUser }) => {
  const [statusFilter, setStatusFilter] = useState<string>('Pending');
  const [branchFilter, setBranchFilter] = useState<number | 'All'>('All');
  
  // Modals
  const [approveModalApp, setApproveModalApp] = useState<any | null>(null);
  const [rejectModalApp, setRejectModalApp] = useState<any | null>(null);
  const [rejectionReason, setRejectionReason] = useState<string>('');
  
  // Feedback
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  const res = adoptionController.getApplications(
    currentUser,
    statusFilter,
    branchFilter === 'All' ? undefined : branchFilter
  );
  const applications: any[] = res.success && res.data ? res.data : [];

  const handleConfirmApproval = async () => {
    if (!approveModalApp) return;
    const result = await adoptionController.approveApplication(approveModalApp.id, currentUser);
    if (result.success) {
      setFeedback({
        type: 'success',
        text: `Application #${approveModalApp.id} approved! Adoption created, animal status updated to Adopted, and open visit bookings flagged.`
      });
      setApproveModalApp(null);
      setRefreshKey((k) => k + 1);
    } else {
      setFeedback({ type: 'error', text: result.error || 'Approval failed.' });
    }
  };

  const handleConfirmRejection = async () => {
    if (!rejectModalApp) return;
    if (!rejectionReason.trim()) {
      setFeedback({ type: 'error', text: 'Rejection reason is required.' });
      return;
    }

    const result = await adoptionController.rejectApplication(rejectModalApp.id, rejectionReason, currentUser);
    if (result.success) {
      setFeedback({ type: 'success', text: `Application #${rejectModalApp.id} rejected with reason recorded.` });
      setRejectModalApp(null);
      setRejectionReason('');
      setRefreshKey((k) => k + 1);
    } else {
      setFeedback({ type: 'error', text: result.error || 'Rejection failed.' });
    }
  };

  return (
    <div className="glass-panel" key={refreshKey}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20, flexWrap: 'wrap', gap: 16 }}>
        <div>
          <h2>Adoption Application Review Queue</h2>
          <p style={{ color: '#94a3b8', fontSize: '0.9rem' }}>
            Verify applicant details, linked visit history, and execute transactional approval/rejection.
          </p>
        </div>

        <div style={{ display: 'flex', gap: 16, alignItems: 'center' }}>
          {currentUser.role === 'OrgAdmin' && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <Filter size={16} color="#94a3b8" />
              <select
                className="form-select"
                value={branchFilter}
                onChange={(e) => setBranchFilter(e.target.value === 'All' ? 'All' : Number(e.target.value))}
                style={{ width: 'auto' }}
              >
                <option value="All">All Branches</option>
                <option value={1}>Downtown Shelter (Branch 1)</option>
                <option value={2}>Westside Rescue (Branch 2)</option>
              </select>
            </div>
          )}

          <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <label style={{ fontSize: '0.9rem', color: '#94a3b8' }}>Status:</label>
            <select
              className="form-select"
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              style={{ width: 'auto' }}
            >
              <option value="Pending">Pending Review</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="All">All Applications</option>
            </select>
          </div>
        </div>
      </div>

      {feedback && (
        <div className={`alert-banner ${feedback.type === 'success' ? 'alert-info' : 'alert-error'}`}>
          {feedback.type === 'success' ? <ShieldCheck size={18} /> : <AlertTriangle size={18} />}
          <span>{feedback.text}</span>
          <button
            onClick={() => setFeedback(null)}
            style={{ marginLeft: 'auto', background: 'none', border: 'none', color: 'inherit', cursor: 'pointer' }}
          >
            ✕
          </button>
        </div>
      )}

      {applications.length === 0 ? (
        <div style={{ padding: 40, textAlign: 'center', color: '#94a3b8' }}>
          No adoption applications found matching filter criteria.
        </div>
      ) : (
        <div className="data-table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>App ID</th>
                <th>Animal</th>
                <th>Applicant</th>
                <th>Branch</th>
                <th>App Date</th>
                <th>Linked Visit</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {applications.map((app) => (
                <tr key={app.id}>
                  <td>
                    <strong>#{app.id}</strong>
                  </td>
                  <td>
                    <div>
                      <strong>{app.animalName}</strong> ({app.animalSpecies})
                    </div>
                    <span style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
                      Status: <span className={`status-pill status-${app.animalStatus}`}>{app.animalStatus}</span>
                    </span>
                  </td>
                  <td>
                    <div>{app.adopterName}</div>
                    <span style={{ fontSize: '0.8rem', color: '#64748b' }}>{app.adopterEmail}</span>
                  </td>
                  <td>{app.branchName}</td>
                  <td>{app.applicationDate}</td>
                  <td>
                    {app.linkedVisit ? (
                      <div>
                        <span style={{ color: '#34d399', fontWeight: 600 }}>Visit #{app.linkedVisit.id}</span>
                        <div style={{ fontSize: '0.8rem', color: '#94a3b8' }}>
                          {app.linkedVisit.slotDate} • {app.linkedVisit.status}
                        </div>
                      </div>
                    ) : (
                      <span style={{ color: '#64748b' }}>No Visit Linked</span>
                    )}
                  </td>
                  <td>
                    <span className={`status-pill status-${app.status}`}>{app.status}</span>
                  </td>
                  <td>
                    {app.status === 'Pending' ? (
                      <div style={{ display: 'flex', gap: 8 }}>
                        <button
                          className="btn btn-success"
                          style={{ padding: '6px 12px', fontSize: '0.85rem' }}
                          onClick={() => setApproveModalApp(app)}
                        >
                          <CheckCircle size={14} /> Approve
                        </button>
                        <button
                          className="btn btn-danger"
                          style={{ padding: '6px 12px', fontSize: '0.85rem' }}
                          onClick={() => setRejectModalApp(app)}
                        >
                          <XCircle size={14} /> Reject
                        </button>
                      </div>
                    ) : (
                      <span style={{ fontSize: '0.85rem', color: '#64748b' }}>
                        {app.status === 'Rejected' ? `Reason: ${app.rejectionReason}` : 'Finalized'}
                      </span>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* APPROVAL CONFIRMATION MODAL */}
      {approveModalApp && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">Approve Adoption — App #{approveModalApp.id}</h3>
              <button
                onClick={() => setApproveModalApp(null)}
                style={{ background: 'none', border: 'none', color: '#fff', fontSize: '1.2rem', cursor: 'pointer' }}
              >
                ✕
              </button>
            </div>

            <div className="alert-banner alert-warning">
              <AlertTriangle size={24} />
              <div>
                <strong>Critical Action Confirmation:</strong>
                <ul style={{ marginTop: 6, marginLeft: 16 }}>
                  <li>Animal <strong>{approveModalApp.animalName}</strong> status will change from <code>Available</code> to <code>Adopted</code>.</li>
                  <li>An official <strong>Adoption</strong> record will be created.</li>
                  <li>Approved by will be recorded as <strong>{currentUser.name}</strong>.</li>
                  <li>Other open visit bookings for this animal will be flagged for staff follow-up.</li>
                </ul>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12, marginTop: 24 }}>
              <button className="btn btn-secondary" onClick={() => setApproveModalApp(null)}>
                Cancel
              </button>
              <button className="btn btn-success" onClick={handleConfirmApproval}>
                Confirm & Create Adoption
              </button>
            </div>
          </div>
        </div>
      )}

      {/* REJECTION REASON MODAL */}
      {rejectModalApp && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">Reject Application — App #{rejectModalApp.id}</h3>
              <button
                onClick={() => setRejectModalApp(null)}
                style={{ background: 'none', border: 'none', color: '#fff', fontSize: '1.2rem', cursor: 'pointer' }}
              >
                ✕
              </button>
            </div>

            <div className="form-group">
              <label className="form-label">
                Rejection Reason <span style={{ color: '#f87171' }}>* (Required)</span>
              </label>
              <textarea
                className="form-textarea"
                rows={3}
                placeholder="Enter formal rejection reason for historical record..."
                value={rejectionReason}
                onChange={(e) => setRejectionReason(e.target.value)}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12, marginTop: 24 }}>
              <button className="btn btn-secondary" onClick={() => setRejectModalApp(null)}>
                Cancel
              </button>
              <button className="btn btn-danger" disabled={!rejectionReason.trim()} onClick={handleConfirmRejection}>
                Confirm Rejection
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
