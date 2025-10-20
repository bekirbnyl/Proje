import React from 'react';

interface AuthLayoutProps {
  children: React.ReactNode;
}

const AuthLayout: React.FC<AuthLayoutProps> = ({ children }) => {
  return (
    <div className="min-h-screen bg-gradient-to-br from-indigo-900 via-purple-900 to-pink-900 flex items-center justify-center py-6 sm:py-12 px-4 sm:px-6 lg:px-8 relative overflow-hidden">
      {/* Background decorations */}
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-24 -left-24 w-32 h-32 sm:w-48 sm:h-48 bg-blue-500/10 rounded-full blur-2xl animate-pulse"></div>
        <div className="absolute top-1/3 -right-16 w-40 h-40 sm:w-64 sm:h-64 bg-purple-500/10 rounded-full blur-2xl animate-pulse delay-1000"></div>
        <div className="absolute -bottom-16 left-1/4 w-28 h-28 sm:w-40 sm:h-40 bg-pink-500/10 rounded-full blur-2xl animate-pulse delay-2000"></div>
      </div>
      
      <div className="relative max-w-sm sm:max-w-md w-full space-y-6 sm:space-y-8">
        <div className="text-center">
          <div className="mx-auto w-14 h-14 sm:w-16 sm:h-16 bg-white/10 backdrop-blur-lg rounded-xl sm:rounded-2xl flex items-center justify-center mb-4 sm:mb-6 border border-white/20 shadow-xl">
            <svg className="w-6 h-6 sm:w-8 sm:h-8 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
            </svg>
          </div>
          <h1 className="text-2xl sm:text-3xl lg:text-4xl font-bold text-white mb-2 sm:mb-3 bg-gradient-to-r from-white to-blue-100 bg-clip-text text-transparent">
            CinemaHub
          </h1>
          <p className="text-blue-200 text-sm sm:text-base font-medium">Premium Cinema Experience</p>
          <div className="mt-3 sm:mt-4 flex items-center justify-center space-x-1.5">
            <div className="w-1 h-1 sm:w-1.5 sm:h-1.5 bg-blue-400 rounded-full animate-bounce"></div>
            <div className="w-1 h-1 sm:w-1.5 sm:h-1.5 bg-purple-400 rounded-full animate-bounce delay-100"></div>
            <div className="w-1 h-1 sm:w-1.5 sm:h-1.5 bg-pink-400 rounded-full animate-bounce delay-200"></div>
          </div>
        </div>
        
        <div className="bg-white/95 backdrop-blur-lg rounded-xl sm:rounded-2xl shadow-xl p-5 sm:p-6 lg:p-8 border border-white/20">
          {children}
        </div>
        
        {/* Footer */}
        <div className="text-center text-white/70 text-xs sm:text-sm">
          <p>© 2024 CinemaHub. Premium cinema experience.</p>
        </div>
      </div>
    </div>
  );
};

export default AuthLayout;
