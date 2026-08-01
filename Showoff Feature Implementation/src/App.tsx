import { useState, useCallback } from 'react';
import type { Page, ModalState, User } from './types';
import {
  getShowoffs, getPublicShowoffs, getShowoff,
  toggleLike, deleteShowoff, incrementViews, PUBLIC_PROFILES,
} from './data';
import Navbar from './components/Navbar';
import ShowoffFeed from './components/ShowoffFeed';
import ShowoffProfile from './components/ShowoffProfile';
import ShowoffCreateEdit from './components/ShowoffCreateEdit';
import ShowoffViewModal from './components/ShowoffViewModal';
import ShowoffReports from './components/ShowoffReports';
import UserProfilePage from './components/UserProfilePage';
import { DeleteModal, ManageModal, LoginModal } from './components/Modals';

const GUEST_USER: User = {
  id: '',
  username: '',
  avatar: '',
  isStaff: false,
  isLoggedIn: false,
};

const LOGGED_IN_USER: User = {
  id: 'user1',
  username: 'admin_user',
  avatar: 'AU',
  isStaff: true,
  isLoggedIn: true,
};

export default function App() {
  const [user, setUser] = useState<User>(LOGGED_IN_USER);
  const [page, setPage] = useState<Page>({ name: 'feed' });
  const [modal, setModal] = useState<ModalState>(null);
  const [showoffs, setShowoffs] = useState(() => getPublicShowoffs());
  const [allShowoffs, setAllShowoffs] = useState(() => getShowoffs());
  const [tick, setTick] = useState(0);

  // Re-read data from store
  const refresh = useCallback(() => {
    setShowoffs(getPublicShowoffs());
    setAllShowoffs(getShowoffs());
    setTick((t) => t + 1);
  }, []);

  function navigateTo(p: Page) {
    setModal(null);
    setPage(p);
  }

  function handleToggleAuth() {
    setUser((u) => u.isLoggedIn ? GUEST_USER : LOGGED_IN_USER);
    setModal(null);
  }

  function handleView(id: string, staffView?: boolean) {
    incrementViews(id);
    refresh();
    setModal({ type: 'view', showoffId: id, staffView });
  }

  function handleLike(id: string) {
    if (!user.isLoggedIn) { setModal({ type: 'login' }); return; }
    toggleLike(id, user.id);
    refresh();
  }

  function handleDelete(id: string) {
    setModal({ type: 'delete', showoffId: id });
  }

  function confirmDelete(id: string) {
    deleteShowoff(id);
    refresh();
    setModal(null);
    if (page.name !== 'profile') setPage({ name: 'profile' });
  }

  function handleEdit(id: string) {
    setModal(null);
    setPage({ name: 'edit', showoffId: id });
  }

  function handleCreate() {
    setModal(null);
    setPage({ name: 'create' });
  }

  function handleSaveCreateEdit() {
    refresh();
    setPage({ name: 'profile' });
  }

  function handleLoginRequired() {
    setModal({ type: 'login' });
  }

  function handleLogin() {
    setUser(LOGGED_IN_USER);
    setModal(null);
  }

  function handleTagClick(tag: string) {
    setModal(null);
    setPage({ name: 'tag', tag });
  }

  function handleOwnerClick(ownerId: string) {
    setModal(null);
    if (ownerId === user.id) {
      setPage({ name: 'profile' });
    } else if (PUBLIC_PROFILES[ownerId]) {
      setPage({ name: 'user-profile', userId: ownerId });
    }
    // owners without a public profile: do nothing (name is non-clickable effectively)
  }

  // Feed showoffs depend on context
  const feedShowoffs = (page.name === 'tag')
    ? showoffs.filter((s) => s.tags.includes((page as { name: 'tag'; tag: string }).tag))
    : showoffs;

  const viewingShowoff = modal?.type === 'view' ? getShowoff(modal.showoffId) : null;
  const deletingShowoff = modal?.type === 'delete' ? getShowoff(modal.showoffId) : null;
  const editingShowoff = page.name === 'edit' ? getShowoff((page as { name: 'edit'; showoffId: string }).showoffId) : undefined;

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', background: '#0d1117' }}>
      <Navbar
        user={user}
        currentPage={page}
        onNavigate={navigateTo}
        onToggleAuth={handleToggleAuth}
      />

      <main style={{ flex: 1 }}>
        {/* Create page */}
        {page.name === 'create' && (
          <ShowoffCreateEdit
            user={user}
            onSave={handleSaveCreateEdit}
            onCancel={() => setPage({ name: 'profile' })}
          />
        )}

        {/* Edit page */}
        {page.name === 'edit' && editingShowoff && (
          <ShowoffCreateEdit
            user={user}
            editShowoff={editingShowoff}
            onSave={handleSaveCreateEdit}
            onCancel={() => setPage({ name: 'profile' })}
          />
        )}

        {/* Profile page */}
        {page.name === 'profile' && (
          <ShowoffProfile
            showoffs={allShowoffs}
            user={user}
            onNavigate={navigateTo}
            onView={handleView}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onLike={handleLike}
            onTagClick={handleTagClick}
            onCreate={handleCreate}
          />
        )}

        {/* User public profile */}
        {page.name === 'user-profile' && PUBLIC_PROFILES[(page as { name: 'user-profile'; userId: string }).userId] && (
          <UserProfilePage
            userId={(page as { name: 'user-profile'; userId: string }).userId}
            profile={PUBLIC_PROFILES[(page as { name: 'user-profile'; userId: string }).userId]}
            showoffs={allShowoffs}
            currentUser={user}
            onView={handleView}
            onLike={handleLike}
            onTagClick={handleTagClick}
            onBack={() => setPage({ name: 'feed' })}
          />
        )}

        {/* Staff reports */}
        {page.name === 'reports' && (
          <ShowoffReports
            showoffs={allShowoffs}
            user={user}
            onRefresh={refresh}
            onView={handleView}
          />
        )}

        {/* Feed (default) */}
        {(page.name === 'feed' || page.name === 'tag') && (
          <ShowoffFeed
            showoffs={page.name === 'tag' ? allShowoffs : showoffs}
            user={user}
            currentPage={page}
            onView={handleView}
            onEdit={handleEdit}
            onDelete={handleDelete}
            onLike={handleLike}
            onTagClick={handleTagClick}
            onNavigate={navigateTo}
          />
        )}
      </main>

      {/* Footer */}
      <footer style={{ borderTop: '1px solid #21262d', padding: '16px 24px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: 12, color: '#484f58' }}>
        <span>© 2026 · Arcane Vault. All rights reserved.</span>
        <div style={{ display: 'flex', gap: 16 }}>
          <button style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: 12, color: '#484f58', fontFamily: 'inherit' }}>Home</button>
          <button style={{ background: 'none', border: 'none', cursor: 'pointer', fontSize: 12, color: '#484f58', fontFamily: 'inherit' }}>Privacy Policy</button>
        </div>
      </footer>

      {/* Modals */}
      {modal?.type === 'view' && viewingShowoff && (
        <ShowoffViewModal
          showoff={viewingShowoff}
          user={user}
          staffView={modal.staffView}
          onClose={() => setModal(null)}
          onLike={handleLike}
          onEdit={(id) => { setModal(null); handleEdit(id); }}
          onDelete={(id) => setModal({ type: 'delete', showoffId: id })}
          onOwnerClick={handleOwnerClick}
          onTagClick={handleTagClick}
          onLoginRequired={handleLoginRequired}
          onShareClick={() => {}}
        />
      )}

      {modal?.type === 'delete' && deletingShowoff && (
        <DeleteModal
          showoff={deletingShowoff}
          onConfirm={() => confirmDelete(deletingShowoff.id)}
          onCancel={() => setModal(null)}
        />
      )}

      {modal?.type === 'manage' && (
        <ManageModal
          showoffs={allShowoffs}
          user={user}
          onEdit={handleEdit}
          onDelete={(id) => { setModal({ type: 'delete', showoffId: id }); }}
          onClose={() => setModal(null)}
        />
      )}

      {modal?.type === 'login' && (
        <LoginModal
          message={modal.message}
          onLogin={handleLogin}
          onClose={() => setModal(null)}
        />
      )}
    </div>
  );
}
