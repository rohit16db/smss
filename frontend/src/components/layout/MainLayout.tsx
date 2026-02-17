import { Outlet } from 'react-router-dom';
import { Header } from './Header';

export const MainLayout = () => {
  return (
    <div className="flex flex-col min-h-screen bg-gray-50">
      <Header />
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
};
