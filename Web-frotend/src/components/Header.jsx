import "./Header.scss";
import { Search } from "lucide-react";

export default function Header({ title, subtitle }) {
  const user = JSON.parse(localStorage.getItem("user") || "null");

  return (
    <header className="page-header">
      <div className="page-header-left">
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
      </div>

      <div className="page-header-right">

        <div className="search-box">
          <Search size={18} />
          <input
            type="text"
            placeholder="Search..."
          />
        </div>

       

        <div className="header-user">
          <div className="header-avatar">
            {user?.firstName?.[0]}
            {user?.lastName?.[0]}
          </div>

          <div>
            <span>{user?.firstName} {user?.lastName}</span>
            <small>
              {user?.role === 2 ? "Administrator" : "Employee"}
            </small>
          </div>
        </div>

      </div>
    </header>
  );
}