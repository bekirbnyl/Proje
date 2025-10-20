import React from 'react';
import { Link } from 'react-router-dom';
import { useSelector } from 'react-redux';
import { RootState } from '../../store';
import { USER_ROLES } from '../../types';

const AdminDashboard: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);

  const hasFullAdmin = user?.roles.includes(USER_ROLES.ADMIN);
  const hasReportAccess = user?.roles.some(role => 
    [USER_ROLES.ADMIN, USER_ROLES.YONETIM, USER_ROLES.SINEMA_MUDURU].includes(role as any)
  );

  const cardStyle = {
    background: 'white',
    padding: '24px',
    borderRadius: '16px',
    boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1)',
    border: '1px solid #e5e7eb',
    transition: 'all 0.3s ease',
    textDecoration: 'none',
    color: 'inherit',
    display: 'block',
    position: 'relative' as const,
    overflow: 'hidden'
  };

  const iconStyle = {
    width: '48px',
    height: '48px',
    borderRadius: '12px',
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: '16px',
    fontSize: '24px'
  };

  return (
    <div style={{ padding: '24px 0' }}>
      <div style={{
        background: 'linear-gradient(135deg, #3b82f6, #8b5cf6)',
        borderRadius: '24px',
        padding: '48px',
        color: 'white',
        marginBottom: '32px',
        boxShadow: '0 10px 25px -3px rgba(0, 0, 0, 0.1)'
      }}>
        <h1 style={{ fontSize: '48px', fontWeight: 'bold', margin: '0 0 8px 0' }}>Admin Dashboard</h1>
        <p style={{ fontSize: '18px', opacity: 0.9, margin: 0 }}>
          Sinema sisteminizin genel görünümüne hoş geldiniz
        </p>
      </div>

      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', 
        gap: '24px',
        marginBottom: '32px'
      }}>
        <Link
          to="/admin/movies"
          style={cardStyle}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
          }}
        >
          <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #3b82f6, #2563eb)' }}>
            🎬
          </div>
          <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Film Yönetimi</h3>
          <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>Film kataloğunu ekleyin, düzenleyin ve yönetin</p>
          <div style={{ display: 'flex', alignItems: 'center', color: '#3b82f6', fontWeight: '500' }}>
            <span>Filmlere Git</span>
            <span style={{ marginLeft: '8px' }}>→</span>
          </div>
        </Link>

        <Link
          to="/admin/screenings"
          style={cardStyle}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
          }}
        >
          <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #059669, #047857)' }}>
            📅
          </div>
          <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Seans Yönetimi</h3>
          <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>Film seanslarını planlayın ve yönetin</p>
          <div style={{ display: 'flex', alignItems: 'center', color: '#059669', fontWeight: '500' }}>
            <span>Seanslara Git</span>
            <span style={{ marginLeft: '8px' }}>→</span>
          </div>
        </Link>

        <Link
          to="/admin/halls"
          style={cardStyle}
          onMouseEnter={(e) => {
            e.currentTarget.style.transform = 'translateY(-4px)';
            e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
          }}
          onMouseLeave={(e) => {
            e.currentTarget.style.transform = 'translateY(0)';
            e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
          }}
        >
          <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #8b5cf6, #7c3aed)' }}>
            🏛️
          </div>
          <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Salon Yönetimi</h3>
          <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>Salonları ve koltuk düzenlerini yapılandırın</p>
          <div style={{ display: 'flex', alignItems: 'center', color: '#8b5cf6', fontWeight: '500' }}>
            <span>Salonlara Git</span>
            <span style={{ marginLeft: '8px' }}>→</span>
          </div>
        </Link>

        {hasReportAccess && (
          <Link
            to="/admin/reports"
            style={cardStyle}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-4px)';
              e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            }}
          >
            <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #6366f1, #5b21b6)' }}>
              📊
            </div>
            <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Raporlar</h3>
            <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>Analitikleri görüntüleyin ve veri dışa aktarın</p>
            <div style={{ display: 'flex', alignItems: 'center', color: '#6366f1', fontWeight: '500' }}>
              <span>Raporlara Git</span>
              <span style={{ marginLeft: '8px' }}>→</span>
            </div>
          </Link>
        )}

        {hasFullAdmin && (
          <Link
            to="/admin/member-approvals"
            style={cardStyle}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-4px)';
              e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            }}
          >
            <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #f59e0b, #d97706)' }}>
              👥
            </div>
            <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Üye Onayları</h3>
            <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>Üye başvurularını onaylayın ve yönetin</p>
            <div style={{ display: 'flex', alignItems: 'center', color: '#f59e0b', fontWeight: '500' }}>
              <span>Onaylara Git</span>
              <span style={{ marginLeft: '8px' }}>→</span>
            </div>
          </Link>
        )}

        {hasFullAdmin && (
          <Link
            to="/admin/settings"
            style={cardStyle}
            onMouseEnter={(e) => {
              e.currentTarget.style.transform = 'translateY(-4px)';
              e.currentTarget.style.boxShadow = '0 10px 25px -3px rgba(0, 0, 0, 0.1)';
            }}
            onMouseLeave={(e) => {
              e.currentTarget.style.transform = 'translateY(0)';
              e.currentTarget.style.boxShadow = '0 4px 6px -1px rgba(0, 0, 0, 0.1)';
            }}
          >
            <div style={{ ...iconStyle, background: 'linear-gradient(135deg, #6b7280, #4b5563)' }}>
              ⚙️
            </div>
            <h3 style={{ fontSize: '20px', fontWeight: '600', margin: '0 0 8px 0', color: '#1f2937' }}>Sistem Ayarları</h3>
            <p style={{ color: '#6b7280', margin: '0 0 16px 0' }}>İş kurallarını ve parametreleri yapılandırın</p>
            <div style={{ display: 'flex', alignItems: 'center', color: '#6b7280', fontWeight: '500' }}>
              <span>Ayarlara Git</span>
              <span style={{ marginLeft: '8px' }}>→</span>
            </div>
          </Link>
        )}
      </div>

      {/* System Status Cards */}
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', 
        gap: '16px'
      }}>
        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '12px',
          boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
          border: '1px solid #e5e7eb'
        }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{ marginRight: '12px', fontSize: '20px' }}>🎬</div>
            <div>
              <dt style={{ fontSize: '14px', color: '#6b7280', margin: 0 }}>Aktif Filmler</dt>
              <dd style={{ fontSize: '24px', fontWeight: '600', color: '#1f2937', margin: 0 }}>--</dd>
            </div>
          </div>
        </div>

        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '12px',
          boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
          border: '1px solid #e5e7eb'
        }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{ marginRight: '12px', fontSize: '20px' }}>📅</div>
            <div>
              <dt style={{ fontSize: '14px', color: '#6b7280', margin: 0 }}>Bugünkü Seanslar</dt>
              <dd style={{ fontSize: '24px', fontWeight: '600', color: '#1f2937', margin: 0 }}>--</dd>
            </div>
          </div>
        </div>

        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '12px',
          boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
          border: '1px solid #e5e7eb'
        }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{ marginRight: '12px', fontSize: '20px' }}>🎫</div>
            <div>
              <dt style={{ fontSize: '14px', color: '#6b7280', margin: 0 }}>Bugün Satılan Biletler</dt>
              <dd style={{ fontSize: '24px', fontWeight: '600', color: '#1f2937', margin: 0 }}>--</dd>
            </div>
          </div>
        </div>

        <div style={{
          background: 'white',
          padding: '20px',
          borderRadius: '12px',
          boxShadow: '0 1px 3px 0 rgba(0, 0, 0, 0.1)',
          border: '1px solid #e5e7eb'
        }}>
          <div style={{ display: 'flex', alignItems: 'center' }}>
            <div style={{ marginRight: '12px', fontSize: '20px' }}>👥</div>
            <div>
              <dt style={{ fontSize: '14px', color: '#6b7280', margin: 0 }}>Bekleyen Onaylar</dt>
              <dd style={{ fontSize: '24px', fontWeight: '600', color: '#1f2937', margin: 0 }}>--</dd>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;