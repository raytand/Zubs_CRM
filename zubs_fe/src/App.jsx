import { useState, useEffect } from "react";
import { Routes, Route, Navigate } from "react-router-dom";

import Auth from "./pages/Auth";
import Navbar from "./components/Navbar";

import Appointments from "./pages/Appointments";
import Patients from "./pages/Patients";
import Services from "./pages/Services";
import Payments from "./pages/Payments";
import Doctors from "./pages/Doctors";
import Users from "./pages/Users";
import MedicalRecords from "./pages/MedicalRecords";
import TreatmentRecords from "./pages/TreatmentRecords";
import DentalCharts from "./pages/DentalCharts";

import { isAuthenticated, logout } from "./services/auth";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);

  useEffect(() => {
    setIsLoggedIn(isAuthenticated());
  }, []);

  const handleLogin = () => {
    setIsLoggedIn(true);
  };

  const handleLogout = async () => {
    try {
      await logout();
    } catch (err) {
      console.error("Logout error:", err);
    } finally {
      setIsLoggedIn(false);
    }
  };

  // Не залогінений
  if (!isLoggedIn) {
    return (
      <Routes>
        <Route path="/auth" element={<Auth onLogin={handleLogin} />} />
        <Route path="*" element={<Navigate to="/auth" replace />} />
      </Routes>
    );
  }

  // Залогінений
  return (
    <>
      <Navbar onLogout={handleLogout} />

      <Routes>
        <Route path="/" element={<Appointments />} />
        <Route path="/patients" element={<Patients />} />
        <Route path="/services" element={<Services />} />
        <Route path="/payments" element={<Payments />} />
        <Route path="/doctors" element={<Doctors />} />
        <Route path="/users" element={<Users />} />
        <Route path="/medical-records" element={<MedicalRecords />} />
        <Route path="/treatment-records" element={<TreatmentRecords />} />
        <Route path="/dental-charts" element={<DentalCharts />} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </>
  );
}

export default App;
