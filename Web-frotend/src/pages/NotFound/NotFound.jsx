import { Link } from 'react-router-dom';
import './NotFound.scss';

export default function NotFound() {
  return (
   <div className="notfound-page">
  <span className="notfound-code">404</span>
  <h1>Page not found</h1>
  <p>The page you're looking for doesn't exist or has been moved.</p>
  <Link to="/dashboard" className="btn-primary">Go to Dashboard</Link>
</div>
  );
}
