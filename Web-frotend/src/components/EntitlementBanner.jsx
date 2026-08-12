import React from 'react';
import { Award, AlertCircle, CheckCircle2 } from 'lucide-react';
import './EntitlementBanner.scss';

export default function EntitlementBanner({ entitlement }) {
  if (!entitlement) {
    return (
      <div className="entitlement-banner entitlement-banner--standard">
        <Award size={18} />
        <span>Standard Account Plan · Unlimited Portal Ticket Entitlement</span>
      </div>
    );
  }

  const { planName = 'Silver Plan', totalAllowed = 50, usedCount = 12, isBlocked = false } = entitlement;
  const remaining = Math.max(0, totalAllowed - usedCount);
  const percentUsed = Math.min(100, Math.round((usedCount / totalAllowed) * 100));

  if (isBlocked || remaining === 0) {
    return (
      <div className="entitlement-banner entitlement-banner--exhausted">
        <AlertCircle size={18} />
        <div>
          <strong>{planName} · Quota Exhausted ({usedCount} of {totalAllowed} used)</strong>
          <p>Ticket creation is currently blocked for this account. Please upgrade your support plan.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="entitlement-banner entitlement-banner--active">
      <div className="banner-left">
        <CheckCircle2 size={18} />
        <span>
          <strong>{planName}</strong> · {usedCount} of {totalAllowed} tickets used (<strong>{remaining} remaining</strong>)
        </span>
      </div>
      <div className="progress-mini">
        <div className="progress-fill" style={{ width: `${percentUsed}%` }} />
      </div>
    </div>
  );
}
