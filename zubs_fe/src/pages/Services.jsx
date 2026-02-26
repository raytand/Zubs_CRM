import { useState, useEffect } from "react";
import {
  getAllServices,
  createService,
  updateService,
  deleteService,
} from "../services/services";
import "./styles/Services.css";

const empty = {
  id: null,
  code: "",
  name: "",
  description: "",
  price: 0,
};

// ================= MODAL =================
const Modal = ({ onClose, children }) => (
  <div className="modal-backdrop" onClick={onClose}>
    <div className="modal" onClick={(e) => e.stopPropagation()}>
      {children}
    </div>
  </div>
);

// ================= SERVICE CARD =================
const ServiceCard = ({ service, onEdit, onDelete }) => {
  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div className="service-card">
      <div className="service-header">
        <div className="service-code-badge">{service.code}</div>
        <div className="service-actions">
          <button
            className="btn-edit"
            onClick={() => onEdit(service)}
            title="Редагувати"
          >
            ✏️
          </button>
          <button
            className="btn-delete"
            onClick={() => onDelete(service.id)}
            title="Видалити"
          >
            🗑️
          </button>
        </div>
      </div>

      <h3 className="service-name">{service.name}</h3>

      {service.description && (
        <p className="service-description">{service.description}</p>
      )}

      <div className="service-footer">
        <div className="service-price">
          <span className="price-label">Ціна:</span>
          <span className="price-value">{formatPrice(service.price)}</span>
        </div>
      </div>
    </div>
  );
};

// ================= MAIN COMPONENT =================
export default function Services() {
  const [list, setList] = useState([]);
  const [form, setForm] = useState(empty);
  const [edit, setEdit] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [viewMode, setViewMode] = useState("cards"); // "cards" or "table"
  const [sortBy, setSortBy] = useState("name"); // "name", "code", "price"

  const load = async () => setList(await getAllServices());

  useEffect(() => {
    load();
  }, []);

  const submit = async (e) => {
    e.preventDefault();

    const dto = {
      ...form,
      price: parseFloat(form.price) || 0,
    };

    try {
      if (edit) {
        await updateService(form.id, dto);
      } else {
        await createService(dto);
      }
      setForm(empty);
      setEdit(false);
      setShowModal(false);
      load();
    } catch (error) {
      console.error("Save error:", error);
      alert(
        "Помилка збереження: " +
          (error.response?.data?.message ||
            error.message ||
            "Невідома помилка"),
      );
    }
  };

  const editRow = (s) => {
    setForm(s);
    setEdit(true);
    setShowModal(true);
  };

  const remove = async (id) => {
    if (!confirm("Видалити послугу?")) return;
    await deleteService(id);
    setList(list.filter((x) => x.id !== id));
  };

  // Filter services
  const filteredList = list.filter((s) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      s.name.toLowerCase().includes(searchLower) ||
      s.code.toLowerCase().includes(searchLower) ||
      s.description?.toLowerCase().includes(searchLower)
    );
  });

  // Sort services
  const sortedList = [...filteredList].sort((a, b) => {
    switch (sortBy) {
      case "name":
        return a.name.localeCompare(b.name);
      case "code":
        return a.code.localeCompare(b.code);
      case "price":
        return b.price - a.price; // Descending
      default:
        return 0;
    }
  });

  // Calculate statistics
  const stats = {
    total: list.length,
    avgPrice:
      list.length > 0
        ? list.reduce((sum, s) => sum + s.price, 0) / list.length
        : 0,
    maxPrice: list.length > 0 ? Math.max(...list.map((s) => s.price)) : 0,
  };

  const formatPrice = (price) => {
    return new Intl.NumberFormat("uk-UA", {
      style: "currency",
      currency: "UAH",
      minimumFractionDigits: 2,
    }).format(price);
  };

  return (
    <div className="page">
      <div className="content">
        {/* Header */}
        <div className="page-header">
          <div>
            <h1>Послуги</h1>
            <p className="page-subtitle">
              Загалом: {stats.total} • Середня ціна:{" "}
              {formatPrice(stats.avgPrice)} • Макс:{" "}
              {formatPrice(stats.maxPrice)}
            </p>
          </div>
          <button
            className="btn-primary"
            onClick={() => {
              setForm(empty);
              setEdit(false);
              setShowModal(true);
            }}
          >
            + Додати послугу
          </button>
        </div>

        {/* Search and Controls */}
        <div className="controls">
          <div className="search-box">
            <span className="search-icon">🔍</span>
            <input
              type="text"
              placeholder="Пошук за назвою, кодом..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input"
            />
            {searchTerm && (
              <button
                className="clear-search"
                onClick={() => setSearchTerm("")}
              >
                ✕
              </button>
            )}
          </div>

          <div className="sort-dropdown">
            <label>Сортувати:</label>
            <select value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
              <option value="name">За назвою</option>
              <option value="code">За кодом</option>
              <option value="price">За ціною</option>
            </select>
          </div>

          <div className="view-toggle">
            <button
              className={`toggle-btn ${viewMode === "cards" ? "active" : ""}`}
              onClick={() => setViewMode("cards")}
              title="Картки"
            >
              ▦
            </button>
            <button
              className={`toggle-btn ${viewMode === "table" ? "active" : ""}`}
              onClick={() => setViewMode("table")}
              title="Таблиця"
            >
              ☰
            </button>
          </div>
        </div>

        {/* Results Count */}
        {searchTerm && (
          <p className="results-info">
            Знайдено: {sortedList.length} з {list.length}
          </p>
        )}

        {/* Modal Form */}
        {showModal && (
          <Modal onClose={() => setShowModal(false)}>
            <form className="service-form" onSubmit={submit}>
              <h2>{edit ? "Редагувати послугу" : "Нова послуга"}</h2>

              <div className="form-group">
                <label>Код послуги *</label>
                <input
                  required
                  placeholder="Наприклад: CONS-01"
                  value={form.code}
                  onChange={(e) => setForm({ ...form, code: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Назва послуги *</label>
                <input
                  required
                  placeholder="Наприклад: Консультація стоматолога"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                />
              </div>

              <div className="form-group">
                <label>Опис</label>
                <textarea
                  rows="4"
                  placeholder="Детальний опис послуги..."
                  value={form.description}
                  onChange={(e) =>
                    setForm({ ...form, description: e.target.value })
                  }
                />
              </div>

              <div className="form-group">
                <label>Ціна (грн) *</label>
                <input
                  required
                  type="number"
                  step="0.01"
                  min="0"
                  placeholder="0.00"
                  value={form.price}
                  onChange={(e) => setForm({ ...form, price: e.target.value })}
                />
              </div>

              <div className="form-actions">
                <button type="submit" className="btn-submit">
                  {edit ? "Зберегти зміни" : "Створити послугу"}
                </button>
                <button
                  type="button"
                  className="btn-cancel"
                  onClick={() => setShowModal(false)}
                >
                  Скасувати
                </button>
              </div>
            </form>
          </Modal>
        )}

        {/* Data Display */}
        {sortedList.length === 0 ? (
          <div className="empty-state">
            <div className="empty-icon">💼</div>
            <h3>Послуг не знайдено</h3>
            <p>
              {searchTerm
                ? "Спробуйте змінити критерії пошуку"
                : "Додайте першу послугу для початку роботи"}
            </p>
          </div>
        ) : viewMode === "cards" ? (
          <div className="services-grid">
            {sortedList.map((s) => (
              <ServiceCard
                key={s.id}
                service={s}
                onEdit={editRow}
                onDelete={remove}
              />
            ))}
          </div>
        ) : (
          <div className="table-container">
            <table className="services-table">
              <thead>
                <tr>
                  <th>Код</th>
                  <th>Назва</th>
                  <th>Опис</th>
                  <th>Ціна</th>
                  <th>Дії</th>
                </tr>
              </thead>
              <tbody>
                {sortedList.map((s) => (
                  <tr key={s.id}>
                    <td>
                      <span className="code-badge">{s.code}</span>
                    </td>
                    <td>
                      <strong>{s.name}</strong>
                    </td>
                    <td className="description-cell">{s.description || "—"}</td>
                    <td>
                      <strong className="price-highlight">
                        {formatPrice(s.price)}
                      </strong>
                    </td>
                    <td>
                      <div className="table-actions">
                        <button
                          className="btn-edit-sm"
                          onClick={() => editRow(s)}
                        >
                          ✏️
                        </button>
                        <button
                          className="btn-delete-sm"
                          onClick={() => remove(s.id)}
                        >
                          🗑️
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
