import React, { useState } from 'react';
import { User, Animal } from '../data/schema';
import { animalService } from '../services/animalService';
import { visitController } from '../controllers/visitController';
import { adoptionController } from '../controllers/adoptionController';
import { AboutBento } from '../components/ui/about-bento';
import { Calendar, Heart, CheckCircle, AlertCircle, FileText } from 'lucide-react';

interface AdopterPortalProps {
  currentUser: User;
}

export const AdopterPortal: React.FC<AdopterPortalProps> = ({ currentUser }) => {
  const [activeSubTab, setActiveSubTab] = useState<'animals' | 'applications' | 'visits'>('animals');
  const [selectedBranch, setSelectedBranch] = useState<number | 'All'>('All');
  
  // Modals
  const [visitModalAnimal, setVisitModalAnimal] = useState<Animal | null>(null);
  const [selectedSlotId, setSelectedSlotId] = useState<number | null>(null);
  
  const [appModalAnimal, setAppModalAnimal] = useState<Animal | null>(null);
  const [selectedVisitId, setSelectedVisitId] = useState<number | null>(null);
  const [appNotes, setAppNotes] = useState<string>('');
  
  // Feedback messages
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  // Refresh trigger
  const [refreshKey, setRefreshKey] = useState(0);

  const animals = animalService.getAnimals(currentUser, selectedBranch === 'All' ? undefined : selectedBranch);
  const myApplicationsRes = adoptionController.getApplications(currentUser);
  const myApplications: any[] = myApplicationsRes.success && myApplicationsRes.data ? myApplicationsRes.data : [];
  
  const myVisitsRes = visitController.getVisitBookings(currentUser);
  const myVisits: any[] = myVisitsRes.success && myVisitsRes.data ? myVisitsRes.data : [];

  const handleBookVisit = () => {
    if (!visitModalAnimal || !selectedSlotId) return;
    const res = visitController.bookVisit(
      { visitSlotId: selectedSlotId, animalId: visitModalAnimal.id },
      currentUser
    );

    if (res.success) {
      setMessage({ type: 'success', text: `Visit slot booked for ${visitModalAnimal.name}! Animal remains available.` });
      setVisitModalAnimal(null);
      setSelectedSlotId(null);
      setRefreshKey((k) => k + 1);
    } else {
      setMessage({ type: 'error', text: res.error || 'Failed to book visit.' });
    }
  };

  const handleSubmitApplication = () => {
    if (!appModalAnimal) return;
    const res = adoptionController.submitApplication(
      {
        animalId: appModalAnimal.id,
        visitBookingId: selectedVisitId,
        notes: appNotes
      },
      currentUser
    );

    if (res.success) {
      setMessage({ type: 'success', text: `Adoption application submitted for ${appModalAnimal.name}!` });
      setAppModalAnimal(null);
      setSelectedVisitId(null);
      setAppNotes('');
      setRefreshKey((k) => k + 1);
    } else {
      setMessage({ type: 'error', text: res.error || 'Failed to submit application.' });
    }
  };

  const getAvailableSlots = (branchId: number) => {
    const res = visitController.getVisitSlots(branchId);
    return res.success && res.data ? res.data : [];
  };

  return (
    <div key={refreshKey}>
      {message && (
        <div className={`alert-banner ${message.type === 'success' ? 'alert-info' : 'alert-error'}`}>
          {message.type === 'success' ? <CheckCircle size={18} /> : <AlertCircle size={18} />}
          <span>{message.text}</span>
          <button
            onClick={() => setMessage(null)}
            style={{ marginLeft: 'auto', background: 'none', border: 'none', color: 'inherit', cursor: 'pointer' }}
          >
            ×
          </button>
        </div>
      )}

      {/* Sub Navigation */}
      <div style={{ display: 'flex', gap: 12, marginBottom: 24 }}>
        <button
          className={`btn ${activeSubTab === 'animals' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveSubTab('animals')}
        >
          <Heart size={16} /> Browse Pets & Bento Showcase
        </button>
        <button
          className={`btn ${activeSubTab === 'applications' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveSubTab('applications')}
        >
          <FileText size={16} /> My Applications ({myApplications.length})
        </button>
        <button
          className={`btn ${activeSubTab === 'visits' ? 'btn-primary' : 'btn-secondary'}`}
          onClick={() => setActiveSubTab('visits')}
        >
          <Calendar size={16} /> My Visits ({myVisits.length})
        </button>
      </div>

      {/* SUB-TAB 1: BROWSE ANIMALS WITH BENTO GRID */}
      {activeSubTab === 'animals' && (
        <div>
          {/* Integrated AboutBento Component displaying Pets in Chocolate Brown theme */}
          <AboutBento
            animals={animals}
            onBookVisit={(pet) => setVisitModalAnimal(pet)}
            onApply={(pet) => setAppModalAnimal(pet)}
          />

          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 20 }}>
            <h2>All Rescued Pets ({animals.length})</h2>
            <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <label style={{ fontSize: '0.9rem', color: '#EADBC8' }}>Filter Branch:</label>
              <select
                className="form-select"
                value={selectedBranch}
                onChange={(e) => setSelectedBranch(e.target.value === 'All' ? 'All' : Number(e.target.value))}
                style={{ width: 'auto' }}
              >
                <option value="All">All Branches</option>
                <option value={1}>Downtown Shelter (Branch 1)</option>
                <option value={2}>Westside Rescue (Branch 2)</option>
              </select>
            </div>
          </div>

          <div className="animals-grid">
            {animals.map((animal) => (
              <div className="animal-card" key={animal.id}>
                {animal.imageUrl && (
                  <img src={animal.imageUrl} alt={animal.name} className="animal-img" />
                )}
                <div className="animal-body">
                  <div className="animal-header">
                    <span className="animal-name">{animal.name}</span>
                    <span className={`status-pill status-${animal.status}`}>{animal.status}</span>
                  </div>
                  <div className="animal-meta">
                    {animal.species} • {animal.breed} • {animal.age} yrs • {animal.gender}
                  </div>
                  <div className="animal-desc">{animal.description}</div>
                  <div style={{ fontSize: '0.8rem', color: '#B08968', marginBottom: 16 }}>
                    Branch: {animal.branchId === 1 ? 'Downtown Shelter' : 'Westside Rescue'}
                  </div>

                  <div className="card-actions">
                    <button
                      className="btn btn-secondary"
                      disabled={animal.status !== 'Available'}
                      onClick={() => setVisitModalAnimal(animal)}
                      style={{ flex: 1 }}
                    >
                      <Calendar size={15} /> Book Visit
                    </button>
                    <button
                      className="btn btn-primary"
                      disabled={animal.status !== 'Available'}
                      onClick={() => setAppModalAnimal(animal)}
                      style={{ flex: 1 }}
                    >
                      <Heart size={15} /> Apply
                    </button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* SUB-TAB 2: MY APPLICATIONS */}
      {activeSubTab === 'applications' && (
        <div className="glass-panel">
          <h2 style={{ marginBottom: 16 }}>My Adoption Applications</h2>
          {myApplications.length === 0 ? (
            <p style={{ color: '#EADBC8' }}>You have not submitted any adoption applications yet.</p>
          ) : (
            <div className="data-table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>App ID</th>
                    <th>Animal</th>
                    <th>Branch</th>
                    <th>Date</th>
                    <th>Linked Visit</th>
                    <th>Status</th>
                    <th>Notes / Rejection Reason</th>
                  </tr>
                </thead>
                <tbody>
                  {myApplications.map((app) => (
                    <tr key={app.id}>
                      <td>#{app.id}</td>
                      <td>
                        <strong>{app.animalName}</strong> ({app.animalSpecies})
                      </td>
                      <td>{app.branchName}</td>
                      <td>{app.applicationDate}</td>
                      <td>
                        {app.linkedVisit ? (
                          <span style={{ fontSize: '0.85rem', color: '#D4B896' }}>
                            Visit #{app.linkedVisit.id} ({app.linkedVisit.slotDate})
                          </span>
                        ) : (
                          <span style={{ color: '#B08968' }}>None</span>
                        )}
                      </td>
                      <td>
                        <span className={`status-pill status-${app.status}`}>{app.status}</span>
                      </td>
                      <td>
                        {app.status === 'Rejected' ? (
                          <span style={{ color: '#f87171', fontWeight: 500 }}>
                            Reason: {app.rejectionReason}
                          </span>
                        ) : (
                          app.notes || '—'
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* SUB-TAB 3: MY VISITS */}
      {activeSubTab === 'visits' && (
        <div className="glass-panel">
          <h2 style={{ marginBottom: 16 }}>My Booked Visits</h2>
          {myVisits.length === 0 ? (
            <p style={{ color: '#EADBC8' }}>You have no booked visits.</p>
          ) : (
            <div className="data-table-container">
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Visit ID</th>
                    <th>Animal</th>
                    <th>Date</th>
                    <th>Time Slot</th>
                    <th>Booking Status</th>
                    <th>Staff Notes</th>
                  </tr>
                </thead>
                <tbody>
                  {myVisits.map((visit) => (
                    <tr key={visit.id}>
                      <td>#{visit.id}</td>
                      <td>
                        <strong>{visit.animalName}</strong> ({visit.animalSpecies})
                      </td>
                      <td>{visit.slotDate || visit.bookingDate}</td>
                      <td>{visit.slotTime || 'Morning Slot'}</td>
                      <td>
                        <span className={`status-pill status-${visit.status}`}>{visit.status}</span>
                      </td>
                      <td>{visit.staffNotes || '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {/* BOOK VISIT MODAL */}
      {visitModalAnimal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">Book Visit for {visitModalAnimal.name}</h3>
              <button
                onClick={() => setVisitModalAnimal(null)}
                style={{ background: 'none', border: 'none', color: '#FFFDF9', fontSize: '1.2rem', cursor: 'pointer' }}
              >
                ✕
              </button>
            </div>
            <div className="alert-banner alert-info">
              <Calendar size={18} />
              <div>
                <strong>PawTrack Visit Policy:</strong> Booking a visit allows you to meet {visitModalAnimal.name} in person. It does NOT reserve or lock the animal.
              </div>
            </div>

            <div className="form-group">
              <label className="form-label">Select Available Visit Slot:</label>
              <select
                className="form-select"
                onChange={(e) => setSelectedSlotId(Number(e.target.value))}
                value={selectedSlotId || ''}
              >
                <option value="">-- Choose Slot --</option>
                {getAvailableSlots(visitModalAnimal.branchId).map((slot: any) => (
                  <option key={slot.id} value={slot.id} disabled={slot.bookedCount >= slot.capacity}>
                    {slot.date} ({slot.startTime} - {slot.endTime}) — {slot.capacity - slot.bookedCount} spots left
                  </option>
                ))}
              </select>
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12, marginTop: 24 }}>
              <button className="btn btn-secondary" onClick={() => setVisitModalAnimal(null)}>
                Cancel
              </button>
              <button className="btn btn-primary" disabled={!selectedSlotId} onClick={handleBookVisit}>
                Confirm Booking
              </button>
            </div>
          </div>
        </div>
      )}

      {/* SUBMIT APPLICATION MODAL */}
      {appModalAnimal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">Apply for {appModalAnimal.name}</h3>
              <button
                onClick={() => setAppModalAnimal(null)}
                style={{ background: 'none', border: 'none', color: '#FFFDF9', fontSize: '1.2rem', cursor: 'pointer' }}
              >
                ✕
              </button>
            </div>

            <div className="form-group">
              <label className="form-label">Link Completed Visit (Optional):</label>
              <select
                className="form-select"
                value={selectedVisitId || ''}
                onChange={(e) => setSelectedVisitId(e.target.value ? Number(e.target.value) : null)}
              >
                <option value="">No visit linked / Apply directly</option>
                {myVisits
                  .filter((v) => v.animalId === appModalAnimal.id)
                  .map((v) => (
                    <option key={v.id} value={v.id}>
                      Visit #{v.id} ({v.slotDate}) — {v.status}
                    </option>
                  ))}
              </select>
            </div>

            <div className="form-group">
              <label className="form-label">Application Notes / Experience:</label>
              <textarea
                className="form-textarea"
                rows={3}
                placeholder="Tell us about your home environment, past pet experience..."
                value={appNotes}
                onChange={(e) => setAppNotes(e.target.value)}
              />
            </div>

            <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 12, marginTop: 24 }}>
              <button className="btn btn-secondary" onClick={() => setAppModalAnimal(null)}>
                Cancel
              </button>
              <button className="btn btn-primary" onClick={handleSubmitApplication}>
                Submit Application
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
