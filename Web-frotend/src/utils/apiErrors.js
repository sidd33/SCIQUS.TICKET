// Turns raw API error responses into messages a user can actually act on.
function extractRawMessage(err) {
  const data = err?.response?.data;
  if (!data) return '';
  if (typeof data === 'string') return data;
  return data.detail || data.message || data.title || '';
}

export function getFriendlyErrorMessage(err, context = {}) {
  const status = err?.response?.status;
  const raw = extractRawMessage(err);
  const lower = raw.toLowerCase();

  if (
    context.action === 'reassign' &&
    (lower.includes('department') || status === 409)
  ) {
    return "This agent can't be assigned to this ticket — they're in a different department than the one this ticket belongs to. Choose an agent from the matching department instead.";
  }

  if (status === 401) return 'Your session has expired. Please log in again.';
  if (status === 403) return "You don't have permission to do that.";
  if (status === 404) return 'This item no longer exists — it may have already been deleted.';
  if (status >= 500) return "Something went wrong on our end. Please try again in a moment.";

  return raw || 'Something went wrong. Please try again.';
}