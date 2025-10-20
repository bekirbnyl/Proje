import React from 'react';
import { useSelector } from 'react-redux';
import { Link, useLocation } from 'react-router-dom';
import { RootState } from '../../store';
import { USER_ROLES } from '../../types';

const Sidebar: React.FC = () => {
  const { isAuthenticated, user } = useSelector((state: RootState) => state.auth);
  const { sidebarOpen } = useSelector((state: RootState) => state.ui);
  const location = useLocation();

  if (!isAuthenticated) {
    return null;
  }

  const hasAdminAccess = user?.roles.some(role => 
    [USER_ROLES.ADMIN, USER_ROLES.SINEMA_MUDURU].includes(role as any)
  );

  const hasDeskAccess = user?.roles.some(role => 
    [USER_ROLES.GISE_GOREVLISI, USER_ROLES.GISE_AMIRI, USER_ROLES.SINEMA_MUDURU, USER_ROLES.ADMIN].includes(role as any)
  );

  const hasReportAccess = user?.roles.some(role => 
    [USER_ROLES.ADMIN, USER_ROLES.YONETIM, USER_ROLES.SINEMA_MUDURU].includes(role as any)
  );

  const isActive = (path: string) => location.pathname === path;

  const menuItems = [
    {
      name: 'Dashboard',
      path: '/dashboard',
      icon: '🏠',
    },
    {
      name: 'Filmler',
      path: '/movies',
      icon: '🎬',
    },
    {
      name: 'Profilim',
      path: '/profile',
      icon: '👤',
    },
  ];

  const adminMenuItems = hasAdminAccess ? [
    {
      name: 'Admin Panel',
      path: '/admin',
      icon: '⚙️',
    },
    {
      name: 'Film Yönetimi',
      path: '/admin/movies',
      icon: '🎭',
    },
    {
      name: 'Seans Yönetimi',
      path: '/admin/screenings',
      icon: '📅',
    },
    {
      name: 'Salon Yönetimi',
      path: '/admin/halls',
      icon: '🏛️',
    },
  ] : [];

  const reportMenuItems = hasReportAccess ? [
    {
      name: 'Raporlar',
      path: '/admin/reports',
      icon: '📊',
    },
  ] : [];

  const memberApprovalMenuItems = user?.roles.includes(USER_ROLES.ADMIN) ? [
    {
      name: 'Üye Onayları',
      path: '/admin/member-approvals',
      icon: '👥',
    },
    {
      name: 'Sistem Ayarları',
      path: '/admin/settings',
      icon: '🔧',
    },
  ] : [];

  const deskMenuItems = hasDeskAccess ? [
    {
      name: 'Gişe',
      path: '/desk',
      icon: '🎫',
    },
    {
      name: 'Bilet Satış',
      path: '/desk/ticket-sales',
      icon: '💰',
    },
  ] : [];

  const allMenuItems = [
    ...menuItems,
    ...adminMenuItems,
    ...reportMenuItems,
    ...memberApprovalMenuItems,
    ...deskMenuItems,
  ];

  return (
    <aside style={{
      position: 'fixed',
      left: 0,
      top: '64px',
      height: 'calc(100vh - 64px)',
      width: sidebarOpen ? '256px' : '64px',
      backgroundColor: 'white',
      borderRight: '1px solid #e5e7eb',
      boxShadow: '1px 0 3px 0 rgba(0, 0, 0, 0.1)',
      transition: 'width 0.3s ease',
      zIndex: 30,
      overflow: 'hidden'
    }}>
      <nav style={{ padding: '16px 8px' }}>
        <ul style={{ listStyle: 'none', margin: 0, padding: 0, display: 'flex', flexDirection: 'column', gap: '4px' }}>
          {allMenuItems.map((item) => (
            <li key={item.path}>
              <Link
                to={item.path}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  padding: '12px',
                  borderRadius: '12px',
                  textDecoration: 'none',
                  fontSize: '14px',
                  fontWeight: '500',
                  transition: 'all 0.2s ease',
                  justifyContent: sidebarOpen ? 'flex-start' : 'center',
                  ...(isActive(item.path) ? {
                    background: 'linear-gradient(135deg, #3b82f6, #2563eb)',
                    color: 'white',
                    boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)'
                  } : {
                    color: '#6b7280',
                    backgroundColor: 'transparent'
                  })
                }}
                onMouseEnter={(e) => {
                  if (!isActive(item.path)) {
                    e.currentTarget.style.backgroundColor = '#f3f4f6';
                    e.currentTarget.style.color = '#3b82f6';
                  }
                }}
                onMouseLeave={(e) => {
                  if (!isActive(item.path)) {
                    e.currentTarget.style.backgroundColor = 'transparent';
                    e.currentTarget.style.color = '#6b7280';
                  }
                }}
                title={!sidebarOpen ? item.name : ''}
              >
                <span style={{ fontSize: '20px', display: 'flex', alignItems: 'center', justifyContent: 'center', width: '20px', height: '20px' }}>
                  {item.icon}
                </span>
                {sidebarOpen && (
                  <span style={{ marginLeft: '12px', whiteSpace: 'nowrap' }}>{item.name}</span>
                )}
              </Link>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
};

export default Sidebar;