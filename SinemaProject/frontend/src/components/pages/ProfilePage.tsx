import React, { useEffect, useState } from 'react';
import { useSelector } from 'react-redux';
import { RootState } from '../../store';
import { useAppDispatch } from '../../store/hooks';
import apiService from '../../services/api';
import { addNotification } from '../../store/slices/uiSlice';

const ProfilePage: React.FC = () => {
  const dispatch = useAppDispatch();
  const { user, isAuthenticated } = useSelector((state: RootState) => state.auth);
  
  const [memberInfo, setMemberInfo] = useState<any>(null);
  const [hasVipApplication, setHasVipApplication] = useState(false);
  const [isApplyingForVip, setIsApplyingForVip] = useState(false);
  const [vipApplicationReason, setVipApplicationReason] = useState('');

  useEffect(() => {
    const fetchMemberInfo = async () => {
      if (!isAuthenticated || !user?.memberId) return;
      
      try {
        // This would need a new API endpoint to get member details
        // For now, we'll use the current user info
        setMemberInfo({
          fullName: user.displayName,
          email: user.email,
          isVip: user.isVip,
          isApproved: user.isApproved
        });
      } catch (error) {
        console.error('Error fetching member info:', error);
      }
    };

    fetchMemberInfo();
  }, [isAuthenticated, user]);

  const handleVipApplication = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!vipApplicationReason.trim()) {
      dispatch(addNotification({
        type: 'error',
        title: 'Hata',
        message: 'Lütfen VIP başvuru nedeninizi belirtin.',
      }));
      return;
    }

    setIsApplyingForVip(true);
    try {
      // This would need a new API endpoint for VIP applications
      await apiService.applyForVip(vipApplicationReason);
      
      dispatch(addNotification({
        type: 'success',
        title: 'Başvuru Gönderildi',
        message: 'VIP üyelik başvurunuz başarıyla gönderildi. Yönetici onayı bekleniyor.',
      }));
      
      setHasVipApplication(true);
      setVipApplicationReason('');
    } catch (error) {
      dispatch(addNotification({
        type: 'error',
        title: 'Başvuru Hatası',
        message: 'VIP başvurunuz gönderilemedi. Lütfen tekrar deneyin.',
      }));
    } finally {
      setIsApplyingForVip(false);
    }
  };

  if (!isAuthenticated) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">Giriş Gerekli</h2>
        <p className="text-gray-600">Profilinizi görüntülemek için lütfen giriş yapın.</p>
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Profilim</h1>
        <p className="mt-1 text-sm text-gray-500">Hesap bilgilerinizi ve üyelik durumunuzu görüntüleyin</p>
      </div>

      {/* User Info Card */}
      <div className="bg-white rounded-lg shadow-md p-6">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">Kişisel Bilgiler</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="text-sm font-medium text-gray-500">Ad Soyad</label>
            <p className="text-gray-900">{memberInfo?.fullName || 'Belirtilmemiş'}</p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-500">E-posta</label>
            <p className="text-gray-900">{memberInfo?.email || 'Belirtilmemiş'}</p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-500">Üyelik Durumu</label>
            <p className={`font-medium ${memberInfo?.isApproved ? 'text-green-600' : 'text-yellow-600'}`}>
              {memberInfo?.isApproved ? 'Onaylanmış' : 'Onay Bekliyor'}
            </p>
          </div>
          <div>
            <label className="text-sm font-medium text-gray-500">VIP Durumu</label>
            <div className="flex items-center gap-2">
              <p className={`font-medium ${memberInfo?.isVip ? 'text-purple-600' : 'text-gray-600'}`}>
                {memberInfo?.isVip ? 'VIP Üye' : 'Standart Üye'}
              </p>
              {memberInfo?.isVip && (
                <span className="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                  ⭐ VIP
                </span>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* VIP Benefits Card */}
      <div className="bg-gradient-to-r from-purple-50 to-indigo-50 rounded-lg p-6 border border-purple-200">
        <h2 className="text-xl font-semibold text-gray-900 mb-4">VIP Üyelik Avantajları</h2>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="flex items-start gap-3">
            <span className="text-purple-600 text-xl">🎟️</span>
            <div>
              <h3 className="font-medium text-gray-900">Aylık Bedava Bilet</h3>
              <p className="text-sm text-gray-600">Her ay 1 adet bedava bilet hakkı</p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <span className="text-purple-600 text-xl">👥</span>
            <div>
              <h3 className="font-medium text-gray-900">Misafir İndirimi</h3>
              <p className="text-sm text-gray-600">Misafir biletlerinde %20 indirim</p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <span className="text-purple-600 text-xl">📅</span>
            <div>
              <h3 className="font-medium text-gray-900">Erken Rezervasyon</h3>
              <p className="text-sm text-gray-600">7 gün önceden rezervasyon yapabilme</p>
            </div>
          </div>
          <div className="flex items-start gap-3">
            <span className="text-purple-600 text-xl">⭐</span>
            <div>
              <h3 className="font-medium text-gray-900">Özel Etkinlikler</h3>
              <p className="text-sm text-gray-600">VIP etkinlik ve gösterimlere davet</p>
            </div>
          </div>
        </div>
      </div>

      {/* VIP Application Card */}
      {!memberInfo?.isVip && memberInfo?.isApproved && !hasVipApplication && (
        <div className="bg-white rounded-lg shadow-md p-6">
          <h2 className="text-xl font-semibold text-gray-900 mb-4">VIP Üyelik Başvurusu</h2>
          <p className="text-gray-600 mb-4">
            VIP üyelik avantajlarından yararlanmak için başvuru yapabilirsiniz. 
            Başvurunuz yönetici tarafından değerlendirilecektir.
          </p>
          
          <form onSubmit={handleVipApplication} className="space-y-4">
            <div>
              <label htmlFor="reason" className="block text-sm font-medium text-gray-700 mb-2">
                Başvuru Nedeniniz
              </label>
              <textarea
                id="reason"
                value={vipApplicationReason}
                onChange={(e) => setVipApplicationReason(e.target.value)}
                rows={4}
                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent"
                placeholder="VIP üyelik için neden başvurduğunuzu açıklayın..."
                required
              />
            </div>
            
            <button
              type="submit"
              disabled={isApplyingForVip}
              className="w-full md:w-auto px-6 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-md transition-colors duration-200 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {isApplyingForVip ? 'Başvuru Gönderiliyor...' : 'VIP Üyelik İçin Başvur'}
            </button>
          </form>
        </div>
      )}

      {/* VIP Application Status */}
      {hasVipApplication && !memberInfo?.isVip && (
        <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-6">
          <h2 className="text-xl font-semibold text-yellow-800 mb-2">VIP Başvuru Durumu</h2>
          <p className="text-yellow-700">
            VIP üyelik başvurunuz alınmıştır ve yönetici onayı beklenmektedir. 
            Başvurunuzun durumu hakkında e-posta ile bilgilendirileceksiniz.
          </p>
        </div>
      )}

      {/* Already VIP */}
      {memberInfo?.isVip && (
        <div className="bg-purple-50 border border-purple-200 rounded-lg p-6">
          <h2 className="text-xl font-semibold text-purple-800 mb-2">🎉 VIP Üyesiniz!</h2>
          <p className="text-purple-700">
            Tebrikler! VIP üyelik avantajlarından yararlanabilirsiniz. 
            Özel indirimleriniz ve ayrıcalıklarınız otomatik olarak uygulanacaktır.
          </p>
        </div>
      )}
    </div>
  );
};

export default ProfilePage;